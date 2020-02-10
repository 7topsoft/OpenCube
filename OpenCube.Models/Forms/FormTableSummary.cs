using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenCube.Models.Attributes;

namespace OpenCube.Models.Forms
{
    /// <summary>
    /// 대시보드 테이블 요약 정보
    /// </summary>
    [JsonObject]
    public class FormTableSummary : BaseModel
    {
        #region Constructors
        public FormTableSummary(FormTable formTable)
        {
            formTable.ThrowIfNull(nameof(formTable));

            this.FormId = formTable.FormId;
            this.Name = formTable.Name;
            this.SortOrder = formTable.SortOrder;
        }
        #endregion

        #region Methods
        public override void Validate()
        { }
        #endregion

        #region Properties
        /// <summary>
        /// 대시보드 테이블 ID
        /// </summary>
        [Printable]
        public Guid FormId { get; set; }

        /// <summary>
        /// 대시보드 테이블 이름
        /// </summary>
        [Printable]
        public string Name { get; set; }

        /// <summary>
        /// 페이지 내 정렬 순서
        /// </summary>
        [Printable]
        public int SortOrder { get; set; }
        #endregion
    }
}
