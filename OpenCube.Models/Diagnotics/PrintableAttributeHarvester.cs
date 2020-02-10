using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCube.Models.Attributes;
using StatePrinting.FieldHarvesters;

namespace OpenCube.Models.Diagnotics
{
    /// <summary>
    /// <see cref="PrintableAttribute"/>가 붙은 프로퍼티만 추출한다.
    /// </summary>
    public class PrintableAttributeHarvester : IFieldHarvester
    {
        public bool CanHandleType(Type type)
        {
            return true;
        }

        public List<SanitizedFieldInfo> GetFields(Type type)
        {
            var fields = new HarvestHelper().GetFieldsAndProperties(type);
            return fields.Where(x => x.FieldInfo.CustomAttributes.FirstOrDefault(o => o.AttributeType == typeof(PrintableAttribute)) != null)
                .ToList();
        }
    }
}
