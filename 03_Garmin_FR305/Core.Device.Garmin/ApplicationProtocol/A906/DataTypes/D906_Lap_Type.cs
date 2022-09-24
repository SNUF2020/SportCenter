using System;
using System.Runtime.InteropServices;

using uint8 = System.Byte;
using uint16 = System.UInt16;
using time_type = System.UInt32;
using uint32 = System.UInt32;
using float32 = System.Single;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.Common.DataTypes;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A906.Interfaces;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A906.DataTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct D906_Lap_Type : IA906
    {
        public time_type start_time;
        public uint32 total_time;  /* In hundredths of a second */
        public float32 total_distance; /* In meters */
        public position_type begin;  /* Invalid if both lat and lon are 0x7FFFFFFF */
        public position_type end;  /* Invalid if both lat and lon are 0x7FFFFFFF */
        public uint16 calories;
        public uint8 track_index;  /* See below */
        public uint8 unused;  /* Unused. Set to 0. */

        /// <summary>
        /// Constructor to create a D906 type from its byte array representative.
        /// </summary>
        /// <param name="data">The byte array representation of the struct</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException">If the length of the data parameter is less than that of the minimum size of the struct</exception>
        public D906_Lap_Type(byte[] data)
        {
            const int typeSizeWithoutUnknownBytes = 32;

            if (data == null)
                throw new ArgumentNullException();

            if (data.Length < typeSizeWithoutUnknownBytes)
                throw new ArgumentException("TODO: (must localize) D906_Lap_Type. length of data cannot be less than size of data structure");

            int offset = 0;

            start_time = BitConverter.ToUInt32(data, offset);
            offset = offset + 4;

            total_time = BitConverter.ToUInt32(data, offset);
            offset = offset + 4;

            total_distance = BitConverter.ToSingle(data, offset);
            offset = offset + 4;

            begin.lat = BitConverter.ToInt32(data, offset);
            offset = offset + 4;

            begin.lon = BitConverter.ToInt32(data, offset);
            offset = offset + 4;

            end.lat = BitConverter.ToInt32(data, offset);
            offset = offset + 4;

            end.lon = BitConverter.ToInt32(data, offset);
            offset = offset + 4;

            calories = BitConverter.ToUInt16(data, offset);
            offset = offset + 2;

            track_index = data[offset];
            offset = offset + 1;

            unused = data[offset];
            offset = offset + 1;
        }
    }
}
