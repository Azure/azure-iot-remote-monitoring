using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace DCP_eUICC_V2 {

  /// <summary>
  /// Model for minimal subscription information when fetching euicc information
  /// </summary>
  [DataContract]
  public class EuiccSubscriptionV1 {
    /// <summary>
    /// Gets or Sets Imsi
    /// </summary>
    [DataMember(Name="imsi", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "imsi")]
    public string Imsi { get; set; }

    /// <summary>
    /// Gets or Sets State
    /// </summary>
    [DataMember(Name="state", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "state")]
    public string State { get; set; }

    /// <summary>
    /// Gets or Sets OperatorId
    /// </summary>
    [DataMember(Name="operatorId", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "operatorId")]
    public string OperatorId { get; set; }

    /// <summary>
    /// Gets or Sets OperatorName
    /// </summary>
    [DataMember(Name="operatorName", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "operatorName")]
    public string OperatorName { get; set; }

    /// <summary>
    /// Gets or Sets Msisdn
    /// </summary>
    [DataMember(Name="msisdn", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "msisdn")]
    public string Msisdn { get; set; }

    /// <summary>
    /// Gets or Sets Iccid
    /// </summary>
    [DataMember(Name="iccid", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "iccid")]
    public string Iccid { get; set; }

    /// <summary>
    /// Gets or Sets LocaleList
    /// </summary>
    [DataMember(Name="localeList", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "localeList")]
    public List<EuiccLocaleV1> LocaleList { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class EuiccSubscriptionV1 {\n");
      sb.Append("  Imsi: ").Append(Imsi).Append("\n");
      sb.Append("  State: ").Append(State).Append("\n");
      sb.Append("  OperatorId: ").Append(OperatorId).Append("\n");
      sb.Append("  OperatorName: ").Append(OperatorName).Append("\n");
      sb.Append("  Msisdn: ").Append(Msisdn).Append("\n");
      sb.Append("  Iccid: ").Append(Iccid).Append("\n");
      sb.Append("  LocaleList: ").Append(LocaleList).Append("\n");
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
