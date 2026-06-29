using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ControleFinanceiroWeb.Models;

namespace ControleFinanceiroWeb.Helpers
{
    // Utility class to perform generic model validation based on Data Annotations.
    public static class ModelValidator
    {
        // Validates an object based on its Data Annotations attributes, returning a ServiceResult.
        public static ServiceResult Validate(object model)
        {
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(model, context, results, validateAllProperties: true);

            if (!isValid)
            {
                var sb = new StringBuilder();

                foreach (var validationResult in results)
                {
                    sb.AppendLine($"• {validationResult.ErrorMessage}");
                }

                return new ServiceResult
                {
                    Success = false,
                    Message = sb.ToString()
                };
            }

            return new ServiceResult { Success = true };
        }
    }
}