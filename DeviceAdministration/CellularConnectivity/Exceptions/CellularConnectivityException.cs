using System;

namespace DeviceManagement.Infrustructure.Connectivity.Exceptions
{
    [Serializable]
    public class CellularConnectivityException : Exception
    {
        public CellularConnectivityException(Exception exception)
            : base(exception.Message, exception.InnerException)
        {
        }
    }
}