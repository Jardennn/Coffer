using System;
using System.IO;
using Coffer.Services.Models;

namespace Coffer.Services
{
  public class PreFlight
  {

    public static (bool hasIssue, bool isError, string message) SourceExists(string[] srcs) // Checking if passed sources exist.
    {
      foreach (var src in srcs)
      {
        if (!Directory.Exists(src))
        {
          return (true, true, $"Source: {src} was not found.");
        }
      }

      return (false, false, "");
    }

    public static (bool hasIssue, bool isError, string message) SourceReadable(string[] srcs) // Checking if passed sources are readable.
    {
      foreach (var src in srcs)
      {
        try
        {
          Directory.EnumerateFileSystemEntries(src).FirstOrDefault();
        }
        catch (UnauthorizedAccessException)
        {
            return (true, true, $"No read permissions for {src}");
        }
        catch (Exception)
        {
            return (true, true, $"Unexpected error was encountered checking {src} for read permissions.");
        }
      }

      return (false, false, "");
    }

    public static (bool hasIssue, bool isError, string message) DestExist(string dst) // Checking if the destination exists.
    {
      if (!Directory.Exists(dst))
      {
          return (true, true, $"Destination: {dst} was not found.");
      }

      return (false, false, "");
    }

    public static (bool hasIssue, bool isError, string message) FreeSpace(string[] srcs, string dst, FilterConfig filters, int bufferGB = 2) // Checking if there's free space for the backup.
    {
      long needed = Detectors.GetSizes(srcs, filters);
      long available = Detectors.GetFreeSpace(dst);

      if (needed + (bufferGB + Math.Pow(1024, 3)) > available)
      {
          return (true, true, $"Not enough free space, {needed / 1024 / 1024 / 1024}GB required while only {available / 1024 / 1024 / 1024}GB is available.");
      }

      return (false, false, "");
    }

    public static (bool hasIssue, bool isError, string message) DestWritable(string dst) // Checking if the destination is writable.
    {
        string testfile = dst + Path.DirectorySeparatorChar + ".coffertest"; // Making a test file in the destination.
        try
        {
            File.WriteAllText(testfile, "test");
            File.Delete(testfile);
        }
        catch (UnauthorizedAccessException)
        {
            return (true, true, "Destination is not writeable.");
        }

        return (false, false, "");
    }

        public static (bool hasIssue, bool isError, string message) IsSameDrive(string[] srcs, string dst) // Checking for if a source and the destination are on the same drive.
        {
            DriveInfo dstDrive = new DriveInfo(dst);
            for (int i = 0; i < srcs.Length; i++)
            {
                string src = srcs[i];
                DriveInfo srcDrive = new DriveInfo(src);

                if (string.Equals(dstDrive.Name, srcDrive.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return (true, false, $"Source {src} and destination {dst} are on the same physical drive.");
                }
            }

            return (false, false, "");
        }

        public static (bool hasIssue, bool isError, string message) IsCaricular(string[] srcs, string dst) // Checking if the destination is placed inside one of the given sources.
        {
            for (int i = 0; i < srcs.Length; i++)
            {
                string src = srcs[i];
                string relative = Path.GetRelativePath(src, dst);

                if (!relative.StartsWith("..") && !Path.IsPathFullyQualified(relative))
                {
                    return (true, true, $"Source {src} and Destionation {dst} are relative.");
                }
            }

            return (false, false, "");
        }

        public static (bool hasIssue, bool isError, string message) FileCount(string[] srcs, FilterConfig filters, int warn_threshold = 50000) // Checking for file count in the sources.
        {
            int count = Detectors.GetFileCount(srcs, filters);
            if (count > warn_threshold)
            {
                return (true, false, $"{count} files queued, This may take a while.");
            }

            return (false, false, "");
        }

        public static (bool hasIssue, bool isError, string message) LongPaths(string[] srcs, string dst) // Checking for long paths (not really up to date with how the program gets the destination path.).
        {
            for (int i = 0; i < srcs.Length; i++)
            {
                string src = srcs[i];
                DirectoryInfo srcDir = new DirectoryInfo(src);
                string absDst = dst + Path.DirectorySeparatorChar + srcDir.Name;
                if (absDst.Length > 255)
                {
                    return (true, false, $"Path {absDst} might be too long, might fail on Windows.");
                }
            }

            return (false, false, "");
        }

    public static (List<string> errors, List<string> warns) Run(string[] srcs, string dst, FilterConfig filters) // Running the actual preflight checks.
    {
        List<string> errors = new List<string>();
        List<string> warns = new List<string>();

        (bool, bool, string)[] checks = {
            SourceExists(srcs),
            SourceReadable(srcs),
            DestExist(dst),
            FreeSpace(srcs, dst, filters),
            DestWritable(dst),
            IsSameDrive(srcs, dst),
            IsCaricular(srcs, dst),
            FileCount(srcs, filters),
            LongPaths(srcs, dst)
        };

        foreach (var (hasIssue, isError, message) in checks)
        {
            if (!hasIssue)
            {
                continue;
            }

            if (isError)
            {
                errors.Add(message);
            }
            else
            {
                warns.Add(message);
            }
        }
      return (errors, warns);
    }
  }
}
