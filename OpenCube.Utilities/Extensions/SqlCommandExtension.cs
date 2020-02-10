using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.SqlClient
{
    /// <summary>
    /// SqlCommand Extensions
    /// </summary>
    public static class SqlCommandExtension
    {
        /// <summary>
        /// This will add an array of parameters to a SqlCommand. This is used for an IN statement.
        /// Use the returned value for the IN part of your SQL call. (i.e. SELECT * FROM table WHERE field IN ({paramNameRoot}))
        /// http://stackoverflow.com/a/2377651/3781540
        /// </summary>
        /// <param name="cmd">The SqlCommand object to add parameters to.</param>
        /// <param name="placeHolder">What the parameter should be named followed by a unique value for each value. This value surrounded by {} in the CommandText will be replaced.</param>
        /// <param name="values">The array of strings that need to be added as parameters.</param>
        public static SqlParameter[] AddArrayParameters<T>(this SqlCommand cmd, string placeHolder, IEnumerable<T> values)
        {
            // An array cannot be simply added as a parameter to a SqlCommand so we need to loop through things and add it manually. 
            // Each item in the array will end up being it's own SqlParameter so the return value for this must be used as part of the
            // IN statement in the CommandText.
            var parameters = new List<SqlParameter>();
            var parameterNames = new List<string>();
            var paramNbr = 1;
            foreach (var value in values)
            {
                var paramName = string.Format("@{0}{1}", placeHolder, paramNbr++);
                parameterNames.Add(paramName);
                parameters.Add(cmd.Parameters.AddWithValue(paramName, value));
            }

            cmd.CommandText = cmd.CommandText.Replace("{" + placeHolder + "}", string.Join(", ", parameterNames));

            return parameters.ToArray();
        }
    }
}
