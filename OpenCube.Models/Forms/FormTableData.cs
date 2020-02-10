using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenCube.Models.Data;

namespace OpenCube.Models.Forms
{
    /// <summary>
    /// 대시보드 테이블 렌더링이 필요한 데이터 묶음
    /// </summary>
    [JsonObject]
    public class FormTableData
    {
        /// <summary>
        /// 데이터가 컨펌 되었는지 여부를 반환한다.
        /// </summary>
        public bool IsConfirmed => Data.Values.Any() && Data.Values.All(o => o.FileSource.IsConfirmed);

        /// <summary>
        /// 데이터 날짜 정보
        /// </summary>
        public DataDateRange DataTerm { get; set; }

        /// <summary>
        /// 업무 영역 별 데이터 모음 (Key: FormTableSection.ScriptVariable)
        /// </summary>
        public Dictionary<string, DataFileCellValues> Data { get; set; } = new Dictionary<string, DataFileCellValues>();

        /// <summary>
        /// 화면 렌더링에 필요한 Html 템플릿
        /// </summary>
        public FormHtmlTemplate HtmlTemplate { get; set; }
    }
}
