﻿using DatabaseControllerProvider;

namespace FormularPortal.Core.Models
{
    public class FormElementOption : IDbModel
    {
        [CompareField("element_option_id")]
        public int ElementOptionId { get; set; }
        [CompareField("element_id")]
        public int ElementId { get; set; }
        [CompareField("name")]
        public string Name { get; set; } = string.Empty;
        public int Id => ElementOptionId;
        public virtual Dictionary<string, object?> GetParameters()
        {
            return new Dictionary<string, object?>
            {
                { "ELEMENT_OPTION_ID", ElementOptionId },
                { "ELEMENT_ID", ElementId },
                { "NAME", Name },
            };
        }
    }
}
