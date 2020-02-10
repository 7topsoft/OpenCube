using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OpenCube.Models.Data
{
    /// <summary>
    /// 대시보드 테이블 내 업무 영역 별 셀 데이터 리스트
    /// </summary>
    [JsonObject]
    public class DataFileCellValues : BaseModel
    {
        #region Constructors
        public DataFileCellValues(Guid formId, Guid formSectionId, Guid fileSourceId)
        {
            formId.ThrowIfEmpty(nameof(formId));
            formSectionId.ThrowIfEmpty(nameof(formSectionId));
            fileSourceId.ThrowIfEmpty(nameof(fileSourceId));

            this.FormId = formId;
            this.FormSectionId = formSectionId;
            this.FileSourceId = fileSourceId;
        }
        #endregion

        #region Methods
        public override void Validate()
        {
            ScriptVariable.ThrowIfNullOrWhiteSpace(nameof(ScriptVariable));
        }
        #endregion

        #region Properties
        /// <summary>
        /// 대시보드 테이블 ID
        /// </summary>
        public Guid FormId { get; }

        /// <summary>
        /// 업무 영역 ID
        /// </summary>
        public Guid FormSectionId { get; }

        /// <summary>
        /// 파일 소스 ID
        /// </summary>
        public Guid FileSourceId { get; }

        /// <summary>
        /// 업무 영역의 스크립트 변수명
        /// </summary>
        public string ScriptVariable { get; set; }

        /// <summary>
        /// 데이터의 원본 파일 정보
        /// </summary>
        public DataFileSource FileSource { get; set; }

        /// <summary>
        /// 시트 별 셀 데이터 리스트 (Key: SheetName, Key: Cell Location)
        /// </summary>
        public Dictionary<string, Dictionary<string, DataFileCellValue>> Sheets { get; } = new Dictionary<string, Dictionary<string, DataFileCellValue>>();

        ///// <summary>
        ///// 데이터 시작일
        ///// </summary>
        //public DateTimeOffset BeginDate { get; set; }

        ///// <summary>
        ///// 데이터 종료일
        ///// </summary>
        //public DateTimeOffset EndDate { get; set; }
        #endregion
    }
}
