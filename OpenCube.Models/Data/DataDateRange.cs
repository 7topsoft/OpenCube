
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenCube.Models.Forms;

namespace OpenCube.Models.Data
{
    /// <summary>
    /// 데이터 기간
    /// </summary>
    [JsonObject]
    public class DataDateRange : DateRange
    {
        public DataDateRange()
        { }

        public DataDateRange(DateTimeOffset beginDate, DateTimeOffset endDate)
        {
            this.BeginDate = beginDate;
            this.EndDate = endDate;
        }

        public DataDateRange(DateRange dateRange)
        {
            dateRange.ThrowIfNull(nameof(dateRange));

            this.BeginDate = dateRange.BeginDate;
            this.EndDate = dateRange.EndDate;
        }

        /// <summary>
        /// 데이터 업로드 주기
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public DataUploadInterval UploadInterval { get; set; }

        /// <summary>
        /// 기준이 되는 원본 날짜
        /// </summary>
        public DateTimeOffset SourceDate { get; set; }

        /// <summary>
        /// 금일/금주/금월 실적에 해당하는 날짜인지 여부
        /// </summary>
        public bool IsCurrentData { get; set; } = true;
    }
}
