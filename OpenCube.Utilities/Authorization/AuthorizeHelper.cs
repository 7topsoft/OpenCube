using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenCube.Utilities
{
    public static class AuthorizeHelper
    {
        /// <summary>
        /// 도메인 네임이 포함된 유저 이름을 도메인 네임과 유저 ID로 파싱하여 반환한다.
        /// </summary>
        public static string[] ParseUserName(string userName)
        {
            var list = new List<string>();

            if (userName != null)
            {
                var temp = userName.Split('\\').Where(o => o.IsNotNullOrWhiteSpace()).ToArray();
                if (temp.Length == 2)
                {
                    list.Add(temp[0]);
                    list.Add(temp[1]);
                }
                else if (temp.Length == 1)
                {
                    list.Add(string.Empty);
                    list.Add(temp[0]);
                }
            }

            if (!list.Any())
            {
                list.AddRange(new[] { string.Empty, userName ?? string.Empty });
            }

            return list.ToArray();
        }
    }
}