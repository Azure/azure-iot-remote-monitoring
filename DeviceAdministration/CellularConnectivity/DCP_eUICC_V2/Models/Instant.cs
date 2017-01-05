using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace DCP_eUICC_V2 {

  /// <summary>
  /// 
  /// </summary>
  [DataContract]
  public class Instant {
    /// <summary>
    /// Gets or Sets Nano
    /// </summary>
    [DataMember(Name="nano", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "nano")]
    public int? Nano { get; set; }

    /// <summary>
    /// Gets or Sets EpochSecond
    /// </summary>
    [DataMember(Name="epochSecond", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "epochSecond")]
    public long? EpochSecond { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class Instant {\n");
      sb.Append("  Nano: ").Append(Nano).Append("\n");
      sb.Append("  EpochSecond: ").Append(EpochSecond).Append("\n");
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
