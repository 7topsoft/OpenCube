using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenCube.Models.Attributes;
using OpenCube.Models.Logging;

namespace OpenCube.Models.Forms
{
    /// <summary>
    /// 대시보드 테이블과 매핑되는 HTML 양식 정보
    /// </summary>
    [JsonObject]
    public class FormHtmlTemplate : BaseModel
    {
        #region Constructors
        public FormHtmlTemplate(Guid formId, Guid templateId)
        {
            formId.ThrowIfEmpty(nameof(formId));
            templateId.ThrowIfEmpty(nameof(templateId));

            FormId = formId;
            HtmlTemplateId = templateId;
        }
        #endregion

        #region Methods
        public static FormHtmlTemplate ParseFrom(DataRow dr)
        {
            dr.ThrowIfNull(nameof(dr));

            Guid formId = dr.Get<Guid>("FormID");
            Guid templateId = dr.Get<Guid>("HtmlTemplateID");

            return new FormHtmlTemplate(formId, templateId)
            {
                CreatorId = dr.Get<string>("CreatorId"),
                Description = dr.Get<string>("Description"),
                ScriptContent = dr.Get<string>("ScriptContent"),
                HtmlContent = dr.Get<string>("HtmlContent"),
                StyleContent = dr.Get<string>("StyleContent"),
                CreatedDate = dr.Get<DateTimeOffset>("CreatedDate"),
                UpdatedDate = dr.Get<DateTimeOffset?>("UpdatedDate"),
                // merged -----------------------------------------
                FormName = dr.Get<string>("FormName")
            };
        }

        public static FormHtmlTemplate ParseFrom(DataRow dr, out int totalCount)
        {
            dr.ThrowIfNull(nameof(dr));

            totalCount = dr.Get<int>("TotalCount");

            return ParseFrom(dr);
        }

        public static FormHtmlTemplate CreateFrom(IFormHtmlTemplateUpdatable fields)
        {
            fields.ThrowIfNull(nameof(fields));

            return new FormHtmlTemplate(Guid.NewGuid(), Guid.NewGuid())
            {
                Description = fields.Description,
                ScriptContent = fields.ScriptContent,
                HtmlContent = fields.HtmlContent,
                StyleContent = fields.StyleContent,
                CreatedDate = DateTimeOffset.Now
            };
        }

        /// <summary>
        /// 새 양식 ID가 부여된 사본을 만들어서 반환한다.
        /// </summary>
        public static FormHtmlTemplate CopyFrom(FormHtmlTemplate other, Guid? htmlTemplateId)
        {
            other.ThrowIfNull(nameof(other));

            return new FormHtmlTemplate(other.FormId, htmlTemplateId.HasValue ? htmlTemplateId.Value : Guid.NewGuid())
            {
                CreatorId = other.CreatorId,
                Description = other.Description,
                ScriptContent = other.ScriptContent,
                HtmlContent = other.HtmlContent,
                StyleContent = other.StyleContent,
                CreatedDate = DateTimeOffset.Now,
                UpdatedDate = null,
                // merged ----------------------------------
                FormName = other.FormName,
            };
        }

        public void Update(IFormHtmlTemplateUpdatable fields)
        {
            List<UpdatedField> updated = null;
            this.Update(fields, out updated);
        }

        public void Update(IFormHtmlTemplateUpdatable fields, out List<UpdatedField> updated)
        {
            fields.ThrowIfNull(nameof(fields));

            updated = new List<UpdatedField>();

            if (Description != fields.Description)
            {
                updated.Add(new UpdatedField
                {
                    FieldName = nameof(Description),
                    OldValue = Description,
                    NewValue = fields.Description
                });

                Description = fields.Description;
            }

            if (ScriptContent != fields.ScriptContent)
            {
                updated.Add(new UpdatedField
                {
                    FieldName = nameof(ScriptContent),
                    OldValue = ScriptContent,
                    NewValue = fields.ScriptContent
                });

                ScriptContent = fields.ScriptContent;
            }

            if (HtmlContent != fields.HtmlContent)
            {
                updated.Add(new UpdatedField
                {
                    FieldName = nameof(HtmlContent),
                    OldValue = HtmlContent,
                    NewValue = fields.HtmlContent
                });

                HtmlContent = fields.HtmlContent;
            }

            if (StyleContent != fields.StyleContent)
            {
                updated.Add(new UpdatedField
                {
                    FieldName = nameof(StyleContent),
                    OldValue = StyleContent,
                    NewValue = fields.StyleContent
                });

                StyleContent = fields.StyleContent;
            }

            if (updated.Any())
            {
                UpdatedDate = DateTimeOffset.Now;

                Validate();
            }
        }

        public override void Validate()
        {
            FormId.ThrowIfEmpty(nameof(FormId));
            HtmlTemplateId.ThrowIfEmpty(nameof(HtmlTemplateId));
            CreatorId.ThrowIfNullOrWhiteSpace(nameof(CreatorId));
        }
        #endregion

        #region Properties - Table Mapped
        /// <summary>
        /// 대시보드 테이블 ID
        /// </summary>
        [Printable]
        public Guid FormId { get; }

        /// <summary>
        /// 템플릿 ID
        /// </summary>
        [Printable]
        public Guid HtmlTemplateId { get; }

        /// <summary>
        /// 만든 유저 ID
        /// </summary>
        [Printable]
        public string CreatorId { get; set; }

        /// <summary>
        /// 템플릿 설명
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 자바스크립트 내용
        /// </summary>
        public string ScriptContent { get; set; }

        /// <summary>
        /// 템플릿 내용(HTML 본문)
        /// </summary>
        public string HtmlContent { get; set; }

        /// <summary>
        /// 스타일 내용(css)
        /// </summary>
        public string StyleContent { get; set; }

        /// <summary>
        /// 생성일
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// 업데이트일
        /// </summary>
        public DateTimeOffset? UpdatedDate { get; set; }
        #endregion

        #region Properties - Merged
        /// <summary>
        /// 대시보드 테이블 이름
        /// </summary>
        [Printable]
        public string FormName { get; set; }
        #endregion
    }
}
