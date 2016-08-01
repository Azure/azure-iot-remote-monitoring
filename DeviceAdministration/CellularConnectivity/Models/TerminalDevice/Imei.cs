namespace DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice
{
    public class Imei
    {
        public Imei()
        {
        }

        public Imei(string id)
        {
            Id = id;
        }

        public string Id { get; set; }
    }
}