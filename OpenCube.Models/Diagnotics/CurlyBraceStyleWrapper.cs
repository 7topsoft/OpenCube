using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StatePrinting.Configurations;
using StatePrinting.Introspection;
using StatePrinting.OutputFormatters;

namespace OpenCube.Models.Diagnotics
{
    /// <summary>
    /// <see cref="CurlyBraceStyle"/>의 기존 Print 동작을 보정해준다.
    /// </summary>
    public class CurlyBraceStyleWrapper : IOutputFormatter
    {
        private readonly CurlyBraceStyle _formatter;

        public CurlyBraceStyleWrapper(Configuration configuration)
        {
            _formatter = new CurlyBraceStyle(configuration);
        }

        public string Print(List<Token> tokens)
        {
            var typeToken = tokens.FirstOrDefault(o => o.Tokenkind == TokenType.FieldnameWithTypeAndReference);
            var filter = tokens.Where(o => o != typeToken).ToList();

            string print = _formatter.Print(filter);
            print = print.Replace("{,", "{"); // ex) "{, PropertyName = PropertyValue }"
            print = $"{typeToken.FieldType.Name} {print}"; // ex) ClassName { PropertyName = PropertyValue }

            return print;
        }
    }
}
