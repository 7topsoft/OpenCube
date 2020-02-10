using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCube.Models;
using OpenCube.Models.Data;
using OpenCube.Models.Forms;

namespace OpenCube.Core.Repositories
{
    /// <summary>
    /// 대시보드 테이블 DAC
    /// </summary>
    public class FormTableRepository : BaseRepository
    {
        #region Constructors
        public FormTableRepository()
        { }
        #endregion

        #region Methods - FormTable
        /// <summary>
        /// 대시보드 테이블 아이템을 반환한다.
        /// </summary>
        public FormTable SelectFormTable(Guid formId)
        {
            string procCommandName = "up_FormTable_Select";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "FormID", DbType.Guid, formId);

                using (DataSet ds = Connection.ExecuteDataSet(command))
                {
                    ValidateTableCount(ds, 2);

                    // 1. 대시보드 테이블
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        var form = FormTable.ParseFrom(ds.Tables[0].Rows[0]);

                        // 2. 대시보드 테이블 양식
                        if (ds.Tables[1].Rows.Count > 0)
                        {
                            form.HtmlTemplate = FormHtmlTemplate.ParseFrom(ds.Tables[1].Rows[0]);
                        }

                        return form;
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 대시보드 테이블 리스트를 반환한다.
        /// </summary>
        /// <param name="queryUserId">값이 있는 경우, 해당 유저가 접근 가능한 데이터만 반환</param>
        public IEnumerable<FormTable> SelectFormTableList(string queryUserId = null)
        {
            string procCommandName = "up_FormTable_SelectList";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);

                Connection.AddInParameter(command, "QueryUserID", DbType.String, queryUserId);

                using (DataSet ds = Connection.ExecuteDataSet(command))
                {
                    ValidateTableCount(ds, 2);

                    var result = new List<FormTable>();

                    // 1. 대시보드 테이블 리스트
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        var form = FormTable.ParseFrom(dr);
                        result.Add(form);
                    }

                    // 2. 대시보드 테이블 양식 리스트
                    foreach (DataRow dr in ds.Tables[1].Rows)
                    {
                        var template = FormHtmlTemplate.ParseFrom(dr);
                        var forms = result.FindAll(o => o.FormId == template.FormId && o.HtmlTemplateId == template.HtmlTemplateId);
                        if (forms.Any())
                        {
                            forms.ForEach(o =>
                            {
                                o.HtmlTemplate = template;
                            });
                        }
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 페이징 처리된 대시보드 테이블 리스트를 반환한다.
        /// </summary>
        public PagedModel<FormTable> SelectFormTablePagedList(PagingOption option)
        {
            string procCommandName = "up_FormTable_SelectPagedList";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "PageNumber", DbType.Int32, option.PageNumber);
                Connection.AddInParameter(command, "PageCount", DbType.Int32, option.PageCount);
                Connection.AddInParameter(command, "SortBy", DbType.String, option.SortBy);
                Connection.AddInParameter(command, "OrderBy", DbType.String, option.OrderBy.ToEnumMemberString());

                using (DataSet ds = Connection.ExecuteDataSet(command))
                {
                    ValidateTableCount(ds, 2);

                    var result = new PagedModel<FormTable>(option) { PagingOption = option };
                    int totalCount = 0;

                    // 1. 대시보드 테이블 리스트
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        var form = FormTable.ParseFrom(dr, out totalCount);
                        result.Items.Add(form);
                    }

                    // 2. 대시보드 테이블 양식 리스트
                    foreach (DataRow dr in ds.Tables[1].Rows)
                    {
                        var template = FormHtmlTemplate.ParseFrom(dr);
                        var forms = result.Items.FindAll(o => o.FormId == template.FormId && o.HtmlTemplateId == template.HtmlTemplateId);
                        if (forms.Any())
                        {
                            forms.ForEach(o =>
                            {
                                o.HtmlTemplate = template;
                            });
                        }
                    }

                    result.TotalCount = totalCount;

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 대시보드 테이블을 추가한다.
        /// </summary>
        public bool InsertFormTable(FormTable formTable)
        {
            formTable.ThrowIfNull(nameof(formTable));

            string procCommandName = "up_FormTable_Insert";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "FormID", DbType.Guid, formTable.FormId);
                Connection.AddInParameter(command, "HtmlTemplateID", DbType.Guid, formTable.HtmlTemplateId);
                Connection.AddInParameter(command, "Name", DbType.String, formTable.Name);
                Connection.AddInParameter(command, "Description", DbType.String, formTable.Description);
                Connection.AddInParameter(command, "UploadInterval", DbType.String, formTable.UploadInterval.ToEnumMemberString());
                Connection.AddInParameter(command, "UploadWeekOfMonth", DbType.Int32, formTable.UploadWeekOfMonth);
                Connection.AddInParameter(command, "UploadDayOfWeek", DbType.String, formTable.UploadDayOfWeek.ToEnumMemberString());
                Connection.AddInParameter(command, "IsEnabled", DbType.Boolean, formTable.IsEnabled);
                Connection.AddInParameter(command, "CreatorId", DbType.String, formTable.CreatorId);
                Connection.AddInParameter(command, "CreatedDate", DbType.DateTimeOffset, formTable.CreatedDate);

                return (int)Connection.ExecuteNonQuery(command) > 0;
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 대시보드 테이블을 업데이트한다.
        /// </summary>
        public bool UpdateFormTable(FormTable formTable)
        {
            formTable.ThrowIfNull(nameof(formTable));

            string procCommandName = "up_FormTable_Update";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "FormID", DbType.Guid, formTable.FormId);
                Connection.AddInParameter(command, "HtmlTemplateID", DbType.Guid, formTable.HtmlTemplateId);
                Connection.AddInParameter(command, "Name", DbType.String, formTable.Name);
                Connection.AddInParameter(command, "Description", DbType.String, formTable.Description);
                //Connection.AddInParameter(command, "UploadInterval", DbType.String, formTable.UploadInterval.ToEnumMemberString());
                Connection.AddInParameter(command, "UploadWeekOfMonth", DbType.Int32, formTable.UploadWeekOfMonth);
                Connection.AddInParameter(command, "UploadDayOfWeek", DbType.String, formTable.UploadDayOfWeek.ToEnumMemberString());
                Connection.AddInParameter(command, "IsEnabled", DbType.Boolean, formTable.IsEnabled);
                Connection.AddInParameter(command, "UpdatedDate", DbType.DateTimeOffset, formTable.UpdatedDate);

                return (int)Connection.ExecuteNonQuery(command) > 0;
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 대시보드 테이블을 삭제한다.
        /// </summary>
        public bool DeleteFormTable(Guid formId, string deleterId, DateTimeOffset deletedDate)
        {
            string procCommandName = "up_FormTable_Delete";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "FormID", DbType.Guid, formId);
                Connection.AddInParameter(command, "DeleterID", DbType.String, deleterId);
                Connection.AddInParameter(command, "DeletedDate", DbType.DateTimeOffset, deletedDate);

                return (int)Connection.ExecuteNonQuery(command) > 0;
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }
        #endregion

        #region Methods - FormTableSection
        /// <summary>
        /// 대시보드 테이블 업무 영역을 반환한다.
        /// </summary>
        public FormTableSection SelectFormTableSection(Guid formId, Guid formSectionId)
        {
            string procCommandName = "up_FormTableSection_Select";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "FormID", DbType.Guid, formId);
                Connection.AddInParameter(command, "FormSectionID", DbType.Guid, formSectionId);

                using (DataSet ds = Connection.ExecuteDataSet(command))
                {
                    ValidateTableCount(ds, 4);

                    // 1. 업무 영역
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        var formSection = FormTableSection.ParseFrom(ds.Tables[0].Rows[0]);

                        // 2. 파일 템플릿 정보
                        if (ds.Tables[1].Rows.Count > 0)
                        {
                            formSection.FileTemplate = DataFileTemplate.ParseFrom(ds.Tables[1].Rows[0]);
                        }

                        // 3. 파일 소스 정보
                        if (ds.Tables[2].Rows.Count > 0)
                        {
                            formSection.FileSource = DataFileSource.ParseFrom(ds.Tables[2].Rows[0]);
                        }

                        // 4. 데이터 업로더 정보
                        foreach (DataRow dr in ds.Tables[3].Rows)
                        {
                            var uploader = DataFileSourceUploader.ParseFrom(dr);
                            formSection.FileSourceUploaders[uploader.UserId] = uploader;
                        }

                        return formSection;
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 대시보드 테이블 업무 영역 리스트를 반환한다.
        /// </summary>
        /// <param name="queryUserId">값이 있는 경우, 해당 유저가 접근 가능한 데이터만 반환</param>
        /// <param name="fileSourceDateRanges">값이 있는 경우, 해당 날짜의 데이터 파일 소스 정보를 반환</param>
        public IEnumerable<FormTableSection> SelectFormTableSectionList(Guid? formId, string queryUserId = null, List<DataDateRange> fileSourceDateRanges = null)
        {
            string procCommandName = "up_FormTableSection_SelectList";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);

                if (formId.HasValue)
                {
                    Connection.AddInParameter(command, "FormID", DbType.Guid, formId.Value);
                }

                Connection.AddInParameter(command, "QueryUserID", DbType.String, queryUserId);

                if (fileSourceDateRanges != null)
                {
                    // 특정한 날짜 범위 내의 데이터 소스 값만 반환하기 위해 날짜 범위를 DB에 전달한다.
                    using (var spParamDateRanges = new DataTable()) // type_DataSourceDateRanges
                    {
                        spParamDateRanges.Columns.Add("UploadInterval", typeof(string));
                        spParamDateRanges.Columns.Add("BeginDate", typeof(DateTimeOffset));
                        spParamDateRanges.Columns.Add("EndDate", typeof(DateTimeOffset));
                        spParamDateRanges.Columns.Add("IsCurrentData", typeof(bool));

                        foreach (var dateRange in fileSourceDateRanges)
                        {
                            spParamDateRanges.Rows.Add(new object[]
                            {
                                dateRange.UploadInterval,
                                dateRange.BeginDate,
                                dateRange.EndDate,
                                dateRange.IsCurrentData
                            });
                        }

                        var param = command.Parameters.AddWithValue("SourceDateRanges", spParamDateRanges);
                        param.SqlDbType = SqlDbType.Structured;
                    }
                }

                using (DataSet ds = Connection.ExecuteDataSet(command))
                {
                    ValidateTableCount(ds, 4);

                    var result = new List<FormTableSection>();

                    // 1. 업무 영역
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        var formSection = FormTableSection.ParseFrom(dr);
                        result.Add(formSection);
                    }

                    // 2. 파일 템플릿 정보
                    foreach (DataRow dr in ds.Tables[1].Rows)
                    {
                        var fileTemplate = DataFileTemplate.ParseFrom(dr);
                        var formSections = result.FindAll(o => o.FileTemplateId == fileTemplate.FileTemplateId);
                        if (formSections.Any())
                        {
                            formSections.ForEach(o =>
                            {
                                o.FileTemplate = fileTemplate;
                            });
                        }
                    }

                    // 3. 파일 소스 정보
                    foreach (DataRow dr in ds.Tables[2].Rows)
                    {
                        var fileSource = DataFileSource.ParseFrom(dr);
                        bool isCurrentData = dr.Get<bool>("IsCurrentData");

                        var formSections = result.FindAll(o => o.FormId == fileSource.FormId && o.FormSectionId == fileSource.FormSectionId);
                        if (formSections.Any())
                        {
                            formSections.ForEach(o =>
                            {
                                if (isCurrentData)
                                {
                                    o.FileSource = fileSource;
                                }
                                else
                                {
                                    o.PrevFileSource = fileSource;
                                }
                            });
                        }
                    }

                    // 4. 데이터 업로더 정보
                    foreach (DataRow dr in ds.Tables[3].Rows)
                    {
                        var uploader = DataFileSourceUploader.ParseFrom(ds.Tables[3].Rows[0]);
                        var formSections = result.FindAll(o => o.FormId == uploader.FormId && o.FormSectionId == uploader.FormSectionId);
                        if (formSections.Any())
                        {
                            formSections.ForEach(o =>
                            {
                                o.FileSourceUploaders[uploader.UserId] = uploader;
                            });
                        }
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 페이징 처리된 대시보드 테이블 업무 영역을 반환한다.
        /// </summary>
        public PagedModel<FormTableSection> SelectFormTableSectionPagedList(PagingOption option)
        {
            string procCommandName = "up_FormTableSection_SelectPagedList";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "PageNumber", DbType.Int32, option.PageNumber);
                Connection.AddInParameter(command, "PageCount", DbType.Int32, option.PageCount);
                Connection.AddInParameter(command, "SortBy", DbType.String, option.SortBy);
                Connection.AddInParameter(command, "OrderBy", DbType.String, option.OrderBy.ToEnumMemberString());

                using (DataSet ds = Connection.ExecuteDataSet(command))
                {
                    ValidateTableCount(ds, 4);

                    var result = new PagedModel<FormTableSection>(option) { PagingOption = option };
                    int totalCount = 0;

                    // 1. 업무 영역
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        var formSection = FormTableSection.ParseFrom(dr, out totalCount);
                        result.Items.Add(formSection);
                    }

                    // 2. 파일 템플릿 정보
                    foreach (DataRow dr in ds.Tables[1].Rows)
                    {
                        var fileTemplate = DataFileTemplate.ParseFrom(dr);
                        var formSections = result.Items.FindAll(o => o.FileTemplateId == fileTemplate.FileTemplateId);
                        if (formSections.Any())
                        {
                            formSections.ForEach(o =>
                            {
                                o.FileTemplate = fileTemplate;
                            });
                        }
                    }

                    // 3. 파일 소스 정보
                    foreach (DataRow dr in ds.Tables[2].Rows)
                    {
                        var fileSource = DataFileSource.ParseFrom(dr);
                        var formSections = result.Items.FindAll(o => o.FormId == fileSource.FormId && o.FormSectionId == fileSource.FormSectionId);
                        if (formSections.Any())
                        {
                            formSections.ForEach(o =>
                            {
                                o.FileSource = fileSource;
                            });
                        }
                    }

                    // 4. 데이터 업로더 정보
                    foreach (DataRow dr in ds.Tables[3].Rows)
                    {
                        var uploader = DataFileSourceUploader.ParseFrom(dr);
                        var formSections = result.Items.FindAll(o => o.FormId == uploader.FormId && o.FormSectionId == uploader.FormSectionId);
                        if (formSections.Any())
                        {
                            formSections.ForEach(o =>
                            {
                                o.FileSourceUploaders[uploader.UserId] = uploader;
                            });
                        }
                    }

                    result.TotalCount = totalCount;

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 대시보드 테이블 업무 영역을 추가한다.
        /// </summary>
        public bool InsertFormTableSection(FormTableSection formSection)
        {
            formSection.ThrowIfNull(nameof(formSection));

            string procCommandName = "up_FormTableSection_Insert";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "FormID", DbType.Guid, formSection.FormId);
                Connection.AddInParameter(command, "FormSectionID", DbType.Guid, formSection.FormSectionId);
                Connection.AddInParameter(command, "FileTemplateID", DbType.Guid, formSection.FileTemplateId);
                Connection.AddInParameter(command, "Name", DbType.String, formSection.FormSectionName);
                Connection.AddInParameter(command, "ScriptVariable", DbType.String, formSection.ScriptVariable);
                Connection.AddInParameter(command, "CreatorID", DbType.String, formSection.CreatorId);
                Connection.AddInParameter(command, "IsEnabled", DbType.Boolean, formSection.IsEnabled);
                Connection.AddInParameter(command, "CreatedDate", DbType.DateTimeOffset, formSection.CreatedDate);

                return (int)Connection.ExecuteNonQuery(command) > 0;
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 대시보드 테이블 업무 영역을 업데이트한다. (파일 템플릿 ID가 변경되면 이전에 사용 중이던 파일 템플릿은 삭제 처리 된다.)
        /// </summary>
        public bool UpdateFormTableSection(FormTableSection formSection, string updaterId)
        {
            formSection.ThrowIfNull(nameof(formSection));

            string procCommandName = "up_FormTableSection_Update";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "FormID", DbType.Guid, formSection.FormId);
                Connection.AddInParameter(command, "FormSectionID", DbType.Guid, formSection.FormSectionId);
                Connection.AddInParameter(command, "FileTemplateID", DbType.Guid, formSection.FileTemplateId);
                Connection.AddInParameter(command, "Name", DbType.String, formSection.FormSectionName);
                Connection.AddInParameter(command, "ScriptVariable", DbType.String, formSection.ScriptVariable);
                Connection.AddInParameter(command, "IsEnabled", DbType.Boolean, formSection.IsEnabled);
                Connection.AddInParameter(command, "UpdaterID", DbType.String, updaterId);
                Connection.AddInParameter(command, "UpdatedDate", DbType.DateTimeOffset, formSection.UpdatedDate);

                return (int)Connection.ExecuteNonQuery(command) > 0;
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 대시보드 테이블 업무 영역을 삭제한다.
        /// </summary>
        public bool DeleteFormTableSection(Guid formId, Guid formSectionId, string deleterId, DateTimeOffset deletedDate)
        {
            string procCommandName = "up_FormTableSection_Delete";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "FormID", DbType.Guid, formId);
                Connection.AddInParameter(command, "FormSectionID", DbType.Guid, formSectionId);
                Connection.AddInParameter(command, "DeleterID", DbType.String, deleterId);
                Connection.AddInParameter(command, "DeletedDate", DbType.DateTimeOffset, deletedDate);

                return (int)Connection.ExecuteNonQuery(command) > 0;
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }
        #endregion

        #region Methods - FormTableSectionGroup
        /// <summary>
        /// 대상 업무 영역에 데이터 업로더를 리스트 단위로 추가한다. (이미 있다면 무시하고, 삭제된 유저가 있다면 프로시저 내부에서 상태만 변경한다.)
        /// </summary>
        public bool InsertFormTableSectionGroupMembers(Guid formId, Guid formSectionId, IEnumerable<string> userIds, DateTimeOffset date)
        {
            string procCommandName = "up_FormTableSectionGroup_InsertList";

            try
            {
                using (var spParamUserIDList = new DataTable()) // dbo.type_UserIDList
                {
                    var command = Connection.GetStoredProcCommand(procCommandName);

                    spParamUserIDList.Columns.Add("UserID", typeof(string));

                    foreach (var userId in userIds)
                    {
                        var tempObj = new object[]
                        {
                            userId
                        };

                        spParamUserIDList.Rows.Add(tempObj);
                    }

                    var param = command.Parameters.AddWithValue("UserIDs", spParamUserIDList);
                    param.SqlDbType = SqlDbType.Structured;

                    Connection.AddInParameter(command, "FormID", DbType.Guid, formId);
                    Connection.AddInParameter(command, "FormSectionID", DbType.Guid, formSectionId);
                    Connection.AddInParameter(command, "CreatedDate", DbType.DateTimeOffset, date);

                    return (int)Connection.ExecuteNonQuery(command) > 0;
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }
        #endregion

        #region Methods - FormHtmlTemplate
        /// <summary>
        /// 대시보드 테이블 html 양식 아이템을 반환한다.
        /// </summary>
        public FormHtmlTemplate SelectFormHtmlTemplate(Guid formId, Guid templateId)
        {
            string procCommandName = "up_FormHtmlTemplate_Select";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "FormID", DbType.Guid, formId);
                Connection.AddInParameter(command, "HtmlTemplateID", DbType.Guid, templateId);

                using (DataSet ds = Connection.ExecuteDataSet(command))
                {
                    ValidateTableCount(ds, 1);

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        return FormHtmlTemplate.ParseFrom(ds.Tables[0].Rows[0]);
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 대시보드 테이블 html 양식 리스트를 반환한다.
        /// </summary>
        public IEnumerable<FormHtmlTemplate> SelectFormHtmlTemplateList()
        {
            string procCommandName = "up_FormHtmlTemplate_SelectList";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);

                using (DataSet ds = Connection.ExecuteDataSet(command))
                {
                    ValidateTableCount(ds, 1);

                    var list = new List<FormHtmlTemplate>();

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        var template = FormHtmlTemplate.ParseFrom(dr);
                        list.Add(template);
                    }

                    return list;
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 페이징 처리된 대시보드 테이블 html 양식 리스트를 반환한다.
        /// </summary>
        public PagedModel<FormHtmlTemplate> SelectFormHtmlTemplatePagedList(PagingOption option)
        {
            string procCommandName = "up_FormHtmlTemplate_SelectPagedList";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "PageNumber", DbType.Int32, option.PageNumber);
                Connection.AddInParameter(command, "PageCount", DbType.Int32, option.PageCount);
                Connection.AddInParameter(command, "SortBy", DbType.String, option.SortBy);
                Connection.AddInParameter(command, "OrderBy", DbType.String, option.OrderBy.ToEnumMemberString());

                using (DataSet ds = Connection.ExecuteDataSet(command))
                {
                    ValidateTableCount(ds, 1);

                    var result = new PagedModel<FormHtmlTemplate>(option) { PagingOption = option };
                    int totalCount = 0;

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        var template = FormHtmlTemplate.ParseFrom(dr, out totalCount);
                        result.Items.Add(template);
                    }

                    result.TotalCount = totalCount;

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 대시보드 테이블 html 양식을 추가한다.
        /// </summary>
        public bool InsertFormHtmlTemplate(FormHtmlTemplate template)
        {
            template.ThrowIfNull(nameof(template));

            string procCommandName = "up_FormHtmlTemplate_Insert";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "FormID", DbType.Guid, template.FormId);
                Connection.AddInParameter(command, "HtmlTemplateID", DbType.Guid, template.HtmlTemplateId);
                Connection.AddInParameter(command, "CreatorId", DbType.String, template.CreatorId);
                Connection.AddInParameter(command, "Description", DbType.String, template.Description);
                Connection.AddInParameter(command, "ScriptContent", DbType.String, template.ScriptContent);
                Connection.AddInParameter(command, "HtmlContent", DbType.String, template.HtmlContent);
                Connection.AddInParameter(command, "StyleContent", DbType.String, template.StyleContent);
                Connection.AddInParameter(command, "CreatedDate", DbType.DateTimeOffset, template.CreatedDate);

                return (int)Connection.ExecuteNonQuery(command) > 0;
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 대시보드 테이블 html 양식을 업데이트한다.
        /// </summary>
        public bool UpdateFormHtmlTemplate(FormHtmlTemplate template)
        {
            template.ThrowIfNull(nameof(template));

            string procCommandName = "up_FormHtmlTemplate_Update";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "FormID", DbType.Guid, template.FormId);
                Connection.AddInParameter(command, "HtmlTemplateID", DbType.Guid, template.HtmlTemplateId);
                Connection.AddInParameter(command, "Description", DbType.String, template.Description);
                Connection.AddInParameter(command, "ScriptContent", DbType.String, template.ScriptContent);
                Connection.AddInParameter(command, "HtmlContent", DbType.String, template.HtmlContent);
                Connection.AddInParameter(command, "StyleContent", DbType.String, template.StyleContent);
                Connection.AddInParameter(command, "UpdatedDate", DbType.DateTimeOffset, template.UpdatedDate);

                return (int)Connection.ExecuteNonQuery(command) > 0;
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }
        #endregion

        #region Methods - DataFileTemplate
        /// <summary>
        /// 업무 영역 - 파일 템플릿 정보를 추가한다.
        /// </summary>
        public bool InsertDateFileTemplate(DataFileTemplate fileTemplate)
        {
            fileTemplate.ThrowIfNull(nameof(fileTemplate));

            string procCommandName = "up_DataFileTemplate_Insert";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "FormID", DbType.Guid, fileTemplate.FormId);
                Connection.AddInParameter(command, "FormSectionID", DbType.Guid, fileTemplate.FormSectionId);
                Connection.AddInParameter(command, "FileTemplateID", DbType.Guid, fileTemplate.FileTemplateId);
                Connection.AddInParameter(command, "CreatorID", DbType.String, fileTemplate.CreatorId);
                Connection.AddInParameter(command, "FileName", DbType.String, fileTemplate.FileName);
                Connection.AddInParameter(command, "Extension", DbType.String, fileTemplate.Extension);
                Connection.AddInParameter(command, "Size", DbType.Int64, fileTemplate.Size);
                Connection.AddInParameter(command, "Path", DbType.String, fileTemplate.FileRelativePath);
                Connection.AddInParameter(command, "ParseOption", DbType.String, fileTemplate.ParseOption.ToJson());
                Connection.AddInParameter(command, "CreatedDate", DbType.DateTimeOffset, fileTemplate.CreatedDate);

                return (int)Connection.ExecuteNonQuery(command) > 0;
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 업무 영역 - 파일 템플릿 정보를 업데이트한다.
        /// </summary>
        public bool UpdateDateFileTemplate(DataFileTemplate fileTemplate)
        {
            fileTemplate.ThrowIfNull(nameof(fileTemplate));

            string procCommandName = "up_DataFileTemplate_Update";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "FormID", DbType.Guid, fileTemplate.FormId);
                Connection.AddInParameter(command, "FormSectionID", DbType.Guid, fileTemplate.FormSectionId);
                Connection.AddInParameter(command, "FileTemplateID", DbType.Guid, fileTemplate.FileTemplateId);
                Connection.AddInParameter(command, "FileName", DbType.String, fileTemplate.FileName);
                Connection.AddInParameter(command, "ParseOption", DbType.String, fileTemplate.ParseOption.ToJson());

                return (int)Connection.ExecuteNonQuery(command) > 0;
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }
        #endregion
    }
}
