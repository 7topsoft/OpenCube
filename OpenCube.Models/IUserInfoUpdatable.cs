using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCube.Models
{
    /// <summary>
    /// 업데이트 가능한 유저 정보 필드 모음
    /// </summary>
    public interface IUserInfoUpdatable
    {
        UserPermissionGroupType PermissionGroup { get; set; }

        bool IsReportEnabled { get; set; }
    }
}
