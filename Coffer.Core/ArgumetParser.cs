using Coffer.Services.Models;

namespace Coffer.Core
{
  public class ArgumetParser
  {
    public static void Apply(string[] args, BackupProfile profile)
    {
      for (int i = 0; i < args.Length; i++)
      {
        switch (args[i])
        {
          case "--destination":
            profile.DestinationPath = args[++i];
            break;

          case "--source":
            profile.SourcePath.Add(args[++i]);
            break;

          case "": // Here should be settings for filters (from the first filter to the last)


          case "--help":
            PrintHelp();
            Environment.Exit(0);
            break;

          default:
            Console.WriteLine($"Unknown argument: {args[i]}");
            Environment.Exit(1);
            break;
        }
      }
    }

    private static void PrintHelp()
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
