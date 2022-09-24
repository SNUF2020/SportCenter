using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32.SafeHandles;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A001;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.Interfaces
{
    public interface IGarminDevice : IPhysicalDevice
    {
        #region Public Fields

        SafeFileHandle Handle
        {
            get;
            set;
        }

        int PacketSize
        {
            get;
            set;
        }

        bool HasSessionStarted
        {
            get;
        }

        uint DeviceUnitId
        {
            get;
        }

        List<Protocol_Data_Type> ProtocolDataTypes
        {
            get;
        }

        #endregion

        #region Public Methods

        void StartSession();

        #endregion

        #region Public Events

        event EventHandler<SessionStartedEventArgs> SessionStarted;

        #endregion
    }
}
