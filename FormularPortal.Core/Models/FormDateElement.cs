﻿namespace FormularPortal.Core.Models
{
    public class FormDateElement : FormElement
    {
        public bool SetDefaultValueToCurrentDate { get; set; }

        public FormDateElement()
        {
            Type = ElementType.Date;
        }
        public override Dictionary<string, object?> GetParameters()
        {
            var parameters = base.GetParameters();
            parameters.Add("IS_CURRENT_DATE_DEFAULT", SetDefaultValueToCurrentDate);
            return parameters;
        }
    }
}
