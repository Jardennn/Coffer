using System;
using System.IO;
using Coffer.Services;
using Coffer.Services.Models;
using Coffer.Services.Helpers;

namespace Coffer.Core
{
    class Program
    {
        static void Main(String[] args)
        {
            BackupProfile profile = ProfileService.Load(ProfileService.GetActiveProfile());

            string? profileSwitch = ArgumentParser.GetProfileName(args);
            if (profileSwitch != null && profileSwitch != ProfileService.GetActiveProfile())
            {
                ProfileService.SetActiveProfile(profileSwitch);
                profile = ProfileService.Load(profileSwitch);
                Console.WriteLine($"Switched to profile: {profileSwitch}");
                return;
            }

            var (result, message) = ArgumentParser.ProfileActions(args);
            switch (result)
            {
                case ArgumentParser.Result.Modified:
                    Console.WriteLine(message);
                    return;

                case ArgumentParser.Result.Cancelled:
                    Console.WriteLine($"[INFO] {message}");
                    return;

                case ArgumentParser.Result.Error:
                    Console.WriteLine($"[ERROR] {message}");
                    Console.WriteLine("Run 'coffer --help' for usage information.");
                    return;
            }

            (bool success, bool mod, string? error) modified = ArgumentParser.ApplyModify(args, profile);
            if (modified.mod)
            {
                ProfileService.Save(profile, ProfileService.GetActiveProfile());
                Console.WriteLine("Profile updated.");
                return;
            }
            else if (!modified.success)
            {
                Console.WriteLine($"[ERROR] {modified.error}");
                Console.WriteLine("Run 'coffer --help' for usage information.");
                return;
            }

            var action = ArgumentParser.GetAction(args);
            switch (action)
            {
                case ArgumentParser.Action.Run:
                    RunBackup(profile);
                    break;
                case ArgumentParser.Action.Status:
                    // Print status function here (PrintStatus(profile))
                    PrintStatus(profile);
                    break;
                case ArgumentParser.Action.Help:
                    ArgumentParser.PrintHelp();
                    break;
                case ArgumentParser.Action.ListProfiles:
                    ProfileService.ListProfiles();
                    break;
                case null:
                    ArgumentParser.PrintHelp();
                    break;
            }
        }

        static void RunBackup(BackupProfile profile)
        {
            FilterConfig filters = profile.Filters;
            List<string> srcpaths = profile.SourcePaths;
            string dst = profile.DestinationPath;
            int chunksize = profile.copyConfig.ChunkSizeMB;

            Console.WriteLine("Starting backup.");
            Console.WriteLine("--------------------------------");
            Console.WriteLine("Starting preflight checks.");
            var item = PreFlight.Run(srcpaths, dst, filters);

            if (item.valBlock != "")
            {
                Console.WriteLine($"[ERROR] {item.valBlock}");
                Console.WriteLine("There was an error in finding one of the sources/destination or in reading/writing. Fix the above error and try again.");
                Environment.Exit(1);
            }

            if (item.errors.Count != 0)
            {
                foreach (var error in item.errors)
                {
                    Console.WriteLine($"[ERROR] {error}");
                }
                Console.WriteLine("Backup cannot proceed, Fix the above errors and try again.");
                Environment.Exit(1);
            }

            if (item.warns.Count != 0)
            {
                foreach (var warn in item.warns)
                {
                    Console.WriteLine($"[WARN] {warn}");
                }

                Console.WriteLine("Continue anyway? (y/n)");
                char confirm = Char.Parse(Console.ReadLine() ?? " ");

                while (true)
                {
                    if (char.ToLower(confirm) == 'n')
                    {
                        Console.WriteLine("Backup cancelled.");
                        Environment.Exit(0);
                    }
                    else if (char.ToLower(confirm) == 'y')
                    {
                        break;
                    }
                    Console.WriteLine("Input was invalid, try again");
                    Console.WriteLine("Continue anyway? (y/n)");
                    confirm = Char.Parse(Console.ReadLine() ?? " ");
                }
            }
            Console.WriteLine("\nPreflight checks passed, starting backup...\n");
            Console.WriteLine("--------------------------------\n");

            foreach (string srcpath in srcpaths) // Going through every path in srcpaths
            {
                // string srcpath = srcpaths[i];
                List<FileInfo> filepaths = Scanner.Scan(srcpath, filters);

                if (filepaths.Count == 0)
                {
                    Console.WriteLine($"No files found in {srcpath}, skipping.");
                    continue;
                }

                foreach (var filepath in filepaths)
                {
                    string filepathstr = filepath.FullName;
                    string dstpath = GetDest.GetDestPath(srcpath, filepathstr, dst);

                    Directory.CreateDirectory(Path.GetDirectoryName(dstpath)!); // Creates a directory in the same name of a directory that contains a nested source file.

                    var decision = Copier.ShouldCopy(filepath, dstpath, profile.copyConfig.duplicateHandle);

                    switch (decision)
                    {
                        case Copier.CopyDecision.Skip:
                            Console.WriteLine($"[SKIP] {filepath.Name}");
                            continue;

                        case Copier.CopyDecision.Rename:
                            dstpath = PathHelper.GetRenamedPath(dstpath);
                            goto case Copier.CopyDecision.Copy;

                        case Copier.CopyDecision.Copy:
                            Console.WriteLine($"Beginning data transfer for: {filepathstr}");
                            string srchash = Copier.CopyFile(filepathstr, dstpath, chunksize, onProgress: (copied, total) => // Using lambda to pass onProgress as a function that will take 'copied' and 'total' as arguments.
                            {
                                int percent = (int)Math.Clamp((double)copied / total * 100, 0, 100); // Percentage calculation for how much of the file was copied. Math.Clamp is for limiting the bar to be within 0 to 100, to prevent crashes.
                                int filled = percent / 2; // Making the progress bar 50 characters wide
                                string bar = new string('█', filled) + new string('░', 50 - filled); // Two strings are combined to make the progress bar, the filled blocks are in the same length as 'filled' that changes dynamically and the empty blocks are the full length of the bar (50) - the filled blocks (filled variable).
                                Console.Write($"\r [{bar}] {percent}% {copied / 1024 / 1024}MB / {total / 1024 / 1024}MB"); // Output of the progress bar, \r moves the console back to the beginning of the line for updating the progress bar.
                            });
                            Console.WriteLine();

                            if (profile.copyConfig.VerifyAfterCopy)
                            {
                                bool verification = Verifier.Verify(srchash, dstpath, 4, onProgress: (appended, total) =>
                                {
                                    int percent = (int)Math.Clamp((double)appended / total * 100, 0, 100);
                                    int filled = percent / 2;
                                    string bar = new string('█', filled) + new string('░', 50 - filled);
                                    Console.Write($"\r [{bar}] {percent}%");
                                });
                                Console.WriteLine();
                                Console.WriteLine();

                                if (verification)
                                {
                                    Console.WriteLine("✓ OK");
                                }
                                else
                                    Console.WriteLine("✗ FAILED — checksum mismatch");
                            }
                            Console.Write("\n--------------------------------\n");

                            break;
                    }

                }
            }
            Console.WriteLine("--------------------------------");
            Console.WriteLine("Backup complete.");
        }

        static void PrintStatus(BackupProfile profile)
        {
            Console.WriteLine("Coffer profile status");
            Console.WriteLine();
            Console.WriteLine($"Currently loaded/active profile: {ProfileService.GetActiveProfile()}");
            Console.WriteLine();

            if (profile.SourcePaths.Count == 0)
            {
                Console.WriteLine("No sources have been set yet.");
            }

            else
            {
                Console.WriteLine("Sources:");
                foreach (string source in profile.SourcePaths)
                {
                    Console.WriteLine($"     * {source}");
                }
            }

            Console.WriteLine();

            if (string.IsNullOrWhiteSpace(profile.DestinationPath))
            {
                Console.WriteLine("Destination not set yet.");
            }
            else
            {
                Console.WriteLine($"Destination: {profile.DestinationPath}");
            }

            Console.WriteLine();

            Console.WriteLine("Active filters:");
            bool anyFilterChanges = false;
            BackupProfile defaults = new BackupProfile();

            if (profile.Filters.ExcludeExtensions != defaults.Filters.ExcludeExtensions && profile.Filters.ExcludeExtensions?.Count > 0)
            {
                Console.WriteLine($"Excluded Extensions: {string.Join(", ", profile.Filters.ExcludeExtensions)}");
                anyFilterChanges = true;
            }

            if (profile.Filters.IncludeExtensions?.Count > 0)
            {
                Console.WriteLine($"Included Extensions: {string.Join(", ", profile.Filters.IncludeExtensions)}");
                anyFilterChanges = true;
            }

            if (profile.Filters.IncludeExtensions?.Count == 0)
            {
                Console.WriteLine($"Included extensions list is empty, add an extension or clear (--clr-include-ext) to make the backup work.");
                anyFilterChanges = true;
            }

            if (profile.Filters.MaxSizeMB != defaults.Filters.MaxSizeMB)
            {
                if (profile.Filters.MaxSizeMB is null)
                {
                    Console.WriteLine($"Max size per file: unlimited");
                }
                else
                {
                    Console.WriteLine($"Max size per file: {profile.Filters.MaxSizeMB}MB");
                }
                anyFilterChanges = true;
            }

            if (profile.Filters.MinSizeMB != defaults.Filters.MinSizeMB)
            {
                Console.WriteLine($"Min size for file: {profile.Filters.MinSizeMB}MB");
                anyFilterChanges = true;
            }

            if (profile.Filters.ExcludeFolders != defaults.Filters.ExcludeFolders && profile.Filters.ExcludeFolders?.Count > 0)
            {
                Console.WriteLine($"Excluded folder names: {string.Join(", ", profile.Filters.ExcludeFolders)}");
                anyFilterChanges = true;
            }

            if (profile.Filters.ExcludePaths != defaults.Filters.ExcludePaths && profile.Filters.ExcludePaths?.Count > 0)
            {
                Console.WriteLine($"Excluded paths: {string.Join(", ", profile.Filters.ExcludePaths)}");
                anyFilterChanges = true;
            }

            if (profile.Filters.IncludeFolders?.Count > 0)
            {
                Console.WriteLine($"Included folders: {string.Join(", ", profile.Filters.IncludeFolders)}");
                anyFilterChanges = true;
            }

            if (profile.Filters.IncludeFolders?.Count == 0)
            {
                Console.WriteLine($"Included folders list is empty, add a folder name or clear (--clr-include-folders) to make the backup work.");
                anyFilterChanges = true;
            }

            if (profile.Filters.IncludeFileName?.Count > 0)
            {
                Console.WriteLine($"Included file names: {string.Join(", ", profile.Filters.IncludeFileName)}");
                anyFilterChanges = true;
            }

            if (profile.Filters.IncludeFileName?.Count == 0)
            {
                Console.WriteLine($"Included file names list is empty, add an file name (or a part of it) or clear (--clr-include-files) to make the backup work.");
                anyFilterChanges = true;
            }

            if (profile.Filters.SkipHidden)
            {
                Console.WriteLine("Skipping hidden files.");
                anyFilterChanges = true;
            }

            if (profile.Filters.SkipReadOnly)
            {
                Console.WriteLine("Skipping read only files.");
                anyFilterChanges = true;
            }

            if (profile.Filters.SkipSystemFiles)
            {
                Console.WriteLine("Skipping system files.");
                anyFilterChanges = true;
            }

            if (profile.Filters.FollowSymlinks)
            {
                Console.WriteLine("Following symlinks.");
                anyFilterChanges = true;
            }

            if (profile.Filters.ModifiedAfter != defaults.Filters.ModifiedAfter)
            {
                Console.WriteLine($"Including files modified after: {profile.Filters.ModifiedAfter:yyyy-MM-dd}");
                anyFilterChanges = true;
            }

            if (profile.Filters.ModifiedBefore != defaults.Filters.ModifiedBefore)
            {
                Console.WriteLine($"Including files modified before: {profile.Filters.ModifiedBefore:yyyy-MM-dd}");
                anyFilterChanges = true;
            }

            if (profile.Filters.CreatedAfter != defaults.Filters.CreatedAfter)
            {
                Console.WriteLine($"Including files created after: {profile.Filters.CreatedAfter:yyyy-MM-dd}");
                anyFilterChanges = true;
            }

            if (profile.Filters.CreatedBefore != defaults.Filters.CreatedBefore)
            {
                Console.WriteLine($"Including files created before: {profile.Filters.CreatedBefore:yyyy-MM-dd}");
                anyFilterChanges = true;
            }

            if (profile.Filters.ModifiedWithinDays != defaults.Filters.ModifiedWithinDays)
            {
                Console.WriteLine($"Including files modified within {profile.Filters.ModifiedWithinDays} days");
                anyFilterChanges = true;
            }

            if (profile.Filters.CreatedWithinDays != defaults.Filters.CreatedWithinDays)
            {
                Console.WriteLine($"Including files created within {profile.Filters.CreatedWithinDays} days");
                anyFilterChanges = true;
            }

            if (profile.copyConfig.duplicateHandle != defaults.copyConfig.duplicateHandle)
            {
                Console.WriteLine($"Handling duplicate files with {profile.copyConfig.duplicateHandle}");
                anyFilterChanges = true;
            }

            if (profile.copyConfig.ChunkSizeMB != defaults.copyConfig.ChunkSizeMB)
            {
                Console.WriteLine($"Transfer chunk size in MB: {profile.copyConfig.ChunkSizeMB}");
                anyFilterChanges = true;
            }

            if (!profile.copyConfig.VerifyAfterCopy)
            {
                Console.WriteLine("Not verifying after file transfer.");
                anyFilterChanges = true;
            }

            if (!anyFilterChanges)
                Console.WriteLine("All filters are running at defaults.");

        }
    }
}
