using System;
using System.Text;
using System.Runtime.InteropServices;

using uint8 = System.Byte;
using uint16 = System.UInt16;
using uint32 = System.UInt32;
using float32 = System.Single;
using time_type = System.UInt32;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A906.Interfaces;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.Common.DataTypes;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A906.DataTypes
{
    /// <summary>
    ///  NB!. Forerunner 205/305 returns 48 data bytes when transfering laps. 
    ///  This can either map onto D1001_Lap_Type with 7 unknown bytes or
    ///  D1011 with 5 unknown bytes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct D1015_Lap_Type : IA906
    {
        public uint16 index;            /* Unique among all laps received from device */
        public uint16 unused;           /* Unused. Set to 0. */
        public time_type start_time;    /* Start of lap time */
        public uint32 total_time;       /* Duration of lap, in hundredths of a second */
        public float32 total_dist;      /* Distance in meters */
        public float32 max_speed;       /* In meters per second */
        public position_type begin;     /* Invalid if both lat and lon are 0x7FFFFFFF */
        public position_type end;       /* Invalid if both lat and lon are 0x7FFFFFFF */
        public uint16 calories;         /* Calories burned this lap */
        public uint8 avg_heart_rate;    /* In beats-per-minute, 0 if invalid */
        public uint8 max_heart_rate;    /* In beats-per-minute, 0 if invalid */
        public uint8 intensity;         /* Same as D1001 */
        public uint8 avg_cadence;       /* In revolutions-per-minute, 0xFF if invalid */
        public uint8 trigger_method;    /* See below */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public uint8[] unknown;

        /// <summary>
        /// Constructor to create a D1015 type from its byte array representative.
        /// </summary>
        /// <param name="data">The byte array representation of the struct</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException">If the length of the data parameter is less than that of the minimum size of the struct</exception>
        public D1015_Lap_Type(byte[] data)
        {
            const int typeSizeWithoutUnknownBytes = 43;

            if (data == null)
                throw new ArgumentNullException();

            if (data.Length < typeSizeWithoutUnknownBytes)
                throw new ArgumentException("TODO: (must localize) D1015_Lap_Type. length of data cannot be less than size of data structure");

            int offset = 0;

            index = BitConverter.ToUInt16(data, offset);
            offset = offset + 2;

            unused = BitConverter.ToUInt16(data, offset);
            offset = offset + 2;

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

            avg_cadence = data[offset];
            offset = offset + 1;

            trigger_method = data[offset];
            offset = offset + 1;

            int unknownLength = data.Length - typeSizeWithoutUnknownBytes;
            if (unknownLength > 0)
            {
                unknown = new byte[unknownLength];
                Buffer.BlockCopy(data, offset, unknown, 0, unknownLength);
            }
            else
            {
                unknown = null;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format(@"index : {0}", index));
            sb.AppendLine(string.Format(@"unused : {0}", unused));
            sb.AppendLine(string.Format(@"start_time : {0}", start_time));
            sb.AppendLine(string.Format(@"total_time : {0}", total_time));
            sb.AppendLine(string.Format(@"total_dist : {0}", total_dist));
            sb.AppendLine(string.Format(@"max_speed : {0}", max_speed));
            sb.AppendLine(string.Format(@"begin.lat : {0}", begin.lat.ToString()));
            sb.AppendLine(string.Format(@"begin.lon : {0}", begin.lon.ToString()));
            sb.AppendLine(string.Format(@"end.lat : {0}", end.lat.ToString()));
            sb.AppendLine(string.Format(@"end.lon : {0}", end.lon.ToString()));
            sb.AppendLine(string.Format(@"calories : {0}", calories));
            sb.AppendLine(string.Format(@"avg_heart_rate : {0}", avg_heart_rate));
            sb.AppendLine(string.Format(@"max_heart_rate : {0}", max_heart_rate));
            sb.AppendLine(string.Format(@"intensity : {0}", intensity));
            sb.AppendLine(string.Format(@"avg_cadence : {0}", avg_cadence));
            sb.AppendLine(string.Format(@"trigger_method : {0}", trigger_method));

            return sb.ToString();

        }
    }
}
