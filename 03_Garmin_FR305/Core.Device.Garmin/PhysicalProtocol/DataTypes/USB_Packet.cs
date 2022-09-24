using System;
using System.Runtime.InteropServices;
using SC._03_Garmin_FR305.Core.Device.Garmin.PhysicalProtocol.Enumerations;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.PhysicalProtocol.DataTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct USB_Packet
    {
        public USB_PacketType packetType;   // byte 0    
        public byte reserved1;              // byte 1
        public Int16 reserved2;             // byte 2 & 3
        public Int16 packetId;              // byte 4 & 5
        public Int16 reserved3;             // byte 6 & 7
        public Int32 dataSize;              // byte 8,9,10,11 
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public byte[] data;                 // byte 12+

        public USB_Packet(USB_PacketType packetType, Int16 packetId, Int32 dataSize)
        {
            this.reserved1 = 0;
            this.reserved2 = 0;
            this.reserved3 = 0;

            this.packetType = packetType;
            this.packetId = packetId;
            this.dataSize = dataSize;
            this.data = new byte[dataSize];
        }

        public USB_Packet(USB_PacketType packetType, Int16 packetId, Int32 dataSize, byte[] data)
        {
            this.reserved1 = 0;
            this.reserved2 = 0;
            this.reserved3 = 0;

            this.packetType = packetType;
            this.packetId = packetId;
            this.dataSize = dataSize;
            this.data = new byte[dataSize];
            Buffer.BlockCopy(data, 0, this.data, 0, dataSize);
        }
    }
}
