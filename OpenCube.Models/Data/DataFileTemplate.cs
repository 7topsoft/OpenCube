using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using OpenCube.Models.Attributes;
using OpenCube.Models.Logging;

namespace OpenCube.Models.Data
{
    /// <summary>
    /// 유저 사용할 파일 템플릿 정보
    /// </summary>
    [JsonObject]
    public class DataFileTemplate : BaseModel
    {
        #region Fields
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region Constructors
        public DataFileTemplate(Guid formId, Guid formSectionId, Guid fileTemplateId)
        {
            formId.ThrowIfEmpty(nameof(formId));
            formSectionId.ThrowIfEmpty(nameof(formSectionId));
            fileTemplateId.ThrowIfEmpty(nameof(fileTemplateId));

            this.FormId = formId;
            this.FormSectionId = formSectionId;
            this.FileTemplateId = fileTemplateId;
        }
        #endregion

        #region Methods
        public static DataFileTemplate ParseFrom(DataRow dr)
        {
            dr.ThrowIfNull(nameof(dr));

            Guid formId = dr.Get<Guid>("FormID");
            Guid formSectionId = dr.Get<Guid>("FormSectionID");
            Guid fileTemplateId = dr.Get<Guid>("FileTemplateID");

            string parseOptionJson = null;
            DataFileParseOption parseOption = null;

            try
            {
                parseOptionJson = dr.Get<string>("ParseOption");

                if (parseOptionJson.IsNotNullOrWhiteSpace())
                {
                    parseOption = DataFileParseOption.ParseFrom(parseOptionJson);
                }
                else
                {
                    // nothing
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"ParseOption 컬럼 값을 복원하는데 실패하였습니다."
                    + $"\r\n\r\n"
                    + $"값: {parseOptionJson}");
            }

            return new DataFileTemplate(formId, formSectionId, fileTemplateId)
            {
                CreatorId = dr.Get<string>("CreatorID"),
                DeleterId = dr.Get<string>("DeleterID"),
                FileName = dr.Get<string>("FileName"),
                Extension = dr.Get<string>("Extension"),
                Size = dr.Get<long>("Size"),
                FileRelativePath = dr.Get<string>("Path"),
                ParseOption = parseOption,
                IsDeleted = dr.Get<bool>("IsDeleted"),
                CreatedDate = dr.Get<DateTimeOffset>("CreatedDate"),
                DeletedDate = dr.Get<DateTimeOffset?>("DeletedDate")
            };
        }

        public void Update(IDataFileTemplateUpdatable fields)
        {
            List<UpdatedField> updated = null;
            this.Update(fields, out updated);
        }

        public void Update(IDataFileTemplateUpdatable fields, out List<UpdatedField> updated)
        {
            fields.ThrowIfNull(nameof(fields));

            updated = new List<UpdatedField>();

            if (FileName != fields.FileName && fields.FileName.IsNotNullOrWhiteSpace())
            {
                updated.Add(new UpdatedField
                {
                    FieldName = nameof(FileName),
                    OldValue = FileName,
                    NewValue = fields.FileName
                });

                FileName = fields.FileName;
            }

            // ParseOption은 일일히 비교하기 어려우므로 무조건 업데이트 처리
            {
                updated.Add(new UpdatedField
                {
                    FieldName = nameof(ParseOption),
                    OldValue = ParseOption,
                    NewValue = fields.ParseOption
                });

                ParseOption = fields.ParseOption;
            }

            if (updated.Any())
            {
                Validate();
            }
        }

        public List<UpdatedField> Diff(DataFileTemplate other)
        {
            other.ThrowIfNull(nameof(other));

            var diff = new List<UpdatedField>();

            if (FileName != other.FileName)
            {
                diff.Add(new UpdatedField
                {
                    FieldName = nameof(FileName),
                    OldValue = FileName,
                    NewValue = other.FileName
                });
            }

            // ParseOption은 일일히 비교하기 어려우므로 무조건 업데이트 처리
            {
                diff.Add(new UpdatedField
                {
                    FieldName = nameof(ParseOption),
                    OldValue = ParseOption,
                    NewValue = other.ParseOption
                });
            }

            return diff;
        }

        public override void Validate()
        {
            FileName.ThrowIfNullOrWhiteSpace(nameof(FileName));
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
        public Guid FormId { get; }

        /// <summary>
        /// 대시보드 테이블 - 업무 영역 ID
        /// </summary>
        public Guid FormSectionId { get; }

        /// <summary>
        /// 파일 템플릿 ID
        /// </summary>
        [Printable]
        public Guid FileTemplateId { get; }

        /// <summary>
        /// 만든 유저 ID
        /// </summary>
        public string CreatorId { get; set; }

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
        /// 파일 파싱 옵션
        /// </summary>
        public DataFileParseOption ParseOption { get; set; }

        /// <summary>
        /// 삭제 여부
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 생성일
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// 삭제일
        /// </summary>
        public DateTimeOffset? DeletedDate { get; set; }
        #endregion
    }
}
