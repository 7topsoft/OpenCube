using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// <see cref="object"/> extensions
    /// </summary>
    public static class ObjectExtension
    {
        /// <summary>
        /// T가 null이면 예외를 발생시킨다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNull<T>(this T self, string paramName, string message = null) where T : class
        {
            if (self == null)
            {
                throw new ArgumentNullException(paramName, message);
            }
        }

        /// <summary>
        /// string이 null 또는 empty라면 예외를 발생시킨다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNullOrEmpty(this string self, string paramName, string message = null)
        {
            if (string.IsNullOrEmpty(self))
            {
                throw new ArgumentNullException(paramName, message);
            }
        }

        /// <summary>
        /// string이 null 또는 공백으로 채워져 있다면 예외를 발생시킨다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNullOrWhiteSpace(this string self, string paramName, string message = null)
        {
            if (string.IsNullOrWhiteSpace(self))
            {
                throw new ArgumentNullException(paramName, message);
            }
        }

        /// <summary>
        /// 정수가 범위를 벗어난다면 예외를 발생시킨다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfOutOfRange(this int self, string parameterName = null, int min = 0, int max = Int32.MaxValue)
        {
            if (self > max || self < min)
            {
                ThrowArgumentOutOfRangeException(parameterName, self, string.Format("{0} is out of range.", parameterName ?? "Value"));
            }
        }

        /// <summary>
        /// Int64가 범위를 벗어난다면 예외를 발생시킨다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfOutOfRange(this long self, string parameterName = null, long min = 0, long max = Int64.MaxValue)
        {
            if (self > max || self < min)
            {
                ThrowArgumentOutOfRangeException(parameterName, self, string.Format("{0} is out of range.", parameterName ?? "Value"));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ThrowArgumentOutOfRangeException(string paramName, object actualValue, string message)
        {
            throw new ArgumentOutOfRangeException(paramName, actualValue, message);
        }

        /// <summary>
        /// 객체가 null이 아니면 ToString() 결과값을, null이라면 string.Empty를 반환한다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToStringOrEmpty(this object self)
        {
            return self != null ? self.ToString() : string.Empty;
        }
    }
}
