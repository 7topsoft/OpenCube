using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StatePrinting.ValueConverters;

namespace OpenCube.Models.Diagnotics
{
    public class DateTimeValueConverter : IValueConverter
    {
        public bool CanHandleType(Type t)
        {
            return t == typeof(DateTimeOffset) || t == typeof(DateTime);
        }

        public string Convert(object source)
        {
            Type type = source.GetType();
            string value = null;

            if (type == typeof(DateTimeOffset))
            {
                value = ((DateTimeOffset)source).ToString("o");
            }
            else if (type == typeof(DateTime))
            {
                value = ((DateTime)source).ToString("o");
            }
            else
            {
                throw new NotImplementedException($"Type: '{type.FullName}'");
            }

            return $"\"{value}\"";
        }
    }
}
