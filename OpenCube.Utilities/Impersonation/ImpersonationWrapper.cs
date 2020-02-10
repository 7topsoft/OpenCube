using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCube.Utilities.Impersonation
{
    /// <summary>
    /// Impersonation Wrapper
    /// </summary>
    public class ImpersonationWrapper : IDisposable
    {
        #region Constructors
        public ImpersonationWrapper(bool isUsed, string domain, string creditId, string creditPwd)
        {
            // Impersonation을 사용할 경우만 값을 초기화 시켜준다.
            if (isUsed)
            {
                domain.ThrowIfNullOrWhiteSpace(nameof(domain));
                creditId.ThrowIfNullOrWhiteSpace(nameof(creditId));
                creditPwd.ThrowIfNullOrWhiteSpace(nameof(creditPwd));

                Impersonation = new Impersonation();

                Impersonation.ImpersonationStart(domain, creditId, creditPwd);
            }
        }
        #endregion

        #region Methods
        public void Dispose()
        {
            if (Impersonation != null)
            {
                Impersonation.ImpersonationEnd();

                Impersonation = null;
            }
        }
        #endregion

        #region Properties
        private Impersonation Impersonation { get; set; }
        #endregion
    }
}
