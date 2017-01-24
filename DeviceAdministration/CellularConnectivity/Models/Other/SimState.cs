namespace DeviceManagement.Infrustructure.Connectivity.Models.Other
{
    public class SimState
    {
        private string _id;
        public string Id
        {
            get { return _id ?? Name; }
            set { _id = value; }
        }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }
}
