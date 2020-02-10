using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using NLog;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace OpenCube.DataParsers
{
    /// <summary>
    /// 엑셀 파서
    /// </summary>
    public class ExcelParser : IDisposable
    {
        #region Inner Classes
        /// <summary>
        /// 셀 좌표
        /// </summary>
        public class CellLocation
        {
            /// <summary>
            /// 생성자
            /// </summary>
            /// <param name="location">셀 좌표 (ex: "A1")</param>
            public CellLocation(string location)
            {
                location.ThrowIfNullOrWhiteSpace(nameof(location));

                int alphabetLength = -1;
                for (int i = 0; i < location.Length; i++)
                {
                    if (!char.IsLetter(location[i]))
                    {
                        alphabetLength = i;
                        break;
                    }
                }

                this.Location = location;

                if (alphabetLength != -1)
                {
                    this.ColumnIndex = ConvertToColumnIndex(location.Substring(0, alphabetLength));

                    if (alphabetLength < location.Length)
                    {
                        string text = location.Substring(alphabetLength, location.Length - alphabetLength);

                        int rowIndex = -1;
                        if (int.TryParse(text, out rowIndex))
                        {
                            this.RowIndex = rowIndex;
                        }
                        else
                        {
                            throw new ArgumentException($"올바르지 않은 좌표 값입니다. 좌표: '{location}'");
                        }
                    }
                    else
                    {
                        throw new ArgumentException($"올바르지 않은 좌표 값입니다. 행 좌표 값이 존재하지 않습니다. 좌표: '{location}'");
                    }
                }
                else
                {
                    throw new ArgumentException($"올바르지 않은 좌표 값입니다. 열 좌표 값이 존재하지 않습니다. 좌표: '{location}'");
                }
            }

            public string Location { get; }

            public int ColumnIndex { get; }

            public int RowIndex { get; }
        }
        #endregion

        #region Fields
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private IWorkbook _workbook = null;
        #endregion

        #region Constructors
        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="filePath">엑셀 확장자를 포함한 파일 경로</param>
        public ExcelParser(string filePath, ExcelFileType type)
        {
            filePath.ThrowIfNullOrWhiteSpace(nameof(filePath));

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Path: '{filePath}'");
            }

            this.FilePath = filePath;
            this.FileType = type;
            this._workbook = CreateWorkbook(filePath, type);
        }
        #endregion

        #region Methods
        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;

            // NOTE(jhlee): NotImplementedException 발생
            //if (_workbook != null)
            //{
            //    _workbook.Dispose();
            //    _workbook = null;
            //}
        }

        /// <summary>
        /// NPOI IWorkbook 인스턴스를 생성하여 반환한다.
        /// </summary>
        /// <param name="filePath">파일 확장자를 포함한 경로</param>
        private static IWorkbook CreateWorkbook(string filePath, ExcelFileType type)
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    IWorkbook workbook = null;

                    if (type == ExcelFileType.xlsx)
                    {
                        workbook = new XSSFWorkbook(fs);
                    }
                    else if (type == ExcelFileType.xls)
                    {
                        workbook = new HSSFWorkbook(fs);
                    }
                    else
                    {
                        throw new NotImplementedException($"ExcelFileType: '{type}'");
                    }

                    return workbook;
                }
            }
            catch (Exception ex)
            {
                if (ex is NotImplementedException)
                {
                    ExceptionDispatchInfo.Capture(ex).Throw();
                    // not reached
                }

                throw new FileLoadException($"파일을 읽을 수 없습니다. 관리자에게 문의 바랍니다.", ex);
            }
        }

        /// <summary>
        /// 엑셀 시트 정보를 반환한다.
        /// </summary>
        private ISheet GetSheet(string sheetName)
        {
            sheetName.ThrowIfNullOrWhiteSpace(nameof(sheetName));

            // TODO(jhlee): 엉뚱한 sheetName일 경우 오류가 반환되는지 확인 필요
            return _workbook.GetSheet(sheetName);
        }

        /// <summary>
        /// 엑셀 시트에서  Filter에 해당하는 값들을 반환한다.
        /// </summary>
        public List<IExcelCellValue> GetValuesFromSheet(string sheetName, ExcelParseOption option, Func<IExcelCellValue> valueCreator)
        {
            option.Validate();

            var sheet = GetSheet(sheetName);
            if (sheet == null)
            {
                throw new InvalidOperationException($"시트 이름에 해당하는 엑셀 시트가 존재하지 않습니다. 시트이름: \"{sheetName}\"");
            }

            var values = new List<IExcelCellValue>();
            var beginPoint = new CellLocation(option.BeginPoint);
            var endPoint = new CellLocation(option.EndPoint);

            // 1. Sheet에서 필터내의 값을 검색한다. 
            for (int i = beginPoint.RowIndex - 1; i <= endPoint.RowIndex - 1; i++)
            {
                // 1-1. Sheet의 Row를 반환한다.
                var rows = sheet.GetRow(i);
                if (rows != null)
                {
                    // 1-2. Row 내에 존재하는 Cell값을 검색한다.
                    for (var j = 0; j < rows.PhysicalNumberOfCells; j++)
                    {
                        var cell = rows.Cells[j];
                        if (cell.ColumnIndex >= beginPoint.ColumnIndex - 1 && cell.ColumnIndex <= endPoint.ColumnIndex - 1)
                        {
                            values.Add(GetCellValue(cell, valueCreator));
                        }
                    }
                }
                else
                {
                    logger.Warn($"시트에서 대상 row를 찾을 수 없습니다. File: '{FilePath}', SheetName: '{sheet.SheetName}', Row: '{i + 1}'");
                }
            }

            return values;
        }

        /// <summary>
        /// 셀 타입에 해당하는 Value를 반환한다.
        /// </summary>
        private IExcelCellValue GetCellValue(ICell cell, Func<IExcelCellValue> valueCreator, CellType? preferredType = null)
        {
            var value = valueCreator();
            value.Column = ConvertToColumnName(cell.ColumnIndex + 1);
            value.Row = cell.RowIndex + 1;
            value.ValueType = ConvertCellValueType(preferredType.HasValue ? preferredType.Value : cell.CellType);

            switch (value.ValueType)
            {
                case ExcelCellValueType.Blank:
                    value.ValueType = ExcelCellValueType.String;
                    value.Value = string.Empty;
                    break;
                case ExcelCellValueType.Numeric:
                    value.Value = cell.NumericCellValue;
                    break;
                case ExcelCellValueType.String:
                    value.Value = cell.StringCellValue;
                    break;
                case ExcelCellValueType.Boolean:
                    value.Value = cell.BooleanCellValue;
                    break;
                case ExcelCellValueType.Formula:
                    value = GetCellValue(cell, valueCreator, cell.CachedFormulaResultType); // 재귀로 처리
                    break;
                default:
                    value.ValueType = ExcelCellValueType.Unknown;
                    value.Value = $"Unknown value";

                    logger.Warn($"Not supported type of cell. SheetName: '{value.SheetName}', Location: '{value.Location}', Type: '{value.ValueType}'");
                    break;
            }

            return value;
        }

        /// <summary>
        /// 엑셀 좌표 숫자를 알파벳 좌표로 치환한다.
        /// cf. https://stackoverflow.com/questions/181596/how-to-convert-a-column-number-eg-127-into-an-excel-column-eg-aa
        /// </summary>
        private static string ConvertToColumnName(int columnNumber)
        {
            var dividend = columnNumber;
            var columnName = "";
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        /// <summary>
        /// 엑셀 좌표 알파벳을 숫자로 치환한다. (zero-based)
        /// cf. https://stackoverflow.com/questions/667802/what-is-the-algorithm-to-convert-an-excel-column-letter-into-its-number
        /// </summary>
        public static int ConvertToColumnIndex(string columnName)
        {
            columnName.ThrowIfNullOrWhiteSpace(nameof(columnName));
            columnName = columnName.ToUpperInvariant();

            int sum = 0;
            for (int i = 0; i < columnName.Length; i++)
            {
                sum *= 26;
                sum += (columnName[i] - 'A' + 1);
            }

            return sum;
        }

        public static ExcelCellValueType ConvertCellValueType(CellType type)
        {
            switch (type)
            {
                case CellType.Blank: return ExcelCellValueType.Blank;
                case CellType.Boolean: return ExcelCellValueType.Boolean;
                case CellType.Numeric: return ExcelCellValueType.Numeric;
                case CellType.String: return ExcelCellValueType.String;
                case CellType.Formula: return ExcelCellValueType.Formula;
                default: return ExcelCellValueType.Unknown;
            }
        }
        #endregion

        #region Properties
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// 파일 경로
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// 파일명
        /// </summary>
        public string FileName => Path.GetFileName(FilePath);

        /// <summary>
        /// 엑셀 파일 타입
        /// </summary>
        public ExcelFileType FileType { get; private set; }
        #endregion
    }
}
