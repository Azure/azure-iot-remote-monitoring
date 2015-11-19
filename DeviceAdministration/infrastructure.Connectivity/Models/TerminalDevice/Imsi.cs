namespace DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice
{
    public class Imsi
    {
        public Imsi()
        {
        }

        public Imsi(string id)
        {
            Id = id;
        }

        public string Id { get; set; }
    }
}