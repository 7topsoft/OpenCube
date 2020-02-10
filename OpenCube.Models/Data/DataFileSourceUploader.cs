using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OpenCube.Models.Data
{
    /// <summary>
    /// 데이터 파일 업로더
    /// </summary>
    [JsonObject]
    public class DataFileSourceUploader : BaseModel
    {
        #region Constructors
        public DataFileSourceUploader(Guid formId, Guid formSectionId)
        {
            formId.ThrowIfEmpty(nameof(formId));
            formSectionId.ThrowIfEmpty(nameof(formSectionId));

            this.FormId = formId;
            this.FormSectionId = formSectionId;
        }
        #endregion

        #region Methods
        public static DataFileSourceUploader ParseFrom(DataRow dr)
        {
            dr.ThrowIfNull(nameof(dr));

            Guid formId = dr.Get<Guid>("FormID");
            Guid formSectionId = dr.Get<Guid>("FormSectionID");

            return new DataFileSourceUploader(formId, formSectionId)
            {
                IsDeleted = dr.Get<bool>("IsDeleted"),
                CreatedDate = dr.Get<DateTimeOffset>("CreatedDate"),
                DeletedDate = dr.Get<DateTimeOffset?>("DeletedDate"),
                // merged -----------------------------------------------------
                UserInfo = UserInfo.ParseFrom(dr)
            };
        }

        public override void Validate()
        {
            // nothing
        }
        #endregion

        #region Properties - Origin
        /// <summary>
        /// 대시보드 ID
        /// </summary>
        public Guid FormId { get; }

        /// <summary>
        /// 업무 영역 ID
        /// </summary>
        public Guid FormSectionId { get; }

        /// <summary>
        /// 삭제 여부
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 생성 날짜
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// 삭제 날짜
        /// </summary>
        public DateTimeOffset? DeletedDate { get; set; }
        #endregion

        #region Properties - Mapped
        /// <summary>
        /// 유저 ID
        /// </summary>
        public string UserId => UserInfo?.UserId;

        /// <summary>
        /// 업로더 정보
        /// </summary>
        public UserInfo UserInfo { get; set; }
        #endregion
    }
}
