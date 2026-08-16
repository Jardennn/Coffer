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

        public static long GetSizes(List<string> srcs, FilterConfig filters)
        {
          long totalsize = 0;
          foreach (string src in srcs)
          {
                long size = Scanner.Scan(src, filters).Sum(file => file.Length);
                totalsize += size;
          }

          return totalsize;
        }

        public static long? GetFreeSpace(string dst)
        {
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                long targetDrive = Helpers.OSHelper.GetDeviceID(dst);

                foreach (var drive in DriveInfo.GetDrives())
                {
                    try
                    {
                        if (Helpers.OSHelper.GetDeviceID(drive.RootDirectory.FullName) == targetDrive)
                        {
                            return drive.AvailableFreeSpace;
                        }
                    }

                    catch (IOException)
                    {
                        continue;
                    }
                }
            }

            else
            {
                string? driveroot = Path.GetPathRoot(dst);
                long freespace = 0;

                if (!string.IsNullOrEmpty(driveroot))
                {
                    DriveInfo drive = new DriveInfo(driveroot);

                    if (drive.IsReady)
                    {
                        Console.WriteLine(drive.AvailableFreeSpace);
                        freespace = drive.AvailableFreeSpace;
                    }
                }

                return freespace;
            }
            return null;
        }

        public static int GetFileCount(List<string> srcs, FilterConfig filters)
        {
            int TotalCount = 0;
            foreach (string src in srcs)
            {
                int count = Scanner.Scan(src, filters).Count;
                TotalCount += count;
            }
            return TotalCount;
        }
    }
}
