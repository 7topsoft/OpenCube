using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCube.Models.Data
{
    /// <summary>
    /// 업데이트 가능한 엑셀 파일 템플릿 정보
    /// </summary>
    public interface IDataFileTemplateUpdatable
    {
        string FileName { get; set; }

        DataFileParseOption ParseOption { get; set; }
    }
}
