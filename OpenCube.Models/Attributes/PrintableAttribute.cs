using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCube.Models.Attributes
{
    /// <summary>
    /// ToString 메소드 호출 시 Key = Value의 텍스트 포맷으로 표시할 프로퍼티를 지정한다.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PrintableAttribute : Attribute
    {
        public PrintableAttribute()
        { }
    }
}
