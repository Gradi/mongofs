using System.Collections.Generic;
using System.Linq;
using DokanNet;

namespace MongoFs.Extensions
{
    public static class EnumerableOfFileInformationExtensions
    {
        public static IEnumerable<FileInformation> FilterWithGlobPattern(this IEnumerable<FileInformation> infos, string? pattern)
        {
            if (pattern != null)
            {
                return infos.Where(i => GlobExpressions.Glob.IsMatch(i.FileName, pattern));
            }
            else
            {
                return infos;
            }
        }
    }
}
