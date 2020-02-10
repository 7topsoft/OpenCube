using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// 기간
    /// </summary>
    public class DateRange
    {
        /// <summary>
        /// 시작일
        /// </summary>
        public DateTimeOffset BeginDate { get; set; }

        /// <summary>
        /// 종료일
        /// </summary>
        public DateTimeOffset EndDate { get; set; }

        /// <summary>
        /// 해당 날짜가 이 주에 포함되어 있는지 여부를 반환한다.
        /// </summary>
        public bool IsIn(DateTimeOffset date)
        {
            return BeginDate <= date && date <= EndDate;
        }

        public override string ToString()
        {
            return $"{BeginDate} ~ {EndDate}";
        }
    }
}
