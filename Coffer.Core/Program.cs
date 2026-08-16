using System;
using System.IO;
using Coffer.Services;
using Coffer.Services.Models;

namespace Coffer.Core
{
    class Program
    {
        static void Main(String[] args)
        {

            // Now instead of putting your absolute paths in an array you need to put your root source paths in an array and root destination path in a single string.
          //  string[] srcpaths = ["/path/to/root/source/"];
          //  string dst = "/path/to/root/destination/";

            BackupProfile profile = ProfileService.load();
            FilterConfig filters = profile.Filters;
            List<string> srcpaths = profile.SourcePaths;
            string dst = profile.DestinationPath;

            Console.WriteLine("Starting backup.");
            Console.WriteLine("--------------------------------");
            Console.WriteLine("Starting preflight checks.");
            var item = PreFlight.Run(srcpaths, dst, filters);

            if (item.valBlock != "")
            {
                Console.WriteLine($"[ERROR] {item.valBlock}");
                Console.WriteLine("There was an error in finding one of the sources/destination or in reading/writing. Fix the above error and try again.");
                Environment.Exit(1);
            }

            if (item.errors.Count != 0)
            {
                foreach (var error in item.errors)
                {
                    Console.WriteLine($"[ERROR] {error}");
                }
                Console.WriteLine("Backup cannot proceed, Fix the above errors and try again.");
                Environment.Exit(1);
            }

            if (item.warns.Count != 0)
            {
                foreach (var warn in item.warns)
                {
                    Console.WriteLine($"[WARN] {warn}");
                }

                Console.WriteLine("Continue anyway? (y/n)");
                char confirm = Char.Parse(Console.ReadLine() ?? " ");

                while (true)
                {
                    if (char.ToLower(confirm) == 'n')
                    {
                        Console.WriteLine("Backup cancelled.");
                        Environment.Exit(0);
                    }
                    else if (char.ToLower(confirm) == 'y')
                    {
                        break;
                    }
                    Console.WriteLine("Input was invalid, try again");
                    Console.WriteLine("Continue anyway? (y/n)");
                    confirm = Char.Parse(Console.ReadLine() ?? " ");
                }
            }
            Console.WriteLine("\nPreflight checks passed, starting backup...\n");
            Console.WriteLine("--------------------------------\n");

            foreach (string srcpath in srcpaths) // Going through every path in srcpaths
            {
               // string srcpath = srcpaths[i];
                List<FileInfo> filepaths = Scanner.Scan(srcpath, filters);

                if (filepaths.Count == 0)
                {
                    Console.WriteLine($"No files found in {srcpath}, skipping.");
                    continue;
                }

                foreach (var filepath in filepaths)
                {
                        string filepathstr = filepath.FullName;
                        string dstpath = GetDest.GetDestPath(srcpath, filepathstr, dst);

                        Directory.CreateDirectory(Path.GetDirectoryName(dstpath)!); // Creates a directory in the same name of a directory that contains a nested source file.

                        Console.WriteLine($"Beginning data transfer for: {filepathstr}");
                        string srchash = Copier.CopyFile(filepathstr, dstpath, 4, onProgress: (copied, total) => // Using lambda to pass onProgress as a function that will take 'copied' and 'total' as arguments.
                        {
                            int percent = (int)Math.Clamp((double)copied / total * 100, 0, 100); // Percentage calculation for how much of the file was copied. Math.Clamp is for limiting the bar to be within 0 to 100, to prevent crashes.
                            int filled = percent / 2; // Making the progress bar 50 characters wide
                            string bar = new string('█', filled) + new string('░', 50 - filled); // Two strings are combined to make the progress bar, the filled blocks are in the same length as 'filled' that changes dynamically and the empty blocks are the full length of the bar (50) - the filled blocks (filled variable).
                            Console.Write($"\r [{bar}] {percent}% {copied / 1024 / 1024}MB / {total / 1024 / 1024}MB"); // Output of the progress bar, \r moves the console back to the beginning of the line for updating the progress bar.
                        });
                        Console.WriteLine();

                        Console.WriteLine("\nTransfer complete, beginning hash verficiation."); // I added a progress bar to show to progress of data being appended to the hash for verification. For working with large files.
                        bool verification = Verifier.Verify(srchash, dstpath, 4, onProgress: (appended, total) =>
                        {
                            int percent = (int)Math.Clamp((double)appended / total * 100, 0, 100);
                            int filled = percent / 2;
                            string bar = new string('█', filled) + new string('░', 50 - filled);
                            Console.Write($"\r [{bar}] {percent}%");
                        });
                        Console.WriteLine();
                        Console.WriteLine();

                        if (verification)
                        {
                            Console.WriteLine("✓ OK");
                        }
                        else
                            Console.WriteLine("✗ FAILED — checksum mismatch");

                        Console.Write("\n--------------------------------\n");
                }
            }
            Console.WriteLine("--------------------------------");
            Console.WriteLine("Backup complete.");
        }
    }
}
