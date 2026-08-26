using Coffer.Services.Models;

namespace Coffer.Core
{
    public class ArgumentParser
    {
        public static (bool, bool, string? message) ApplyModify(string[] args, BackupProfile profile)
        {
            bool modified = false;
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--destination":
                        profile.DestinationPath = args[++i];
                        modified = true;
                        break;

                    case "--source":
                        while (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                        {
                            profile.SourcePaths.Add(args[++i]);
                        }
                        modified = true;
                        break;

                    // Filters modification

                    case "--exclude-ext":
                        while (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                        {
                            profile.Filters.ExcludeExtensions.Add(args[++i]);
                        }
                        modified = true;
                        break;

                    case "--include-ext":
                        profile.Filters.IncludeExtensions ??= new List<string>(); // If the IncludeExtensions list is null, the argument will make a new list. If it isn't null it woulnd't do anything.
                        while (i + 1 < args.Length && !args[i + 1].StartsWith("--")) // Going through every argument given until it reaches an out of bounds error or another passed flag.
                        {
                            profile.Filters.IncludeExtensions.Add(args[++i]);
                        }
                        modified = true;
                        break;

                    case "--max-mb":
                        if (!long.TryParse(args[++i], out long maxSize) || maxSize <= 0)
                        {
                            return (false, false, $"Invalid value for --max-mb: '{args[i]}'. Expected a positive number.");
                        }
                        profile.Filters.MaxSizeMB = maxSize;
                        modified = true;
                        break;

                    case "--min-mb":
                        if (!long.TryParse(args[++i], out long minSize) || minSize < 0)
                        {
                            return (false, false, $"Invalid value for --max-mb: '{args[i]}'. Expected a positive number or zero.");
                        }
                        profile.Filters.MaxSizeMB = minSize;
                        modified = true;
                        break;

                    case "--exclude-folder":
                        while (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                        {
                            profile.Filters.ExcludeFolders.Add(args[++i]);
                        }
                        modified = true;
                        break;

                    case "--exclude-path":
                        while (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                        {
                            profile.Filters.ExcludePaths.Add(args[++i]);
                        }
                        modified = true;
                        break;

                    case "--skip-hidden":
                        if (!bool.TryParse(args[++i], out bool skipHidden))
                        {
                            return (false, false, $"Invalid value for --skip-hidden: '{args[i]}'. Expected a boolean value (true/false).");
                        }
                        profile.Filters.SkipHidden = skipHidden;
                        modified = true;
                        break;

                    case "--skip-read-only":
                        if (!bool.TryParse(args[++i], out bool skipReadOnly))
                        {
                            return (false, false, $"Invalid value for --skip-read-only: '{args[i]}'. Expected a boolean value (true/false).");
                        }
                        profile.Filters.SkipReadOnly = skipReadOnly;
                        modified = true;
                        break;

                    case "--skip-system-files":
                        if (!bool.TryParse(args[++i], out bool skipSystemFiles))
                        {
                            return (false, false, $"Invalid value for --skip-system-files: '{args[i]}'. Expected a boolean value (true/false).");
                        }
                        profile.Filters.SkipSystemFiles = skipSystemFiles;
                        modified = true;
                        break;

                    case "--follow-symlinks":
                        if (!bool.TryParse(args[++i], out bool followSymlinks))
                        {
                            return (false, false, $"Invalid value for --follow-symlinks: '{args[i]}'. Expected a boolean value (true/false).");
                        }
                        profile.Filters.FollowSymlinks = followSymlinks;
                        modified = true;
                        break;

                    case "--mod-after":
                        if (!DateTime.TryParse(args[++i], out DateTime modifiedAfter))
                        {
                            return (false, false, $"Invalid value for --mod-after: '{args[i]}'. Expected a DateTime value (yyyy-MM-dd).");
                        }
                        profile.Filters.ModifiedAfter = modifiedAfter;
                        modified = true;
                        break;

                    case "--mod-before":
                        if (!DateTime.TryParse(args[++i], out DateTime modifiedBefore))
                        {
                            return (false, false, $"Invalid value for --mod-before: '{args[i]}'. Expected a DateTime value (yyyy-MM-dd).");
                        }
                        profile.Filters.ModifiedBefore = modifiedBefore;
                        modified = true;
                        break;

                    case "--created-after":
                        if (!DateTime.TryParse(args[++i], out DateTime createdAfter))
                        {
                            return (false, false, $"Invalid value for --created-after: '{args[i]}'. Expected a DateTime value (yyyy-MM-dd).");
                        }
                        profile.Filters.CreatedAfter = createdAfter;
                        modified = true;
                        break;

                    case "--created-before":
                        if (!DateTime.TryParse(args[++i], out DateTime createdBefore))
                        {
                            return (false, false, $"Invalid value for --created-before: '{args[i]}'. Expected a DateTime value (yyyy-MM-dd).");
                        }
                        profile.Filters.CreatedBefore = createdBefore;
                        modified = true;
                        break;

                    case "--mod-within-days":
                        if (!int.TryParse(args[++i], out int modWithin) || modWithin <= 0)
                        {
                            return (false, false, $"Invalid value for --mod-within-days: '{args[i]}'. Expected a positive number.");
                        }
                        profile.Filters.ModifiedWithinDays = modWithin;
                        modified = true;
                        break;

                    case "--created-within-days":
                        if (!int.TryParse(args[++i], out int createdWithin) || createdWithin <= 0)
                        {
                            return (false, false, $"Invalid value for --created-within-days: '{args[i]}'. Expected a positive number.");
                        }
                        profile.Filters.CreatedWithinDays = createdWithin;
                        modified = true;
                        break;

                        // Nullifiers / removers

                    case "--rm-src":
                        while (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                        {
                            profile.SourcePaths?.Remove(args[++i]);
                        }
                        modified = true;
                        break;

                    case "--clr-srcs":
                        profile.SourcePaths = new() { };
                        modified = true;
                        break;

                    case "--rm-excluded-ext":
                        profile.Filters.ExcludeExtensions.Remove(args[++i]);
                        modified = true;
                        break;

                    case "--clr-exclude-ext":
                        profile.Filters.ExcludeExtensions = new() { };
                        modified = true;
                        break;

                    case "--rm-included-ext":
                        profile.Filters.IncludeExtensions?.Remove(args[++i]);
                        modified = true;
                        break;

                    case "--clr-include-ext":
                        profile.Filters.IncludeExtensions = null;
                        modified = true;
                        break;

                    case "--clr-max-mb":
                        profile.Filters.MaxSizeMB = null;
                        modified = true;
                        break;

                    case "--rm-excluded-folder":
                        profile.Filters.ExcludeFolders.Remove(args[++i]);
                        modified = true;
                        break;

                    case "--clr-exclude-folder":
                        profile.Filters.ExcludeFolders = new() { };
                        modified = true;
                        break;

                    case "--rm-excluded-path":
                        profile.Filters.ExcludePaths.Remove(args[++i]);
                        modified = true;
                        break;

                    case "--clr-exclude-path":
                        profile.Filters.ExcludePaths = new() { };
                        modified = true;
                        break;

                    case "--clr-mod-after":
                        profile.Filters.ModifiedAfter = null;
                        modified = true;
                        break;

                    case "--clr-mod-before":
                        profile.Filters.ModifiedBefore = null;
                        modified = true;
                        break;

                    case "--clr-created-after":
                        profile.Filters.CreatedAfter = null;
                        modified = true;
                        break;

                    case "--clr-created-before":
                        profile.Filters.CreatedBefore = null;
                        modified = true;
                        break;

                    case "--clr-modified-within":
                        profile.Filters.ModifiedWithinDays = null;
                        modified = true;
                        break;

                    case "--clr-created-within":
                        profile.Filters.CreatedWithinDays = null;
                        modified = true;
                        break;
                }
            }
            return (true, modified, null);
        }

        public static string? GetProfileName(string[] args)
        {
            string? profilename = "default";
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--profile")
                {
                    profilename = args[++i];
                    break;
                }
            }
            return profilename;
        }

        public static Action? GetAction(string[] args)
        {
            if (args.Contains("--run"))
                return Action.Run;
            if (args.Contains("--status"))
                return Action.Status;
            if (args.Contains("--help"))
                return Action.Help;
            if (args.Contains("--list-profiles"))
                return Action.ListProfiles;
            return null;
        }

        public enum Action
        {
            Run,
            Status,
            Help,
            ListProfiles
        }

        public static void PrintHelp()
        {
            Console.WriteLine("Coffer - Local backup tool");
            Console.WriteLine();
            Console.WriteLine("Usage: coffer [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("   --profile \"<profile name>\"            Load an existing profile or create a new profile if the given profile doesn't exist");
            Console.WriteLine("   --source  \"<path>\"                    Add a source path to the list on the config");
            Console.WriteLine("   --destination \"<path>\"                Set a destination path for the copy");
            Console.WriteLine("   --help                                Show this message");
            Console.WriteLine();
            Console.WriteLine("Actions - interaction with the program.");
            Console.WriteLine("   --run                                 Run the backup");
            Console.WriteLine("   --status                              Print the status of the current loaded profile");
            Console.WriteLine("   --list-profiles                       List the saved available profiles");
            Console.WriteLine();
            Console.WriteLine("Filters - Configurating filters for the profile config.");
            Console.WriteLine("   --exclude-ext \"<extensions>\"          Add extensions to exclude onto ExcludeExtensions filter (e.g. .txt)");
            Console.WriteLine("   --include-ext \"<extensions>\"          Add extensions to include onto IncludeExtensions filter (e.g. .txt)");
            Console.WriteLine("   --max-mb <size (in MB)>               Set the maximum MB size per file in the backup");
            Console.WriteLine("   --min-mb <size (in MB)>               Set the minimum MB size per file in the backup");
            Console.WriteLine("   --exclude-folder \"<folder name>\"      Add a folder name to exclude onto ExcludeFolder filter");
            Console.WriteLine("   --exclude-path \"<abs path>\"           Add a path to exclude onto ExcludePath filter (e.g. /path/to/exclude)");
            Console.WriteLine("   --skip-hidden <true/false>            Toggle if to skip hidden files on backup or not");
            Console.WriteLine("   --skip-read-only <true/false>         Toggle if to skip read only files on backup or not");
            Console.WriteLine("   --skip-system-files <true/false>      Toggle if to skip system files on backup or not");
            Console.WriteLine("   --follow-symlinks <true/false>        Toggle if to follow symlinks on backup or not");
            Console.WriteLine("   --mod-after <yyyy-MM-dd>              Only include files modified after given date");
            Console.WriteLine("   --mod-before <yyyy-MM-dd>             Only include files modified before given date");
            Console.WriteLine("   --created-after <yyyy-MM-dd>          Only include files created after given date");
            Console.WriteLine("   --created-before <yyyy-MM-dd>         Only include files created before given date");
            Console.WriteLine("   --mod-within-days <days>              Only include files modified within the given days count");
            Console.WriteLine("   --created-within-days                 Only include files created within the given days count");
            Console.WriteLine();
            Console.WriteLine("Nullifiers - Remove/clear configured filters");
            Console.WriteLine("   --rm-src \"<source>\"                   Remove a source off the sources list");
            Console.WriteLine("   --clr-srcs                            Clear the sources list (removes all sources from the list)");
            Console.WriteLine("   --rm-excluded-ext \"<extension>\"       Remove an extension off the excluded extensions list");
            Console.WriteLine("   --clr-exclude-ext                     Clear the excluded extensions list");
            Console.WriteLine("   --rm-included-ext \"<extension>\"       Remove an included extension off the included extensions list");
            Console.WriteLine("   --clr-include-ext                     Clear the included extensions list");
            Console.WriteLine("   --clr-max-mb                          Set the maximum MB size per file to unlimited (any size passes)");
            Console.WriteLine("   --rm-excluded-folder \"<folder name>\"  Remove an excluded folder name off the list of excluded folders");
            Console.WriteLine("   --clr-exclude-folder                  Clear the list of excluded folders");
            Console.WriteLine("   --rm-excluded-path \"<path>\"           Remove a path off the excluded paths list");
            Console.WriteLine("   --clr-exclude-path                    Clear the list of excluded paths");
            Console.WriteLine("   --clr-mod-after                       Set the ModifiedAfter filter to null (removing the limit)");
            Console.WriteLine("   --clr-mod-before                      Set the ModifiedBefore filter to null (removing the limit)");
            Console.WriteLine("   --clr-created-after                   Set the CreatedAfter filter to null (removing the limit)");
            Console.WriteLine("   --clr-created-before                  Set the CreatedBefore filter to null (removing the limit)");
            Console.WriteLine("   --clr-modified-within                 Set the ModifiedWithinDays filter to null (removing the limit)");
            Console.WriteLine("   --clr-created-within                  Set the CreatedWithinDays filter to null (removing the limit)");
        }
    }
}
