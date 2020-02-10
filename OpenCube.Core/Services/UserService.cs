using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using OpenCube.Core.Repositories;
using OpenCube.Models;
using OpenCube.Models.Logging;

namespace OpenCube.Core.Services
{
    /// <summary>
    /// 유저 서비스
    /// </summary>
    public class UserService : BaseService
    {
        #region Fields
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region Constructos
        public UserService(IUserIdentity identity) : base(identity)
        { }
        #endregion

        #region Methods
        /// <summary>
        /// 유저 정보를 반환한다.
        /// </summary>
        public UserInfo GetUser(string userId)
        {
            userId.ThrowIfNullOrWhiteSpace(nameof(userId));

            using (var repo = new UserRepository())
            {
                return repo.SelectUser(userId, false);
            }
        }

        /// <summary>
        /// 유저 리스트를 반환한다.
        /// </summary>
        public UserInfo[] GetUserList(string groupType, string sortBy, PagingOrderBy orderBy)
        {
            using (var repo = new UserRepository())
            {
                return repo.SelectUserList(groupType, sortBy, orderBy)
                    .ToArray();
            }
        }

        /// <summary>
        /// 페이징 처리된 유저 리스트를 반환한다.
        /// </summary>
        public PagedModel<UserInfo> GetUserPagedList(string groupType, SearchOption option)
        {
            using (var repo = new UserRepository())
            {
                return repo.SelectUserPagedList(groupType, option);
            }
        }

        /// <summary>
        /// 유저를 추가한다.
        /// </summary>
        public UserInfo AddUser(string userId, UserPermissionGroupType permissionGroup)
        {
            using (var repo = new UserRepository())
            {
                if (repo.InsertUser(userId, permissionGroup))
                {
                    var user = repo.SelectUser(userId, false);
                    if (user == null)
                    {
                        throw new ObjectNotFoundException($"사용자를 찾을 수 없습니다. 대상: \"{userId}\"");
                    }

                    logger.Info($"새로운 사용자를 등록하였습니다. 대상: \"{user.UserId}\""
                        + $"\r\n\r\n"
                        + $"{user}");

                    return user;
                }
                else
                {
                    throw new DataException($"사용자 등록에 실패했습니다. 대상: \"{userId}\"");
                }
            }
        }

        /// <summary>
        /// 사용자 리스트를 추가한다.
        /// </summary>
        public void AddUsers(UserInfo[] users)
        {
            using (var repo = new UserRepository())
            {
                if (repo.InsertUserList(users))
                {
                    logger.Info($"새로운 사용자를 등록하였습니다. 사용자 수: \"{users.Length}\""
                        + $"\r\n\r\n"
                        + $"등록된 사용자: [\r\n"
                        + string.Join("\r\n", users.Select(o => "\t" + o.ToString()))
                        + $"\r\n]"
                        );
                }
            }
        }

        /// <summary>
        /// 사용자 정보를 업데이트한다.
        /// </summary>
        public UserInfo UpdateUser(string userId, IUserInfoUpdatable fields)
        {
            userId.ThrowIfNullOrWhiteSpace(nameof(userId));
            fields.ThrowIfNull(nameof(fields));

            using (var repo = new UserRepository())
            {
                var user = repo.SelectUser(userId, false);
                if (user == null)
                {
                    throw new ObjectNotFoundException($"사용자를 찾을 수 없습니다. 대상: \"{userId}\"");
                }

                List<UpdatedField> updated = null;
                user.Update(fields, out updated);

                if (repo.UpdateUser(user))
                {
                    logger.Info($"사용자 정보를 업데이트하였습니다. 대상: \"{user.UserId}\""
                        + $"\r\n\r\n"
                        + $"{user}");
                }

                return user;
            }
        }

        /// <summary>
        /// 사용자 정보를 삭제한다.
        /// </summary>
        public bool DeleteUser(string userId)
        {
            userId.ThrowIfNullOrWhiteSpace(nameof(userId));

            using (var repo = new UserRepository())
            {
                if (repo.DeleteUser(userId, DateTimeOffset.Now))
                {
                    logger.Info($"사용자가 삭제되었습니다. 대상: \"{userId}\"");

                    return true;
                }

                return false;
            }
        }
        #endregion
    }
}
