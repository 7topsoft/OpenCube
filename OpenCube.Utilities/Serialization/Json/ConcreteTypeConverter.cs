using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace OpenCube.Utilities.Serialization.Json
{
    /// <summary>
    /// cf. https://stackoverflow.com/a/12202914/3781540
    /// </summary>
    public class ConcreteTypeConverter<TConcrete> : JsonConverter
    {
        public ConcreteTypeConverter(bool camelCaseText = false)
        {
            CamelCaseText = camelCaseText;
        }

        public override bool CanConvert(Type objectType)
        {
            //assume we can convert to anything for now
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //explicitly specify the concrete type we want to create

            if (CamelCaseText)
            {
                serializer.ContractResolver = new CamelCasePropertyNamesContractResolver();
            }

            return serializer.Deserialize<TConcrete>(reader);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //use the default serialization - it works fine

            if (CamelCaseText)
            {
                serializer.ContractResolver = new CamelCasePropertyNamesContractResolver();
            }

            serializer.Serialize(writer, value);
        }

        public bool CamelCaseText { get; private set; }
    }
}
