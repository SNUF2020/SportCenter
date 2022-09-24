using System;
using System.Collections.Generic;

namespace SC._03_Garmin_FR305.Core.Device
{
    public interface IPhysicalDeviceDiscoverer<T> where T : IPhysicalDevice
    {
        List<T> Devices
        { get; }
    }
}
