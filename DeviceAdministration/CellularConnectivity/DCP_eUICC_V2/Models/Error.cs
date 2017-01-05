using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace DCP_eUICC_V2 {

  /// <summary>
  /// Error response model
  /// </summary>
  [DataContract]
  public class Error {
    /// <summary>
    /// Error code
    /// </summary>
    /// <value>Error code</value>
    [DataMember(Name="code", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "code")]
    public int? Code { get; set; }

    /// <summary>
    /// Response HTTP status
    /// </summary>
    /// <value>Response HTTP status</value>
    [DataMember(Name="httpStatus", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "httpStatus")]
    public int? HttpStatus { get; set; }

    /// <summary>
    /// Error message
    /// </summary>
    /// <value>Error message</value>
    [DataMember(Name="message", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "message")]
    public string Message { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class Error {\n");
      sb.Append("  Code: ").Append(Code).Append("\n");
      sb.Append("  HttpStatus: ").Append(HttpStatus).Append("\n");
      sb.Append("  Message: ").Append(Message).Append("\n");
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
