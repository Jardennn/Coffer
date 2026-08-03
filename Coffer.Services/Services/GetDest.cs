using System.IO;

namespace Coffer.Services
{
    public class GetDest
    {
        public static string GetDestPath(string srcRoot, string src, string dst)
        {
            string relative = Path.GetRelativePath(srcRoot, src);

            string dstpath = dst + Path.GetFileName(srcRoot) + relative;

            return dstpath;
        }
    }
}
