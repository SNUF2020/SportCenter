using SC._03_Garmin_FR305.Core.Device.Garmin.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.PhysicalProtocol
{
    public partial class GarminDeviceDiscoverer : IGarminDeviceDiscoverer
    {
        #region Protected Methods

        protected virtual void OnStartInitialize(EventArgs e)
        {
            if (StartInitialize != null)
                StartInitialize(this, e);
        }

        protected virtual void OnFinishInitialize(FinishInitializeEventArgs e)
        {
            if (FinishInitialize != null)
                FinishInitialize(this, e);
        }

        protected virtual void OnStartFindDevices(EventArgs e)
        {
            if (StartFindDevices != null)
                StartFindDevices(this, e);
        }

        protected virtual void OnFinishFindDevices(FinishFindDevicesEventArgs e)
        {
            if (FinishFindDevices != null)
                FinishFindDevices(this, e);
        }

        #endregion

        #region Public Events

        public event EventHandler StartInitialize;
        public event EventHandler<FinishInitializeEventArgs> FinishInitialize;

        public event EventHandler StartFindDevices;
        public event EventHandler<FinishFindDevicesEventArgs> FinishFindDevices;
        //TODO: must read up on cancelable events : public event CancelEventHandler CancelFindDevice;

        #endregion
    }
}
