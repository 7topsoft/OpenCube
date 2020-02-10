using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data
{
    /// <summary>
    /// DataRow Extension
    /// </summary>
    public static class DataRowExtension
    {
        /// <summary>
        /// DataRow로부터 지정된 column의 data를 꺼내어 반환한다.
        /// column이 없거나 NULL인 경우 default value를 반환한다.
        /// </summary>
        public static T Get<T>(this DataRow self, string column, T defaultValue = default(T))
        {
            if (!self.Table.Columns.Contains(column))
            {
                return defaultValue;
            }

            if (self[column] == null || self[column] == DBNull.Value)
            {
                return defaultValue;
            }

            try
            {
                return self.Field<T>(column);
            }
            catch (InvalidCastException ex)
            {
                Type fromType = self.Table.Columns[column].DataType;
                Type toType = typeof(T);
                throw new InvalidCastException($"Specified cast is not valid. {fromType.Name} => {toType.Name}", ex);
            }
        }
    }
}