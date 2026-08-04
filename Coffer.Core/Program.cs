using System;
using System.IO;
using Coffer.Services;

namespace Coffer.Core
{
    class Program
    {
        static void Main(String[] args)
        {

            // Now instead of putting your absolute paths in an array you need to put your root source paths in an array and root destination path in a single string.
             string[] srcpaths = ["/path/to/root/source/"];
             string dst = "/path/to/root/destination/";

            Console.WriteLine("Starting data transfer.");
            for (int i = 0; i < srcpaths.Length; i++) // Going through every path in srcpaths
            {
                string srcpath = srcpaths[i];
                List<FileInfo> filepaths = Scanner.Scan(srcpath);

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
                        int percent = (int)((double)copied / total * 100); // Percentage calculation for how much of the file was copied.
                        int filled = percent / 2; // Making the progress bar 50 characters wide
                        string bar = new string('█', filled) + new string('░', 50 - filled); // Two strings are combined to make the progress bar, the filled blocks are in the same length as 'filled' that changes dynamically and the empty blocks are the full length of the bar (50) - the filled blocks (filled variable).
                        Console.Write($"\r [{bar}] {percent}% {copied / 1024 / 1024}MB / {total / 1024 / 1024}MB"); // Output of the progress bar, \r moves the console back to the beginning of the line for updating the progress bar.
                    });
                    Console.WriteLine();

                    Console.WriteLine("Transfer complete, beginning hash verficiation.");
                    bool verification = Verifier.Verify(srchash, dstpath, 4);

                    if (verification)
                    {
                        Console.WriteLine("Verify complete.");
                    }
                    else
                        Console.WriteLine("Verification failed.");

                    Console.Write("\n--------------------------------\n");
                }


            }
            Console.WriteLine("Backup complete.");
        }
    }
}
