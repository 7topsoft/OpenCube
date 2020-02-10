using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCube.Utilities;

namespace OpenCube.Core.Repositories
{
    /// <summary>
    /// DAC 담당 부모 클래스
    /// </summary>
    public class BaseRepository : IDisposable
    {
        #region Fields
        private const string ConnectionStringKey = "SKT_MNO_Dashboard";
        #endregion

        #region Constructor
        public BaseRepository()
        {
            Connection = new TransactedConnection(ConnectionStringKey);
        }
        #endregion

        #region Methods
        public virtual void Dispose()
        {
            if (!IsDisposed)
            {
                Connection.Dispose();
                Connection = null;
                IsDisposed = true;
            }
        }

        public virtual void BeginTransaction()
        {
            if (IsDisposed)
            {
                return;
            }

            Connection.BeginTransaction();
        }

        public virtual void CommitTransaction()
        {
            if (IsDisposed)
            {
                return;
            }

            Connection.CommitTransaction();
        }

        public virtual void RollBackTransaction()
        {
            if (IsDisposed)
            {
                return;
            }

            Connection.RollbackTransaction();
        }

        public virtual void ValidateTableCount(DataSet ds, int expectedCount)
        {
            if (ds.Tables.Count < expectedCount)
            {
                throw new DataException($"프로시저가 반환해야 할 테이블의 수가 올바르지 않습니다."
                    + $"\r\n"
                    + $"반환된 테이블 개수: {ds.Tables.Count}, 반환 되어야할 테이블 개수: {expectedCount}");
            }
        }
        #endregion

        #region Properties
        public ITransactedConnection Connection { get; private set; }

        public bool IsDisposed { get; private set; } = false;
        #endregion
    }
}
