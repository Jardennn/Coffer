using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Coffer.Services
{
    public class Detectors
    {
        public static bool HiddenDetect(FileInfo file)
        {
            if (OperatingSystem.IsWindows())
            {
                if (file.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    return true;
                }

                DirectoryInfo? dir = file.Directory;
                while(dir != null)
                {
                    if (dir.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        return true;
                    }
                    dir = dir.Parent;
                }

                return false;
            }

            if (OperatingSystem.IsLinux())
            {
                if (file.Name.StartsWith("."))
                {
                    return true;
                }

                DirectoryInfo? dir = file.Directory;
                while (dir != null)
                {
                    if (dir.Name.StartsWith("."))
                    {
                        return true;
                    }
                    dir = dir.Parent;
                }

                return false;
            }

            if (OperatingSystem.IsMacOS())
            {
                if (file.Name.StartsWith(".") || file.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    return true;
                }

                DirectoryInfo? dir = file.Directory;
                while (dir != null)
                {
                    if (dir.Name.StartsWith(".") || dir.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        return true;
                    }
                    dir = dir.Parent;
                }

                return false;
            }
            return false;
        }

        public static bool SystemFileDetect(FileInfo file)
        {
          if (OperatingSystem.IsWindows())
          {
            if (file.Attributes.HasFlag(FileAttributes.System))
            {
              return true;
            }

            return false;
          }

          else
          {
            if (file.FullName.StartsWith("/sys/", StringComparison.Ordinal) ||
                file.FullName.StartsWith("/proc/", StringComparison.Ordinal) ||
                file.FullName.StartsWith("/dev/", StringComparison.Ordinal) ||
                file.FullName.StartsWith("/bin/",StringComparison.Ordinal) ||
                file.FullName.StartsWith("/sbin/", StringComparison.Ordinal) ||
                file.FullName.StartsWith("/var/", StringComparison.Ordinal)
                )
            {
              return true;
            }

            return false;
          }
        }

        public static bool SymlinkParentDetect(FileInfo file)
        {
          DirectoryInfo? dir = file.Directory;
          while (dir != null)
          {
            if (dir.LinkTarget != null)
            {
              return true;
            }
            dir = dir.Parent;
          }

          return false;
        }
    }
}
