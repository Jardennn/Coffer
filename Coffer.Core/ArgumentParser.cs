using Coffer.Services.Models;

namespace Coffer.Core
{
    public class ArgumentParser
    {
        public static bool ApplyModify(string[] args, BackupProfile profile)
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
                        profile.Filters.MaxSizeMB = long.Parse(args[++i]);
                        modified = true;
                        break;

                    case "--min-mb":
                        profile.Filters.MinSizeMB = long.Parse(args[++i]);
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
                        profile.Filters.SkipHidden = bool.Parse(args[++i]);
                        modified = true;
                        break;

                    case "--skip-read-only":
                        profile.Filters.SkipReadOnly = bool.Parse(args[++i]);
                        modified = true;
                        break;

                    case "--skip-system-files":
                        profile.Filters.SkipSystemFiles = bool.Parse(args[++i]);
                        modified = true;
                        break;

                    case "--follow-symlinks":
                        profile.Filters.FollowSymlinks = bool.Parse(args[++i]);
                        modified = true;
                        break;

                    case "--mod-after":
                        if (!DateTime.TryParse(args[++i], out DateTime modifiedAfter))
                        {
                            return false;
                        }
                        profile.Filters.ModifiedAfter = modifiedAfter;
                        modified = true;
                        break;

                    case "--mod-before":
                        if (!DateTime.TryParse(args[++i], out DateTime modifiedBefore))
                        {
                            return false;
                        }
                        profile.Filters.ModifiedBefore = modifiedBefore;
                        modified = true;
                        break;

                    case "--created-after":
                        if (!DateTime.TryParse(args[++i], out DateTime createdAfter))
                        {
                            return false;
                        }
                        profile.Filters.CreatedAfter = createdAfter;
                        modified = true;
                        break;

                    case "--created-before":
                        if (!DateTime.TryParse(args[++i], out DateTime createdBefore))
                        {
                            return false;
                        }
                        profile.Filters.CreatedBefore = createdBefore;
                        modified = true;
                        break;

                    case "--mod-within-days":
                        profile.Filters.ModifiedWithinDays = int.Parse(args[++i]);
                        modified = true;
                        break;

                    case "--created-within-days":
                        profile.Filters.CreatedWithinDays = int.Parse(args[++i]);
                        modified = true;
                        break;

                }
            }
            return modified;
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
            Console.WriteLine("   --source <path>           Add a source path to the list on the config");
            Console.WriteLine("   --destination <path>      Set a destination path for the copy");
            Console.WriteLine("   --help                    Show this message");
        }
    }
}
