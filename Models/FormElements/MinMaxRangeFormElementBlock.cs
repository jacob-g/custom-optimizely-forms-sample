using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Forms.Core;
using EPiServer.Forms.Core.Internal;
using EPiServer.Forms.Core.Models;
using EPiServer.Forms.Core.Models.Internal;
using EPiServer.Forms.Core.Validation;
using EPiServer.Forms.EditView.DataAnnotations;
using EPiServer.Forms.Implementation.Elements.BaseClasses;
using EPiServer.Framework.Localization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace FormsTest.Models.FormElements
{
    [ContentType(DisplayName = "Min-Max Range", GUID = "80ABAC02-ACE3-4828-81FF-F7F58D322ACD", Description = "A mininum and maximum number")]
    [AvailableValidatorTypes(Include = new[] { typeof(MinMaxRangeValidator) })]
    public class MinMaxRangeFormElementBlock : InputElementBlockBase, IElementCustomFormatValue, IElementRequireClientResources
    {
        [Display(Name = "Lower Bound", Order = 100)]
        public virtual int? LowerBound { get; set; }
        
        [Display(Name = "Upper Bound", Order = 200)]
        public virtual int? UpperBound { get; set; }

        public override ElementInfo GetElementInfo()
        {
            var baseInfo = base.GetElementInfo();
            baseInfo.CustomBinding = true;
            return baseInfo;
        }

        public IEnumerable<Tuple<string, string>> GetExtraResources()
        {
            return new List<Tuple<string, string>>
            {
                new Tuple<string, string>("script", "/Static/js/rangeForm.js")
            };
        }

        public override object GetSubmittedValue()
        {
            var rawSubmittedData = HttpContext.Current.Request.Form;

            var strValue = base.GetSubmittedValue() as string ?? string.Empty;

            var isJavaScriptSupport = rawSubmittedData.Get(EPiServer.Forms.Constants.FormWithJavaScriptSupport);
            if (isJavaScriptSupport == "true") //if the user's browser support JS, then deserialize the value provided by the frontend
            {
                var values = JsonConvert.DeserializeObject<List<int>>(strValue);
                if ((values?.Count ?? 0) != 2)
                    return null;

                return Tuple.Create(values[0], values[1]);
            }

            //if the user's browser does not support JS, we need to extract the value ourselves from the HTML raw form fields
            var minName = $"{FormElement.ElementName}_min";
            var maxName = $"{FormElement.ElementName}_max";

            if (!int.TryParse(rawSubmittedData[minName], out var min) || !int.TryParse(rawSubmittedData[maxName], out var max))
                return null;

            return Tuple.Create(min, max);
        }

        public object GetFormattedValue()
        {
            var submittedVal = GetSubmittedValue() as Tuple<int, int>;

            if (submittedVal is null)
                return string.Empty;

            return $"{submittedVal.Item1} to {submittedVal.Item2}";
        }

        public override string Validators
        {
            get {
                var customValidator = string.Concat(typeof(MinMaxRangeValidator).FullName);

                var validators = this.GetPropertyValue(content => content.Validators);

                return string.IsNullOrEmpty(validators) ? customValidator : string.Concat(validators, EPiServer.Forms.Constants.RecordSeparator, customValidator);
            }

            set {
                this.SetPropertyValue(content => content.Validators, value);
            }
        }
    }

    public class MinMaxRangeValidator : ElementValidatorBase
    {
        public override bool? Validate(IElementValidatable targetElement)
        {
            var submittedValue = targetElement.GetSubmittedValue() as Tuple<int, int>;

            return submittedValue is null || submittedValue.Item1 < submittedValue.Item2;
        }

        public override bool AvailableInEditView
        {
            get {
                return false;
            }
        }

        public override IValidationModel BuildValidationModel(IElementValidatable targetElement)
        {
            var model = base.BuildValidationModel(targetElement);
            if (model != null)
            {
                model.Message = LocalizationService.Current.GetString("Form.Error.MinMaxRangeError");
            }
            return model;
        }
    }
}