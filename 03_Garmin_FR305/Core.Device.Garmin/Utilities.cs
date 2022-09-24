using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SC._03_Garmin_FR305.Core.Device.Garmin.PhysicalProtocol;
using SC._03_Garmin_FR305.Core.Device.Garmin.PhysicalProtocol.DataTypes;
using SC._03_Garmin_FR305.Core.Device.Garmin.PhysicalProtocol.Enumerations;

namespace SC._03_Garmin_FR305.Core.Device.Garmin
{
    internal static class Utilities
    {
        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;

            int objLength = Marshal.SizeOf(obj);
            byte[] arr = new byte[objLength];

            GCHandle gcHandle = GCHandle.Alloc(arr, GCHandleType.Pinned);
            IntPtr gcHandlePtr = gcHandle.AddrOfPinnedObject();
            Marshal.StructureToPtr(obj, gcHandlePtr, false);

            gcHandle.Free();
            gcHandlePtr = IntPtr.Zero;

            return arr;
        }

        public static object ByteArrayToObject(byte[] data, object objectType)
        {
            int dataLength = Marshal.SizeOf(objectType);
            if (dataLength > data.Length)
                return null;

            GCHandle gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr gcHandleAddress = gcHandle.AddrOfPinnedObject();
            object retObj = Marshal.PtrToStructure(gcHandleAddress, objectType.GetType());

            gcHandle.Free();
            return retObj;
        }

        public static uint CTL_CODE(uint deviceType, uint function, uint method, uint access)
        {
            return ((deviceType) << 16) | ((access) << 14) | ((function) << 2) | (method);
        }

        /// Before calling this function, make sure the (PACKET_HEADER_SIZE + dataSize) is less than
        /// or equal to device.packetsize 
        public static void CreateUSBPacketByteArray(USB_PacketType packetType, Int16 packetId, Int32 dataSize, byte[] data, ref byte[] destinationArray)
        {

            /// TODO: The size of the array should not exceed device.packetsize. throw exception if it is

            if (destinationArray == null)
            {
                // The USB Packet header (12 bytes) and data amounting to the size of the data buffer
                destinationArray = new byte[GarminUSBConstants.PACKET_HEADER_SIZE + dataSize];
            }
            else
            {
                Array.Clear(destinationArray, 0, destinationArray.Length);
                Array.Resize(ref destinationArray, GarminUSBConstants.PACKET_HEADER_SIZE + dataSize);
            }

            List<byte> bufferList = new List<byte>();

            bufferList.Add((byte)packetType);                           // byte 0  
            bufferList.Add(new byte());                                 // byte 1
            bufferList.AddRange(BitConverter.GetBytes(new Int16()));    // byte 2 & 3
            bufferList.AddRange(BitConverter.GetBytes(packetId));       // byte 4 & 5
            bufferList.AddRange(BitConverter.GetBytes(new Int16()));    // byte 6 & 7
            bufferList.AddRange(BitConverter.GetBytes(dataSize));       // byte 8,9,10,11
            if ((data != null) && (data.Length != 0))
            {
                bufferList.AddRange(data);                              // byte 12+
            }

            //Copy only the amount of bytes specified in dataSize
            Buffer.BlockCopy(bufferList.ToArray(), 0, destinationArray, 0, destinationArray.Length);
        }

        public static void CreateUSBPacketByteArray(USB_Packet usbPacket, ref byte[] destinationArray)
        {
            CreateUSBPacketByteArray(usbPacket.packetType, usbPacket.packetId, usbPacket.dataSize, usbPacket.data, ref destinationArray);
        }

        public static USB_Packet CreateUSBPacketFromByteArray(byte[] byteArray)
        {
            UInt32 offset = 0;

            USB_Packet usbPacket = new USB_Packet();

            usbPacket.packetType = (USB_PacketType)byteArray[offset];
            offset = offset + 1;

            usbPacket.reserved1 = byteArray[offset];
            offset = offset + 1;

            usbPacket.reserved2 = BitConverter.ToInt16(byteArray, (int)offset);
            offset = offset + 2;

            usbPacket.packetId = BitConverter.ToInt16(byteArray, (int)offset);
            offset = offset + 2;

            usbPacket.reserved3 = BitConverter.ToInt16(byteArray, (int)offset);
            offset = offset + 2;

            usbPacket.dataSize = BitConverter.ToInt32(byteArray, (int)offset);
            offset = offset + 4;

            usbPacket.data = new byte[usbPacket.dataSize];

            Array.Copy(byteArray, offset, usbPacket.data, 0, usbPacket.dataSize);
            // hier hackt's imme rmal wieder

            return usbPacket;
        }

        /// <summary>
        /// Returns byte 4&5 of byteArray as a 16-bit signed integer. 
        /// It is presumed that the parameter byteArray is the byte array equivalent of the USB_Packet type.
        /// </summary>
        /// <param name="byteArray">The byte array equivalent of the USB_Packet type</param>
        /// <returns>-1 if an error occurred, or the packetId</returns>
        public static Int16 GetPacketIDFromByteArray(byte[] byteArray)
        {
            if (byteArray.Length < GarminUSBConstants.PACKET_HEADER_SIZE)
            {
                return -1;  //TODO: Need to throw exception
            }
            /// byte 4&5 of the usbpacket
            Int16 theId = -1;

            try
            {
                theId = BitConverter.ToInt16(byteArray, 4);
            }
            catch
            {
                return -1;
            }

            return theId;
        }

        /// <summary>
        /// Returns byte 8,9,10,11 of byteArray as a 16bit signed integer. 
        /// It is presumed that the parameter byteArray is the byte array equivalent of the USB_Packet type.
        /// </summary>
        /// <param name="byteArray">The byte array equivalent of the USB_Packet type</param>
        /// <returns>32-bit signed integer. -1 if an error occurred</returns>
        public static Int32 GetPacketDataSizeFromByteArray(byte[] byteArray)
        {
            if (byteArray.Length < GarminUSBConstants.PACKET_HEADER_SIZE)
            {
                return -1; //TODO: should throw an exception
            }
            /// byte 8,9,10,11 of the usbpacket
            Int32 dataSize = -1;

            try
            {
                dataSize = BitConverter.ToInt32(byteArray, 8);
            }
            catch
            {
                return -1;
            }

            return dataSize;
        }

        /// <summary>
        /// Converts a list of byte arrays to a single byte array
        /// </summary>
        /// <param name="byteArrayList">A list of byte arrays</param>
        /// <returns>A single byte array with the items (byte arrays) of the list appended</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static byte[] ByteArrayListToByteArray(List<byte[]> byteArrayList)
        {
            if (byteArrayList == null)
                throw new ArgumentNullException();

            int x = 0;
            foreach (byte[] arr in byteArrayList)
            {
                x = x + arr.Length;
            }

            int offset = 0;
            byte[] array = new byte[x];
            foreach (byte[] arr in byteArrayList)
            {
                Buffer.BlockCopy(arr, 0, array, offset, arr.Length);
                offset = offset + arr.Length;
            }

            return array;
        }
    }
}
