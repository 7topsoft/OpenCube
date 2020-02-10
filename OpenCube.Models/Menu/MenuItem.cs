using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenCube.Models.Attributes;
using OpenCube.Models.Forms;
using OpenCube.Models.Logging;

namespace OpenCube.Models.Menu
{
    /// <summary>
    /// 메뉴 아이템
    /// </summary>
    [JsonObject]
    public class MenuItem : BaseModel
    {
        #region Constructors
        public MenuItem(Guid menuId)
        {
            menuId.ThrowIfEmpty(nameof(menuId));

            this.MenuId = menuId;
        }
        #endregion

        #region Methods
        public static MenuItem ParseFrom(DataRow dr)
        {
            dr.ThrowIfNull(nameof(dr));

            Guid menuId = dr.Get<Guid>("MenuID");

            return new MenuItem(menuId)
            {
                Name = dr.Get<string>("Name"),
                GroupName = dr.Get<string>("GroupName"),
                IconName = dr.Get<string>("IconName"),
                SortOrder = dr.Get<int>("SortOrder"),
                IsVisible = dr.Get<bool>("IsVisible"),
                IsDeleted = dr.Get<bool>("IsDeleted"),
                CreatorId = dr.Get<string>("CreatorId"),
                DeleterId = dr.Get<string>("DeleterId"),
                CreatedDate = dr.Get<DateTimeOffset>("CreatedDate"),
                UpdatedDate = dr.Get<DateTimeOffset?>("UpdatedDate"),
                DeletedDate = dr.Get<DateTimeOffset?>("DeletedDate"),
            };
        }

        public static MenuItem ParseFrom(DataRow dr, out int totalCount)
        {
            dr.ThrowIfNull(nameof(dr));

            totalCount = dr.Get<int>("TotalCount");

            return MenuItem.ParseFrom(dr);
        }

        public static MenuItem CreateFrom(IMenuItemUpdatable fields)
        {
            fields.ThrowIfNull(nameof(fields));

            return new MenuItem(Guid.NewGuid())
            {
                Name = fields.Name,
                GroupName = fields.GroupName,
                IconName = fields.IconName,
                IsVisible = fields.IsVisible,
                SortOrder = fields.SortOrder,
                CreatedDate = DateTimeOffset.Now
            };
        }

        public void Update(IMenuItemUpdatable fields)
        {
            List<UpdatedField> updated = null;
            this.Update(fields, out updated);
        }

        public void Update(IMenuItemUpdatable fields, out List<UpdatedField> updated)
        {
            fields.ThrowIfNull(nameof(fields));

            updated = new List<UpdatedField>();

            if (Name != fields.Name)
            {
                updated.Add(new UpdatedField
                {
                    FieldName = nameof(Name),
                    OldValue = Name,
                    NewValue = fields.Name
                });

                Name = fields.Name;
            }

            if (GroupName != fields.GroupName)
            {
                updated.Add(new UpdatedField
                {
                    FieldName = nameof(GroupName),
                    OldValue = GroupName,
                    NewValue = fields.GroupName
                });

                GroupName = fields.GroupName;
            }

            if (IconName != fields.IconName)
            {
                updated.Add(new UpdatedField
                {
                    FieldName = nameof(IconName),
                    OldValue = IconName,
                    NewValue = fields.IconName
                });

                IconName = fields.IconName;
            }

            if (SortOrder != fields.SortOrder)
            {
                updated.Add(new UpdatedField
                {
                    FieldName = nameof(SortOrder),
                    OldValue = SortOrder,
                    NewValue = fields.SortOrder
                });

                SortOrder = fields.SortOrder;
            }

            if (IsVisible != fields.IsVisible)
            {
                updated.Add(new UpdatedField
                {
                    FieldName = nameof(IsVisible),
                    OldValue = IsVisible,
                    NewValue = fields.IsVisible
                });

                IsVisible = fields.IsVisible;
            }

            if (fields.FormTables != null && fields.FormTables.Any())
            {
                if (fields.FormTables.Length != FormTables.Count)
                {
                    updated.Add(new UpdatedField
                    {
                        FieldName = nameof(FormTables),
                        OldValue = $"[ {string.Join(", ", FormTables.Select(o => $"'{o.FormId}'"))} ]",
                        NewValue = $"[ {string.Join(", ", fields.FormTables.Select(formId => $"'{formId}'"))} ]"
                    });

                    // repository에서 처리
                }
            }

            if (updated.Any())
            {
                UpdatedDate = DateTimeOffset.Now;

                Validate();
            }
        }

        public override void Validate()
        {
            MenuId.ThrowIfEmpty(nameof(MenuId));
            Name.ThrowIfNullOrWhiteSpace(nameof(Name));
            SortOrder.ThrowIfOutOfRange(nameof(SortOrder), 0);
            CreatorId.ThrowIfNullOrWhiteSpace(nameof(CreatorId));
        }
        #endregion

        #region Properties - Table Mapped
        /// <summary>
        /// 메뉴 ID
        /// </summary>
        [Printable]
        public Guid MenuId { get; }

        /// <summary>
        /// 메뉴명
        /// </summary>
        [Printable]
        public string Name { get; set; }

        /// <summary>
        /// 메뉴 그룹명
        /// </summary>
        [Printable]
        public string GroupName { get; set; }

        /// <summary>
        /// 메뉴 아이콘
        /// </summary>
        [Printable]
        public string IconName { get; set; }

        /// <summary>
        /// 순서
        /// </summary>
        [Printable]
        public int SortOrder { get; set; }

        /// <summary>
        /// 보임 여부
        /// </summary>
        [Printable]
        public bool IsVisible { get; set; }

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
        public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// 업데이트일
        /// </summary>
        public DateTimeOffset? UpdatedDate { get; set; }

        /// <summary>
        /// 삭제일
        /// </summary>
        public DateTimeOffset? DeletedDate { get; set; }
        #endregion

        #region Properties - Merged
        /// <summary>
        /// 대시보드 테이블 리스트
        /// </summary>
        [JsonIgnore]
        public List<FormTable> FormTables { get; } = new List<FormTable>();

        /// <summary>
        /// 대시보드 테이블 요약 정보 리스트
        /// </summary>
        [JsonProperty("formTables")]
        public IEnumerable<FormTableSummary> FormTableSummaries => FormTables.Select(o => o.ToSummary());
        #endregion
    }
}
