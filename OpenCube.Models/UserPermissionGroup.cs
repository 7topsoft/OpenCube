using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenCube.Models.Attributes;

namespace OpenCube.Models
{
    /// <summary>
    /// 유저 권한 그룹
    /// </summary>
    [JsonObject]
    public class UserPermissionGroup : BaseModel
    {
        #region Constructors
        public UserPermissionGroup(UserPermissionGroupType type)
        {
            this.GroupType = type;
            this.Permissions = GetPermissionsBy(type);
        }
        #endregion

        #region Methods
        public static UserPermission GetPermissionsBy(UserPermissionGroupType groupType)
        {
            var perms = UserPermission.None;

            switch (groupType)
            {
                case UserPermissionGroupType.None:
                    break;
                case UserPermissionGroupType.Reviewer:
                    perms = UserPermission.DataReview;
                    break;
                case UserPermissionGroupType.Executive:
                    perms = UserPermission.DataReview;
                    break;
                case UserPermissionGroupType.Uploader:
                    perms = UserPermission.DataUpload;
                    break;
                case UserPermissionGroupType.Administrator:
                    perms = UserPermission.All & ~UserPermission.UserImpersonation; // 다른 사용자로 로그인하기 기능은 제외
                    break;
                case UserPermissionGroupType.System:
                    perms = UserPermission.All;
                    break;
            }

            return perms;
        }

        public static UserPermissionGroup ParseFrom(string value)
        {
            value.ThrowIfNull(nameof(value));

            var groupType = EnumExtension.ParseEnumMember<UserPermissionGroupType>(value);
            return new UserPermissionGroup(groupType);
        }

        public override void Validate()
        {
            // nothing
        }
        #endregion

        #region Properties
        /// <summary>
        /// 그룹 타입
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [Printable]
        public UserPermissionGroupType GroupType { get; }

        /// <summary>
        /// 권한 (정수로 반환)
        /// </summary>
        [Printable]
        public UserPermission Permissions { get; set; }
        #endregion
    }
}
