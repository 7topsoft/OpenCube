using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenCube.DataParsers;
using OpenCube.Utilities.Serialization.Json;

namespace OpenCube.Models.Data
{
    /// <summary>
    /// 엑셀 파일 파싱 옵션
    /// </summary>
    [JsonObject]
    public class DataFileParseOption : BaseModel
    {
        #region Methods
        public static DataFileParseOption ParseFrom(string json)
        {
            return JsonConvert.DeserializeObject<DataFileParseOption>(json, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasingExceptDictionaryKeysResolver()
            });
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasingExceptDictionaryKeysResolver()
            });
        }

        public override string ToString()
        {
            return ToJson();
        }

        public override void Validate()
        {
            if (!Sheets.Any())
            {
                throw new ArgumentException($"정의된 시트 별 옵션이 없습니다. Field: 'sheets'");
            }

            foreach (var kv in Sheets)
            {
                foreach (var option in kv.Value)
                {
                    option.Validate();
                }
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// 시트 별 옵션 리스트 (Key: 시트명(SheetName))
        /// </summary>
        public Dictionary<string, ExcelParseOption[]> Sheets { get; set; }
        #endregion
    }
}
