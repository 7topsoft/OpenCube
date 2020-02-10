using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// <see cref="String"/> extensions
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Indicates whether the specified string is null or an System.String.Empty string.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty(this string self)
        {
            return string.IsNullOrEmpty(self);
        }

        /// <summary>
        /// Indicates whether a specified string is null, empty, or consists only of white-space characters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrWhiteSpace(this string self)
        {
            return string.IsNullOrWhiteSpace(self);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNullOrEmpty(this string self)
        {
            return !string.IsNullOrEmpty(self);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNullOrWhiteSpace(this string self)
        {
            return !string.IsNullOrWhiteSpace(self);
        }

        /// <summary>
        /// Returns a value indicating whether a specified substring occurs within this string.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this string self, string value, StringComparison comparison)
        {
            self.ThrowIfNull(nameof(self));
            return self.IndexOf(value, comparison) >= 0;
        }

        /// <summary>
        /// 텍스트의 앞쪽에서부터 oldValue에 해당하는 내용을 newValue로 치환하되, 한 번만 수행한다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReplaceFirst(this string self, string oldValue, string newValue, bool ignoreCase = false)
        {
            int pos = self.IndexOf(oldValue, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            if (pos < 0)
            {
                return self;
            }

            return self.Substring(0, pos) + newValue + self.Substring(pos + oldValue.Length);
        }

        /// <summary>
        /// 텍스트의 뒷쪽에서부터 oldValue에 해당하는 내용을 newValue로 치환하되, 한 번만 수행한다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReplaceLast(this string self, string oldValue, string newValue, bool ignoreCase = false)
        {
            int pos = self.LastIndexOf(oldValue, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            if (pos < 0)
            {
                return self;
            }

            return self.Substring(0, pos) + newValue + self.Substring(pos + oldValue.Length);
        }

        /// <summary>
        /// 대소문자 구분 없이 두 텍스트를 비교한 결과를 반환한다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsIgnoreCase(this string self, string value)
        {
            return self.Equals(value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 16진수 텍스트를 byte 배열로 변환하여 반환한다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToByteArray(this string self)
        {
            if (self.IsNullOrWhiteSpace())
            {
                return Enumerable.Empty<byte>().ToArray();
            }

            self = self.Replace("0x", string.Empty);
            return Enumerable.Range(0, self.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(self.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
