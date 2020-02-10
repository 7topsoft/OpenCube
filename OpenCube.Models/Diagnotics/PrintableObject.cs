using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using StatePrinting;

namespace OpenCube.Models.Diagnotics
{
    /// <summary>
    /// <see cref="Attributes.PrintableAttribute"/>이 적용된 프로퍼티들을 <see cref="object.ToString"/>을 호출할 때 자동으로 출력해주는 부모 클래스
    /// </summary>
    public abstract class PrintableObject
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly Stateprinter printer = StateprinterHelper.Create();

        public override string ToString()
        {
            try
            {
                return printer.PrintObject(this);
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"오브젝트를 로깅하는데 오류가 발생하였습니다."
                    + $"\r\n"
                    + $"[Printable] 속성이 동일한 키에 할당되어 있는지 확인하십시오.");

                return this.GetType().FullName;
            }
        }
    }
}
