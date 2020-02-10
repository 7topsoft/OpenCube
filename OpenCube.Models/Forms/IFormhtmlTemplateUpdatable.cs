using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCube.Models.Forms
{
    /// <summary>
    /// 업데이트 가능한 대시보드 테이블 html 양식 필드
    /// </summary>
    public interface IFormHtmlTemplateUpdatable
    {
        string Description { get; set; }

        string ScriptContent { get; set; }

        string HtmlContent { get; set; }

        string StyleContent { get; set; }
    }
}
