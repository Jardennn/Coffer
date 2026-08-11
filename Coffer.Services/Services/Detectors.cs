using System;
using System.IO;
using System.Runtime.InteropServices;
using Coffer.Services.Models;

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

        public static long GetSizes(string[] srcs, FilterConfig filters)
        {
          long totalsize = 0;
          foreach(var srcstring in srcs)
          {
            DirectoryInfo src = new DirectoryInfo(srcstring);

            long size = src.EnumerateFiles("*", SearchOption.AllDirectories).Where(file => Scanner.ShouldInclude(file, filters)).Sum(file => file.Length);
            totalsize += size;
          }

          return totalsize;
        }

        public static long GetFreeSpace(string dst)
        {
            string driveroot = Path.GetPathRoot(dst);
            long freespace = 0;

            if (!string.IsNullOrEmpty(driveroot))
            {
                DriveInfo drive = new DriveInfo(driveroot);

                if (drive.IsReady)
                {
                    freespace = drive.AvailableFreeSpace;
                }
            }

            return freespace;
        }

        public static int GetFileCount(string[] srcs, FilterConfig filters)
        {
            int TotalCount = 0;
            for (int i = 0; i < srcs.Length; i++)
            {
                string src = srcs[i];
                DirectoryInfo srcDir = new DirectoryInfo(src);
                var enumeriationOptions = new EnumerationOptions
                {
                    IgnoreInaccessible = true,
                    RecurseSubdirectories = true
                };
                int count = srcDir.EnumerateFiles("*", enumeriationOptions).Count(file => Scanner.ShouldInclude(file, filters));
                TotalCount += count;
            }
            return TotalCount;
        }
    }
}
