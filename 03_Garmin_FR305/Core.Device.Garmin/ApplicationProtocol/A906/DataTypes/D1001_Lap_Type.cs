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
    public struct D1001_Lap_Type : IA906
    {
        public uint32 index;            /* Unique among all laps received from device */
        public time_type start_time;    /* Start of lap time */
        public uint32 total_time;       /* Duration of lap, in hundredths of a second */
        public float32 total_dist;      /* Distance in meters */
        public float32 max_speed;       /* In meters per second */
        public position_type begin;     /* Invalid if both lat and lon are 0x7FFFFFFF */
        public position_type end;       /* Invalid if both lat and lon are 0x7FFFFFFF */
        public uint16 calories;         /* Calories burned this lap */
        public uint8 avg_heart_rate;    /* In beats-per-minute, 0 if invalid */
        public uint8 max_heart_rate;    /* In beats-per-minute, 0 if invalid */
        public uint8 intensity;         /* See below */

        /// <summary>
        /// Constructor to create a D1001 type from its byte array representative.
        /// </summary>
        /// <param name="data">The byte array representation of the struct</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException">If the length of the data parameter is less than that of the minimum size of the struct</exception>
        public D1001_Lap_Type(byte[] data)
        {
            const int typeSizeWithoutUnknownBytes = 41;

            if (data == null)
                throw new ArgumentNullException();

            if (data.Length < typeSizeWithoutUnknownBytes)
                throw new ArgumentException("TODO: (must localize) D1001_Lap_Type. length of data cannot be less than size of data structure");

            int offset = 0;

            index = BitConverter.ToUInt32(data, offset);
            offset = offset + 4;

            start_time = BitConverter.ToUInt32(data, offset);
            offset = offset + 4;

            total_time = BitConverter.ToUInt32(data, offset);
            offset = offset + 4;

            total_dist = BitConverter.ToSingle(data, offset);
            offset = offset + 4;

            max_speed = BitConverter.ToSingle(data, offset);
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

            avg_heart_rate = data[offset];
            offset = offset + 1;

            max_heart_rate = data[offset];
            offset = offset + 1;

            intensity = data[offset];
            offset = offset + 1;
        }
    }
}
