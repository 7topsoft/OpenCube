using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCube.Models.Data;

namespace OpenCube.Models
{
    /// <summary>
    /// 업데이트 가능한 대시보드 테이블 영역 필드
    /// </summary>
    public interface IFormTableSectionUpdatable
    {
        string FormSectionName { get; set; }

        string ScriptVariable { get; set; }

        bool IsEnabled { get; set; }

        List<string> FileSourceUploaders { get; set; }

        IDataFileTemplateUpdatable FileTemplate { get; set; }
    }
}
