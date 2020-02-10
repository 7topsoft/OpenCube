using System;
using System.Collections.Generic;
using System.Configuration.Abstractions;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using NLog;
using OpenCube.Core.Repositories;
using OpenCube.DataParsers;
using OpenCube.Models;
using OpenCube.Models.Data;
using OpenCube.Models.Forms;
using OpenCube.Utilities.IO;

namespace OpenCube.Core.Services
{
    /// <summary>
    /// 대시보드 데이터 추가/조회 서비스
    /// </summary>
    public class DashboardService : BaseService
    {
        #region Fields
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region Constructors
        public DashboardService(IUserIdentity identity) : base(identity)
        { }
        #endregion

        #region Methods - FormTable
        /// <summary>
        /// 해당 날짜의 모든 실적 데이터를 컨펌 처리한다.
        /// </summary>
        public void ConfirmAllFormTableData(DateTimeOffset date)
        {
            using (var formService = new FormTableService(CurrentUser))
            {
                var forms = formService.GetFormTableList()
                    .Where(o =>
                    {
                        if (!o.IsEnabled)
                        {
                            return false;
                        }

                        // NOTE(jhlee): 체크 하지 않음 (데이터가 늦게 업로드 되었을 수도 있으므로)
                        //return FormTableService.IsInUploadInterval(o, date);
                        return true;
                    })
                    .ToList();

                if (!forms.Any())
                {
                    logger.Warn($"데이터 확인 처리할 대시보드가 없습니다.");

                    return;
                }

                using (var repo = new DashboardRepository())
                {
                    repo.BeginTransaction();

                    FormTable targetForm = null;

                    try
                    {
                        foreach (var form in forms)
                        {
                            targetForm = form; // for logging
                            ConfirmFormTableDataInternal(repo, form, date);
                        }

                        repo.CommitTransaction();
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"데이터 확인 중 알 수 없는 오류가 발생하였습니다. 대시보드: \"{targetForm?.Name}\""
                            + $"\r\n\r\n"
                            + $"{targetForm}");

                        try
                        {
                            repo.RollBackTransaction();
                        }
                        catch (Exception rex)
                        {
                            logger.Fatal(ex, $"데이터 확인 함수에서 롤백 실행중 치명적인 에러가 발생했습니다. 대시보드: \"{targetForm?.Name}\""
                                + $"\r\n\r\n"
                                + $"{targetForm}");

                            ExceptionDispatchInfo.Capture(rex).Throw();
                            // not reached
                        }

                        ExceptionDispatchInfo.Capture(ex).Throw();
                    }
                }
            }
        }

        /// <summary>
        /// 해당 날짜의 실적 데이터를 컨펌 처리한다.
        /// </summary>
        public void ConfirmFormTableData(Guid formId, DateTimeOffset date)
        {
            using (var formService = new FormTableService(CurrentUser))
            {
                var form = formService.GetFormTable(formId);
                if (form == null)
                {
                    throw new ObjectNotFoundException($"컨펌 처리할 대시보드를 찾을 수 없습니다.\r\n대시보드 ID: \"{formId}\"");
                }

                using (var repo = new DashboardRepository())
                {
                    repo.BeginTransaction();

                    try
                    {
                        ConfirmFormTableDataInternal(repo, form, date);

                        repo.CommitTransaction();
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"데이터 확인 중 알 수 없는 오류가 발생하였습니다. 대시보드: \"{form.Name}\""
                            + $"\r\n\r\n"
                            + $"{form}");

                        try
                        {
                            repo.RollBackTransaction();
                        }
                        catch (Exception rex)
                        {
                            logger.Fatal(ex, $"데이터 확인 함수에서 롤백 실행중 치명적인 에러가 발생했습니다. 대시보드: \"{form.Name}\""
                                + $"\r\n\r\n"
                                + $"{form}");

                            ExceptionDispatchInfo.Capture(rex).Throw();
                            // not reached
                        }

                        ExceptionDispatchInfo.Capture(ex).Throw();
                    }
                }
            }
        }

        /// <summary>
        /// 대시보드의 특정 날짜 데이터 파일을 모두 확인 처리 한다.
        /// </summary>
        private bool ConfirmFormTableDataInternal(DashboardRepository repo, FormTable form, DateTimeOffset sourceDate)
        {
            // 날짜 범위에 해당하는 데이터를 모두 조회하기 위해 날짜 계산
            var dateRange = form.GetDateRangeByUploadInterval(sourceDate);

            var list = repo.SelectDataFileSourceListBySourceDateRange(form.FormId, dateRange.BeginDate, dateRange.EndDate)
                .Where(o => !o.IsConfirmed)
                .ToList();

            if (list.Any())
            {
                Guid htmlTemplateId = form.HtmlTemplateId;

                // 1. 대시보드 양식 html 사본을 생성한다.
                if (AppSettings.AppSetting<bool>("FormHtmlTemplate.CopyIfConfirmed"))
                {
                    htmlTemplateId = Guid.NewGuid();

                    using (var formRepo = new FormTableRepository())
                    {
                        var htmlTemplate = FormHtmlTemplate.CopyFrom(form.HtmlTemplate, htmlTemplateId);
                        formRepo.InsertFormHtmlTemplate(htmlTemplate);

                        logger.Info($"대상 대시보드의 양식이 사본 처리되었습니다. 대시보드: \"{form.Name}\""
                            + $"\r\n\r\n"
                            + $"대시보드 정보:"
                            + $"{form}"
                            + $"\r\n\r\n"
                            + $"이전 양식 정보:"
                            + $"{form.HtmlTemplate}"
                            + $"\r\n\r\n"
                            + $"신규 양식 정보:"
                            + $"{htmlTemplate}");
                    }
                }

                // 2. 데이터 파일 소스 컨펌 처리
                foreach (var fileSource in list)
                {
                    repo.UpdateDataFileSourceAsConfirmed(form.FormId, fileSource.FileSourceId, htmlTemplateId, CurrentUser.UserId, DateTimeOffset.Now, fileSource.SourceDate);
                }

                logger.Info($"대상 대시보드의 실적 데이터 파일들을 확인 처리했습니다. 대시보드: \"{form.Name}\", 실적 기준일: \"{sourceDate.ToString("yyyy-MM-dd")}\""
                    + $"\r\n\r\n"
                    + $"대상 데이터 파일({list.Count}개): [\r\n"
                    + string.Join("\r\n", list.Select(o => "\t" + o.ToString()))
                    + $"\r\n"
                    + $"]");

                return true;
            }

            return false;
        }

        /// <summary>
        /// 처리된 실적 데이터를 취소 처리한다.
        /// </summary>
        public void CancelConfirmFormTableData(Guid formId, DateTimeOffset date)
        {
            using (var formService = new FormTableService(CurrentUser))
            {
                var form = formService.GetFormTable(formId);
                if (form == null)
                {
                    throw new ObjectNotFoundException($"데이터 확인을 취소할 대시보드를 찾을 수 없습니다.\r\n대시보드 ID: \"{formId}\"");
                }

                using (var repo = new DashboardRepository())
                {
                    var dateRange = form.GetDateRangeByUploadInterval(date);

                    var list = repo.SelectDataFileSourceListBySourceDateRange(formId, dateRange.BeginDate, dateRange.EndDate)
                        .ToList();

                    if (!list.Any())
                    {
                        logger.Warn($"기간 내 대시보드에 업로드된 데이터 파일이 없습니다. 대시보드: \"{form.Name}\", 실적 기준일: \"{date.ToString("yyyy-MM-dd")}\""
                            + $"\r\n\r\n"
                            + $"{form}");

                        return;
                    }

                    try
                    {
                        foreach (var fileSource in list)
                        {
                            repo.UpdateDataFileSourceAsCanceled(formId, fileSource.FileSourceId);
                        }

                        logger.Info($"대상 대시보드의 실적 데이터 파일들 확인 취소 하였습니다. 대시보드: \"{form.Name}\", 실적 기준일: \"{date.ToString("yyyy-MM-dd")}\""
                            + $"\r\n\r\n"
                            + $"대상 데이터 파일({list.Count}개): [\r\n"
                            + string.Join("\r\n", list.Select(o => "\t" + o.ToString()))
                            + $"\r\n"
                            + $"]");

                        repo.CommitTransaction();
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"데이터 확인 취소 중 알 수 없는 오류가 발생하였습니다. 대시보드: \"{form.Name}\""
                            + $"\r\n\r\n"
                            + $"{form}");

                        try
                        {
                            repo.RollBackTransaction();
                        }
                        catch (Exception rex)
                        {
                            logger.Fatal(ex, $"데이터 확인 취소 함수에서 롤백 실행중 치명적인 에러가 발생했습니다. 대시보드: \"{form.Name}\""
                                + $"\r\n\r\n"
                                + $"{form}");

                            ExceptionDispatchInfo.Capture(rex).Throw();
                            // not reached
                        }

                        ExceptionDispatchInfo.Capture(ex).Throw();
                    }
                }
            }
        }

        /// <summary>
        /// 해당 날짜가 포함된 대상 대시보드 테이블의 데이터를 반환한다.
        /// </summary>
        public FormTableData GetFormTableData(Guid formId, DateTimeOffset date)
        {
            formId.ThrowIfEmpty(nameof(formId));

            using (var formService = new FormTableService(CurrentUser))
            using (var repo = new DashboardRepository())
            {
                var form = formService.GetFormTable(formId);
                if (form == null)
                {
                    throw new ObjectNotFoundException($"대시보드를 찾을 수 없습니다.\r\n대시보드 ID: \"{formId}\"");
                }

                var dateRange = form.GetDateRangeByUploadInterval(date);

                string queryUserId = null;
                if (!CurrentUser.HasPermission(UserPermission.DataReview))
                {
                    queryUserId = CurrentUser.UserId;
                }

                bool onlyConfirmed = CurrentUser.HasPermission(UserPermission.DataConfirm) ? false : true;
                if (onlyConfirmed)
                {
                    if (CurrentUser.GroupType == UserPermissionGroupType.Uploader)
                    {
                        onlyConfirmed = false; // 본인이 업로드 한 데이터를 볼 수 있어야 함.
                    }
                }

                var formData = new FormTableData
                {
                    DataTerm = new DataDateRange(dateRange)
                    {
                        UploadInterval = form.UploadInterval,
                        SourceDate = date
                    },
                    Data = repo.SelectDataFileCellValueList(formId, dateRange.BeginDate, dateRange.EndDate, onlyConfirmed, queryUserId),
                    HtmlTemplate = form.HtmlTemplate
                };

                if (formData.Data.Any())
                {
                    var fileSource = formData.Data.First().Value.FileSource;
                    if (fileSource != null && fileSource.HtmlTemplateId.Value != form.HtmlTemplateId)
                    {
                        var htmlTemplateCopy = formService.GetFormHtmlTemplate(formId, fileSource.HtmlTemplateId.Value);
                        formData.HtmlTemplate = htmlTemplateCopy;
                    }
                    else
                    {
                        // TODO(jhlee): 로그
                    }
                }

                return formData;
            }
        }
        #endregion

        #region Methods - DataFileSource
        public PagedModel<DataFileSource> GetDataFileSourcePagedList(DataFileSourceSearchOption option)
        {
            option.ThrowIfNull(nameof(option));
            option.Validate();

            using (var repo = new DashboardRepository())
            {
                string queryUserId = null;
                if (!CurrentUser.HasPermission(UserPermission.DataReview))
                {
                    queryUserId = CurrentUser.UserId;
                }

                return repo.SelectDataFileSourcePagedList(option, queryUserId);
            }
        }

        /// <summary>
        /// 파일 소스를 반환한다.
        /// </summary>
        public DataFileSource GetDataFileSource(Guid formId, Guid formSectionId, Guid fileSourceId)
        {
            formId.ThrowIfEmpty(nameof(formId));
            formSectionId.ThrowIfEmpty(nameof(formSectionId));
            fileSourceId.ThrowIfEmpty(nameof(fileSourceId));

            using (var repo = new DashboardRepository())
            {
                return repo.SelectDataFileSource(formId, formSectionId, fileSourceId);
            }
        }

        /// <summary>
        /// 파일 소스를 등록한다. 같은 기간에 등록된 파일 소스가 있다면 삭제 처리 된다.
        /// </summary>
        public bool AddDataFileSource(DataFileSource fileSource)
        {
            fileSource.ThrowIfNull(nameof(fileSource));
            fileSource.Validate();

            using (var service = new FormTableService(CurrentUser))
            {
                // 1. FormTable, FormTableSection 체크

                var formTable = service.GetFormTable(fileSource.FormId);
                if (formTable == null)
                {
                    throw new ObjectNotFoundException($"업로드 대상이 되는 대시보드 테이블을 찾을 수 없습니다.\r\n대시보드 ID: \"{fileSource.FormId}\"");
                }
                else if (!formTable.IsEnabled || formTable.IsDeleted)
                {
                    string state = formTable.IsDeleted ? "삭제" : "비활성화";
                    throw new InvalidOperationException($"대상 대시보드 테이블이 {state} 된 상태이므로 파일 데이터를 업로드 할 수 없습니다.\r\n대시보드 ID: \"{fileSource.FormId}\"");
                }

                var formSection = service.GetFormTableSection(fileSource.FormId, fileSource.FormSectionId);
                if (formSection == null)
                {
                    throw new ObjectNotFoundException($"업로드 대상이 되는 업무 영역을 찾을 수 없습니다.\r\n업무영역 ID: \"{fileSource.FormSectionId}\"");
                }
                else if (!formSection.IsEnabled || formSection.IsDeleted)
                {
                    string state = formTable.IsDeleted ? "삭제" : "비활성화";
                    throw new InvalidOperationException($"대상 업무 영역이 {state} 된 상태이므로 파일 데이터를 업로드 할 수 없습니다.\r\n업무영역 ID: \"{fileSource.FormSectionId}\"");
                }

                fileSource.FileTemplateId = formSection.FileTemplate.FileTemplateId;
                fileSource.HtmlTemplateId = formTable.HtmlTemplateId;

                using (var repo = new DashboardRepository())
                {
                    // 3. 실제 파일 경로 체크
                    string filePath = fileSource.GetFileAbsolutePath(DiskPathForDataFileSource);
                    if (!File.Exists(filePath))
                    {
                        logger.Error($"업로드 된 데이터 파일 소스를 찾을 수 없습니다. 파일 경로: \"{filePath}\"");

                        throw new FileNotFoundException($"파일을 찾을 수 없습니다. 파일: \"{fileSource.FileName}\"");
                    }

                    var allValues = new List<DataFileCellValue>();
                    ExcelFileType fileType;

                    // 4. 파일 타입 체크
                    if (!Enum.TryParse<ExcelFileType>(fileSource.Extension, true, out fileType))
                    {
                        throw new FileLoadException($"지원하지 않는 엑셀 파일 타입입니다. 파일: \"{fileSource.FileName}\"");
                    }

                    // 5. 엑셀로부터 값을 읽음
                    using (var parser = new ExcelParser(filePath, fileType))
                    {
                        var parseOption = formSection.FileTemplate.ParseOption;

                        foreach (var kv in parseOption.Sheets)
                        {
                            foreach (var sheetOption in kv.Value)
                            {
                                string sheetName = kv.Key;

                                var values = parser.GetValuesFromSheet(kv.Key, sheetOption, () =>
                                {
                                    return new DataFileCellValue(fileSource.FileSourceId, sheetName)
                                    {
                                        CreatedDate = fileSource.CreatedDate
                                    };
                                }).Cast<DataFileCellValue>()
                                .ToList();

                                allValues.AddRange(values);
                            }
                        }

                        if (!allValues.Any())
                        {
                            logger.Warn($"업로드 된 데이터 파일로부터 읽어들인 엑셀 값이 없습니다. 파일: \"{fileSource.FileName}\""
                                + $"\r\n\r\n"
                                + $"{fileSource}");
                        }
                    }

                    // 6. DB에 추가
                    repo.BeginTransaction();

                    try
                    {
                        // 6. 이미 동일한 기간에 해당되는 파일이 있는지 체크
                        // => 있다면 삭제
                        var dateRange = formTable.GetDateRangeByUploadInterval(fileSource.SourceDate);
                        var oldFileSourceList = repo.SelectDataFileSourceListBySourceDateRange(fileSource.FormId, fileSource.FormSectionId, dateRange.BeginDate, dateRange.EndDate);
                        if (oldFileSourceList != null)
                        {
                            var duplicated = oldFileSourceList
                                .Where(o => o.FileSourceId != fileSource.FileSourceId)
                                .ToArray();

                            if (duplicated.Any())
                            {
                                logger.Warn($"해당 기간에 파일 소스가 2개 이상 존재합니다. 파일: {duplicated.Select(o => o.FileName).ToString()}"
                                    + $"\r\n\r\n"
                                    + $"업무 영역 ID: \"{fileSource.FormSectionId}\""
                                    + $"\r\n"
                                    + $"기간: \"{dateRange.BeginDate.ToString("yyyy-MM-dd")}\" ~ \"{dateRange.EndDate.ToString("yyyy-MM-dd")}\"");

                                if (duplicated.Any(o => o.IsConfirmed))
                                {
                                    // 관리자급 권한이 없다면 오류 처리 (관리자는 컨펌된 데이터가 있더라도 업로드 가능하도록 요청)
                                    if (CurrentUser.GroupType < UserPermissionGroupType.Administrator)
                                    {
                                        if (dateRange.BeginDate.ToString("yyyy-MM-dd").Equals(dateRange.EndDate.ToString("yyyy-MM-dd")))
                                        {
                                            throw new InvalidOperationException($"대상 기간에 이미 최종 컨펌된 데이터가 있습니다."
                                             + $"\r\n"
                                             + $"기간: \"{dateRange.BeginDate.ToString("yyyy-MM-dd")}\"");
                                        }
                                        else
                                        {
                                            throw new InvalidOperationException($"대상 기간에 이미 최종 컨펌된 데이터가 있습니다."
                                                + $"\r\n"
                                                + $"기간: \"{dateRange.BeginDate.ToString("yyyy-MM-dd")}\" ~ \"{dateRange.EndDate.ToString("yyyy-MM-dd")}\"");
                                        }
                                        
                                    }
                                }

                                foreach (var o in duplicated)
                                {
                                    if (repo.DeleteDataFileSource(
                                        formId: o.FormId,
                                        formSectionId: o.FormSectionId,
                                        fileSourceId: o.FileSourceId,
                                        deleterId: CurrentUser.UserId,
                                        deletedDate: DateTimeOffset.Now))
                                    {
                                        logger.Warn($"새로운 데이터를 업로드 하기 위해 대상 기간에 해당되는 데이터 파일이 삭제되었습니다. 파일: \"{o.FileName}\""
                                            + $"\r\n\r\n"
                                            + $"기간: \"{dateRange.BeginDate}\" ~ \"{dateRange.EndDate}\""
                                            + $"\r\n\r\n"
                                            + $"{o}");
                                    }
                                }
                            }
                        }

                        // 7 파일 데이터 추가
                        if (repo.InsertDataFileSource(fileSource))
                        {
                            if (allValues.Any())
                            {
                                repo.InsertDataFileCellValueList(formTable.FormId, formSection.FormSectionId, fileSource.FileSourceId, allValues, allValues.First().CreatedDate);
                            }

                            repo.CommitTransaction();

                            logger.Info($"새 데이터 파일을 추가하였습니다. 파일: \"{fileSource.FileName}\""
                                + $"\r\n\r\n"
                                + $"{fileSource}");

                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"데이터 파일 추가 중 알 수 없는 오류가 발생하였습니다. 대시보드: \"{formSection.FormName}\", 업무영역: \"{formSection.FormSectionName}\""
                            + $"\r\n\r\n"
                            + $"{formSection}");

                        try
                        {
                            repo.RollBackTransaction();
                        }
                        catch (Exception rex)
                        {
                            logger.Fatal(ex, $"데이터 파일 추가 중 치명적인 오류가 발생하였습니다. 대시보드: \"{formSection.FormName}\", 업무영역: \"{formSection.FormSectionName}\""
                                + $"\r\n\r\n"
                                + $"{formSection}");

                            ExceptionDispatchInfo.Capture(rex).Throw();
                            // not reached
                        }

                        ExceptionDispatchInfo.Capture(ex).Throw();
                    }

                    return false; // not reached
                }
            }
        }

        /// <summary>
        /// 파일 소스를 삭제한다.
        /// </summary>
        public bool DeleteDataFileSource(Guid formId, Guid formSectionId, Guid fileSourceId)
        {
            using (var repo = new DashboardRepository())
            {
                var dataFileSource = repo.SelectDataFileSource(formId, formSectionId, fileSourceId);
                if (dataFileSource == null)
                {
                    throw new ObjectNotFoundException($"삭제 할 대상 데이터 파일을 찾을 수 없습니다."
                        + $"\r\n"
                        + $"대시보드 ID: \"{formId}\", 업무 영역 ID: \"{formSectionId}\", 파일 ID: \"{fileSourceId}\"");
                }

                if (repo.DeleteDataFileSource(formId, formSectionId, fileSourceId, CurrentUser.UserId, DateTimeOffset.Now))
                {
                    logger.Info($"데이터 파일을 삭제하였습니다. 파일: \"{dataFileSource.FileName}\""
                        + $"\r\n\r\n"
                        + $"{dataFileSource}");

                    return true;
                }

                return false;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// 유저가 올린 데이터 파일이 임시로 저장될 디스크 경로를 반환한다.
        /// </summary>
        public static string DiskTempPathForDataFileSource
        {
            get
            {
                var path = AppSettings.AppSetting<string>("DataFile.Source.UploadTempPath")
                    .Replace('/', Path.DirectorySeparatorChar);

                return PathResolver.GetFullPath(path);
            }
        }

        /// <summary>
        /// 유저가 올린 데이터 파일이 저장될 디스크 경로를 반환한다.
        /// </summary>
        public static string DiskPathForDataFileSource
        {
            get
            {
                var path = AppSettings.AppSetting<string>("DataFile.Source.UploadPath")
                    .Replace('/', Path.DirectorySeparatorChar);

                return PathResolver.GetFullPath(path);
            }
        }
        #endregion
    }
}
