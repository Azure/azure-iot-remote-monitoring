using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace DCP_eUICC_V2 {

  /// <summary>
  /// Service request response model
  /// </summary>
  [DataContract]
  public class ServiceRequestResponseV1 {
    /// <summary>
    /// Gets or Sets ServiceRequestId
    /// </summary>
    [DataMember(Name="serviceRequestId", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "serviceRequestId")]
    public string ServiceRequestId { get; set; }

    /// <summary>
    /// Gets or Sets Success
    /// </summary>
    [DataMember(Name="success", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "success")]
    public bool? Success { get; set; }

    /// <summary>
    /// Error message in case of unsuccesfull service request
    /// </summary>
    /// <value>Error message in case of unsuccesfull service request</value>
    [DataMember(Name="errorMessage", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "errorMessage")]
    public string ErrorMessage { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class ServiceRequestResponseV1 {\n");
      sb.Append("  ServiceRequestId: ").Append(ServiceRequestId).Append("\n");
      sb.Append("  Success: ").Append(Success).Append("\n");
      sb.Append("  ErrorMessage: ").Append(ErrorMessage).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

    /// <summary>
    /// Get the JSON string presentation of the object
    /// </summary>
    /// <returns>JSON string presentation of the object</returns>
    public string ToJson() {
      return JsonConvert.SerializeObject(this, Formatting.Indented);
    }

}
}
