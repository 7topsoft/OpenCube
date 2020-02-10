using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OpenCube.Models
{
    /// <summary>
    /// 유저 권한
    /// </summary>
    [Flags]
    public enum UserPermission
    {
        /// <summary>
        /// 권한 없음
        /// </summary>
        [EnumMember(Value = "NONE")]
        None = 0,

        /// <summary>
        /// 데이터 업로드 가능 (대시보드 페이지에서 본인 데이터는 조회/다운로드 가능)
        /// </summary>
        [EnumMember(Value = "DATA_UPLOAD")]
        DataUpload = 1 << 0,

        /// <summary>
        /// 데이터 조회 (모든 데이터)
        /// </summary>
        [EnumMember(Value = "DATA_REVIEW")]
        DataReview = 1 << 1,

        /// <summary>
        /// 데이터 파일 삭제
        /// </summary>
        [EnumMember(Value = "DATA_DELETE")]
        DataDelete = 1 << 2,

        /// <summary>
        /// 데이터 컨펌/컨펌 취소
        /// </summary>
        [EnumMember(Value = "DATA_CONFIRM")]
        DataConfirm = 1 << 3,

        /// <summary>
        /// 다른 유저로 로그인 하기
        /// </summary>
        [EnumMember(Value = "USER_IMPERSONATION")]
        UserImpersonation = 1 << 4,

        /// <summary>
        /// 전체
        /// </summary>
        [EnumMember(Value = "ALL")]
        All = int.MaxValue
    }
}
