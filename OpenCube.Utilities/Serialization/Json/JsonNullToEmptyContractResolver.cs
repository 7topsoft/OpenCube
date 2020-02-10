using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace OpenCube.Utilities.Serialization.Json
{
    /// <summary>
    /// json 필드 중 string 또는 array의 값이 null일 경우 string.Empty 또는 "[]"으로 치환하도록 한다.
    /// cf. http://stackoverflow.com/questions/23830206/json-convert-empty-string-instead-of-null
    /// usage)
    /// 
    /// JsonSerializerSettings settings = new JsonSerializerSettings();
    /// settings.ContractResolver = new JsonNullToEmptyContractResolver();
    /// vat json = JsonConvert.SerializeObject(value, settings);
    /// </summary>
    public class JsonNullToEmptyContractResolver : CamelCasingExceptDictionaryKeysResolver
    {
        public JsonNullToEmptyContractResolver(bool nullToEmpty = true)
        {
            this.CanNullToEmpty = nullToEmpty;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (property != null && CanNullToEmpty)
            {
                property.ValueProvider = new NullToEmptyValueProvider(property.PropertyType, property.ValueProvider);
            }

            return property;
        }

        /// <summary>
        /// null 값인 필드들에 기본 값을 할당할 것인지 여부
        /// </summary>
        public bool CanNullToEmpty { get; set; }

        private class NullToEmptyValueProvider : IValueProvider
        {
            private readonly Type _propType;
            private readonly IValueProvider _provider;

            public NullToEmptyValueProvider(Type propType, IValueProvider provider)
            {
                if (propType == null)
                {
                    throw new ArgumentNullException(nameof(propType));
                }

                if (provider == null)
                {
                    throw new ArgumentNullException(nameof(provider));
                }

                _propType = propType;
                _provider = provider;
            }

            public object GetValue(object target)
            {
                var value = _provider.GetValue(target);
                if (value == null)
                {
                    if (_propType == typeof(string))
                    {
                        value = string.Empty;
                    }
                    else if (_propType.IsArray)
                    {
                        value = Enumerable.Empty<object>();
                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(_propType))
                    {
                        value = Enumerable.Empty<object>();
                    }
                    else
                    {
                        // nothing
                    }
                }

                return value;
            }

            public void SetValue(object target, object value)
            {
                _provider.SetValue(target, value);
            }
        }
    }
}
