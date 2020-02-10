using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCube.Models.Menu
{
    /// <summary>
    /// 업데이트 가능한 메뉴 아이템 필드
    /// </summary>
    public interface IMenuItemUpdatable
    {
        string Name { get; set; }

        string GroupName { get; set; }

        string IconName { get; set; }

        int SortOrder { get; set; }

        bool IsVisible { get; set; }

        /// <summary>
        /// 대시보드 테이블 ID 리스트
        /// </summary>
        Guid[] FormTables { get; set; }
    }
}
