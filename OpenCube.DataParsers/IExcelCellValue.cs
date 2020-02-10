using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.SS.UserModel;

namespace OpenCube.DataParsers
{
    /// <summary>
    /// 엑셀 시트로부터 읽은 셀 값
    /// </summary>
    public interface IExcelCellValue
    {
        /// <summary>
        /// 시트 이름
        /// </summary>
        string SheetName { get;  }

        /// <summary>
        /// 열 (cf. "A", "B", "C")
        /// </summary>
        string Column { get; set; }

        /// <summary>
        /// 행 (one-based)
        /// </summary>
        int Row { get; set; }

        /// <summary>
        /// 열과 행을 합친 좌표
        /// </summary>
        string Location { get; }

        /// <summary>
        /// 셀 값
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// 셀 타입
        /// </summary>
        ExcelCellValueType ValueType { get; set; }
    }
}
