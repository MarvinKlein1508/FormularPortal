﻿namespace FormularPortal.Core.Models
{
    public abstract class FormElement : ICloneable, IDbModel
    {
        [CompareField("element_id")]
        public int ElementId { get; set; }
        [CompareField("form_id")]
        public int FormId { get; set; }
        [CompareField("row_id")]
        public int RowId { get; set; }
        [CompareField("column_id")]
        public int ColumnId { get; set; }
        [CompareField("name")]
        public string Name { get; set; } = string.Empty;
        [CompareField("type")]
        public ElementType Type { get; set; }
        [CompareField("is_active")]
        public bool IsActive { get; set; }
        [CompareField("is_required")]
        public bool IsRequired { get; set; }
        [CompareField("sort_order")]
        public int SortOrder { get; set; }

        public object Clone()
        {
            object tmp = this.MemberwiseClone();

            return tmp;
        }

        public override string ToString() => Name;

        public virtual Dictionary<string, object?> GetParameters()
        {
            return new Dictionary<string, object?>
            {
                { "ELEMENT_ID", ElementId },
                { "FORM_ID", FormId },
                { "ROW_ID", RowId },
                { "COLUMN_ID", ColumnId },
                { "NAME", Name },
                { "IS_ACTIVE", IsActive },
                { "IS_REQUIRED", IsRequired },
                { "SORT_ORDER", SortOrder },
            };
        }

    }
    public enum ElementType
    {
        Checkbox,
        Date,
        File,
        Label,
        Number,
        Radio,
        Select,
        Table,
        Text,
        Textarea
    }
}
