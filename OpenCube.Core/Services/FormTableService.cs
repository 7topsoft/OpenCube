using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using NLog;
using OpenCube.Core.Repositories;
using OpenCube.Models;
using OpenCube.Models.Data;
using OpenCube.Models.Forms;
using OpenCube.Models.Logging;

namespace OpenCube.Core.Services
{
    /// <summary>
    /// 대시보드 테이블 서비스
    /// </summary>
    public class FormTableService : BaseService
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #region Constructors
        public FormTableService(IUserIdentity identity)
            : base(identity)
        { }
        #endregion

        #region Methods - FormTable
        /// <summary>
        /// 대시보드 테이블 정보를 반환한다,
        /// </summary>
        public FormTable GetFormTable(Guid formId)
        {
            using (var repo = new FormTableRepository())
            {
                return repo.SelectFormTable(formId);
            }
        }

        /// <summary>
        /// 대시보드 테이블 리스트를 반환한다,
        /// </summary>
        public FormTable[] GetFormTableList()
        {
            using (var repo = new FormTableRepository())
            {
                string queryUserId = null;
                if (!CurrentUser.HasPermission(UserPermission.DataReview))
                {
                    queryUserId = CurrentUser.UserId;
                }

                return repo.SelectFormTableList(queryUserId)
                    .ToArray();
            }
        }

        /// <summary>
        /// 페이징 처리 된 대시보드 테이블 리스트를 반환한다,
        /// </summary>
        public PagedModel<FormTable> GetFormTablePagedList(PagingOption option)
        {
            option.ThrowIfNull(nameof(option));
            option.Validate();

            using (var repo = new FormTableRepository())
            {
                return repo.SelectFormTablePagedList(option);
            }
        }

        /// <summary>
        /// 대시보드 테이블을 추가한다.
        /// </summary>
        public bool AddFormTable(FormTable formTable)
        {
            using (var repo = new FormTableRepository())
            {
                if (!formTable.HasHtmlTemplate)
                {
                    throw new ArgumentNullException($"대시보드에 해당하는 HTML양식을 찾을 수 없습니다. 대시보드: {formTable.Name}"
                        + $"\r\n"
                        + $"대시보드 ID: {formTable.FormId}");
                }

                repo.BeginTransaction();

                try
                {
                    if (repo.InsertFormTable(formTable))
                    {
                        if (repo.InsertFormHtmlTemplate(formTable.HtmlTemplate))
                        {
                            repo.CommitTransaction();

                            logger.Info($"새 대시보드를 추가하였습니다. 대시보드: \"{formTable.Name}\""
                                + $"\r\n\r\n"
                                + $"{formTable}");

                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"대시보드 추가 중 알 수 없는 오류가 발생하였습니다. 대시보드: \"{formTable.Name}\""
                        + $"\r\n\r\n"
                        + $"{formTable}");

                    try
                    {
                        repo.RollBackTransaction();
                    }
                    catch (Exception rex)
                    {
                        logger.Fatal(ex, $"대시보드 추가 함수에서 롤백 실행중 치명적인 에러가 발생했습니다. 대시보드: \"{formTable.Name}\""
                            + $"\r\n\r\n"
                            + $"{formTable}");

                        ExceptionDispatchInfo.Capture(rex).Throw();
                        // not reached
                    }

                    ExceptionDispatchInfo.Capture(ex).Throw();
                    return false; // not reached
                }

                return false;
            }
        }

        /// <summary>
        /// 대시보드 테이블을 업데이트한다.
        /// </summary>
        public FormTable UpdateFormTable(Guid formId, IFormTableUpdatable fields)
        {
            using (var repo = new FormTableRepository())
            {
                var formTable = repo.SelectFormTable(formId);
                if (formTable == null)
                {
                    throw new ObjectNotFoundException($"업데이트 할 대상 대시보드를 찾을 수 없습니다.\r\n대시보드 ID: \"{formId}\"");
                }

                List<UpdatedField> updated = null;
                formTable.Update(fields, out updated);

                if (repo.UpdateFormTable(formTable))
                {
                    logger.Info($"대시보드 정보가 업데이트 되었습니다. 대시보드: \"{formTable.Name}\""
                        + $"\r\n\r\n"
                        + $"updated: {UpdatedField.Print(updated)}"
                        + $"\r\n\r\n"
                        + $"{formTable}");

                    return formTable;
                }

                return null;
            }
        }

        /// <summary>
        /// 대시보드 테이블을 삭제한다.
        /// </summary>
        public bool DeleteFormTable(Guid formId)
        {
            using (var repo = new FormTableRepository())
            {
                var formTable = repo.SelectFormTable(formId);
                if (formTable == null)
                {
                    throw new ObjectNotFoundException($"삭제 할 대상 대시보드를 찾을 수 없습니다.\r\n대시보드 ID: \"{formId}\"");
                }

                if (repo.DeleteFormTable(formId, CurrentUser.UserId, DateTimeOffset.Now))
                {
                    logger.Info($"대시보드를 삭제하였습니다. 대시보드: \"{formTable.Name}\""
                        + $"\r\n\r\n"
                        + $"{formTable}");

                    return true;
                }

                return false;
            }
        }
        #endregion

        #region Methods - FormTableSection
        /// <summary>
        /// 대시보드 테이블 업무 영역을 반환한다.
        /// </summary>
        public FormTableSection GetFormTableSection(Guid formId, Guid formSectionId)
        {
            using (var repo = new FormTableRepository())
            {
                return repo.SelectFormTableSection(formId, formSectionId);
            }
        }

        /// <summary>
        /// 대시보드 테이블 업무 영역을 반환한다.
        /// </summary>
        /// <param name="date">값이 있는 경우, 해당 날짜의 데이터 파일 소스 정보를 반환</param>
        public FormTableSection[] GetFormTableSectionList(Guid? formId = null, DateTimeOffset? date = null)
        {
            using (var repo = new FormTableRepository())
            {
                string queryUserId = null;
                if (!CurrentUser.HasPermission(UserPermission.DataReview))
                {
                    queryUserId = CurrentUser.UserId;
                }

                List<DataDateRange> dateRangeList = null;

                if (date != null)
                {
                    dateRangeList = GetDateRangeByUploadInterval(date.Value, true).ToList();
                }

                return repo.SelectFormTableSectionList(formId, queryUserId, dateRangeList).ToArray();
            }
        }

        /// <summary>
        /// 페이징 처리 된 대시보드 테이블 업무 영역 리스트를 반환한다.
        /// </summary>
        public PagedModel<FormTableSection> GetFormTableSectionPagedList(PagingOption option)
        {
            option.ThrowIfNull(nameof(option));
            option.Validate();

            using (var repo = new FormTableRepository())
            {
                return repo.SelectFormTableSectionPagedList(option);
            }
        }

        /// <summary>
        /// 대시보드 테이블 업무 영역을 추가한다.
        /// </summary>
        public bool AddFormTableSection(FormTableSection formSection, IEnumerable<string> fileSourceUploaders)
        {
            using (var repo = new FormTableRepository())
            {
                repo.BeginTransaction();

                try
                {
                    if (repo.InsertFormTableSection(formSection))
                    {
                        repo.InsertFormTableSectionGroupMembers(formSection.FormId, formSection.FormSectionId, fileSourceUploaders, formSection.CreatedDate);

                        if (formSection.FileTemplate != null)
                        {
                            repo.InsertDateFileTemplate(formSection.FileTemplate);
                        }
                        else
                        {
                            logger.Warn($"업무영역 정보로부터 파일 템플릿 정보를 찾을 수 없습니다. 업무영역: \"{formSection.FormSectionName}\""
                                + $"\r\n\r\n"
                                + $"{formSection}");
                        }

                        repo.CommitTransaction();

                        logger.Info($"새 업무영역을 추가하였습니다. 업무영역: \"{formSection.FormSectionName}\""
                            + $"\r\n\r\n"
                            + $"{formSection}");

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"업무영역 추가 중 알 수 없는 오류가 발생하였습니다. 업무영역: \"{formSection.FormName}/{formSection.FormSectionName}\""
                        + $"\r\n\r\n"
                        + $"{formSection}");

                    try
                    {
                        repo.RollBackTransaction();
                    }
                    catch (Exception rex)
                    {
                        logger.Fatal(ex, $"업무영역 추가 함수에서 롤백 실행중 치명적인 에러가 발생했습니다. 업무영역: \"{formSection.FormName}/{formSection.FormSectionName}\""
                            + $"\r\n\r\n"
                            + $"{formSection}");

                        ExceptionDispatchInfo.Capture(rex).Throw();
                        // not reached
                    }

                    ExceptionDispatchInfo.Capture(ex).Throw();
                    return false; // not reached
                }

                return false;
            }
        }

        /// <summary>
        /// 대시보드 테이블 업무 영역을 업데이트한다.
        /// </summary>
        public FormTableSection UpdateFormTableSection(FormTableSection formSection, IEnumerable<string> fileSourceUploaders)
        {
            formSection.ThrowIfNull(nameof(formSection));

            using (var repo = new FormTableRepository())
            {
                var old = repo.SelectFormTableSection(formSection.FormId, formSection.FormSectionId);
                if (old == null)
                {
                    throw new ObjectNotFoundException($"업데이트될 업무영역을 찾을 수 없습니다. 업무영역: \"{formSection.FormSectionName}\""
                        + $"\r\n"
                        + $"업무영역 ID: {formSection.FileSourceId}");
                }

                repo.BeginTransaction();

                try
                {
                    var diff = old.Diff(formSection);

                    if (repo.UpdateFormTableSection(formSection, CurrentUser.UserId))
                    {
                        repo.InsertFormTableSectionGroupMembers(formSection.FormId, formSection.FormSectionId, fileSourceUploaders, DateTimeOffset.Now);

                        if (old.FileTemplateId != formSection.FileTemplateId)
                        {
                            repo.InsertDateFileTemplate(formSection.FileTemplate);
                        }
                        else
                        {
                            repo.UpdateDateFileTemplate(formSection.FileTemplate);
                        }

                        repo.CommitTransaction();

                        logger.Info($"업무영역이 업데이트 되었습니다. 업무영역: \"{formSection.FormSectionName}\""
                            + $"\r\n\r\n"
                            + $"updated: {UpdatedField.Print(diff)}"
                            + $"\r\n\r\n"
                            + $"{formSection}");

                        return formSection;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"업무영역 업데이트 중 확인 중 알 수 없는 오류가 발생하였습니다. 업무영역: \"{formSection.FormName}/{formSection.FormSectionName}\""
                        + $"\r\n\r\n"
                        + $"{formSection}");

                    try
                    {
                        repo.RollBackTransaction();
                    }
                    catch (Exception rex)
                    {
                        logger.Fatal(ex, $"업무영역 업데이트 함수에서 롤백 실행중 치명적인 에러가 발생했습니다. 업무영역: \"{formSection.FormName}/{formSection.FormSectionName}\""
                            + $"\r\n\r\n"
                            + $"{formSection}");

                        ExceptionDispatchInfo.Capture(rex).Throw();
                        // not reached
                    }

                    ExceptionDispatchInfo.Capture(ex).Throw();
                    return null; // not reached
                }

                return null;
            }
        }

        /// <summary>
        /// 대시보드 테이블 업무 영역을 업데이트한다. (필드만 업데이트한다.)
        /// </summary>
        public FormTableSection UpdateFormTableSectionFields(Guid formId, Guid formSectionId, IFormTableSectionUpdatable fields)
        {
            using (var repo = new FormTableRepository())
            {
                var formSection = repo.SelectFormTableSection(formId, formSectionId);
                if (formSection == null)
                {
                    throw new ObjectNotFoundException($"업데이트 할 대상 업무 영역을 찾을 수 없습니다.\r\n업무 영역 ID: \"{formSectionId}\"");
                }

                List<UpdatedField> updated = null;
                formSection.Update(fields, out updated);

                repo.BeginTransaction();

                try
                {
                    if (repo.UpdateFormTableSection(formSection, CurrentUser.UserId))
                    {
                        repo.InsertFormTableSectionGroupMembers(formId, formSectionId, fields.FileSourceUploaders, DateTimeOffset.Now);
                        repo.UpdateDateFileTemplate(formSection.FileTemplate);
                        repo.CommitTransaction();

                        logger.Info($"업무영이 업데이트 되었습니다. 업무영역: \"{formSection.FormSectionName}\""
                            + $"\r\n\r\n"
                            + $"updated: {UpdatedField.Print(updated)}"
                            + $"\r\n\r\n"
                            + $"{formSection}");

                        return formSection;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"업무영역 필드 업데이트 중 알 수 없는 오류가 발생하였습니다.  업무영역: '{formSection.FormName}/{formSection.FormSectionName}'"
                        + $"\r\n\r\n"
                        + $"{formSection}");

                    try
                    {
                        repo.RollBackTransaction();
                    }
                    catch (Exception rex)
                    {
                        logger.Fatal(ex, $"업무영역 필드 업데이트 함수에서 롤백 실행중 치명적인 에러가 발생했습니다.  업무영역: '{formSection.FormName}/{formSection.FormSectionName}'"
                            + $"\r\n\r\n"
                            + $"{formSection}");

                        ExceptionDispatchInfo.Capture(rex).Throw();
                        // not reached
                    }

                    ExceptionDispatchInfo.Capture(ex).Throw();
                    return null; // not reached
                }

                return null;
            }
        }

        /// <summary>
        /// 대시보드 테이블 업무 영역을 삭제한다.
        /// </summary>
        public bool DeleteFormTableSection(Guid formId, Guid formSectionId)
        {
            using (var repo = new FormTableRepository())
            {
                var formSection = repo.SelectFormTableSection(formId, formSectionId);
                if (formSection == null)
                {
                    throw new ObjectNotFoundException($"삭제 할 대상 업무 영역을 찾을 수 없습니다.\r\n업무영역 ID: \"{formSectionId}\"");
                }

                if (repo.DeleteFormTableSection(formId, formSectionId, CurrentUser.UserId, DateTimeOffset.Now))
                {
                    logger.Info($"업무영역을 삭제하였습니다. 업무영역: {formSection.FormSectionName}"
                        + $"\r\n\r\n"
                        + $"{formSection}");

                    return true;
                }

                return false;
            }
        }
        #endregion

        #region Methods - FormHtmlTemplate
        /// <summary>
        /// 대시보드 테이블 html 양식 정보를 반환한다.
        /// </summary>
        public FormHtmlTemplate GetFormHtmlTemplate(Guid formId, Guid templateId)
        {
            using (var repo = new FormTableRepository())
            {
                return repo.SelectFormHtmlTemplate(formId, templateId);
            }
        }

        /// <summary>
        /// 대시보드 테이블 html 양식 리스트를 반환한다.
        /// </summary>
        public FormHtmlTemplate[] GetFormHtmlTemplateList()
        {
            using (var repo = new FormTableRepository())
            {
                return repo.SelectFormHtmlTemplateList()
                    .ToArray();
            }
        }

        /// <summary>
        /// 페이징 처리 된 대시보드 테이블 html 양식 리스트를 반환한다.
        /// </summary>
        public PagedModel<FormHtmlTemplate> GetFormHtmlTemplatePagedList(PagingOption option)
        {
            option.ThrowIfNull(nameof(option));
            option.Validate();

            using (var repo = new FormTableRepository())
            {
                return repo.SelectFormHtmlTemplatePagedList(option);
            }
        }

        /// <summary>
        /// 대시보드 테이블 html 양식을 추가한다.
        /// </summary>
        public bool AddFormHtmlTemplate(FormHtmlTemplate template)
        {
            using (var repo = new FormTableRepository())
            {
                if (repo.InsertFormHtmlTemplate(template))
                {
                    logger.Info($"새 대시보드 테이블 양식을 추가하였습니다. 대시보드: \"{template.FormName}\""
                        + $"\r\n\r\n"
                        + $"{template}");

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 대시보드 테이블 html 양식을 업데이트한다.
        /// </summary>
        public FormHtmlTemplate UpdateFormHtmlTemplate(Guid formId, Guid templateId, IFormHtmlTemplateUpdatable fields)
        {
            using (var repo = new FormTableRepository())
            {
                var template = repo.SelectFormHtmlTemplate(formId, templateId);
                if (template == null)
                {
                    throw new ObjectNotFoundException($"업데이트 할 대상 대시보드 양식을 찾을 수 없습니다.\r\n양식 ID: \"{templateId}\"");
                }

                List<UpdatedField> updated = null;
                template.Update(fields, out updated);

                if (repo.UpdateFormHtmlTemplate(template))
                {
                    logger.Info($"대시보드 양식이 업데이트 되었습니다. 대시보드: \"{template.FormName}\""
                        + $"\r\n\r\n"
                        + $"updated: {UpdatedField.Print(updated)}"
                        + $"\r\n\r\n"
                        + $"{template}");

                    return template;
                }

                return null;
            }
        }

        /// <summary>
        /// 대상 대시보드의 양식 HTML을 사본처리한다.
        /// </summary>
        public FormHtmlTemplate CopyFormHtmlTemplate(Guid formId, Guid templateId)
        {
            var newTemplateId = Guid.NewGuid();

            using (var repo = new FormTableRepository())
            {
                var form = repo.SelectFormTable(formId);
                if (form == null || form.IsDeleted)
                {
                    throw new ObjectNotFoundException($"사본 처리 할 대시보드를 찾을 수 없습니다.\r\n대시보드 ID: \"{formId}\"");
                }

                var old = repo.SelectFormHtmlTemplate(formId, templateId);
                if (old == null)
                {
                    throw new ObjectNotFoundException($"사본 처리 할 대시보드 양식을 찾을 수 없습니다.\r\n양식 ID: \"{templateId}\"");
                }

                repo.BeginTransaction();

                try
                {
                    // 1. 새 HTML 사본 레코드를 DB에 추가
                    var htmlTemplate = FormHtmlTemplate.CopyFrom(old, newTemplateId);
                    if (repo.InsertFormHtmlTemplate(htmlTemplate))
                    {
                        // 2. 대시보드가 참조하고 있는 HTML 양식 ID를 업데이트
                        form.HtmlTemplateId = newTemplateId;
                        form.HtmlTemplate = htmlTemplate;
                        repo.UpdateFormTable(form);

                        repo.CommitTransaction();

                        logger.Info($"대시보드 양식이 사본 처리되었습니다. 대시보드: \"{old.FormName}\""
                            + $"\r\n새 양식 ID: \"{newTemplateId}\", 사본 처리된 양식 ID: \"{templateId}\""
                            + $"\r\n\r\n"
                            + $"새 양식 정보: \r\n"
                            + $"{htmlTemplate}");

                        return htmlTemplate;
                    }

                    throw new InvalidOperationException($"대상 대시보드의 새 양식을 DB에 등록하지 못했습니다.\r\n대시보드: \"{form.Name}({formId})\""
                        + $"\r\n새 양식 ID: \"{newTemplateId}\", 사본 처리된 양식 ID: \"{templateId}\"");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"대시보드 양식을 사본 처리하는 도중 알 수 없는 오류가 발생하였습니다. 대시보드: \"{form.Name}({formId})\"");

                    try
                    {
                        repo.RollBackTransaction();
                    }
                    catch (Exception rex)
                    {
                        logger.Fatal(ex, $"대시보드 양식을 사본 처리하는 함수에서 롤백 실행 중 치명적인 에러가 발생했습니다. 대시보드: \"{form.Name}({formId})\"");

                        ExceptionDispatchInfo.Capture(rex).Throw();
                        // not reached
                    }

                    ExceptionDispatchInfo.Capture(ex).Throw();
                    return null; // not reached
                }
            }
        }
        #endregion

        #region Methods - Helper
        /// <summary>
        /// |date|가 대상 대시보드의 실적 업로드 기간에 해당되는지 여부를 반환한다.
        /// </summary>
        public static bool IsInUploadInterval(FormTable form, DateTimeOffset date)
        {
            bool result = true;

            switch (form.UploadInterval)
            {
                case DataUploadInterval.Daily:
                    break;
                case DataUploadInterval.Weekly:

                    // 업로드 요일을 비교한다.
                    if (form.UploadDayOfWeek != DataUploadDayOfWeek.All)
                    {
                        result = (DayOfWeek)form.UploadDayOfWeek == date.DayOfWeek;
                    }
                    break;
                case DataUploadInterval.Monthly:

                    // 업로드 주차를 비교한다.
                    result = date.WeekRangeOfMonth(DayOfWeek.Wednesday).Week == form.UploadWeekOfMonth;

                    if (result && (form.UploadDayOfWeek != DataUploadDayOfWeek.All))
                    {
                        result = (DayOfWeek)form.UploadDayOfWeek == date.DayOfWeek;
                    }
                    break;
            }

            return result;
        }

        /// <summary>
        /// 대상 날짜 기준으로 월, 주, 일에 해당되는 날짜 범위를 반환한다.
        /// </summary>
        /// <param name="withPrevInterval">직전 업로드 주기에 해당하는 날짜도 반환할 것인지 여부</param>
        public static IEnumerable<DataDateRange> GetDateRangeByUploadInterval(DateTimeOffset date, bool withPrevInterval)
        {
            var rangeList = new List<DataDateRange>();

            var addDateRanges = new Action<DateTimeOffset, DataUploadInterval, bool>((d, interval, isCurrentData) =>
            {
                var tempTerm = new DateRange();

                switch (interval)
                {
                    case DataUploadInterval.Daily:
                        tempTerm.BeginDate = d.BeginOfDate();
                        tempTerm.EndDate = d.EndOfDate();
                        break;
                    case DataUploadInterval.Weekly:
                        tempTerm = d.WeekRangeOfMonth(DayOfWeek.Wednesday);
                        break;
                    case DataUploadInterval.Monthly:
                        tempTerm.BeginDate = d.BeginOfMonth();
                        tempTerm.EndDate = d.EndOfMonth();
                        break;
                }

                rangeList.Add(new DataDateRange(tempTerm)
                {
                    SourceDate = date,
                    UploadInterval = interval,
                    IsCurrentData = isCurrentData
                });
            });

            foreach (var interval in (DataUploadInterval[])Enum.GetValues(typeof(DataUploadInterval)))
            {
                if (withPrevInterval)
                {
                    DateTimeOffset temp = date;

                    switch (interval)
                    {
                        case DataUploadInterval.Daily:
                            temp = date.AddDays(-1);
                            break;
                        case DataUploadInterval.Weekly:
                            temp = date.AddDays(-7);
                            break;
                        case DataUploadInterval.Monthly:
                            temp = date.AddMonths(-1);
                            break;
                    }

                    addDateRanges(temp, interval, false);
                }

                addDateRanges(date, interval, true);
            }

            return rangeList;
        }
        #endregion
    }
}
