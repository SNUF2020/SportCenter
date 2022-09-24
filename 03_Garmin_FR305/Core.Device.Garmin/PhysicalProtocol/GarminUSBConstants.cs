using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.PhysicalProtocol
{
    internal class GarminUSBConstants
    {
        public const uint API_VERSION = 1;
        public const uint MAX_BUFFER_SIZE = 4096;
        public const uint ASYNC_DATA_SIZE = 64;

        public const byte PACKET_HEADER_SIZE = 12;
    }
}
