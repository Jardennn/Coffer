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

            foreach (var file in root.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                results.Add(file);
            }
            return results;
        }
        
        public static bool HiddenDetect(FileInfo file)
        {
          if (OperatingSystem.IsWindows())
          {
            if (file.Attributes.HasFlag(FileAttributes.Hidden))
            {
              return true;
            }

            return false;
          }

          if (OperatingSystem.IsLinux())
          {
            if (file.Name.StartsWith("."))
            {
              return true;
            }

            return false;
          }

          if (OperatingSystem.IsMacOS())
          {
            if (file.Name.StartsWith(".") || file.Attributes.HasFlag(FileAttributes.Hidden))
            {
              return true;
            }

            return false;
          }

          return false;
        }

        public static bool ShouldInclude(FileInfo file, FilterConfig filters)
        {
          if (filters.ExcludeExtensions.Contains(file.Extension.ToLower())) // Extension filtering
          {
            return false;
          }

          if (file.Length > filters.MaxSizeMB) // File size limiation in MB
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

          if (filters.SkipHidden && HiddenDetect(file)) // Hidden files filtering (Checking if SkipHidden is set to true and using the hidden detection funtion for detecting if the path is hidden.)
          {
            return false;
          }

          return true;
        }
    }
}
