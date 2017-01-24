using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace DCP_eUICC_V2 {

  /// <summary>
  /// Euicc response model
  /// </summary>
  [DataContract]
  public class EuiccV1 {
    /// <summary>
    /// Gets or Sets Subscriptions
    /// </summary>
    [DataMember(Name="subscriptions", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "subscriptions")]
    public List<EuiccSubscriptionV1> Subscriptions { get; set; }

    /// <summary>
    /// Gets or Sets EuiccId
    /// </summary>
    [DataMember(Name="euiccId", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "euiccId")]
    public string EuiccId { get; set; }

    /// <summary>
    /// Gets or Sets CompanyId
    /// </summary>
    [DataMember(Name="companyId", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "companyId")]
    public string CompanyId { get; set; }

    /// <summary>
    /// Gets or Sets CompanyName
    /// </summary>
    [DataMember(Name="companyName", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "companyName")]
    public string CompanyName { get; set; }

    /// <summary>
    /// Gets or Sets State
    /// </summary>
    [DataMember(Name="state", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "state")]
    public string State { get; set; }

    /// <summary>
    /// Gets or Sets Label
    /// </summary>
    [DataMember(Name="label", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "label")]
    public string Label { get; set; }

    /// <summary>
    /// Gets or Sets LocaleName
    /// </summary>
    [DataMember(Name="localeName", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "localeName")]
    public string LocaleName { get; set; }

    /// <summary>
    /// Gets or Sets BootstrapIcc
    /// </summary>
    [DataMember(Name="bootstrapIcc", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "bootstrapIcc")]
    public string BootstrapIcc { get; set; }

    /// <summary>
    /// Gets or Sets EnabledIcc
    /// </summary>
    [DataMember(Name="enabledIcc", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "enabledIcc")]
    public string EnabledIcc { get; set; }

    /// <summary>
    /// Gets or Sets BootstrapCompanyId
    /// </summary>
    [DataMember(Name="bootstrapCompanyId", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "bootstrapCompanyId")]
    public string BootstrapCompanyId { get; set; }

    /// <summary>
    /// Gets or Sets LocalizationTableId
    /// </summary>
    [DataMember(Name="localizationTableId", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "localizationTableId")]
    public long? LocalizationTableId { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class EuiccV1 {\n");
      sb.Append("  Subscriptions: ").Append(Subscriptions).Append("\n");
      sb.Append("  EuiccId: ").Append(EuiccId).Append("\n");
      sb.Append("  CompanyId: ").Append(CompanyId).Append("\n");
      sb.Append("  CompanyName: ").Append(CompanyName).Append("\n");
      sb.Append("  State: ").Append(State).Append("\n");
      sb.Append("  Label: ").Append(Label).Append("\n");
      sb.Append("  LocaleName: ").Append(LocaleName).Append("\n");
      sb.Append("  BootstrapIcc: ").Append(BootstrapIcc).Append("\n");
      sb.Append("  EnabledIcc: ").Append(EnabledIcc).Append("\n");
      sb.Append("  BootstrapCompanyId: ").Append(BootstrapCompanyId).Append("\n");
      sb.Append("  LocalizationTableId: ").Append(LocalizationTableId).Append("\n");
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
