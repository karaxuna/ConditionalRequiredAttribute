    $.validator.addMethod('conditionalrequired', function (value, element, parameters) {
        var andExpressions = JSON.parse(parameters['expression']);
        var result = true;

        debugger;

        andExpressions.forEach(function (andExpression) {
            var _res = false;
            andExpression.forEach(function (orExpression) {
                var actualValue = _cast(_getElementValue(orExpression.Property), typeof orExpression.Value);
                if (_compare(actualValue, orExpression.Value, orExpression.Operator.Text)) {
                    _res = true;
                    return;
                }
            });
            if (!_res) {
                result = false;
                return;
            }
        });

        function _cast(val, type) {
            if (!val)
                return null;
            
            switch (type) {
                case 'number':
                    return +val;
                case 'boolean':
                    return (val === 'true' || val === 'True');
                case 'string':
                default:
                    return val;
            }
        }

        function _getElementValue(name) {
            var els = document.getElementsByName(name);
            var el1 = els[0];

            if (!els.length)
                return null;
            if (els.length > 1) {
                if (el1.tagName.toLowerCase() === 'input' && (el1.type === 'radio' || el1.type === 'checkbox'))
                    for (var i = 0; i < els.length; i++) {
                        var el = els[i];
                        if (el.checked)
                            return el.value;
                    }
            } else {
                if (el1.tagName.toLowerCase() === 'input' && el1.type === 'checkbox')
                    return (el1.checked ? el1.value : null);
                else
                    return el1.value;
            }
            return null;
        }

        function _compare(actualValue, compareValue, operator) {
            switch (operator) {
                case '==':
                    return actualValue === compareValue;
                case '!=':
                    return actualValue !== compareValue;
                case '>':
                    return actualValue > compareValue;
                case '<':
                    return actualValue < compareValue;
                case '>=':
                    return actualValue >= compareValue;
                case '<=':
                    return actualValue <= compareValue;
                default:
                    throw 'wrong operator';
            }
        }

        if (result)
            return $.validator.methods.required.call(this, value, element, parameters);

        return true;
    });

    $.validator.unobtrusive.adapters.add('conditionalrequired', ['expression'], function(options) {
        options.rules['conditionalrequired'] = { expression: options.params['expression'] };
        options.messages['expression'] = options.message;
    });
