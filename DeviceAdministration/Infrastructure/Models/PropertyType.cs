namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    /// <summary>
    /// The supported types that <see cref="string" /> proprty values 
    /// represent.
    /// </summary>
    public enum PropertyType
    {
        /// <summary>
        /// The <see cref="string" /> property represents a 
        /// <see cref="string" />.
        /// </summary>
        String = 0,

        /// <summary>
        /// The <see cref="string" /> property represents a 
        /// <see cref="DateTime" />.
        /// </summary>
        DateTime,

        /// <summary>
        /// The <see cref="string" /> property represents a
        /// <see cref="short" />, <see cref="int" />, or <see cref="long" />.
        /// </summary>
        Integer,

        /// <summary>
        /// The <see cref="string" /> property represents a 
        /// <see cref="single" /> or <see cref="double" />.
        /// </summary>
        Real,

        /// <summary>
        /// The <see cref="string" /> property represents a Status value.
        /// </summary>
        Status
    }
}
