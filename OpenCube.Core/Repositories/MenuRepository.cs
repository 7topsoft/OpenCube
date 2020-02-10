using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using OpenCube.Models;
using OpenCube.Models.Forms;
using OpenCube.Models.Menu;

namespace OpenCube.Core.Repositories
{
    /// <summary>
    /// 메뉴 DAC
    /// </summary>
    public class MenuRepository : BaseRepository
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #region Constructors
        public MenuRepository()
        { }
        #endregion

        #region Methods - Menu
        /// <summary>
        /// 메뉴 아이템을 반환한다.
        /// </summary>
        public MenuItem SelectMenu(Guid menuId)
        {
            string procCommandName = "up_Menu_Select";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "MenuID", DbType.Guid, menuId);

                using (DataSet ds = Connection.ExecuteDataSet(command))
                {
                    ValidateTableCount(ds, 2);

                    MenuItem menu = null;

                    // 메뉴 정보
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        menu = MenuItem.ParseFrom(ds.Tables[0].Rows[0]);

                        // 메뉴와 매핑된 대시보드 테이블 정보
                        foreach (DataRow dr in ds.Tables[1].Rows)
                        {
                            var formTable = FormTable.ParseFrom(dr);
                            var formTableMenuId = dr.Get<Guid>("MenuId");
                            if (formTableMenuId == menu.MenuId)
                            {
                                menu.FormTables.Add(formTable);
                            }
                            else
                            {
                                logger.Warn($"대상 메뉴 ID와 대시보드 테이블이 매핑된 메뉴의 ID가 서로 일치하지 않습니다."
                                    + $"\r\n* 대상 메뉴: {menu}"
                                    + $"\r\n* 대상 대시보드 테이블: {formTable}"
                                    + $"\r\n* 대상 대시보드 테이블이 매핑된 메뉴 ID: '{formTableMenuId}'");
                            }
                        }
                    }

                    return menu;
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 메뉴 리스트를 반환한다.
        /// </summary>
        public IEnumerable<MenuItem> SelectMenuList(string queryUserId = null)
        {
            string procCommandName = "up_Menu_SelectList";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "QueryUserID", DbType.String, queryUserId);

                using (DataSet ds = Connection.ExecuteDataSet(command))
                {
                    ValidateTableCount(ds, 2);

                    var list = new List<MenuItem>();

                    // 메뉴 정보
                    foreach (DataRow dr1 in ds.Tables[0].Rows)
                    {
                        var menu = MenuItem.ParseFrom(dr1);
                        list.Add(menu);

                        // 메뉴와 매핑된 대시보드 테이블 정보
                        foreach (DataRow dr2 in ds.Tables[1].Rows)
                        {
                            var formTableMenuId = dr2.Get<Guid>("MenuId");
                            if (formTableMenuId == menu.MenuId)
                            {
                                menu.FormTables.Add(FormTable.ParseFrom(dr2));
                            }
                        }
                    }

                    return list;
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 페이징 처리된 메뉴 리스트를 반환한다.
        /// </summary>
        public PagedModel<MenuItem> SelectMenuPagedList(PagingOption option)
        {
            string procCommandName = "up_Menu_SelectPagedList";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "PageNumber", DbType.Int32, option.PageNumber);
                Connection.AddInParameter(command, "PageCount", DbType.Int32, option.PageCount);
                Connection.AddInParameter(command, "SortBy", DbType.String, option.SortBy);
                Connection.AddInParameter(command, "OrderBy", DbType.String, option.OrderBy.ToEnumMemberString());

                using (DataSet ds = Connection.ExecuteDataSet(command))
                {
                    ValidateTableCount(ds, 2);

                    var result = new PagedModel<MenuItem>(option) { PagingOption = option };
                    int totalCount = 0;

                    // 메뉴 정보
                    foreach (DataRow dr1 in ds.Tables[0].Rows)
                    {
                        var menu = MenuItem.ParseFrom(dr1, out totalCount);
                        result.Items.Add(menu);

                        // 메뉴와 매핑된 대시보드 테이블 정보
                        foreach (DataRow dr2 in ds.Tables[1].Rows)
                        {
                            var formTableMenuId = dr2.Get<Guid>("MenuId");
                            if (formTableMenuId == menu.MenuId)
                            {
                                menu.FormTables.Add(FormTable.ParseFrom(dr2));
                            }
                        }
                    }

                    result.TotalCount = totalCount;

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 메뉴 아이템을 추가한다.
        /// </summary>
        public bool InsertMenu(MenuItem menu)
        {
            menu.ThrowIfNull(nameof(menu));

            string procCommandName = "up_Menu_Insert";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "MenuID", DbType.Guid, menu.MenuId);
                Connection.AddInParameter(command, "Name", DbType.String, menu.Name);
                Connection.AddInParameter(command, "GroupName", DbType.String, menu.GroupName);
                Connection.AddInParameter(command, "IconName", DbType.String, menu.IconName);
                Connection.AddInParameter(command, "SortOrder", DbType.Int32, menu.SortOrder);
                Connection.AddInParameter(command, "IsVisible", DbType.Boolean, menu.IsVisible);
                Connection.AddInParameter(command, "CreatorId", DbType.String, menu.CreatorId);
                Connection.AddInParameter(command, "CreatedDate", DbType.DateTimeOffset, menu.CreatedDate);

                return (int)Connection.ExecuteNonQuery(command) > 0;
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 메뉴 아이템을 업데이트한다.
        /// </summary>
        public bool UpdateMenu(MenuItem menu)
        {
            menu.ThrowIfNull(nameof(menu));

            string procCommandName = "up_Menu_Update";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "MenuID", DbType.Guid, menu.MenuId);
                Connection.AddInParameter(command, "Name", DbType.String, menu.Name);
                Connection.AddInParameter(command, "GroupName", DbType.String, menu.GroupName);
                Connection.AddInParameter(command, "IconName", DbType.String, menu.IconName);
                Connection.AddInParameter(command, "SortOrder", DbType.Int32, menu.SortOrder);
                Connection.AddInParameter(command, "IsVisible", DbType.Boolean, menu.IsVisible);
                Connection.AddInParameter(command, "UpdatedDate", DbType.DateTimeOffset, menu.UpdatedDate);

                return (int)Connection.ExecuteNonQuery(command) > 0;
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 메뉴 아이템을 삭제한다.
        /// </summary>
        public bool DeleteMenu(Guid menuId, string deleterId, DateTimeOffset deletedDate)
        {
            string procCommandName = "up_Menu_Delete";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "MenuID", DbType.Guid, menuId);
                Connection.AddInParameter(command, "DeleterID", DbType.String, deleterId);
                Connection.AddInParameter(command, "DeletedDate", DbType.DateTimeOffset, deletedDate);

                return (int)Connection.ExecuteNonQuery(command) > 0;
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }
        #endregion

        #region Methods - MenuFormMap
        /// <summary>
        /// 메뉴에 대상 대시보드 테이블을 매핑시킨다. (기존 매핑된 테이블을 지우고 새로 매핑함)
        /// </summary>
        public bool UpdateMenuFormMap(Guid menuId, Guid[] formIds, DateTimeOffset createdDate)
        {
            menuId.ThrowIfEmpty(nameof(menuId));

            string procCommandName = "up_MenuFormMap_Update";

            try
            {
                using (var spParamFormMap = new DataTable())
                {
                    var command = Connection.GetStoredProcCommand(procCommandName);
                    Connection.AddInParameter(command, "MenuID", DbType.Guid, menuId);
                    Connection.AddInParameter(command, "CreatedDate", DbType.DateTimeOffset, createdDate);

                    // @FormMap
                    {
                        spParamFormMap.Columns.Add("FormID", typeof(Guid));
                        spParamFormMap.Columns.Add("SortOrder", typeof(int));

                        for (int i = 0; i < formIds.Length; i++)
                        {
                            spParamFormMap.Rows.Add(formIds[i], i + 1);
                        }

                        var param = command.Parameters.AddWithValue("FormMap", spParamFormMap);
                        param.SqlDbType = SqlDbType.Structured;
                    }

                    return (int)Connection.ExecuteNonQuery(command) > 0;
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }
        #endregion
    }
}
