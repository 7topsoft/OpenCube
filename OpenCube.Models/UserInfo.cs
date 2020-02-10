using System;
using System.Collections.Generic;
using System.Configuration.Abstractions;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenCube.Models.Attributes;
using OpenCube.Models.Logging;
using OpenCube.Utilities;

namespace OpenCube.Models
{
    /// <summary>
    /// 유저 정보
    /// </summary>
    public class UserInfo : BaseModel
    {
        #region Fields
        public static readonly string DefaultDomainName = ConfigurationManager.Instance.AppSettings.AppSetting<string>("Server.DomainName");

        public static readonly string[] AdminAccounts = ConfigurationManager.Instance.AppSettings.AppSetting<string>("System.Accounts", () => "")
                .Split(';')
                .Where(o => o.IsNotNullOrWhiteSpace())
                .Select(o => o.Trim())
                .ToArray();
        #endregion

        #region Constructors
        public UserInfo(string userId)
        {
            var userName = AuthorizeHelper.ParseUserName(userId);
            if (userName[0].IsNullOrWhiteSpace())
            {
                userName[0] = DefaultDomainName;
            }

            this.DomainName = userName[0];
            this.UserId = userName[1];
            this.PermissionGroup = new UserPermissionGroup(UserPermissionGroupType.None);
        }

        public UserInfo(UserInfo other)
        {
            other.ThrowIfNull(nameof(other));

            this.DomainName = other.DomainName;
            this.UserId = other.UserId;
            this.CompanyCode = other.CompanyCode;
            this.PermissionGroup = new UserPermissionGroup(other.PermissionGroup.GroupType);
            this.IsReportEnabled = other.IsReportEnabled;
            this.IsDeleted = other.IsDeleted;
            this.CreatedDate = other.CreatedDate;
            this.DeletedDate = other.DeletedDate;

            // merged ---------------------------------
            this.UserName = other.UserName;
            this.DeptCode = other.DeptCode;
            this.DeptName = other.DeptName;
            this.MobileTel = other.MobileTel;
        }
        #endregion

        #region Methods
        public static UserInfo ParseFrom(DataRow dr)
        {
            dr.ThrowIfNull(nameof(dr));

            string userId = dr.Get<string>("UserID");
            var permGroup = UserPermissionGroup.ParseFrom(dr.Get<string>("PermissionGroup"));

            // 시스템 그룹에 속한다면 시스템 그룹으로 분류한다.
            // NOTE(jhlee): 리스트 단위로 유저 정보 조회시, SP로 시스템 그룹에 해당하는 유저 ID 리스트를 전달하기 때문에
            // 반환되는 DataTable에는 system 그룹으로 표시되지만, 단 건 셀렉트 시에는 이 로직이 필요함.
            if (permGroup.GroupType != UserPermissionGroupType.System && AdminAccounts.Contains(userId, StringComparer.OrdinalIgnoreCase))
            {
                permGroup = new UserPermissionGroup(UserPermissionGroupType.System);
            }

            return new UserInfo(userId)
            {
                CompanyCode = dr.Get<string>("CompanyCode"),
                PermissionGroup = permGroup,
                IsReportEnabled = dr.Get<bool>("IsReportEnabled"),
                IsDeleted = dr.Get<bool>("IsDeleted"),
                CreatedDate = dr.Get<DateTimeOffset>("CreatedDate"),
                DeletedDate = dr.Get<DateTimeOffset?>("DeletedDate"),
                LastAccessDate = dr.Get<DateTimeOffset?>("LastAccessDate"),
                // merged ---------------------------------
                UserName = dr.Get<string>("UserName"),
                DeptCode = dr.Get<string>("DeptCode"),
                DeptName = dr.Get<string>("DeptName"),
                MobileTel = dr.Get<string>("MobileTel")
            };
        }

        public static UserInfo ParseFrom(DataRow dr, out int totalCount)
        {
            dr.ThrowIfNull(nameof(dr));

            totalCount = dr.Get<int>("TotalCount");

            return UserInfo.ParseFrom(dr);
        }

        public void Update(IUserInfoUpdatable fields)
        {
            List<UpdatedField> updated = null;
            this.Update(fields, out updated);
        }

        public void Update(IUserInfoUpdatable fields, out List<UpdatedField> updated)
        {
            fields.ThrowIfNull(nameof(fields));

            updated = new List<UpdatedField>();

            if (PermissionGroup.GroupType != fields.PermissionGroup)
            {
                updated.Add(new UpdatedField
                {
                    FieldName = nameof(PermissionGroup),
                    OldValue = PermissionGroup,
                    NewValue = fields.PermissionGroup
                });

                PermissionGroup = new UserPermissionGroup(fields.PermissionGroup);
            }

            if (IsReportEnabled != fields.IsReportEnabled)
            {
                updated.Add(new UpdatedField
                {
                    FieldName = nameof(IsReportEnabled),
                    OldValue = IsReportEnabled,
                    NewValue = fields.IsReportEnabled
                });

                IsReportEnabled = fields.IsReportEnabled;
            }

            if (updated.Any())
            {
                Validate();
            }
        }

        public override void Validate()
        {
            UserId.ThrowIfNullOrWhiteSpace(nameof(UserId));
        }

        public bool HasPermission(UserPermission perm)
        {
            return (this.Permissions & perm) == perm;
        }
        #endregion

        #region Properties - Table Mapped
        /// <summary>
        /// 도메인 네임을 포함한 유저 ID를 반환한다.
        /// </summary>
        public string UserAccount
        {
            get
            {
                string account = "";

                if (DomainName.IsNotNullOrEmpty())
                {
                    account = DomainName + "\\";
                }

                return account + UserId;
            }
        }

        /// <summary>
        /// 도메인 네임
        /// </summary>
        [Printable]
        public string DomainName { get; set; }

        /// <summary>
        /// 유저 ID
        /// </summary>
        [Printable]
        public string UserId { get; }

        /// <summary>
        /// 회사 코드
        /// </summary>
        public string CompanyCode { get; set; }

        /// <summary>
        /// 권한 (bit 연산을 위해 정수로 반환)
        /// </summary>        
        public UserPermission Permissions => PermissionGroup != null ? PermissionGroup.Permissions : 0;

        /// <summary>
        /// 권한 그룹 종류
        /// </summary>
        [Printable]
        public UserPermissionGroup PermissionGroup { get; set; }

        /// <summary>
        /// 알림 사용 여부
        /// </summary>
        public bool IsReportEnabled { get; set; }

        /// <summary>
        /// 삭제 여부
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 생성 날짜
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// 삭제 날짜
        /// </summary>
        public DateTimeOffset? DeletedDate { get; set; }

        /// <summary>
        /// 마지막 접속일
        /// </summary>
        public DateTimeOffset? LastAccessDate { get; set; }
        #endregion

        #region Properties - Merged
        /// <summary>
        /// 유저 이름
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 부서 코드
        /// </summary>
        public string DeptCode { get; set; }

        /// <summary>
        /// 부서명
        /// </summary>
        public string DeptName { get; set; }

        /// <summary>
        /// 핸드폰 번호
        /// </summary>
        public string MobileTel { get; set; }

        #endregion
    }
}
