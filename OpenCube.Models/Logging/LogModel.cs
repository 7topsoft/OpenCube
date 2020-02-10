using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenCube.Models.Attributes;

namespace OpenCube.Models.Logging
{
    /// <summary>
    /// 로그 모델 
    /// </summary>
    [JsonObject]
    public class LogModel : BaseModel
    {
        #region Methods
        public static LogModel ParseFrom(DataRow dr)
        {
            dr.ThrowIfNull(nameof(dr));

            return new LogModel()
            {
                Sequence = dr.Get<int>("Sequence"),
                Level = dr.Get<string>("Level"),
                Logger = dr.Get<string>("Logger"),
                Message = dr.Get<string>("Message"),
                ExceptionMessage = dr.Get<string>("ExceptionMessage"),
                ServerIP = dr.Get<string>("ServerIP"),
                ServerHost = dr.Get<string>("ServerHostName"),
                UserId = dr.Get<string>("UserID"),
                ClientIp = dr.Get<string>("ClientIp"),
                RouteURL = dr.Get<string>("RouteURL"),
                RequestURL = dr.Get<string>("RequestURL"),
                Timestamp = dr.Get<DateTimeOffset>("Timestamp"),
            };
        }

        public static LogModel ParseFrom(DataRow dr, out int totalCount)
        {
            dr.ThrowIfNull(nameof(dr));

            totalCount = dr.Get<int>("TotalCount");

            return ParseFrom(dr);
        }

        public override void Validate()
        { }
        #endregion

        #region Properties
        /// <summary>
        /// 순서
        /// </summary>
        [Printable]
        public int Sequence { get; set; }

        /// <summary>
        /// 로그 레벨
        /// </summary>
        [Printable]
        public string Level { get; set; }

        /// <summary>
        /// 로거
        /// </summary>
        [Printable]
        public string Logger { get; set; }

        /// <summary>
        /// 로그 메시지
        /// </summary>
        [Printable]
        public string Message { get; set; }

        /// <summary>
        /// 예외 메시지
        /// </summary>
        [Printable]
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// 서버 IP
        /// </summary>
        [Printable]
        public string ServerIP { get; set; }

        /// <summary>
        /// 서버 Host
        /// </summary>
        [Printable]
        public string ServerHost { get; set; }

        /// <summary>
        /// 유저 ID
        /// </summary>
        [Printable]
        public string UserId { get; set; }

        /// <summary>
        /// 클라이언트 IP
        /// </summary>
        [Printable]
        public string ClientIp { get; set; }

        /// <summary>
        /// Route Url
        /// </summary>
        public string RouteURL { get; set; }

        /// <summary>
        /// Request Url
        /// </summary>
        public string RequestURL { get; set; }

        /// <summary>
        /// 로그 시간
        /// </summary>
        [Printable]
        public DateTimeOffset Timestamp { get; set; }
        #endregion
    }
}
