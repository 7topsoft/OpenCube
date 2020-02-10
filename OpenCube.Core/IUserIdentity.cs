using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCube.Models;

namespace OpenCube.Core
{
    /// <summary>
    /// 유저 정보
    /// </summary>
    public interface IUserIdentity
    {
        bool HasPermission(UserPermission perm);

        UserPermissionGroupType GroupType { get; }

        string UserId { get; }

        string CompanyCode { get; }

        string DeptCode { get; }
    }
}
