using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.PhysicalProtocol.Exceptions
{
    public class GarminUnableToWriteToDeviceException : SystemException
    {
        #region Public Constructors

        public GarminUnableToWriteToDeviceException()
        { }

        public GarminUnableToWriteToDeviceException(string message)
            : base(message)
        { }

        public GarminUnableToWriteToDeviceException(string message, Exception innerException)
            : base(message, innerException)
        { }

        #endregion
    }
}
