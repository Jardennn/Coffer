using System;
using System.IO;
using Coffer.Services;

namespace Coffer.Core
{
    class Program
    {
        static void Main(String[] args)
        {
            // Code for testing copier and verifier, for now.
            string[] paths = {
                "/path/to/your/source/file1.ext",
                "/path/to/your/source/file2.ext",
            };

            string[] dst = {
                "/path/to/your/destination/file1.ext",
                "/path/to/your/destination/file2.ext",
            };

            // In the arrays above switch the placeholder paths with your actual paths.
            // Notice that I used absolute paths as I haven't made yet the function to automatically make the paths connect with their entries (Giving the program the root of your source/destination and making up the absolute path according to entries and children of the given path.).

            Console.WriteLine("Starting data transfer.");
            for (int i = 0; i < paths.Length; i++)
            {
                string path = paths[i];
                string dstpath = dst[i];

                Console.WriteLine($"Beginning data transfer for: {path}");
                string srchash = Copier.CopyFile(path, dstpath, 4);

                Console.WriteLine("Transfer complete, beginning hash verficiation.");
                bool verification = Verifier.Verify(srchash, dstpath, 4);

                if (verification)
                {
                    Console.WriteLine("Verify complete.");
                }
                else
                    Console.WriteLine("Verification failed.");
            }
            Console.WriteLine("Backup complete.");
        }
    }
}
