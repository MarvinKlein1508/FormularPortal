using DatabaseControllerProvider;

namespace FormPortal.Core.Models.FormElements
{
    public abstract class FormElementWithOptions : FormElement
    {
        public List<FormElementOption> Options { get; set; } = new List<FormElementOption>();

        [CompareField("value_string")]
        public string Value { get; set; } = string.Empty;

        public override Dictionary<string, object?> GetParameters()
        {
            var parameters = base.GetParameters();
            parameters["VALUE_STRING"] = Value;
            return parameters;
        }

        public override void SetValue(FormEntryElement element)
        {
            Value = element.ValueString;
        }
    }
}
