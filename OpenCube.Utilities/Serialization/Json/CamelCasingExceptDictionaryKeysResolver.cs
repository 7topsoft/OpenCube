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
    /// Keep casing when serializing dictionaries
    /// cf. https://stackoverflow.com/a/24226442/3781540
    /// </summary>
    public class CamelCasingExceptDictionaryKeysResolver : CamelCasePropertyNamesContractResolver
    {
        public CamelCasingExceptDictionaryKeysResolver(bool exceptDictionaryKeys = true)
        {
            this.IsDictionaryKeysExcepted = exceptDictionaryKeys;
        }

        protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
        {
            JsonDictionaryContract contract = base.CreateDictionaryContract(objectType);

            if (IsDictionaryKeysExcepted)
            {
                contract.DictionaryKeyResolver = propertyName => propertyName;
            }

            return contract;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);

#if DEBUG
            if (properties != null)
            {
                // base 클래스의 프로퍼티가 먼저 출력되도록 보정
                return properties.OrderBy(p => BaseTypesAndSelf(p.DeclaringType).Count()).ToList();
            }
#endif

            return properties;
        }

        /// <summary>
        /// cf. https://stackoverflow.com/a/32572740/3781540
        /// </summary>
        public static IEnumerable<Type> BaseTypesAndSelf(Type type)
        {
            while (type != null)
            {
                yield return type;
                type = type.BaseType;
            }
        }

        /// <summary>
        /// json serialize 시 <see cref="Dictionary{TKey, TValue}"/>는 CamelCase를 적용 안 할 것인지 여부
        /// </summary>
        public bool IsDictionaryKeysExcepted { get; set; }
    }
}
