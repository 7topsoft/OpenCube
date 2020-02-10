using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using OpenCube.Core.Repositories;
using OpenCube.Models;
using OpenCube.Models.Logging;

namespace OpenCube.Core.Services
{
    /// <summary>
    /// 로그 서비스
    /// </summary>
    public class LogService : BaseService
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #region Constructors
        public LogService(IUserIdentity identity)
            : base(identity)
        { }
        #endregion

        #region Methods
        /// <summary>
        /// 페이징 처리 된 로그 리스트를 반환한다,
        /// </summary>
        public PagedModel<LogModel> GetLogPagedList(SearchOption option)
        {
            option.ThrowIfNull(nameof(option));
            option.Validate();

            using (var repo = new LogRepository())
            {
                return repo.SelectLogPagedList(option);
            }
        }

        /// <summary>
        /// 로그를 생성한다.
        /// </summary>
        public bool AddLog(LogModel log)
        {
            log.ThrowIfNull(nameof(log));

            using (var repo = new LogRepository())
            {
                return repo.InsertLog(log);
            }
        }
        #endregion
    }
}
