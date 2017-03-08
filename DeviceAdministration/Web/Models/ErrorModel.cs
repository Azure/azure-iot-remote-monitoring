using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class ErrorModel
    {
        public DateTime TimeStamp { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }

        public ErrorModel()
        {
            this.TimeStamp = DateTime.Now;
        }

        public ErrorModel(string Message) : this()
    {
            this.Message = Message;
        }

        public ErrorModel(System.Exception ex) : this(ex.Message)
    {
            this.StackTrace = ex.StackTrace;
        }

        public override string ToString()
        {
            return this.Message + this.StackTrace;
        }
    }
}