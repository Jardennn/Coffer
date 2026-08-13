using System.IO;
using System.Collections.Generic;
using Coffer.Services.Models;

namespace Coffer.Services
{
    public class Scanner
    {
        public static List<FileInfo> Scan(string srcRoot)
        {
            var results = new List<FileInfo>();
            var root = new DirectoryInfo(srcRoot);

            var enumeriationOptions = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true
            };

            foreach (var file in root.EnumerateFiles("*", enumeriationOptions))
            {
                results.Add(file);
            }
            return results;
        }



        public static bool ShouldInclude(FileInfo file, FilterConfig filters)
        {
            if (filters.ExcludeExtensions.Contains(file.Extension.ToLower())) // Extension exclusion filtering
            {
                return false;
            }

            if (filters.IncludeExtensions != null && !filters.IncludeExtensions.Contains(file.Extension.ToLower())) // Extension inclusion filtering
            {
                return false;
            }

            if (filters.MaxSizeMB.HasValue && file.Length > filters.MaxSizeMB * 1024 * 1024) // File size limiation in MB
            {
                return false;
            }

            if (file.Length < filters.MinSizeMB * 1024 * 1024) // File size filtering by minimum MBs
            {
                return false;
            }

            foreach (var excludedFolder in filters.ExcludeFolders) // Folder filtering (Checking if the given folder is in the full path of the given file)
            {
                if (file.FullName.Contains(Path.DirectorySeparatorChar + excludedFolder + Path.DirectorySeparatorChar))
                {
                  return false;
                }
            }

            foreach (var excludedPath in filters.ExcludePaths)
            {
                if (file.FullName.StartsWith(excludedPath))
                {
                    return false;
                }
            }

            if (filters.SkipHidden && Detectors.HiddenDetect(file)) // Hidden files filtering (Checking if SkipHidden is set to true and using the hidden detection funtion for detecting if the path is hidden.)
            {
                return false;
            }

            if (filters.SkipReadOnly && file.IsReadOnly)
            {
                return false;
            }

            if (filters.SkipSystemFiles && Detectors.SystemFileDetect(file))
            {
                return false;
            }

            if (!filters.FollowSymlinks && file.LinkTarget != null)
            {
                return false;
            }

            if (!filters.FollowSymlinks && Detectors.SymlinkParentDetect(file))
            {
                return false;
            }

            // If file last modification date is before the filter date, it will exclude it.
            if (filters.ModifiedAfter.HasValue && filters.ModifiedAfter.Value > file.LastWriteTime)
            {
                return false;
            }

            // If file modification date is after the filter date, it will exclude it.
            if (filters.ModifiedBefore.HasValue && filters.ModifiedBefore.Value < file.LastWriteTime)
            {
                return false;
            }

            if (filters.CreatedAfter.HasValue && filters.CreatedAfter.Value > file.CreationTime)
            {
                return false;
            }

            if (filters.CreatedBefore.HasValue && filters.CreatedBefore.Value < file.CreationTime)
            {
                return false;
            }

            if (filters.ModifiedWithinDays.HasValue)
            {
              DateTime Cutoff = DateTime.Now.AddDays(-filters.ModifiedWithinDays.Value);

              if (file.LastWriteTime < Cutoff)
              {
                    return false;
              }
            }

            if (filters.CreatedWithinDays.HasValue)
            {
              DateTime Cutoff = DateTime.Now.AddDays(-filters.CreatedWithinDays.Value);

              if (file.CreationTime < Cutoff)
              {
                  return false;
              }
            }

            return true;

        }
    }
}
