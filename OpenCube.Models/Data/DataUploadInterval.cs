using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OpenCube.Models.Data
{
    /// <summary>
    /// 실적 데이터 업로드 주기
    /// </summary>
    public enum DataUploadInterval
    {
        [EnumMember(Value = "daily")]
        Daily,

        [EnumMember(Value = "weekly")]
        Weekly,

        [EnumMember(Value = "monthly")]
        Monthly
    }
}
