using System;
using System.IO;

namespace Coffer.Services
{
  public class PreFlight
  {

    public static (string type, string message) SourceExists(string[] srcs)
    {
      foreach (var src in srcs)
      {
        if (!Directory.Exists(src))
        {
          return ("error", $"Source: {src} was not found.");
        }
      }

      return null;
    }

    public static (string type, string message) SourceReadable(string[] srcs)
    {
      foreach (var src in srcs)
      {
        try
        {
          Directory.EnumerateFileSystemEntries(src).FirstOrDefault();
        }
        catch (UnauthorizedAccessException)
        {
          return ("error", $"No read permissions for {src}");
        }
        catch (Exception)
        {
          return ("error", $"Unexpected error was encountered checking {src} for read permissions.");
        }
      }

      return null;
    }

    public static (string type, string message) DestExist(string dst)
    {
      if (!Directory.Exists(dst))
      {
        return ("error", $"Destination: {dst} was not found.");
      }

      return null;
    }

    public static (string type, string message) FreeSpace(string[] srcs, string dst, FilterConfig filters, int bufferGB = 2)
    {
      long needed = Detectors.GetSizes(srcs, filters);
      long available = Detectors.GetFreeSpace(dst);
      
      if (needed + (bufferGB + Math.Pow(1024, 3)) > available)
      {
        return("error", $"Not enough free space, {needed / 1024 / 1024 / 1024}GB required while only {available / 1024 / 1024 / 1024}GB is available.");
      }

      return null;
    }

    public static (string type, string message) DestWritable(string dst)
    {
      string testfile = dst + Path.DirectorySeparatorChar + ".coffertest";
      try
      {
        File.WriteAllText(testfile, "test");
        File.Delete(testfile);
      }
      catch (UnauthorizedAccessException)
      {
        return ("error", "Destination is not writeable.");
      }

      return null;
    }

    public static (List<string> errors, list<string> warns) Run(string[] srcs, string dst, FilterConfig filters)
    {
      List<string> errors = new List<string>();
      List<string> warns = new List<string>();

      (string, string)[] checks = {
        SourceExists(srcs),
        SourceReadable(srcs),
        DestExist(dst),
        FreeSpace(srcs, dst, filters),
        DestWritable(dst)
      };

      foreach (var check in checks)
      {
        if (check == null)
        {
          continue;
        }

        var (type, message) = check;

        if (type == "error")
        {
          errors.Add(message);
        }
        else if (type == "warn")
        {
          warns.Add(message);
        }

        return (errors, warns);
      }
    }
  }
}
