using System;
using System.Collections.Generic;
using System.Configuration.Abstractions;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenCube.Core.Services
{
    /// <summary>
    /// 모든 서비스 클래스들의 부모 클래스
    /// </summary>
    public class BaseService : IDisposable
    {
        #region Constructors
        public BaseService(IUserIdentity identtiy)
        {
            identtiy.ThrowIfNull(nameof(identtiy));

            this.CurrentUser = identtiy;
        }
        #endregion

        #region Methods
        public virtual void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
        }
        #endregion

        #region Properties
        public bool IsDisposed { get; private set; }

        public IUserIdentity CurrentUser { get; }

        public static IAppSettings AppSettings => ConfigurationManager.Instance.AppSettings;
        #endregion
    }
}
