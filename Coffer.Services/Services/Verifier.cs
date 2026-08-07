using System;
using System.IO;
using System.Security.Cryptography;

namespace Coffer.Services
{
    public class Verifier
    {
        public static bool Verify(string src_hash, string dst_path, int chunk_size_mb, Action<long, long>? onProgress = null)
        {
            int chunk_size = chunk_size_mb * 1024 * 1024;
            byte[] chunk = new byte[chunk_size];
            using IncrementalHash sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

            // For the progress bar
            long bytesappended = 0;
            FileInfo file = new FileInfo(dst_path);
            long totalsize = file.Length;

            using (FileStream dst = File.OpenRead(dst_path))
            {
                int bytesRead;

                while ((bytesRead = dst.Read(chunk, 0, chunk.Length)) > 0)
                {
                    sha256.AppendData(chunk, 0, bytesRead);

                    // Appending data for the progress bar (how many bytes were appended so far and the target bytes count.)
                    bytesappended += bytesRead;
                    onProgress?.Invoke(bytesappended, totalsize);
                }
            }
            byte[] hash = sha256.GetHashAndReset();
            string hexString = Convert.ToHexString(hash);

            if (hexString == src_hash)
                return true;
            else
                return false;
        }
    }
}
