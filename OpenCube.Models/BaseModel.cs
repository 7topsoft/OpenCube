using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCube.Models.Diagnotics;

namespace OpenCube.Models
{
    /// <summary>
    /// 모든 모델 클래스의 부모 클래스
    /// </summary>
    public abstract class BaseModel : PrintableObject
    {
        /// <summary>
        /// 모델 객체의 필드 값들이 올바른지 검증한다.
        /// </summary>
        public abstract void Validate();
    }
}
