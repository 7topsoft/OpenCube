using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OpenCube.Models.Data
{
    /// <summary>
    /// 실적 데이터 업로드 가능한 주차 정보 (int 값은 <see cref="DayOfWeek"/>과 동일하게 맞춤)
    /// </summary>
    public enum DataUploadDayOfWeek
    {
        [EnumMember(Value = "all")]
        All = -1,

        [EnumMember(Value = "sunday")]
        Sunday = 0,

        [EnumMember(Value = "monday")]
        Monday = 1,

        [EnumMember(Value = "tuesday")]
        Tuesday = 2,

        [EnumMember(Value = "wednesday")]
        Wednesday = 3,

        [EnumMember(Value = "thursday")]
        Thursday = 4,

        [EnumMember(Value = "friday")]
        Friday = 5,

        [EnumMember(Value = "saturday")]
        Saturday = 6
    }
}
