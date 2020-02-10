using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// DateTime 확장 메소드
    /// </summary>
    public static class DateExtensions
    {
        /// <summary>
        /// 해당 날짜의 가장 첫 시간을 반환한다.
        /// </summary>
        public static DateTimeOffset BeginOfDate(this DateTimeOffset self)
        {
            return new DateTimeOffset(self.Year, self.Month, self.Day, 0, 0, 0, self.Offset);
        }

        /// <summary>
        /// 해당 날짜의 가장 마지막 시간을 반환한다.
        /// </summary>
        public static DateTimeOffset EndOfDate(this DateTimeOffset self)
        {
            return self.BeginOfDate().AddDays(1).AddTicks(-1);
        }

        /// <summary>
        /// 요일을 기준으로 해당 주의 첫번째 날짜와 시간을 반환한다.
        /// </summary>
        public static DateTimeOffset BeginOfWeek(this DateTimeOffset self, DayOfWeek startOfWeek)
        {
            int diff = (7 + (self.DayOfWeek - startOfWeek)) % 7;
            return self.AddDays(-1 * diff);
        }

        /// <summary>
        /// 요일을 기준으로 해당 주의 마지막 날짜와 시간을 반환한다.
        /// </summary>
        public static DateTimeOffset EndOfWeek(this DateTimeOffset self, DayOfWeek startOfWeek)
        {
            return self.BeginOfWeek(startOfWeek).AddDays(7).AddTicks(-1);
        }

        /// <summary>
        /// 해당 월의 첫번째 날짜와 시간을 반환한다.
        /// </summary>
        public static DateTimeOffset BeginOfMonth(this DateTimeOffset self)
        {
            return new DateTimeOffset(self.Year, self.Month, 1, 0, 0, 0, self.Offset);
        }

        /// <summary>
        /// 해당 월의 마지막 날짜와 시간을 반환한다.
        /// </summary>
        public static DateTimeOffset EndOfMonth(this DateTimeOffset self)
        {
            return self.BeginOfMonth().AddMonths(1).AddTicks(-1);
        }

        /// <summary>
        /// 해당 월의 지정된 요일의 개수가 몇개인지 여부를 반환한다.
        /// </summary>
        public static int DayOfWeekCountInMonth(this DateTimeOffset self, DayOfWeek dayOfWeek)
        {
            var beginOfMonth = self.BeginOfMonth();
            int days = DateTime.DaysInMonth(self.Year, self.Month);
            int count = 0;

            for (int i = 0; i < days; i++)
            {
                if (beginOfMonth.AddDays(i).DayOfWeek == dayOfWeek)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// 해당 월의 지정 요일에 맞는 첫번째 날짜를 반환한다.
        /// </summary>
        public static DateTimeOffset FirstDateInMonth(this DateTimeOffset self, DayOfWeek dayOfWeek)
        {
            var beginOfMonth = self.BeginOfMonth();
            int days = DateTime.DaysInMonth(self.Year, self.Month);

            var find = beginOfMonth;

            for (int i = 0; i < days; i++)
            {
                find = beginOfMonth.AddDays(i);

                if (find.DayOfWeek == dayOfWeek)
                {
                    break;
                }
            }

            return find;
        }

        /// <summary>
        /// 해당 월의 지정 요일에 맞는 마지막 날짜를 반환한다.
        /// </summary>
        public static DateTimeOffset LastDateInMonth(this DateTimeOffset self, DayOfWeek dayOfWeek)
        {
            var endOfMonth = self.EndOfMonth();
            var find = endOfMonth;

            for (int i = 0; i < endOfMonth.Day; i++)
            {
                find = endOfMonth.AddDays(-i);

                if (find.DayOfWeek == dayOfWeek)
                {
                    break;
                }
            }

            return find;
        }

        /// <summary>
        /// 해당 월의 주차 정보를 반환한다.
        /// </summary>
        /// <param name="dayOfWeek">첫 주에 해당 요일이 있는 경우, |dayOfWeek| ~ 일요일까지 1주차라고 간주한다.</param>
        public static DateWeekRange WeekRangeOfMonth(this DateTimeOffset self, DayOfWeek dayOfWeek)
        {
            return self.WeekRangesOfMonth(dayOfWeek).FirstOrDefault(o => o.IsIn(self));
        }

        /// <summary>
        /// 해당 월의 주차 정보를 반환한다.
        /// </summary>
        /// <param name="dayOfWeek">첫 주에 해당 요일이 있는 경우, |dayOfWeek| ~ 일요일까지 1주차라고 간주한다.</param>
        public static DateWeekRange[] WeekRangesOfMonth(this DateTimeOffset self, DayOfWeek dayOfWeek)
        {
            var beginOfMonth = self.BeginOfMonth();
            var endOfMonth = self.EndOfMonth();
            var list = new List<DateWeekRange>();
            var firstDate = DayOfWeek.Monday;

            // 가장 첫번째 월요일을 찾는다.
            var firstMonday = self.FirstDateInMonth(firstDate);
            if (firstMonday.Day > beginOfMonth.Day)
            {
                // 월 첫번째 날짜(1일)가 첫번째 월요일 날짜보다 이전이라면
                // |beginOfMonth| ~ |firstMonday.Day - 1|까지 |dayOfWeek|이 있는지 체크한다.
                DateTimeOffset? find = null;

                for (int i = 0; i < firstMonday.Day - 1; i++)
                {
                    var temp = beginOfMonth.AddDays(i);
                    if (temp.DayOfWeek == dayOfWeek)
                    {
                        find = temp;
                        break;
                    }
                }

                // => 있다면 |beginOfMonth| ~ |firstMonday.Day - 1| 기간을 첫번째 주로 간주한다.
                if (find.HasValue)
                {
                    list.Add(new DateWeekRange
                    {
                        Month = beginOfMonth.Month,
                        Week = 1,
                        BeginDate = beginOfMonth,
                        EndDate = firstMonday.AddTicks(-1)
                    });
                }
                // => 없다면 지난 달의 마지막 주로 간주한다.
                else
                {
                    list.Add(new DateWeekRange
                    {
                        Month = self.AddMonths(-1).Month,
                        Week = DayOfWeekCountInMonth(self, dayOfWeek),
                        BeginDate = BeginOfWeek(self, firstDate),
                        EndDate = EndOfWeek(self, firstDate)
                    });
                }
            }

            var currentDate = firstMonday;
            bool stop = false;

            while (true)
            {
                if (stop)
                {
                    break;
                }

                var begin = currentDate.BeginOfDate();
                var end = begin.AddDays(7).AddTicks(-1);

                list.Add(new DateWeekRange
                {
                    Month = begin.Month,
                    Week = list.Count + 1,
                    BeginDate = begin,
                    EndDate = end
                });

                currentDate = end.AddTicks(1);
                stop = begin.Month != currentDate.Month;
            }

            return list.ToArray();
        }
    }
}
