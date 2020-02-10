using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenCube.Models.Attributes;

namespace OpenCube.Models
{
    /// <summary>
    /// 페이징 옵션
    /// </summary>
    [JsonObject]
    public class PagingOption : BaseModel
    {
        /// <summary>
        /// 페이지 번호
        /// </summary>
        [Printable]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// 페이지 카운트
        /// </summary>
        [Printable]
        public int PageCount { get; set; } = 20;

        /// <summary>
        /// 정렬 기준
        /// </summary>
        [Printable]
        public string SortBy { get; set; } = null;

        /// <summary>
        /// 정렬 순서
        /// </summary>
        [Printable]
        [JsonConverter(typeof(StringEnumConverter))]
        public PagingOrderBy OrderBy { get; set; } = PagingOrderBy.Asc;

        public override void Validate()
        {
            PageNumber.ThrowIfOutOfRange(nameof(PageNumber));
            PageCount.ThrowIfOutOfRange(nameof(PageCount));
        }
    }
}
