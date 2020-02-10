using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OpenCube.Models.Data
{
    /// <summary>
    /// 데이터 밸류 타입
    /// </summary>
    public enum DataFileCellValueType
    {
        /// <summary>
        /// 알 수 없음
        /// </summary>
        [EnumMember(Value = "unknown")]
        Unknown,

        /// <summary>
        /// 숫자 (음수, 양수)
        /// </summary>
        [EnumMember(Value = "number")]
        Number,

        /// <summary>
        /// boolean
        /// </summary>
        [EnumMember(Value = "boolean")]
        Boolean,

        /// <summary>
        /// 텍스트
        /// </summary>
        [EnumMember(Value = "string")]
        String
    }
}
