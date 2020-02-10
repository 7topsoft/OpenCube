using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OpenCube.DataParsers
{
    /// <summary>
    /// 엑셀 셀 값 타입 (NPOI 라이브러리 참조를 피하기 위해 동일하게 선언)
    /// </summary>
    public enum ExcelCellValueType
    {
        [EnumMember(Value = "unknown")]
        Unknown,

        [EnumMember(Value = "blank")]
        Blank,

        [EnumMember(Value = "boolean")]
        Boolean,

        [EnumMember(Value = "numeric")]
        Numeric,

        [EnumMember(Value = "string")]
        String,

        [EnumMember(Value = "formula")]
        Formula
    }

    public static class ExcelCellValueHelper
    {
        public static object Convert(string value, ExcelCellValueType type)
        {
            switch (type)
            {
                default:
                case ExcelCellValueType.Unknown:
                case ExcelCellValueType.String:
                case ExcelCellValueType.Blank:
                case ExcelCellValueType.Formula:
                    return value;
                case ExcelCellValueType.Numeric:
                    {
                        double output = 0;
                        if (double.TryParse(value, out output))
                        {
                            return output;
                        }

                        return value;
                    }
                case ExcelCellValueType.Boolean:
                    {
                        bool output = false;
                        if (bool.TryParse(value, out output))
                        {
                            return output;
                        }

                        return value;
                    }
            }
        }
    }
}