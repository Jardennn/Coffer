using System;
using System.IO;
using System.Security.Cryptography;
using Coffer.Services.Models;

namespace Coffer.Services
{
    public class Copier
    {
        // onProgrss is a variable type that points to a void method that gets two 'long' variables as arguments and can be nullable (as it is already nullable from the start).
        public static string CopyFile(string src, string dst, int chunk_size_mb, Action<long, long>? onProgress = null)
        {
            int chunk_size = chunk_size_mb * 1024 * 1024; // Converting the chunk size from MB's to Bytes.
            byte[] chunk = new byte[chunk_size];
            long bytes_copied = 0;

            FileInfo fileInfo = new FileInfo(src);

            long total_size = fileInfo.Length;  // Getting the source file size in Bytes.

             using IncrementalHash sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); // Creating the hash instance in SHA256.

            using (FileStream source = File.OpenRead(src)) // Starting a stream to read from the source file.
            using (FileStream destination = File.OpenWrite(dst)) // Starting a stream to write to the destination file.
            {
                int bytesRead;

                while ((bytesRead = source.Read(chunk, 0, chunk.Length)) > 0) // Reading the bytes from the source up to how much the chunk is supposed to be, assigning it to an integer that tells how much bytes were read and checking if it's greater than zero.
                {
                    destination.Write(chunk, 0, bytesRead); // Writing to the destination from the chunk that was read in the loop statement, from index 0 to the index that is bytesRead.
                    bytes_copied += bytesRead; // Adding how much bytes were copied in this write, for the output.

                    sha256.AppendData(chunk, 0, bytesRead); // Appending data to the hash from the read chunk. Starting from index 0 up to the index that is assigned in bytesRead.
                    onProgress?.Invoke(bytes_copied, total_size); // Runs the onProgress method, '?' is for continuing on with the code if the method is null (instead of crashing).
                }
            }
            byte[] hash = sha256.GetHashAndReset();
            string hexString = Convert.ToHexString(hash);

            return hexString;
        }

        public static CopyDecision ShouldCopy(FileInfo src, string dest, DuplicateMode mode)
        {
          if (!File.Exists(dest))
          {
            return CopyDecision.Copy;
          }

          switch (mode)
          {
            case DuplicateMode.Skip:
              return CopyDecision.Skip;

            case DuplicateMode.Overwrite:
              return CopyDecision.Copy;

            case DuplicateMode.KeepNewer:
              var destfile = new FileInfo(dest);

              return src.LastWriteTime > destfile.LastWriteTime
                ? CopyDecision.Copy
                : CopyDecision.Skip;

            case DuplicateMode.Rename:
              return CopyDecision.Rename;

            default: 
              return CopyDecision.Copy;
          }
        }

        public enum CopyDecision
        {
          Copy,
          Skip,
          Rename
        }
    }
}
