using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// <see cref="Guid"/> extensions
    /// </summary>
    public static class GuidExtension
    {
        public static void ThrowIfEmpty(this Guid self, string paramName)
        {
            if (self == Guid.Empty)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}
