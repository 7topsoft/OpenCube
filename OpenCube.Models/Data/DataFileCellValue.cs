using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenCube.DataParsers;

namespace OpenCube.Models.Data
{
    /// <summary>
    /// 엑셀 파일로부터 읽은 셀 데이터
    /// </summary>
    [JsonObject]
    public class DataFileCellValue : BaseModel, IExcelCellValue
    {
        #region Constructors
        public DataFileCellValue(Guid fileSourceId, string sheetName)
        {
            fileSourceId.ThrowIfEmpty(nameof(fileSourceId));
            sheetName.ThrowIfNullOrEmpty(nameof(sheetName));

            this.FileSourceId = fileSourceId;
            this.SheetName = sheetName;
        }
        #endregion

        #region Methods
        public static DataFileCellValue ParseFrom(DataRow dr)
        {
            dr.ThrowIfNull(nameof(dr));

            Guid fileSourceId = dr.Get<Guid>("FileSourceID");
            var sheetName = dr.Get<string>("SheetName");

            string value = dr.Get<string>("Value");
            var valueType = EnumExtension.ParseEnumMember<ExcelCellValueType>(dr.Get<string>("Type"));

            return new DataFileCellValue(fileSourceId, sheetName)
            {
                Column = dr.Get<string>("Column"),
                Row = dr.Get<int>("Row"),
                Value = ExcelCellValueHelper.Convert(value, valueType),
                ValueType = valueType,
                CreatedDate = dr.Get<DateTimeOffset>("CreatedDate")
            };
        }

        public override string ToString()
        {
            return $"{{ SheetName: '{SheetName}', Location: '{Location}', Type: {ValueType}, Value: {Value} }}";
        }

        public override void Validate()
        {
            FileSourceId.ThrowIfEmpty(nameof(FileSourceId));
            Column.ThrowIfNullOrWhiteSpace(nameof(Column));
            Row.ThrowIfOutOfRange(nameof(Row));
        }
        #endregion

        #region Properties
        /// <summary>
        /// 원본 파일 ID
        /// </summary>
        public Guid FileSourceId { get; }

        /// <summary>
        /// 시트 이름
        /// </summary>
        public string SheetName { get; }

        /// <summary>
        /// 열 (cf. "A", "B", "C")
        /// </summary>
        [JsonProperty("col")]
        public string Column { get; set; }

        /// <summary>
        /// 행 (cf. "1", "2", "3")
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// 좌표
        /// </summary>
        public string Location => $"{Column}{Row}";

        /// <summary>
        /// 값
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 타입
        /// </summary>
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ExcelCellValueType ValueType { get; set; }

        /// <summary>
        /// 생성일
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.Now;
        #endregion
    }
}
