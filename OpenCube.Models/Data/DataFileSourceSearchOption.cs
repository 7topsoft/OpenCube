using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCube.Models.Attributes;

namespace OpenCube.Models.Data
{
    /// <summary>
    /// 데이터 파일 소스 검색 옵션
    /// </summary>
    public class DataFileSourceSearchOption : PagingOption
    {
        /// <summary>
        /// 대시보드 ID (null이면 전체 검색)
        /// </summary>
        [Printable]
        public Guid? FormId { get; set; }

        /// <summary>
        /// 키워드
        /// </summary>
        [Printable]
        public string SearchKeyword { get; set; }
    }
}
