namespace DeviceManagement.Infrustructure.Connectivity.Models.TerminalDevice
{
    public class Iccid
    {
        public Iccid()
        {
        }

        public Iccid(string id)
        {
            Id = id;
        }

        public string Id { get; set; }
    }
}