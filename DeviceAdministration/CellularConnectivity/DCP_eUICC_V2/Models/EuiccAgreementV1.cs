using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace DCP_eUICC_V2 {

  /// <summary>
  /// EuiccAgreement response model
  /// </summary>
  [DataContract]
  public class EuiccAgreementV1 {
    /// <summary>
    /// Gets or Sets Id
    /// </summary>
    [DataMember(Name="id", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or Sets Name
    /// </summary>
    [DataMember(Name="name", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or Sets FormSpecification
    /// </summary>
    [DataMember(Name="formSpecification", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "formSpecification")]
    public FormSpecificationV1 FormSpecification { get; set; }

    /// <summary>
    /// Gets or Sets LeadOperator
    /// </summary>
    [DataMember(Name="leadOperator", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "leadOperator")]
    public string LeadOperator { get; set; }

    /// <summary>
    /// Gets or Sets ProfileSpecifications
    /// </summary>
    [DataMember(Name="profileSpecifications", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "profileSpecifications")]
    public List<ProfileSpecification> ProfileSpecifications { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class EuiccAgreementV1 {\n");
      sb.Append("  Id: ").Append(Id).Append("\n");
      sb.Append("  Name: ").Append(Name).Append("\n");
      sb.Append("  FormSpecification: ").Append(FormSpecification).Append("\n");
      sb.Append("  LeadOperator: ").Append(LeadOperator).Append("\n");
      sb.Append("  ProfileSpecifications: ").Append(ProfileSpecifications).Append("\n");
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
