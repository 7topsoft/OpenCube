using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCube.DataParsers
{
    /// <summary>
    /// 엑셀 데이터 파싱 옵션
    /// </summary>
    public class ExcelParseOption
    {
        #region Constructors
        public ExcelParseOption(string beginPoint, string endPoint)
        {
            beginPoint.ThrowIfNullOrWhiteSpace(nameof(beginPoint));
            endPoint.ThrowIfNullOrWhiteSpace(nameof(endPoint));

            BeginPoint = beginPoint;
            EndPoint = endPoint;
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return $"{{ BeginPoint: '{BeginPoint}', EndPoint: '{EndPoint}' }}";
        }

        public void Validate()
        {
            new ExcelParser.CellLocation(BeginPoint);
            new ExcelParser.CellLocation(EndPoint);
        }
        #endregion

        #region Properties
        /// <summary>
        /// 읽을 범위의 시작 좌표 (시트 기준)
        /// ex) A5, B4
        /// </summary>
        public string BeginPoint { get; }

        /// <summary>
        /// 읽을 범위의 끝 좌표 (시트 기준)
        /// ex) A5, B4
        /// </summary>
        public string EndPoint { get; }
        #endregion
    }
}
