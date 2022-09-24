using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.PhysicalProtocol.Exceptions
{
    /// <summary>
    /// This exception is thrown when a handle to a file on the device is null
    /// </summary>
    public class GarminDeviceNotFoundException : InvalidOperationException
    {
        #region Public Constructors

        public GarminDeviceNotFoundException()
        { }

        public GarminDeviceNotFoundException(string message)
            : base(message)
        { }

        public GarminDeviceNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        { }

        #endregion
    }
}
