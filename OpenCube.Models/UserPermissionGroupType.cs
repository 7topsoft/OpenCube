using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OpenCube.Models
{
    /// <summary>
    /// 유저 권한 그룹 타입
    /// </summary>
    public enum UserPermissionGroupType
    {
        /// <summary>
        /// 없음
        /// </summary>
        [EnumMember(Value = "none")]
        None = 0,

        /// <summary>
        /// 모든 데이터 조회 가능
        /// </summary>
        [EnumMember(Value = "reviewer")]
        Reviewer = 10,

        /// <summary>
        /// 임원진 (리뷰어와 권한 동일)
        /// </summary>
        [EnumMember(Value = "executive")]
        Executive = 20,

        /// <summary>
        /// 데이터 업로더 (본인 데이터는 조회 가능)
        /// </summary>
        [EnumMember(Value = "uploader")]
        Uploader = 30,

        /// <summary>
        /// 운영자
        /// </summary>
        [EnumMember(Value = "administrator")]
        Administrator = 40,

        /// <summary>
        /// 시스템
        /// </summary>
        [EnumMember(Value = "system")]
        System = 50
    }
}
