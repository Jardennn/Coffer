using System;
using System.IO;

namespace Coffer.Services.Helpers
{
    public class PathHelper
    {
        public static string GetRenamedPath(string dstpath)
        {
            string dir = Path.GetDirectoryName(dstpath); // full path
            string nameNoExt = Path.GetFileNameWithoutExtension(dir); // File name without extension
            string extension = Path.GetExtension(dir); // Extension alone
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

            return Path.Combine(dir, $"{nameNoExt}_{timestamp}{extension}");
        }
    }
}
