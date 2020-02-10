using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCube.Utilities
{
    /// <summary>
    /// cf. https://github.com/QuintSys/Quintsys.Practices.EnterpriseLibrary.Data/blob/master/EnterpriseLibrary.Data/Quintsys.Practices.EnterpriseLibrary.Data/Sql/ITransactedConnection.cs
    /// 
    /// NOTE(jhlee): 편의성을 위해 몇가지 메소드를 오버라이드 처리함.
    /// </summary>
    public interface ITransactedConnection : IDisposable
    {
        /// <summary>
        ///     Opens a transacted database connection.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        ///     Commits and disposes the database transaction.
        /// </summary>
        void CommitTransaction();

        /// <summary>
        ///     Rollback and disposes the database transaction.
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        ///     Creates a <see cref="T:System.Data.SqlClient.SqlCommand" /> for a stored procedure.
        /// </summary>
        /// <param name="storedProcedureName">The name of the stored procedure.</param>
        /// <returns>
        ///     The <see cref="T:System.Data.SqlClient.SqlCommand" /> for the stored procedure.
        /// </returns>
        SqlCommand GetStoredProcCommand(string storedProcedureName);

        /// <summary>
        ///     Adds a new In <see cref="T:System.Data.Common.DbParameter" /> object to the given <paramref name="sqlCommand" />.
        /// </summary>
        /// <param name="sqlCommand">The commmand to add the parameter.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="dbType">One of the <see cref="T:System.Data.DbType" /> values.</param>
        /// <param name="value">The value of the parameter.</param>
        void AddInParameter(SqlCommand sqlCommand, string name, DbType dbType, object value);

        /// <summary>
        ///     Adds a new In <see cref="T:System.Data.Common.DbParameter" /> object to the given <paramref name="sqlCommand" />.
        /// </summary>
        /// <param name="sqlCommand">The commmand to add the parameter.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="sqlDbType">One of the <see cref="T:System.Data.SqlDbType" /> values.</param>
        /// <param name="value">The value of the parameter.</param>
        void AddInParameter(SqlCommand sqlCommand, string name, SqlDbType sqlDbType, object value);

        /// <summary>
        ///     Adds a new Out <see cref="T:System.Data.Common.DbParameter" /> object to the given <paramref name="sqlCommand" />.
        /// </summary>
        /// <param name="sqlCommand">The command to add the out parameter.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="dbType">One of the <see cref="T:System.Data.DbType" /> values.</param>
        /// <param name="size">The maximum size of the data within the column.</param>
        void AddOutParameter(SqlCommand sqlCommand, string name, DbType dbType, int size = -1);

        /// <summary>
        ///     Adds a new Out <see cref="T:System.Data.Common.DbParameter" /> object to the given <paramref name="sqlCommand" />.
        /// </summary>
        /// <param name="sqlCommand">The command to add the out parameter.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="sqlDbType">One of the <see cref="T:System.Data.SqlDbType" /> values.</param>
        /// <param name="size">The maximum size of the data within the column.</param>
        void AddOutParameter(SqlCommand sqlCommand, string name, SqlDbType sqlDbType, int size = -1);

        /// <summary>
        /// Gets a parameter value from a given <paramref name="sqlCommand" />.
        /// </summary>
        /// <param name="sqlCommand">The command that contains the parameter.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// The value of the parameter.
        /// </returns>
        object GetParameterValue(SqlCommand sqlCommand, string name);

        /// <summary>
        /// Gets a parameter value from a given <paramref name="sqlCommand" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlCommand">The sql command that contains the parameter.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <returns></returns>
        T GetParameterValue<T>(SqlCommand sqlCommand, string name);

        /// <summary>
        ///     Executes the <paramref name="sqlCommand" /> within a given
        ///     <see cref="T:System.Data.SqlClient.SqlTransaction" />, and returns the number of rows affected.
        /// </summary>
        /// <param name="sqlCommand">The SQL command.</param>
        /// <param name="commandTimeOut">
        ///     The wait time before terminating the attempt to execute a command and generating an error. The default is 180
        ///     seconds.
        /// </param>
        int ExecuteNonQuery(SqlCommand sqlCommand, int commandTimeOut = 180);

        /// <summary>
        ///     Executes the <paramref name="sqlCommand" /> within a transaction and returns an
        ///     <see cref="T:System.Data.IDataReader" /> through which the result can be read.
        ///     It is the responsibility of the caller to close the connection and reader when finished.
        /// </summary>
        /// <param name="sqlCommand">The command that contains the query to execute.</param>
        /// <param name="commandTimeOut">The command time out.</param>
        /// <returns>
        ///     An <see cref="T:System.Data.IDataReader" /> object.
        /// </returns>
        IDataReader ExecuteReader(SqlCommand sqlCommand, int commandTimeOut = 180);

        /// <summary>
        ///     Executes the <paramref name="sqlCommand" /> within a transaction and returns an
        ///     <see cref="T:System.Data.DataSet" /> through which the result can be read.
        /// </summary>
        /// <param name="sqlCommand">The command that contains the query to execute.</param>
        /// <param name="commandTimeOut">The command time out.</param>
        /// <returns>
        ///     An <see cref="T:System.Data.DataSet" /> object.
        /// </returns>
        DataSet ExecuteDataSet(SqlCommand sqlCommand, int commandTimeOut = 180);

        /// <summary>
        ///     Executes the <paramref name="sqlCommand" /> within a transaction and returns an
        ///     <see cref="T:System.Data.DataSet" /> through which the result can be read.
        /// </summary>
        /// <param name="sqlCommand">The command that contains the query to execute.</param>
        /// <param name="commandTimeOut">The command time out.</param>
        /// <returns>
        ///     An <see cref="T:System.Object" /> object.
        /// </returns>
        object ExecuteScalar(SqlCommand sqlCommand, int commandTimeOut = 180);
    }
}
