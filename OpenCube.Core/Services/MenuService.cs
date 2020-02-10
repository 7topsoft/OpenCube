using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using NLog;
using OpenCube.Core;
using OpenCube.Core.Repositories;
using OpenCube.Models;
using OpenCube.Models.Logging;
using OpenCube.Models.Menu;

namespace OpenCube.Core.Services
{
    /// <summary>
    /// 메뉴 서비스
    /// </summary>
    public class MenuService : BaseService
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #region Constructors
        public MenuService(IUserIdentity identity)
            : base(identity)
        { }
        #endregion

        #region Methods
        /// <summary>
        /// 메뉴 리스트를 반환한다.
        /// </summary>
        public MenuItem GetMenu(Guid menuId)
        {
            using (var repo = new MenuRepository())
            {
                return repo.SelectMenu(menuId);
            }
        }

        /// <summary>
        /// 메뉴 리스트를 반환한다.
        /// </summary>
        public MenuItem[] GetMenuList()
        {
            using (var repo = new MenuRepository())
            {
                string queryUserId = null;
                if (!CurrentUser.HasPermission(UserPermission.DataReview))
                {
                    queryUserId = CurrentUser.UserId;
                }

                return repo.SelectMenuList(queryUserId)
                    .ToArray();
            }
        }

        /// <summary>
        /// 페이징 처리된 메뉴 리스트를 반환한다.
        /// </summary>
        public PagedModel<MenuItem> GetMenuPagedList(PagingOption option)
        {
            using (var repo = new MenuRepository())
            {
                return repo.SelectMenuPagedList(option);
            }
        }

        /// <summary>
        /// 메뉴 아이템을 추가한다.
        /// </summary>
        /// <param name="formIds">매핑 될 대시보드 테이블 ID 리스트</param>
        public MenuItem AddMenu(MenuItem menu, Guid[] formIds = null)
        {
            using (var repo = new MenuRepository())
            {
                repo.BeginTransaction();

                try
                {
                    if (repo.InsertMenu(menu))
                    {
                        repo.UpdateMenuFormMap(menu.MenuId, formIds, DateTimeOffset.Now);
                        repo.CommitTransaction();

                        menu = repo.SelectMenu(menu.MenuId);

                        logger.Info($"새 메뉴를 생성하였습니다. 메뉴: \"{menu.Name}\""
                            + $"\r\n\r\n"
                            + $"{menu}");

                        return menu;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"새 메뉴 생성 중 알 수 없는 오류가 발생하였습니다. 메뉴: \"{menu.Name}\""
                        + $"\r\n\r\n"
                        + $"{menu}");

                    try
                    {
                        repo.RollBackTransaction();
                    }
                    catch (Exception rex)
                    {
                        logger.Fatal(ex, $"새 메뉴 생성 함수에서 롤백 실행중 치명적인 에러가 발생했습니다. 메뉴: \"{menu.Name}\""
                            + $"\r\n\r\n"
                            + $"{menu}");

                        ExceptionDispatchInfo.Capture(rex).Throw();
                        // not reached
                    }

                    ExceptionDispatchInfo.Capture(ex).Throw();
                    return null; // not reached
                }

                return null;
            }
        }

        /// <summary>
        /// 메뉴 아이템을 업데이트한다.
        /// </summary>
        public MenuItem UpdateMenu(Guid menuId, IMenuItemUpdatable fields)
        {
            using (var repo = new MenuRepository())
            {
                var menu = repo.SelectMenu(menuId);
                if (menu == null)
                {
                    throw new ObjectNotFoundException($"업데이트 대상 메뉴를 찾을 수 없습니다.\r\n메뉴 ID: \"{menuId}\"");
                }

                List<UpdatedField> updated = null;
                menu.Update(fields, out updated);

                repo.BeginTransaction();

                try
                {
                    if (repo.UpdateMenu(menu))
                    {
                        repo.UpdateMenuFormMap(menuId, fields.FormTables, DateTimeOffset.Now);
                        repo.CommitTransaction();

                        menu = repo.SelectMenu(menuId);

                        logger.Info(
                            $"메뉴 정보가 업데이트 되었습니다. 메뉴: \"{menu.Name}\""
                            + $"\r\n\r\n"
                            + $"Fields: {UpdatedField.Print(updated)}"
                            + $"\r\n\r\n"
                            + $"{menu}");

                        return menu;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"메뉴 정보를 업데이트 하는 도중 알 수 없는 오류가 발생하였습니다. 메뉴: \"{menu.Name}\""
                        + $"\r\n\r\n"
                        + $"{menu}");

                    try
                    {
                        repo.RollBackTransaction();
                    }
                    catch (Exception rex)
                    {
                        logger.Fatal(ex, $"메뉴 정보 업데이트 함수에서 롤백 실행중 치명적인 에러가 발생했습니다. 메뉴: \"{menu.Name}\""
                            + $"\r\n\r\n"
                            + $"{menu}");

                        ExceptionDispatchInfo.Capture(rex).Throw();
                        // not reached
                    }

                    ExceptionDispatchInfo.Capture(ex).Throw();
                    return null; // not reached
                }

                return null;
            }
        }

        /// <summary>
        /// 메뉴 아이템를 삭제한다.
        /// </summary>
        public bool DeleteMenu(Guid menuId)
        {
            using (var repo = new MenuRepository())
            {
                var menu = repo.SelectMenu(menuId);
                if (menu == null)
                {
                    throw new ObjectNotFoundException($"삭제될 메뉴를 찾을 수 없습니다.\r\n메뉴 ID: \"{menuId}\"");
                }

                if (repo.DeleteMenu(menuId, CurrentUser.UserId, DateTimeOffset.Now))
                {
                    logger.Info($"메뉴 아이템을 삭제하였습니다. 메뉴: \"{menu.Name}\""
                        + $"\r\n\r\n"
                        + $"{menu}");

                    return true;
                }

                return false;
            }
        }
        #endregion
    }
}
