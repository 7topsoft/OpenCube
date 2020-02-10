using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// 한 주의 기간을 나타내는 클래스
    /// </summary>
    public class DateWeekRange : DateRange
    {
        /// <summary>
        /// 월
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// n주차인지 여부를 반환한다.
        /// </summary>
        public int Week { get; set; }

        public override string ToString()
        {
            return $"{Month}월 {Week}주 ({BeginDate} ~ {EndDate})";
        }
    }
}
