using System;
using System.Runtime.InteropServices;

using uint8 = System.Byte;
using uint16 = System.UInt16;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A001.Enumerations;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A001
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct Protocol_Data_Type
    {
        public uint8 tag;
        public uint16 data;

        public Protocol_Data_Type(uint8 tag, uint16 data)
        {
            this.tag = tag;
            this.data = data;
        }

        public Protocol_Data_Type(Protocol_Data_Type_Tag tag, uint16 data)
        {
            this.tag = (uint8)tag;
            this.data = data;
        }
    }
}
