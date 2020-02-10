using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCube.Models.Attributes;

namespace OpenCube.Models
{
    /// <summary>
    /// 검색 옵션
    /// </summary>
    public class SearchOption : PagingOption
    {
        /// <summary>
        /// 검색 타입
        /// </summary>
        [Printable]
        public string SearchType { get; set; }

        /// <summary>
        /// 키워드
        /// </summary>
        [Printable]
        public string SearchKeyword { get; set; }

        /// <summary>
        /// 검색 시작일
        /// </summary>
        public DateTimeOffset? BeginDate { get; set; }

        /// <summary>
        /// 검색 종료일
        /// </summary>
        public DateTimeOffset? EndDate { get; set; }
    }
}
