using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace DCP_eUICC_V2 {

  /// <summary>
  /// eUICC label update request model
  /// </summary>
  [DataContract]
  public class LabelUpdateV1 {
    /// <summary>
    /// Company id of the requester
    /// </summary>
    /// <value>Company id of the requester</value>
    [DataMember(Name="companyId", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "companyId")]
    public string CompanyId { get; set; }

    /// <summary>
    /// Updated label or omitted for clearing the label
    /// </summary>
    /// <value>Updated label or omitted for clearing the label</value>
    [DataMember(Name="label", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "label")]
    public string Label { get; set; }

    /// <summary>
    /// One or more 32 hexadecimal digit eUICC IDs
    /// </summary>
    /// <value>One or more 32 hexadecimal digit eUICC IDs</value>
    [DataMember(Name="euiccIds", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "euiccIds")]
    public List<string> EuiccIds { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class LabelUpdateV1 {\n");
      sb.Append("  CompanyId: ").Append(CompanyId).Append("\n");
      sb.Append("  Label: ").Append(Label).Append("\n");
      sb.Append("  EuiccIds: ").Append(EuiccIds).Append("\n");
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
