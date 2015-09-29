using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GlobalResources;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;

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
    }

    public class ParameterModel : Parameter, IValidatableObject
    {
        public ParameterModel()
        {
            ErrorMessages = new List<string>();
        }

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

        private string GetCommandErrorMessage()
        {
            var errorMessage =
                Strings.ResourceManager.GetString(string.Format("{0}CommandErrorMessage", Type.ToLowerInvariant()));

            if (string.IsNullOrEmpty(errorMessage))
            {
                errorMessage = string.Format(Strings.UnknownCommandParameterType, Type);
            }
            return errorMessage;
        }
    }

    public static class ParametersExtensions
    {
        public static IEnumerable<ParameterModel> ToParametersModel(this List<Parameter> parameters)
        {
            if (parameters == null || parameters[0] == null)
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