using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// <see cref="Enum"/> extensions
    /// </summary>
    public static class EnumExtension
    {
        #region Enumeration
        /// <summary>
        /// enum 값에 포함된 각각의 flag를 추출하여 열거자 형태로 반환한다.
        /// </summary>
        public static IEnumerable<Enum> GetFlags(this Enum self)
        {
            foreach (Enum value in Enum.GetValues(self.GetType()))
            {
                if (self.HasFlag(value))
                {
                    yield return value;
                }
            }
        }

        /// <summary>
        /// enum에 포함 된 값을 string 배열로 반환한다.
        /// </summary>
        /// <returns>"Apple, Banana, Orange"</returns>
        public static string ToFlagsString(this Enum self)
        {
            var list = self.GetFlags().Select(o => o.ToString());
            return string.Join(", ", list);
        }
        #endregion


        #region EnumMember
        private static readonly ConcurrentDictionary<string, string> _enumToValueMap 
            = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private static readonly ConcurrentDictionary<string, object> _valueToEnumMap
            = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// enum에 <see cref="EnumMemberAttribute"/>로 선언된 속성이 있다면 <see cref="EnumMemberAttribute.Value"/>를 반환한다.
        /// 선언된 속성이 없다면 enum명을 그대로 반환한다.
        /// </summary>
        public static string ToEnumMemberString(this Enum self)
        {
            var enumType = self.GetType();
            var enumName = Enum.GetName(enumType, self);
            var key = $"{enumType.FullName}.{enumName}";

            return _enumToValueMap.GetOrAdd(key, _ =>
            {
                var enumMemberAttr = ((EnumMemberAttribute[])enumType.GetField(enumName)
                    .GetCustomAttributes(typeof(EnumMemberAttribute), true))
                    .SingleOrDefault();

                return (enumMemberAttr != null) ? enumMemberAttr.Value : enumName;
            });
        }

        /// <summary>
        /// <see cref="EnumMemberAttribute.Value"/>로 선언된 string을 <see cref="Enum"/>으로 파싱하여 반환한다.
        /// 선언된 속성이 없다면 <see cref="Enum.Parse(Type, string)"/>로 시도한다.
        /// </summary>
        public static T ParseEnumMember<T>(string value, bool ignoreCase = true)
        {
            var enumType = typeof(T);
            var key = $"{enumType.FullName}.{value}";

            return (T)_valueToEnumMap.GetOrAdd(key, _ =>
            {
                foreach (string name in Enum.GetNames(enumType))
                {
                    var enumMemberAttr = ((EnumMemberAttribute[])enumType.GetField(name)
                        .GetCustomAttributes(typeof(EnumMemberAttribute), true))
                        .SingleOrDefault();

                    if (enumMemberAttr != null && enumMemberAttr.Value == value)
                    {
                        return Enum.Parse(enumType, name);
                    }
                }

                return Enum.Parse(enumType, value, ignoreCase);
            });
        }
        #endregion
    }
}
