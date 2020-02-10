using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenCube.Models.Attributes;
using OpenCube.Models.Data;
using OpenCube.Models.Logging;

namespace OpenCube.Models.Forms
{
    /// <summary>
    /// "대시보드 - 업무 영역"에 해당하는 모델 클래스
    /// </summary>
    public class FormTableSection : BaseModel
    {
        #region Constructors
        public FormTableSection(Guid formId, Guid sectionId)
        {
            formId.ThrowIfEmpty(nameof(formId));
            sectionId.ThrowIfEmpty(nameof(sectionId));

            this.FormId = formId;
            this.FormSectionId = sectionId;
        }
        #endregion

        #region Methods
        public override void Validate()
        {
        }

        public static FormTableSection ParseFrom(DataRow dr)
        {
            dr.ThrowIfNull(nameof(dr));

            var formId = dr.Get<Guid>("FormId");
            var sectionId = dr.Get<Guid>("FormSectionId");

            return new FormTableSection(formId, sectionId)
            {
                FileTemplateId = dr.Get<Guid>("FileTemplateId"),
                FormSectionName = dr.Get<string>("FormSectionName"),
                ScriptVariable = dr.Get<string>("ScriptVariable"),
                CreatorId = dr.Get<string>("CreatorID"),
                DeleterId = dr.Get<string>("DeleterID"),
                IsEnabled = dr.Get<bool>("IsEnabled"),
                IsDeleted = dr.Get<bool>("IsDeleted"),
                CreatedDate = dr.Get<DateTimeOffset>("CreatedDate"),
                UpdatedDate = dr.Get<DateTimeOffset?>("UpdatedDate"),
                DeletedDate = dr.Get<DateTimeOffset?>("DeletedDate"),
                // merged ----------------------------------------------------------
                FormName = dr.Get<string>("FormName")
            };
        }

        public static FormTableSection ParseFrom(DataRow dr, out int totalCount)
        {
            dr.ThrowIfNull(nameof(dr));

            totalCount = dr.Get<int>("TotalCount");

            return ParseFrom(dr);
        }

        public void Update(IFormTableSectionUpdatable fields)
        {
            List<UpdatedField> updated = null;
            this.Update(fields, out updated);
        }

        public void Update(IFormTableSectionUpdatable fields, out List<UpdatedField> updated)
        {
            fields.ThrowIfNull(nameof(fields));

            updated = new List<UpdatedField>();

            if (FormSectionName != fields.FormSectionName && fields.FormSectionName.IsNotNullOrWhiteSpace())
            {
                updated.Add(new UpdatedField
                {
                    FieldName = nameof(FormSectionName),
                    OldValue = FormSectionName,
                    NewValue = fields.FormSectionName
                });

                FormSectionName = fields.FormSectionName;
            }

            if (ScriptVariable != fields.ScriptVariable && fields.ScriptVariable.IsNotNullOrWhiteSpace())
            {
                updated.Add(new UpdatedField
                {
                    FieldName = nameof(ScriptVariable),
                    OldValue = ScriptVariable,
                    NewValue = fields.ScriptVariable
                });

                ScriptVariable = fields.ScriptVariable;
            }

            if (IsEnabled != fields.IsEnabled)
            {
                updated.Add(new UpdatedField
                {
                    FieldName = nameof(IsEnabled),
                    OldValue = IsEnabled,
                    NewValue = fields.IsEnabled
                });

                IsEnabled = fields.IsEnabled;
            }

            var oldUploaders = FileSourceUploaders.Keys.OrderBy(o => o).ToArray();
            var newUploaders = fields.FileSourceUploaders.OrderBy(o => o).ToArray();
            if (!oldUploaders.SequenceEqual(newUploaders))
            {
                updated.Add(new UpdatedField
                {
                    FieldName = nameof(FileSourceUploaders),
                    OldValue = oldUploaders,
                    NewValue = newUploaders
                });

                // service 단에서 업데이트 처리함.
            }


            if (fields.FileTemplate != null)
            {
                if (FileTemplate == null)
                {
                    // service 단에서 채워줘야 함.
                }
                else
                {
                    List<UpdatedField> temp = null;
                    FileTemplate.Update(fields.FileTemplate, out temp);

                    updated.AddRange(temp);
                }
            }

            if (updated.Any())
            {
                UpdatedDate = DateTimeOffset.Now;

                Validate();
            }
        }

        public List<UpdatedField> Diff(FormTableSection other)
        {
            other.ThrowIfNull(nameof(other));

            var diff = new List<UpdatedField>();

            if (FormSectionName != other.FormSectionName)
            {
                diff.Add(new UpdatedField
                {
                    FieldName = nameof(FormSectionName),
                    OldValue = FormSectionName,
                    NewValue = other.FormSectionName
                });
            }

            if (IsEnabled != other.IsEnabled)
            {
                diff.Add(new UpdatedField
                {
                    FieldName = nameof(IsEnabled),
                    OldValue = IsEnabled,
                    NewValue = other.IsEnabled
                });
            }

            if (other.FileTemplate != null)
            {
                if (FileTemplate == null)
                {
                    // nothing
                }
                else
                {
                    var temp = FileTemplate.Diff(other.FileTemplate);
                    diff.AddRange(temp);
                }
            }

            return diff;
        }

        public static FormTableSection CreateFrom(Guid formId, Guid formSectionId, IFormTableSectionUpdatable fields)
        {
            fields.ThrowIfNull(nameof(fields));

            return new FormTableSection(formId, formSectionId)
            {
                FormSectionName = fields.FormSectionName,
                IsEnabled = fields.IsEnabled,
                ScriptVariable = fields.ScriptVariable,
                //FileTemplate = fields.FileTemplate, // NOTE(jhlee): 여기서 안하고 Create 하는 쪽에서 직접 주입
                CreatedDate = DateTimeOffset.Now
            };
        }
        #endregion

        #region Properties - Table Mapped
        /// <summary>
        /// 대시보드 테이블 ID
        /// </summary>
        [Printable]
        public Guid FormId { get; }

        /// <summary>
        /// 업무 영역 ID
        /// </summary>
        [Printable]
        public Guid FormSectionId { get; }

        /// <summary>
        /// 파일 템플릿 ID (데이터 업로드 시 사용되는 엑셀 샘플 파일)
        /// </summary>
        [Printable]
        public Guid FileTemplateId { get; set; }

        /// <summary>
        /// 영역명
        /// </summary>
        [Printable]
        public string FormSectionName { get; set; }

        /// <summary>
        /// 대시보드 HTML 양식 템플릿에서 사용할 js 변수명
        /// </summary>
        [Printable]
        public string ScriptVariable { get; set; }

        /// <summary>
        /// 사용 여부
        /// </summary>
        [Printable]
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 삭제 여부
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 만든 유저 ID
        /// </summary>
        public string CreatorId { get; set; }

        /// <summary>
        /// 삭제한 유저 ID
        /// </summary>
        public string DeleterId { get; set; }

        /// <summary>
        /// 생성일
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// 수정일
        /// </summary>
        public DateTimeOffset? UpdatedDate { get; set; }

        /// <summary>
        /// 삭제일
        /// </summary>
        public DateTimeOffset? DeletedDate { get; set; }
        #endregion

        #region Properties - Merged
        /// <summary>
        /// 대시보드 테이블 이름
        /// </summary>
        [Printable]
        public string FormName { get; set; }

        /// <summary>
        /// 파일 템플릿 정보
        /// </summary>
        public DataFileTemplate FileTemplate { get; set; }

        /// <summary>
        /// 파일 템플릿 정보가 있는지 여부
        /// </summary>
        [Printable]
        public bool HasFileTemplate => FileTemplate != null;

        /// <summary>
        /// 파일 소스 정보 (컨펌 여부 상관 없이 가장 마지막 업로드 된 파일)
        /// </summary>
        public DataFileSource FileSource { get; set; }

        /// <summary>
        /// "전일/전주/전월"에 업로드 된 파일 소스 정보
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DataFileSource PrevFileSource { get; set; }

        /// <summary>
        /// 파일 소스 ID
        /// </summary>
        public Guid? FileSourceId => FileSource?.FileSourceId;

        /// <summary>
        /// 파일 소스 정보가 있는지 여부
        /// </summary>
        [Printable]
        public bool HasFileSource => FileSource != null;

        /// <summary>
        /// 이 업무 영역에 데이터 업로드 가능한 업로더 리스트
        /// </summary>
        [JsonProperty("fileSourceUploaders")]
        public List<DataFileSourceUploader> FileSourceUploaderList => FileSourceUploaders.Values.ToList();

        /// <summary>
        /// 이 업무 영역에 데이터 업로드 가능한 업로더 리스트 (Key: UserId)
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, DataFileSourceUploader> FileSourceUploaders { get; } =
            new Dictionary<string, DataFileSourceUploader>(StringComparer.OrdinalIgnoreCase);
        #endregion
    }
}
