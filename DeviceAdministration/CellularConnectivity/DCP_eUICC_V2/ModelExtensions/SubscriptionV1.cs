using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace DCP_eUICC_V2
{

    /// <summary>
    /// Model for subscription
    /// </summary>
    [DataContract]
    //public class SubscriptionV1 {
    public partial class SubscriptionV1
    {
        /// <summary>
        /// Gets or Sets Imsi
        /// </summary>
        [DataMember(Name = "imsi", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "imsi")]
        public string Imsi { get; set; }

        /// <summary>
        /// Gets or Sets State
        /// </summary>
        [DataMember(Name = "state", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        /// <summary>
        /// Gets or Sets OperatorId
        /// </summary>
        [DataMember(Name = "operatorId", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "operatorId")]
        public string OperatorId { get; set; }

        /// <summary>
        /// Gets or Sets OperatorName
        /// </summary>
        [DataMember(Name = "operatorName", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "operatorName")]
        public string OperatorName { get; set; }

        /// <summary>
        /// Gets or Sets Msisdn
        /// </summary>
        [DataMember(Name = "msisdn", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "msisdn")]
        public string Msisdn { get; set; }

        /// <summary>
        /// Gets or Sets Iccid
        /// </summary>
        [DataMember(Name = "iccid", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "iccid")]
        public string Iccid { get; set; }

        /// <summary>
        /// Gets or Sets LocaleList
        /// </summary>
        [DataMember(Name = "localeList", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "localeList")]
        public List<EuiccLocaleV1> LocaleList { get; set; }

        /// <summary>
        /// Gets or Sets Label
        /// </summary>
        [DataMember(Name = "label", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or Sets CompanyId
        /// </summary>
        [DataMember(Name = "companyId", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "companyId")]
        public string CompanyId { get; set; }

        /// <summary>
        /// Gets or Sets CompanyName
        /// </summary>
        [DataMember(Name = "companyName", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "companyName")]
        public string CompanyName { get; set; }

        /// <summary>
        /// Gets or Sets Pin1
        /// </summary>
        [DataMember(Name = "pin1", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "pin1")]
        public string Pin1 { get; set; }

        /// <summary>
        /// Gets or Sets Pin2
        /// </summary>
        [DataMember(Name = "pin2", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "pin2")]
        public string Pin2 { get; set; }

        /// <summary>
        /// Gets or Sets Puk1
        /// </summary>
        [DataMember(Name = "puk1", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "puk1")]
        public string Puk1 { get; set; }

        /// <summary>
        /// Gets or Sets Puk2
        /// </summary>
        [DataMember(Name = "puk2", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "puk2")]
        public string Puk2 { get; set; }

        /// <summary>
        /// Gets or Sets Region
        /// </summary>
        [DataMember(Name = "region", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "region")]
        public string Region { get; set; }

        /// <summary>
        /// Gets or Sets PbrExitDate
        /// </summary>
        [DataMember(Name = "pbrExitDate", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "pbrExitDate")]
        public Instant PbrExitDate { get; set; }

        /// <summary>
        /// Gets or Sets InstallationDate
        /// </summary>
        [DataMember(Name = "installationDate", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "installationDate")]
        public Instant InstallationDate { get; set; }

        /// <summary>
        /// Gets or Sets Specification
        /// </summary>
        [DataMember(Name = "specification", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "specification")]
        public string Specification { get; set; }

        /// <summary>
        /// Gets or Sets SpecificationType
        /// </summary>
        [DataMember(Name = "specificationType", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "specificationType")]
        public string SpecificationType { get; set; }

        /// <summary>
        /// Gets or Sets ArpAssignMentDate
        /// </summary>
        [DataMember(Name = "arpAssignMentDate", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "arpAssignMentDate")]
        public Instant ArpAssignMentDate { get; set; }

        /// <summary>
        /// Gets or Sets ArpName
        /// </summary>
        [DataMember(Name = "arpName", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "arpName")]
        public string ArpName { get; set; }

        /// <summary>
        /// Gets or Sets EuiccId
        /// </summary>
        [DataMember(Name = "euiccId", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "euiccId")]
        public string EuiccId { get; set; }


        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class SubscriptionV1 {\n");
            sb.Append("  Imsi: ").Append(Imsi).Append("\n");
            sb.Append("  State: ").Append(State).Append("\n");
            sb.Append("  OperatorId: ").Append(OperatorId).Append("\n");
            sb.Append("  OperatorName: ").Append(OperatorName).Append("\n");
            sb.Append("  Msisdn: ").Append(Msisdn).Append("\n");
            sb.Append("  Iccid: ").Append(Iccid).Append("\n");
            sb.Append("  LocaleList: ").Append(LocaleList).Append("\n");
            sb.Append("  Label: ").Append(Label).Append("\n");
            sb.Append("  CompanyId: ").Append(CompanyId).Append("\n");
            sb.Append("  CompanyName: ").Append(CompanyName).Append("\n");
            sb.Append("  Pin1: ").Append(Pin1).Append("\n");
            sb.Append("  Pin2: ").Append(Pin2).Append("\n");
            sb.Append("  Puk1: ").Append(Puk1).Append("\n");
            sb.Append("  Puk2: ").Append(Puk2).Append("\n");
            sb.Append("  Region: ").Append(Region).Append("\n");
            sb.Append("  PbrExitDate: ").Append(PbrExitDate).Append("\n");
            sb.Append("  InstallationDate: ").Append(InstallationDate).Append("\n");
            sb.Append("  Specification: ").Append(Specification).Append("\n");
            sb.Append("  SpecificationType: ").Append(SpecificationType).Append("\n");
            sb.Append("  ArpAssignMentDate: ").Append(ArpAssignMentDate).Append("\n");
            sb.Append("  ArpName: ").Append(ArpName).Append("\n");
            sb.Append("  EuiccId: ").Append(EuiccId).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Get the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

    }
}
