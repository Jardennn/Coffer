using System;
using System.IO;
using System.Security.Cryptography;

namespace Coffer.Services
{
    public class Verifier
    {
        public static bool Verify(string src_hash, string dst_path, int chunk_size_mb)
        {
            int chunk_size = chunk_size_mb * 1024 * 1024;
            byte[] chunk = new byte[chunk_size];
            using IncrementalHash sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

            using (FileStream dst = File.OpenRead(dst_path))
            {
                int bytesRead;

                while ((bytesRead = dst.Read(chunk, 0, chunk.Length)) > 0)
                {
                    sha256.AppendData(chunk, 0, bytesRead);
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
