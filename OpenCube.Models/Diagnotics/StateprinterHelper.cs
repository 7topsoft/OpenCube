using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StatePrinting;

namespace OpenCube.Models.Diagnotics
{
    public static class StateprinterHelper
    {
        /// <summary>
        /// 기본 옵션이 적용된 프린터 객체를 반환한다.
        /// </summary>
        public static Stateprinter Create()
        {
            var printer = new Stateprinter();
            var config = printer.Configuration;

            config.SetIndentIncrement(", ")
                .SetNewlineDefinition("")
                .Add(new PrintableAttributeHarvester())
                .Add(new DateTimeValueConverter());

            config.SetOutputFormatter(new CurlyBraceStyleWrapper(config));

            return printer;
        }
    }
}
