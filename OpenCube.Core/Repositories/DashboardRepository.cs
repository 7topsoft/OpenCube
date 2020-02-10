using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenCube.Models;
using OpenCube.Models.Data;

namespace OpenCube.Core.Repositories
{
    /// <summary>
    /// 대시보드 관련 데이터 테이블 DAC
    /// </summary>
    public class DashboardRepository : BaseRepository
    {
        #region Constructors
        public DashboardRepository()
        { }
        #endregion

        #region Methods - DataFileSource
        /// <summary>
        /// 페이징 처리된 실적 파일 데이터 리스트를 반환한다.
        /// </summary>
        /// <param name="queryUserId">값이 있는 경우, 해당 유저가 접근 가능한 데이터만 반환</param>
        public PagedModel<DataFileSource> SelectDataFileSourcePagedList(DataFileSourceSearchOption option, string queryUserId = null)
        {
            string procCommandName = "up_DataFileSource_SelectPagedList";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "PageNumber", DbType.Int32, option.PageNumber);
                Connection.AddInParameter(command, "PageCount", DbType.Int32, option.PageCount);
                Connection.AddInParameter(command, "SortBy", DbType.String, option.SortBy);
                Connection.AddInParameter(command, "OrderBy", DbType.String, option.OrderBy.ToEnumMemberString());

                if (option.FormId.HasValue)
                {
                    Connection.AddInParameter(command, "FormID", DbType.Guid, option.FormId.Value);
                }

                Connection.AddInParameter(command, "SearchKeyword", DbType.String, option.SearchKeyword);
                Connection.AddInParameter(command, "QueryUserID", DbType.String, queryUserId);

                using (DataSet ds = Connection.ExecuteDataSet(command))
                {
                    ValidateTableCount(ds, 2);

                    var result = new PagedModel<DataFileSource>(option) { PagingOption = option };
                    int totalCount = 0;

                    // 1. 유저 정보 테이블
                    var userInfoMap = new Dictionary<string, UserInfo>(StringComparer.OrdinalIgnoreCase);
                    foreach (DataRow dr in ds.Tables[1].Rows)
                    {
                        var userInfo = UserInfo.ParseFrom(dr);
                        userInfoMap[userInfo.UserId] = userInfo;
                    }

                    // 2. 데이터 파일 리스트
                    var dataFileSourceList = new List<DataFileSource>();
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        var dataFile = DataFileSource.ParseFrom(dr, out totalCount);

                        UserInfo userInfo = null;
                        if (userInfoMap.TryGetValue(dataFile.CreatorId, out userInfo))
                        {
                            dataFile.CreatorInfo = userInfo;
                        }
                        else
                        {
                            dataFile.CreatorInfo = new UserInfo(dataFile.CreatorId);
                        }

                        dataFileSourceList.Add(dataFile);
                    }

                    result.TotalCount = totalCount;
                    result.Items = dataFileSourceList;

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 실적 파일 데이터를 셀렉트한다.
        /// </summary>
        public DataFileSource SelectDataFileSource(Guid formId, Guid formSectionId, Guid fileSourceId)
        {
            string procCommandName = "up_DataFileSource_Select";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "FormID", DbType.Guid, formId);
                Connection.AddInParameter(command, "FormSectionID", DbType.Guid, formSectionId);
                Connection.AddInParameter(command, "FileSourceID", DbType.Guid, fileSourceId);

                using (DataSet ds = Connection.ExecuteDataSet(command))
                {
                    ValidateTableCount(ds, 1);

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        return DataFileSource.ParseFrom(ds.Tables[0].Rows[0]);
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
        /// 기간에 해당되는 실적 파일 데이터를 셀렉트한다.
        /// </summary>
        public IEnumerable<DataFileSource> SelectDataFileSourceListBySourceDateRange(Guid formId, Guid formSectionId, DateTimeOffset beginDate, DateTimeOffset endDate)
        {
            string procCommandName = "up_DataFileSource_SelectBySourceDateRange";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "FormID", DbType.Guid, formId);
                Connection.AddInParameter(command, "FormSectionID", DbType.Guid, formSectionId);
                Connection.AddInParameter(command, "BeginDate", DbType.DateTimeOffset, beginDate);
                Connection.AddInParameter(command, "EndDate", DbType.DateTimeOffset, endDate);

                using (DataSet ds = Connection.ExecuteDataSet(command))
                {
                    ValidateTableCount(ds, 1);

                    List<DataFileSource> dataFileSourceList = null;
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        dataFileSourceList = new List<DataFileSource>();
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            dataFileSourceList.Add(DataFileSource.ParseFrom(dr));
                        }
                    }

                    return dataFileSourceList;
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 기간에 해당되는 실적 파일 데이터 리스트를 셀렉트한다. (모든 업무 영역 데이터를 반환)
        /// </summary>
        public IEnumerable<DataFileSource> SelectDataFileSourceListBySourceDateRange(Guid formId, DateTimeOffset beginDate, DateTimeOffset endDate)
        {
            string procCommandName = "up_DataFileSource_SelectListBySourceDateRange";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "FormID", DbType.Guid, formId);
                Connection.AddInParameter(command, "BeginDate", DbType.DateTimeOffset, beginDate);
                Connection.AddInParameter(command, "EndDate", DbType.DateTimeOffset, endDate);

                using (DataSet ds = Connection.ExecuteDataSet(command))
                {
                    ValidateTableCount(ds, 1);

                    return ds.Tables[0].Rows.Cast<DataRow>()
                        .Select(o => DataFileSource.ParseFrom(o));
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 실적 파일 데이터를 추가한다. (동일한 업무 영역에 추가된 실적 파일 데이터가 있다면 삭제 처리된다.)
        /// </summary>
        public bool InsertDataFileSource(DataFileSource fileSource)
        {
            fileSource.ThrowIfNull(nameof(fileSource));

            string procCommandName = "up_DataFileSource_Insert";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "FormID", DbType.Guid, fileSource.FormId);
                Connection.AddInParameter(command, "FormSectionID", DbType.Guid, fileSource.FormSectionId);
                Connection.AddInParameter(command, "FileSourceID", DbType.Guid, fileSource.FileSourceId);
                Connection.AddInParameter(command, "FileTemplateID", DbType.Guid, fileSource.FileTemplateId);
                Connection.AddInParameter(command, "HtmlTemplateID", DbType.Guid, fileSource.HtmlTemplateId);
                Connection.AddInParameter(command, "CreatorId", DbType.String, fileSource.CreatorId);
                Connection.AddInParameter(command, "FileName", DbType.String, fileSource.FileName);
                Connection.AddInParameter(command, "Extension", DbType.String, fileSource.Extension);
                Connection.AddInParameter(command, "Size", DbType.Int64, fileSource.Size);
                Connection.AddInParameter(command, "Path", DbType.String, fileSource.FileRelativePath);
                Connection.AddInParameter(command, "Comment", DbType.String, fileSource.Comment);
                Connection.AddInParameter(command, "CreatedDate", DbType.DateTimeOffset, fileSource.CreatedDate);
                Connection.AddInParameter(command, "SourceDate", DbType.DateTimeOffset, fileSource.SourceDate);

                return (int)Connection.ExecuteNonQuery(command) > 0;
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 실적 파일 데이터를 컨펌 처리한다.
        /// </summary>
        public bool UpdateDataFileSourceAsConfirmed(Guid formId, Guid fileSourceId, Guid htmlTemplateId, string confirmerId, DateTimeOffset confirmedDate, DateTimeOffset sourceDate)
        {
            string procCommandName = "up_DataFileSource_Confirm";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "FormID", DbType.Guid, formId);
                Connection.AddInParameter(command, "FileSourceID", DbType.Guid, fileSourceId);
                Connection.AddInParameter(command, "HtmlTemplateID", DbType.Guid, htmlTemplateId);
                Connection.AddInParameter(command, "ConfirmerId", DbType.String, confirmerId);
                Connection.AddInParameter(command, "ConfirmedDate", DbType.DateTimeOffset, confirmedDate);
                Connection.AddInParameter(command, "SourceDate", DbType.DateTimeOffset, sourceDate);

                return (int)Connection.ExecuteNonQuery(command) > 0;
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 컨펌 처리 된 실적 파일 데이터를 취소 처리한다.
        /// </summary>
        public bool UpdateDataFileSourceAsCanceled(Guid formId, Guid fileSourceId)
        {
            string procCommandName = "up_DataFileSource_ConfirmCancel";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "FormID", DbType.Guid, formId);
                Connection.AddInParameter(command, "FileSourceID", DbType.Guid, fileSourceId);

                return (int)Connection.ExecuteNonQuery(command) > 0;
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }

        /// <summary>
        /// 실적 파일 데이터를 제거한다.
        /// </summary>
        public bool DeleteDataFileSource(Guid formId, Guid formSectionId, Guid fileSourceId, string deleterId, DateTimeOffset deletedDate)
        {
            string procCommandName = "up_DataFileSource_Delete";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "FormID", DbType.Guid, formId);
                Connection.AddInParameter(command, "FormSectionID", DbType.Guid, formSectionId);
                Connection.AddInParameter(command, "FileSourceID", DbType.Guid, fileSourceId);
                Connection.AddInParameter(command, "DeleterId", DbType.String, deleterId);
                Connection.AddInParameter(command, "DeletedDate", DbType.DateTimeOffset, deletedDate);

                return (int)Connection.ExecuteNonQuery(command) > 0;
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }
        #endregion

        #region Methods - DataFileCellValue
        /// <summary>
        /// 대상 대시보드 테이블의 특정 기간 동안의 데이터 리스트를 업무 영별 별로 나누어서 반환한다. (Key: FormTableSection.ScriptVariable)
        /// </summary>
        /// <param name="queryUserId">값이 있는 경우, 해당 유저가 접근 가능한 데이터만 반환</param>
        public Dictionary<string, DataFileCellValues> SelectDataFileCellValueList(Guid formId, DateTimeOffset beginDate, DateTimeOffset endDate, bool onlyConfirmed = false, string queryUserId = null)
        {
            string procCommandName = "up_DataFileCellValue_SelectListByFormID";

            try
            {
                var command = Connection.GetStoredProcCommand(procCommandName);
                Connection.AddInParameter(command, "FormID", DbType.Guid, formId);
                Connection.AddInParameter(command, "BeginDate", DbType.DateTimeOffset, beginDate);
                Connection.AddInParameter(command, "EndDate", DbType.DateTimeOffset, endDate);
                Connection.AddInParameter(command, "OnlyConfirmed", DbType.Boolean, onlyConfirmed);

                Connection.AddInParameter(command, "QueryUserID", DbType.String, queryUserId);

                using (DataSet ds = Connection.ExecuteDataSet(command))
                {
                    ValidateTableCount(ds, 1);

                    var result = new Dictionary<string, DataFileCellValues>();

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            string scriptVariable = dr.Get<string>("ScriptVariable");
                            Guid formSectionId = dr.Get<Guid>("FormSectionID");
                            Guid fileSourceId = dr.Get<Guid>("FileSourceID");

                            var value = DataFileCellValue.ParseFrom(dr);

                            DataFileCellValues formSectionValues = null;

                            // 1. 업무 영역 별 데이터 리스트
                            if (!result.TryGetValue(scriptVariable, out formSectionValues))
                            {
                                result[scriptVariable] = formSectionValues = new DataFileCellValues(formId, formSectionId, fileSourceId)
                                {
                                    ScriptVariable = scriptVariable
                                };

                                formSectionValues.FileSource = this.SelectDataFileSource(formId, formSectionId, fileSourceId);
                            }

                            Dictionary<string, DataFileCellValue> values = null;

                            // 2. 시트 별 셀 데이터
                            if (!formSectionValues.Sheets.TryGetValue(value.SheetName, out values))
                            {
                                formSectionValues.Sheets[value.SheetName] = values = new Dictionary<string, DataFileCellValue>();
                            }

                            values[value.Location] = value;
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
        /// 파일로부터 읽은 셀 값 리스트를 추가한다.
        /// </summary>
        public bool InsertDataFileCellValueList(Guid formId, Guid formSectionId, Guid fileSourceId, List<DataFileCellValue> values, DateTimeOffset createdDate)
        {
            values.ThrowIfNull(nameof(values));

            if (values.Count == 0)
            {
                return false;
            }

            string procCommandName = "up_DataFileCellValue_Insert";

            try
            {
                using (var spParamDataValues = new DataTable()) // dbo.type_DataCellValues
                {
                    var command = Connection.GetStoredProcCommand(procCommandName);

                    Connection.AddInParameter(command, "FormID", DbType.Guid, formId);
                    Connection.AddInParameter(command, "FormSectionID", DbType.Guid, formSectionId);
                    Connection.AddInParameter(command, "FileSourceID", DbType.Guid, fileSourceId);
                    Connection.AddInParameter(command, "CreatedDate", DbType.DateTimeOffset, createdDate);

                    {
                        spParamDataValues.Columns.Add("ShgeetName", typeof(string));
                        spParamDataValues.Columns.Add("Column", typeof(string));
                        spParamDataValues.Columns.Add("Row", typeof(int));
                        spParamDataValues.Columns.Add("Value", typeof(string));
                        spParamDataValues.Columns.Add("Type", typeof(string));

                        foreach (var value in values)
                        {
                            spParamDataValues.Rows.Add(new object[]
                            {
                            value.SheetName,
                            value.Column,
                            value.Row,
                            value.Value,
                            value.ValueType.ToEnumMemberString()
                            });
                        }

                        var param = command.Parameters.AddWithValue("DataCellValues", spParamDataValues);
                        param.SqlDbType = SqlDbType.Structured;
                    }

                    return (int)Connection.ExecuteNonQuery(command) > 0;
                }
            }
            catch (Exception ex)
            {
                throw new DataException($"프로시져 실행 중 예기치 못한 에러가 발생했습니다.\r\n 프로시저: \"{procCommandName}\"", ex);
            }
        }
        #endregion
    }
}
