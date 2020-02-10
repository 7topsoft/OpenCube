using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OpenCube.Models
{
    /// <summary>
    /// 페이징 정렬 방법
    /// </summary>
    public enum PagingOrderBy
    {
        /// <summary>
        /// 오름 차순
        /// </summary>
        [EnumMember(Value = "asc")]
        Asc,

        /// <summary>
        /// 내림 차순
        /// </summary>
        [EnumMember(Value = "desc")]
        Desc
    }
}
