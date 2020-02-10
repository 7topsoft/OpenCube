using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCube.Models;
using OpenCube.Models.Logging;

namespace OpenCube.Core.Repositories
{
    /// <summary>
    /// 로그 테이블 DAC
    /// </summary>
    public class LogRepository : BaseRepository
    {
        #region Constructors
        public LogRepository()
        { }
        #endregion

        #region Methods
        /// <summary>
        /// 페이징 처리된 대시보드 테이블 리스트를 반환한다.
        /// </summary>
        public PagedModel<LogModel> SelectLogPagedList(SearchOption option)
        {
            string procCommandName = "up_Log_SelectPagedList";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "PageNumber", DbType.Int32, option.PageNumber);
                Connection.AddInParameter(command, "PageCount", DbType.Int32, option.PageCount);
                Connection.AddInParameter(command, "SortBy", DbType.String, option.SortBy);
                Connection.AddInParameter(command, "OrderBy", DbType.String, option.OrderBy.ToEnumMemberString());
                Connection.AddInParameter(command, "SearchType", DbType.String, option.SearchType);
                Connection.AddInParameter(command, "SearchKeyword", DbType.String, option.SearchKeyword);

                if (option.BeginDate.HasValue)
                {
                    Connection.AddInParameter(command, "BeginDate", DbType.DateTimeOffset, option.BeginDate.Value);
                }

                if (option.EndDate.HasValue)
                {
                    Connection.AddInParameter(command, "EndDate", DbType.DateTimeOffset, option.EndDate.Value);
                }

                using (DataSet ds = Connection.ExecuteDataSet(command))
                {
                    ValidateTableCount(ds, 1);

                    var result = new PagedModel<LogModel>(option) { PagingOption = option };
                    int totalCount = 0;

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        var menu = LogModel.ParseFrom(dr, out totalCount);
                        result.Items.Add(menu);
                    }

                    result.TotalCount = totalCount;

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 로그를 DB에 추가한다.
        /// </summary>
        public bool InsertLog(LogModel log)
        {
            log.ThrowIfNull(nameof(log));

            string procCommandName = "up_Log_Insert";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "Level", DbType.String, log.Level);
                Connection.AddInParameter(command, "Logger", DbType.String, log.Logger);
                Connection.AddInParameter(command, "Message", DbType.String, log.Message);
                Connection.AddInParameter(command, "ExceptionMessage", DbType.String, log.ExceptionMessage);
                Connection.AddInParameter(command, "ServerIP", DbType.String, log.ServerIP);
                Connection.AddInParameter(command, "ServerHostName", DbType.String, log.ServerHost);
                Connection.AddInParameter(command, "UserID", DbType.String, log.UserId);
                Connection.AddInParameter(command, "ClientIP", DbType.String, log.ClientIp);
                Connection.AddInParameter(command, "RouteURL", DbType.String, log.RouteURL);
                Connection.AddInParameter(command, "RequestURL", DbType.String, log.RequestURL);
                Connection.AddInParameter(command, "Timestamp", DbType.DateTimeOffset, log.Timestamp);

                return (int)Connection.ExecuteNonQuery(command) > 0;
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }
        #endregion
    }
}
