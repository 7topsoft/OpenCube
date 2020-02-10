using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCube.Models;

namespace OpenCube.Core.Repositories
{
    /// <summary>
    /// 유저 테이블 DAC
    /// </summary>
    public class UserRepository : BaseRepository
    {
        public UserRepository()
        { }

        /// <summary>
        /// 유저 정보를 반환한다.
        /// </summary>
        public UserInfo SelectUser(string userId, bool updateAccessDate)
        {
            string procCommandName = "up_User_Select";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "UserID", DbType.String, userId);

                if (updateAccessDate)
                {
                    Connection.AddInParameter(command, "LastAccessDate", DbType.DateTimeOffset, DateTimeOffset.Now);
                }

                using (DataSet ds = Connection.ExecuteDataSet(command))
                {
                    ValidateTableCount(ds, 1);

                    // 유저 정보
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        return UserInfo.ParseFrom(ds.Tables[0].Rows[0]);
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 유저 리스트를 반환한다.
        /// </summary>
        public IEnumerable<UserInfo> SelectUserList(string groupType, string sortBy, PagingOrderBy orderBy)
        {
            string procCommandName = "up_User_SelectList";

            try
            {
                using (var spParamUserIDList = new DataTable()) // dbo.type_UserIDList
                {
                    var command = Connection.GetStoredProcCommand(procCommandName);
                    Connection.AddInParameter(command, "GroupType", DbType.String, groupType);
                    Connection.AddInParameter(command, "SortBy", DbType.String, sortBy);
                    Connection.AddInParameter(command, "OrderBy", DbType.String, orderBy.ToEnumMemberString());

                    // UserIDs
                    {
                        spParamUserIDList.Columns.Add("UserID", typeof(string));

                        foreach (var userId in UserInfo.AdminAccounts)
                        {
                            spParamUserIDList.Rows.Add(new object[]
                            {
                                userId
                            });
                        }

                        var param = command.Parameters.AddWithValue("SystemUserIDs", spParamUserIDList);
                        param.SqlDbType = SqlDbType.Structured;
                    }

                    using (DataSet ds = Connection.ExecuteDataSet(command))
                    {
                        ValidateTableCount(ds, 1);

                        var result = new List<UserInfo>();

                        // 유저 정보
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            result.Add(UserInfo.ParseFrom(dr));
                        }

                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 페이징 처리된 유저 리스트를 반환한다.
        /// </summary>
        public PagedModel<UserInfo> SelectUserPagedList(string groupType, SearchOption option)
        {
            string procCommandName = "up_User_SelectPagedList";

            try
            {
                using (var spParamUserIDList = new DataTable()) // dbo.type_UserIDList
                {
                    var command = Connection.GetStoredProcCommand(procCommandName);
                    Connection.AddInParameter(command, "GroupType", DbType.String, groupType);
                    Connection.AddInParameter(command, "PageNumber", DbType.Int32, option.PageNumber);
                    Connection.AddInParameter(command, "PageCount", DbType.Int32, option.PageCount);
                    Connection.AddInParameter(command, "SortBy", DbType.String, option.SortBy);
                    Connection.AddInParameter(command, "OrderBy", DbType.String, option.OrderBy.ToEnumMemberString());
                    Connection.AddInParameter(command, "SearchType", DbType.String, option.SearchType);
                    Connection.AddInParameter(command, "SearchKeyword", DbType.String, option.SearchKeyword);

                    // UserIDs
                    {
                        spParamUserIDList.Columns.Add("UserID", typeof(string));

                        foreach (var userId in UserInfo.AdminAccounts)
                        {
                            spParamUserIDList.Rows.Add(new object[]
                            {
                                userId
                            });
                        }

                        var param = command.Parameters.AddWithValue("SystemUserIDs", spParamUserIDList);
                        param.SqlDbType = SqlDbType.Structured;
                    }

                    using (DataSet ds = Connection.ExecuteDataSet(command))
                    {
                        ValidateTableCount(ds, 1);

                        var result = new PagedModel<UserInfo>(option) { PagingOption = option };
                        int totalCount = 0;

                        // 유저 정보
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            var user = UserInfo.ParseFrom(dr, out totalCount);
                            result.Items.Add(user);
                        }

                        result.TotalCount = totalCount;

                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 유저 정보를 추가한다.
        /// </summary>
        public bool InsertUser(string userId, UserPermissionGroupType permissionGroup)
        {
            userId.ThrowIfNullOrWhiteSpace(nameof(userId));

            string procCommandName = "up_User_Insert";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "UserID", DbType.String, userId);
                Connection.AddInParameter(command, "CompanyCode", DbType.String, string.Empty);
                Connection.AddInParameter(command, "PermissionGroup", DbType.String, permissionGroup.ToEnumMemberString());
                Connection.AddInParameter(command, "CreatedDate", DbType.DateTimeOffset, DateTimeOffset.Now);

                return (int)Connection.ExecuteNonQuery(command) > 0;
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 유저 정보를 리스트 단위로 추가한다.
        /// </summary>
        public bool InsertUserList(UserInfo[] users)
        {
            string procCommandName = "up_User_InsertList";

            try
            {
                using (var spParamUserInfoList = new DataTable()) // dbo.type_UserInfoList
                {
                    var command = Connection.GetStoredProcCommand(procCommandName);

                    spParamUserInfoList.Columns.Add("UserID", typeof(string));
                    spParamUserInfoList.Columns.Add("CompanyCode", typeof(string));
                    spParamUserInfoList.Columns.Add("PermissionGroup", typeof(string));
                    spParamUserInfoList.Columns.Add("IsReportEnabled", typeof(bool));
                    spParamUserInfoList.Columns.Add("CreatedDate", typeof(DateTimeOffset));

                    foreach (var user in users)
                    {
                        var tempObj = new object[]
                        {
                            user.UserId,
                            string.Empty,
                            user.PermissionGroup.GroupType.ToEnumMemberString(),
                            user.IsReportEnabled,
                            user.CreatedDate
                        };

                        spParamUserInfoList.Rows.Add(tempObj);
                    }

                    var param = command.Parameters.AddWithValue("UserInfoList", spParamUserInfoList);
                    param.SqlDbType = SqlDbType.Structured;

                    return (int)Connection.ExecuteNonQuery(command) > 0;
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 유저 정보를 업데이트한다.
        /// </summary>
        public bool UpdateUser(UserInfo userInfo)
        {
            userInfo.ThrowIfNull(nameof(userInfo));

            string procCommandName = "up_User_Update";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "UserID", DbType.String, userInfo.UserId);
                Connection.AddInParameter(command, "CompanyCode", DbType.String, userInfo.CompanyCode);
                Connection.AddInParameter(command, "PermissionGroup", DbType.String, userInfo.PermissionGroup.GroupType.ToEnumMemberString());
                Connection.AddInParameter(command, "IsReportEnabled", DbType.Boolean, userInfo.IsReportEnabled);
                Connection.AddInParameter(command, "UpdatedDate", DbType.DateTimeOffset, DateTimeOffset.Now);

                return (int)Connection.ExecuteNonQuery(command) > 0;
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 유저 정보를 삭제한다.
        /// </summary>
        public bool DeleteUser(string userId, DateTimeOffset date)
        {
            userId.ThrowIfNullOrWhiteSpace(nameof(userId));

            string procCommandName = "up_User_Delete";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "UserID", DbType.String, userId);
                Connection.AddInParameter(command, "DeletedDate", DbType.DateTimeOffset, date);

                return (int)Connection.ExecuteNonQuery(command) > 0;
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }
    }
}
