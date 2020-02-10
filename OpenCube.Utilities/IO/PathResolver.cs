using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace OpenCube.Utilities.IO
{
    public static class PathResolver
    {
        /// <summary>
        /// 간접 경로를 절대 경로로 변환하여 반환한다.
        /// </summary>
        public static string GetFullPath(string relativePath)
        {
            relativePath.ThrowIfNull(nameof(relativePath));

            if (!Path.IsPathRooted(relativePath))
            {
                if (HostingEnvironment.IsHosted)
                {
                    if (relativePath.StartsWith("~/") || relativePath.StartsWith("~\\"))
                    {
                        return HostingEnvironment.MapPath(relativePath);
                    }
                }

                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
            }

            return relativePath;
        }
    }
}
