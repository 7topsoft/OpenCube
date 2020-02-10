using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCube.Models.Attributes;

namespace OpenCube.Models
{
    /// <summary>
    /// 페이징 처리된 데이터 리스트
    /// </summary>
    public class PagedModel<T>
    {
        public PagedModel()
        { }

        public PagedModel(PagingOption option)
        {
            option.ThrowIfNull(nameof(option));

            PageNumber = option.PageNumber;
            PageCount = option.PageCount;
        }

        public bool HasMore => PageNumber < TotalPageCount;

        /// <summary>
        /// 아이템 리스트
        /// </summary>
        public List<T> Items { get; set; } = Enumerable.Empty<T>().ToList();

        /// <summary>
        /// 아이템 전체 개수
        /// </summary>
        [Printable]
        public int TotalCount { get; set; }

        /// <summary>
        /// 현재 페이지 번호
        /// </summary>
        [Printable]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// 페이지 카운트
        /// </summary>
        [Printable]
        public int PageCount { get; set; }

        /// <summary>
        /// 페이징 처리된 전체 페이지의 개수
        /// </summary>
        [Printable]
        public int TotalPageCount => (int)Math.Ceiling((float)TotalCount / PageCount);

        /// <summary>
        /// 페이징 옵션
        /// </summary>
        public PagingOption PagingOption { get; set; }
    }
}
