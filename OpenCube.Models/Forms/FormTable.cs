using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenCube.Models.Attributes;
using OpenCube.Models.Data;
using OpenCube.Models.Logging;

namespace OpenCube.Models.Forms
{
    /// <summary>
    /// 대시보드 테이블을 정의하는 모델 클래스 (대시보드 페이지 렌더링 시 사용됨)
    /// </summary>
    [JsonObject]
    public class FormTable : BaseModel
    {
        #region Constructors
        public FormTable(Guid formId)
        {
            formId.ThrowIfEmpty(nameof(formId));

            this.FormId = formId;
        }
        #endregion

        #region Methods
        public static FormTable ParseFrom(DataRow dr)
        {
            dr.ThrowIfNull(nameof(dr));

            Guid formId = dr.Get<Guid>("FormID");

            return new FormTable(formId)
            {
                HtmlTemplateId = dr.Get<Guid>("HtmlTemplateID"),
                Name = dr.Get<string>("Name"),
                Description = dr.Get<string>("Description"),
                UploadInterval = EnumExtension.ParseEnumMember<DataUploadInterval>(dr.Get<string>("UploadInterval")),
                UploadWeekOfMonth = dr.Get<int?>("UploadWeekOfMonth"),
                UploadDayOfWeek = EnumExtension.ParseEnumMember<DataUploadDayOfWeek>(dr.Get<string>("UploadDayOfWeek")),
                IsEnabled = dr.Get<bool>("IsEnabled"),
                IsDeleted = dr.Get<bool>("IsDeleted"),
                CreatorId = dr.Get<string>("CreatorId"),
                DeleterId = dr.Get<string>("DeleterId"),
                CreatedDate = dr.Get<DateTimeOffset>("CreatedDate"),
                UpdatedDate = dr.Get<DateTimeOffset?>("UpdatedDate"),
                DeletedDate = dr.Get<DateTimeOffset?>("DeletedDate"),
                LastConfirmedSourceDate = dr.Get<DateTimeOffset?>("LastConfirmedSourceDate"),
                // merged ------------------------------------------------------
                MenuId = dr.Get<Guid?>("MenuId"),
                SortOrder = dr.Get<int>("SortOrder")
            };
        }

        public static FormTable ParseFrom(DataRow dr, out int totalCount)
        {
            dr.ThrowIfNull(nameof(dr));

            totalCount = dr.Get<int>("TotalCount");

            return ParseFrom(dr);
        }

        public static FormTable CreateFrom(IFormTableUpdatable fields)
        {
            fields.ThrowIfNull(nameof(fields));

            var formId = Guid.NewGuid();
            var htmlTemplateId = Guid.NewGuid();
            var createdDate = DateTimeOffset.Now;

            return new FormTable(formId)
            {
                Name = fields.Name,
                Description = fields.Description,
                //UploadInterval = fields.UploadInterval,
                UploadWeekOfMonth = fields.UploadWeekOfMonth,
                UploadDayOfWeek = fields.UploadDayOfWeek,
                IsEnabled = fields.IsEnabled,
                HtmlTemplateId = htmlTemplateId,
                HtmlTemplate = new FormHtmlTemplate(formId, htmlTemplateId)
                {
                    FormName = fields.Name,
                    CreatedDate = createdDate
                },
                CreatedDate = createdDate
            };
        }

        public void Update(IFormTableUpdatable fields)
        {
            List<UpdatedField> updated = null;
            this.Update(fields, out updated);
        }

        public void Update(IFormTableUpdatable fields, out List<UpdatedField> updated)
        {
            fields.ThrowIfNull(nameof(fields));

            updated = new List<UpdatedField>();

            if (Name != fields.Name && fields.Name.IsNotNullOrWhiteSpace())
            {
                updated.Add(new UpdatedField
                {
                    FieldName = nameof(Name),
                    OldValue = Name,
                    NewValue = fields.Name
                });

                Name = fields.Name;
            }

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

            // NOTE(jhlee): 실적 업로드 주기가 변경 되면 데이터 조회/확인 취소 처리에 문제가 생기므로 변경 불가능하도록 수정함.
            //if (UploadInterval != fields.UploadInterval)
            //{
            //    updated.Add(new UpdatedField
            //    {
            //        FieldName = nameof(UploadInterval),
            //        OldValue = UploadInterval,
            //        NewValue = fields.UploadInterval
            //    });

            //    UploadInterval = fields.UploadInterval;
            //}

            if (UploadWeekOfMonth != fields.UploadWeekOfMonth)
            {
                updated.Add(new UpdatedField
                {
                    FieldName = nameof(UploadWeekOfMonth),
                    OldValue = UploadWeekOfMonth,
                    NewValue = fields.UploadWeekOfMonth
                });

                UploadWeekOfMonth = fields.UploadWeekOfMonth;
            }

            if (UploadDayOfWeek != fields.UploadDayOfWeek)
            {
                updated.Add(new UpdatedField
                {
                    FieldName = nameof(UploadDayOfWeek),
                    OldValue = UploadDayOfWeek,
                    NewValue = fields.UploadDayOfWeek
                });

                UploadDayOfWeek = fields.UploadDayOfWeek;
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

            if (updated.Any())
            {
                UpdatedDate = DateTimeOffset.Now;

                Validate();
            }
        }

        public override void Validate()
        {
            FormId.ThrowIfEmpty(nameof(FormId));
            Name.ThrowIfNullOrWhiteSpace(nameof(Name));
            SortOrder.ThrowIfOutOfRange(nameof(SortOrder), 0);
            CreatorId.ThrowIfNullOrWhiteSpace(nameof(CreatorId));
        }

        public FormTableSummary ToSummary()
        {
            return new FormTableSummary(this);
        }

        /// <summary>
        /// 업로드 주기를 기준으로 대상 날짜가 해당되는 범위로 반환한다.
        /// </summary>
        public DateRange GetDateRangeByUploadInterval(DateTimeOffset date)
        {
            var range = new DateRange();

            switch (UploadInterval)
            {
                case DataUploadInterval.Daily:
                    range.BeginDate = date.BeginOfDate();
                    range.EndDate = date.EndOfDate();
                    break;
                case DataUploadInterval.Weekly:
                    range = date.WeekRangeOfMonth(DayOfWeek.Wednesday);
                    break;
                case DataUploadInterval.Monthly:
                    range.BeginDate = date.BeginOfMonth();
                    range.EndDate = date.EndOfMonth();
                    break;
            }

            return range;
        }
        #endregion

        #region Properties - Table Mapped
        /// <summary>
        /// 대시보드 테이블 ID
        /// </summary>
        [Printable]
        public Guid FormId { get; }

        /// <summary>
        /// 대시보드 페이지 렌더링 시 사용되는 html 양식 ID
        /// </summary>
        [Printable]
        public Guid HtmlTemplateId { get; set; }

        /// <summary>
        /// 대시보드 이름 (ex: "Daily 상황판 - Q")
        /// </summary>
        [Printable]
        public string Name { get; set; }

        /// <summary>
        /// 설명 (페이지에 노출됨)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 실적 데이터 업로드 주기
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public DataUploadInterval UploadInterval { get; set; } = DataUploadInterval.Daily;

        /// <summary>
        /// 실적 데이터 업로드 가능한 주차 정보 (1주차, 2주차 등등. null이면 주차 상관 없이 업로드 가능)
        /// </summary>
        public int? UploadWeekOfMonth { get; set; }

        /// <summary>
        /// 실적 데이터 업로드 가능한 요일 정보
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public DataUploadDayOfWeek UploadDayOfWeek { get; set; } = DataUploadDayOfWeek.Monday;

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
        /// 수정한 날짜
        /// </summary>
        public DateTimeOffset? UpdatedDate { get; set; }

        /// <summary>
        /// 삭제한 날짜
        /// </summary>
        public DateTimeOffset? DeletedDate { get; set; }

        /// <summary>
        /// 마지막으로 컨펌 된 날짜
        /// </summary>
        [Printable]
        public DateTimeOffset? LastConfirmedSourceDate { get; set; }
        #endregion

        #region Properties - Merged
        /// <summary>
        /// 매핑된 메뉴 ID
        /// </summary>
        [Printable]
        public Guid? MenuId { get; set; }

        /// <summary>
        /// 대시보드 페이지 내 렌더링 순서 (하나의 페이지에 2개 이상의 대시보드 테이블이 있을 수 있음)
        /// </summary>
        [Printable]
        public int SortOrder { get; set; }

        /// <summary>
        /// 대시보드 HTML 양식 정보
        /// </summary>
        public FormHtmlTemplate HtmlTemplate { get; set; }

        /// <summary>
        /// 대시보드 HTML 양식 정보가 있는지 여부
        /// </summary>
        [Printable]
        public bool HasHtmlTemplate => HtmlTemplate != null;
        #endregion
    }
}
