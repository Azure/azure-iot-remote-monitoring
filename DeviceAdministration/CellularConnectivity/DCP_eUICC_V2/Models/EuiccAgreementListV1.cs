using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace DCP_eUICC_V2 {

  /// <summary>
  /// Euicc agreement list response model
  /// </summary>
  [DataContract]
  public class EuiccAgreementListV1 {
    /// <summary>
    /// Gets or Sets EuiccAgreements
    /// </summary>
    [DataMember(Name="euiccAgreements", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "euiccAgreements")]
    public List<EuiccAgreementV1> EuiccAgreements { get; set; }

    /// <summary>
    /// Gets or Sets ServiceContracts
    /// </summary>
    [DataMember(Name="serviceContracts", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "serviceContracts")]
    public List<ServiceContractV1> ServiceContracts { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class EuiccAgreementListV1 {\n");
      sb.Append("  EuiccAgreements: ").Append(EuiccAgreements).Append("\n");
      sb.Append("  ServiceContracts: ").Append(ServiceContracts).Append("\n");
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
