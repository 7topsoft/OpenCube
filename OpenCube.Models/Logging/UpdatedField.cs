using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCube.Models.Logging
{
    /// <summary>
    /// 업데이트 된 필드 정보
    /// </summary>
    public class UpdatedField
    {
        public string FieldName { get; set; }

        public object OldValue { get; set; }

        public object NewValue { get; set; }

        public override string ToString()
        {
            return $"{FieldName}: '{OldValue}' => '{NewValue}'";
        }

        public static string Print(IEnumerable<UpdatedField> fields)
        {
            return $"{{ {string.Join(", ", fields)} }}";
        }
    }
}
