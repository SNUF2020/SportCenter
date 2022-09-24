using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.PhysicalProtocol.Exceptions
{
    /// <summary>
    /// This exception is thrown when a protocol is not supported
    /// </summary>
    public class GarminProtocolNotSupportedException : SystemException
    {
        #region Public Constructors

        public GarminProtocolNotSupportedException()
        { }

        public GarminProtocolNotSupportedException(string message)
            : base(message)
        { }

        public GarminProtocolNotSupportedException(string message, Exception innerException)
            : base(message, innerException)
        { }

        #endregion
    }
}
