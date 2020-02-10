using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCube.Models.Data;

namespace OpenCube.Models.Forms
{
    /// <summary>
    /// 업데이트 가능한 대시보드 테이블 필드
    /// </summary>
    public interface IFormTableUpdatable
    {
        string Name { get; set; }

        string Description { get; set; }

        // NOTE(jhlee): 실적 업로드 주기가 변경 되면 데이터 조회/확인 취소 처리에 문제가 생기므로 변경 불가능하도록 수정함.
        //DataUploadInterval UploadInterval { get; set; }

        int? UploadWeekOfMonth { get; set; }

        DataUploadDayOfWeek UploadDayOfWeek { get; set; }

        bool IsEnabled { get; set; }
    }
}
