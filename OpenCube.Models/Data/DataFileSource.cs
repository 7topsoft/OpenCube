using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenCube.Models.Attributes;

namespace OpenCube.Models.Data
{
    /// <summary>
    /// 유저가 업로드 한 실적 파일 정보
    /// </summary>
    [JsonObject]
    public class DataFileSource : BaseModel
    {
        #region Constructors
        public DataFileSource(Guid formId, Guid formSectionId, Guid fileSourceId)
        {
            formId.ThrowIfEmpty(nameof(formId));
            formSectionId.ThrowIfEmpty(nameof(formSectionId));
            fileSourceId.ThrowIfEmpty(nameof(fileSourceId));

            this.FormId = formId;
            this.FormSectionId = formSectionId;
            this.FileSourceId = fileSourceId;
        }
        #endregion

        #region Methods
        public static DataFileSource ParseFrom(DataRow dr)
        {
            dr.ThrowIfNull(nameof(dr));

            Guid formId = dr.Get<Guid>("FormID");
            Guid formSectionId = dr.Get<Guid>("FormSectionID");
            Guid fileSourceId = dr.Get<Guid>("FileSourceID");

            return new DataFileSource(formId, formSectionId, fileSourceId)
            {
                FileTemplateId = dr.Get<Guid>("FileTemplateID"),
                HtmlTemplateId = dr.Get<Guid?>("HtmlTemplateID"),
                CreatorId = dr.Get<string>("CreatorID"),
                ConfirmerId = dr.Get<string>("ConfirmerID"),
                DeleterId = dr.Get<string>("DeleterId"),
                FileName = dr.Get<string>("FileName"),
                Extension = dr.Get<string>("Extension"),
                Size = dr.Get<long>("Size"),
                FileRelativePath = dr.Get<string>("Path"),
                Comment = dr.Get<string>("Comment"),
                IsConfirmed = dr.Get<bool>("IsConfirmed"),
                IsDeleted = dr.Get<bool>("IsDeleted"),
                CreatedDate = dr.Get<DateTimeOffset>("CreatedDate"),
                SourceDate = dr.Get<DateTimeOffset>("SourceDate"),
                ConfirmedDate = dr.Get<DateTimeOffset?>("ConfirmedDate"),
                DeletedDate = dr.Get<DateTimeOffset?>("DeletedDate"),
                // --
                FormName = dr.Get<string>("FormName"),
                FormSectionName = dr.Get<string>("FormSectionName")
            };
        }

        public static DataFileSource ParseFrom(DataRow dr, out int totalCount)
        {
            dr.ThrowIfNull(nameof(dr));

            totalCount = dr.Get<int>("TotalCount");

            return ParseFrom(dr);
        }

        public override void Validate()
        {
            // nothing
        }

        /// <summary>
        /// 파일의 절대 경로를 반환한다.
        /// </summary>
        public string GetFileAbsolutePath(string basePath)
        {
            basePath.ThrowIfNull(nameof(basePath));

            string filePath = basePath.TrimEnd(Path.DirectorySeparatorChar);
            string fileRelativePath = FileRelativePath?.TrimStart(Path.DirectorySeparatorChar);
            filePath = $"{filePath}\\{fileRelativePath}";

            return filePath;
        }
        #endregion

        #region Properties - Table Mapped
        /// <summary>
        /// 대시보드 테이블 ID
        /// </summary>
        [Printable]
        public Guid FormId { get; }

        /// <summary>
        /// 대시보드 테이블 - 업무 영역 ID
        /// </summary>
        [Printable]
        public Guid FormSectionId { get; }

        /// <summary>
        /// 파일 원본 ID
        /// </summary>
        [Printable]
        public Guid FileSourceId { get; }

        /// <summary>
        /// 파일 템플릿 ID
        /// </summary>
        [Printable]
        public Guid FileTemplateId { get; set; }

        /// <summary>
        /// 대시보드 양식 ID
        /// </summary>
        [Printable]
        public Guid? HtmlTemplateId { get; set; }

        /// <summary>
        /// 만든 유저 ID
        /// </summary>
        [Printable]
        public string CreatorId { get; set; }

        /// <summary>
        /// 컨펌한 유저 ID
        /// </summary>
        [Printable]
        public string ConfirmerId { get; set; }

        /// <summary>
        /// 삭제한 유저 ID
        /// </summary>
        public string DeleterId { get; set; }

        /// <summary>
        /// 파일명 (확장자명 포함)
        /// </summary>
        [Printable]
        public string FileName { get; set; }

        /// <summary>
        /// 확장자명
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// 파일 크기
        /// </summary>
        [Printable]
        public long Size { get; set; }

        /// <summary>
        /// 파일 간접 경로(서버 로컬 기준)
        /// </summary>
        [JsonIgnore]
        public string FileRelativePath { get; set; }

        /// <summary>
        /// 코멘트
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// 컨펌 여부
        /// </summary>
        [Printable]
        public bool IsConfirmed { get; set; }

        /// <summary>
        /// 삭제 여부
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 생성일
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// 데이터 기준일
        /// </summary>
        [Printable]
        public DateTimeOffset SourceDate { get; set; }

        /// <summary>
        /// 컨펌한 날짜
        /// </summary>
        [Printable]
        public DateTimeOffset? ConfirmedDate { get; set; }

        /// <summary>
        /// 삭제일
        /// </summary>
        public DateTimeOffset? DeletedDate { get; set; }
        #endregion

        #region Properties - Merged
        /// <summary>
        /// 대시보드 테이블 이름
        /// </summary>
        public string FormName { get; set; }

        /// <summary>
        /// 업무 영역 이름
        /// </summary>
        public string FormSectionName { get; set; }

        /// <summary>
        /// 생성 유저 Info
        /// </summary>
        public UserInfo CreatorInfo { get; set; }
        #endregion
    }
}
