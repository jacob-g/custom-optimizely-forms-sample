(function ($) {
    const originalGetCustomElementValue = epi.EPiServer.Forms.Extension.getCustomElementValue;
    const originalBindCustomElementValue = epi.EPiServer.Forms.Extension.bindCustomElementValue;

    $.extend(true, epi.EPiServer.Forms, {
        Extension: {
            getCustomElementValue: function ($element) {
                if ($element.hasClass('Form__Element__MinMaxRange')) {
                    const val = JSON.stringify([$element.find('[data-rangepart=min]').val(), $element.find('[data-rangepart=max]').val()])
                    return val;
                }

                return originalGetCustomElementValue.apply(this, [$element]);
            },
            bindCustomElementValue: function ($element, val) {
                return originalBindCustomElementValue.apply(this, [$element, val]);
            },
        },
        Validators: {
            'FormsTest.Models.FormElements.MinMaxRangeValidator': function (fieldName, fieldValue, validatorMetaData) {
                //FIXME: this isn't being called
                const value = JSON.parse(fieldValue);

                return { isValid: value[0] < value[1] };
            }
        },
    });
})($$epiforms);