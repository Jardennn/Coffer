using System.IO;
using System.Collections.Generic;

namespace Coffer.Services
{
    public class Scanner
    {
        public static List<FileInfo> Scan(string srcRoot)
        {
            var results = new List<FileInfo>();
            var root = new DirectoryInfo(srcRoot);

            foreach (var file in root.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                results.Add(file);
            }
            return results;
        }
    }
}
