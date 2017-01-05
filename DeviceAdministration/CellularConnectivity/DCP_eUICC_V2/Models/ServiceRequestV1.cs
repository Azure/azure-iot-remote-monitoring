using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace DCP_eUICC_V2 {

  /// <summary>
  /// ServiceRequest response model
  /// </summary>
  [DataContract]
  public class ServiceRequestV1 {
    /// <summary>
    /// Gets or Sets ServiceRequestId
    /// </summary>
    [DataMember(Name="serviceRequestId", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "serviceRequestId")]
    public string ServiceRequestId { get; set; }

    /// <summary>
    /// Gets or Sets ServiceRequestType
    /// </summary>
    [DataMember(Name="serviceRequestType", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "serviceRequestType")]
    public string ServiceRequestType { get; set; }

    /// <summary>
    /// Gets or Sets ServiceRequestState
    /// </summary>
    [DataMember(Name="serviceRequestState", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "serviceRequestState")]
    public string ServiceRequestState { get; set; }

    /// <summary>
    /// Gets or Sets CreatedBy
    /// </summary>
    [DataMember(Name="createdBy", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "createdBy")]
    public string CreatedBy { get; set; }

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
    /// Gets or Sets TimeCreated
    /// </summary>
    [DataMember(Name="timeCreated", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "timeCreated")]
    public Instant TimeCreated { get; set; }

    /// <summary>
    /// Gets or Sets LastUpdated
    /// </summary>
    [DataMember(Name="lastUpdated", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "lastUpdated")]
    public Instant LastUpdated { get; set; }

    /// <summary>
    /// Gets or Sets Size
    /// </summary>
    [DataMember(Name="size", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "size")]
    public int? Size { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class ServiceRequestV1 {\n");
      sb.Append("  ServiceRequestId: ").Append(ServiceRequestId).Append("\n");
      sb.Append("  ServiceRequestType: ").Append(ServiceRequestType).Append("\n");
      sb.Append("  ServiceRequestState: ").Append(ServiceRequestState).Append("\n");
      sb.Append("  CreatedBy: ").Append(CreatedBy).Append("\n");
      sb.Append("  CompanyId: ").Append(CompanyId).Append("\n");
      sb.Append("  CompanyName: ").Append(CompanyName).Append("\n");
      sb.Append("  TimeCreated: ").Append(TimeCreated).Append("\n");
      sb.Append("  LastUpdated: ").Append(LastUpdated).Append("\n");
      sb.Append("  Size: ").Append(Size).Append("\n");
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
