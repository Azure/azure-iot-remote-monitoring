using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace DCP_eUICC_V2 {

  /// <summary>
  /// FormSpecification response model
  /// </summary>
  [DataContract]
  public class FormSpecificationV1 {
    /// <summary>
    /// Gets or Sets Description
    /// </summary>
    [DataMember(Name="description", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "description")]
    public string Description { get; set; }

    /// <summary>
    /// Gets or Sets EarliestDeliveryDate
    /// </summary>
    [DataMember(Name="earliestDeliveryDate", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "earliestDeliveryDate")]
    public int? EarliestDeliveryDate { get; set; }

    /// <summary>
    /// Gets or Sets LowOrderThreshold
    /// </summary>
    [DataMember(Name="lowOrderThreshold", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "lowOrderThreshold")]
    public int? LowOrderThreshold { get; set; }

    /// <summary>
    /// Gets or Sets MinimumOrderVolume
    /// </summary>
    [DataMember(Name="minimumOrderVolume", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "minimumOrderVolume")]
    public int? MinimumOrderVolume { get; set; }

    /// <summary>
    /// Gets or Sets OrderIncrement
    /// </summary>
    [DataMember(Name="orderIncrement", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "orderIncrement")]
    public int? OrderIncrement { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class FormSpecificationV1 {\n");
      sb.Append("  Description: ").Append(Description).Append("\n");
      sb.Append("  EarliestDeliveryDate: ").Append(EarliestDeliveryDate).Append("\n");
      sb.Append("  LowOrderThreshold: ").Append(LowOrderThreshold).Append("\n");
      sb.Append("  MinimumOrderVolume: ").Append(MinimumOrderVolume).Append("\n");
      sb.Append("  OrderIncrement: ").Append(OrderIncrement).Append("\n");
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
