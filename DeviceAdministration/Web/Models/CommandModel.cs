using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using GlobalResources;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class CommandModel
    {
        private List<ParameterModel> _parameters; 
        public List<ParameterModel> Parameters
        {
            get
            {
                if (_parameters != null && _parameters.Count == 1 && _parameters[0] == null)
                {
                    _parameters = new List<ParameterModel>();
                }

                return _parameters;
            }

            set { _parameters = value; }
        }
        public string Name { get; set; }
        public string DeviceId { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public string Description { get; set; }
    }

    public class ParameterModel : IValidatableObject
    {
        public ParameterModel()
        {
            ErrorMessages = new List<string>();
        }

        public string Name { get; set; }

        public string Type { get; set; }

        public string Value { get; set; }

        public List<string> ErrorMessages { get; private set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResult = new List<ValidationResult>();
            var isTypeValid = CommandParameterTypeLogic.Instance.IsValid(Type, Value);
            if (!isTypeValid)
            {
                var errorMessage = GetCommandErrorMessage();
                validationResult.Add(new ValidationResult(errorMessage));
                ErrorMessages.Add(errorMessage);
            }

            return validationResult;
        }

        [SuppressMessage(
            "Microsoft.Globalization", 
            "CA1308:NormalizeStringsToUppercase",
            Justification = "Type error messages are based on lower-case English names.")]
        private string GetCommandErrorMessage()
        {
            var errorMessage =
                Strings.ResourceManager.GetString(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}CommandErrorMessage", 
                    Type.ToLowerInvariant()));

            if (string.IsNullOrEmpty(errorMessage))
            {
                errorMessage = 
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Strings.UnknownCommandParameterType, 
                        Type);
            }
            return errorMessage;
        }
    }

    public static class ParametersExtensions
    {
        public static IEnumerable<ParameterModel> ToParametersModel(this List<Parameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return new List<ParameterModel>();
            }
            return parameters.Select(parameter => new ParameterModel
            {
                Name = parameter.Name,
                Type = parameter.Type
            });
        }
    }
}