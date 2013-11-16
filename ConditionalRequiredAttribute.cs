using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Mvc;

namespace CustomAttributes
{
    public class ConditionalRequiredAttribute : RequiredAttribute, IClientValidatable
    {
        private readonly string _expression;
        private readonly ExpressionParser _expressionParser = new ExpressionParser();

        public ConditionalRequiredAttribute(string expression)
        {
            _expression = expression;
        }

        public ConditionalRequiredAttribute(string expressionTemplate, params object[] values)
        {
            _expression = string.Format(expressionTemplate, values);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var result = _expressionParser.Parse(validationContext.ObjectInstance, _expression);
            if(result)
                return base.IsValid(value, validationContext);

            return ValidationResult.Success;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = FormatErrorMessage(metadata.GetDisplayName()),
                ValidationType = "conditionalrequired",
            };

            var viewContext = (ViewContext)context;
            var andExpressions = _expressionParser.GetEqualityExpressions(_expression);

            foreach (var orExpressions in andExpressions)
            {
                foreach (var orExpression in orExpressions)
                {
                    orExpression.Property = viewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(orExpression.Property);
                }
            }

            rule.ValidationParameters.Add("expression", Serializer.Serialize(andExpressions));
            yield return rule;
        }
    }

    public class Serializer
    {
        public static string Serialize(object obj)
        {
            using (var memoryStream = new MemoryStream())
            using (var reader = new StreamReader(memoryStream))
            {
                var serializer = new DataContractJsonSerializer(obj.GetType());
                serializer.WriteObject(memoryStream, obj);
                memoryStream.Position = 0;
                return reader.ReadToEnd();
            }
        }

        public static object Deserialize(string xml, Type toType)
        {
            using (Stream stream = new MemoryStream())
            {
                byte[] data = Encoding.UTF8.GetBytes(xml);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                var deserializer = new DataContractJsonSerializer(toType);
                return deserializer.ReadObject(stream);
            }
        }
    }
}
