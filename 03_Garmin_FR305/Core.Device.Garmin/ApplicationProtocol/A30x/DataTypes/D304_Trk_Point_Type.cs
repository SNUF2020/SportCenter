using System;
using System.Text;
using System.Runtime.InteropServices;

using uint8 = System.Byte;
using float32 = System.Single;
using gbool = System.Boolean;
using time_type = System.UInt32;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A30x.Interfaces;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.Common.DataTypes;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A30x.DataTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct D304_Trk_Point_Type : ITrkPointType
    {
        public position_type posn;
        public time_type time;
        public float32 alt;
        public float32 distance;
        public uint8 heart_rate;
        public uint8 cadence;
        public gbool sensor;
        //public double real_lat;
        //public double real_lon;

        /// <summary>
        /// Constructor to create a D304 type from its byte array representative.
        /// </summary>
        /// <param name="data">The byte array representation of the struct</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException">If the length of the data parameter is less than that of the minimum size of the struct</exception>
        public D304_Trk_Point_Type(byte[] data)
        {
            const int typeSizeWithoutUnknownBytes = 24;

            if (data == null)
                throw new ArgumentNullException();

            if (data.Length < typeSizeWithoutUnknownBytes)
                throw new ArgumentException("TODO: (must localize) D304_Trk_Point_Type. length of data cannot be less than size of data structure");

            int offset = 0;

            posn.lat = BitConverter.ToInt32(data, offset);
            //real_lat = posn.lat * 180 / 2147483648;
            offset = offset + 4;

            posn.lon = BitConverter.ToInt32(data, offset);
            //real_lat = posn.lon * 180 / 2147483648;
            offset = offset + 4;

            time = BitConverter.ToUInt32(data, offset);
            offset = offset + 4;

            alt = BitConverter.ToSingle(data, offset);
            offset = offset + 4;

            distance = BitConverter.ToSingle(data, offset);
            offset = offset + 4;

            heart_rate = data[offset];
            offset = offset + 1;

            cadence = data[offset];
            offset = offset + 1;

            sensor = BitConverter.ToBoolean(data, offset); //In c#, booleans are 4 bytes
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format(@"posn.lat : {0}", posn.lat * 180d / 2147483648d));
            sb.AppendLine(string.Format(@"posn.lon : {0}", posn.lon * 180d / 2147483648d));
            sb.AppendLine(string.Format(@"time : {0}", time));
            sb.AppendLine(string.Format(@"alt : {0}", alt));
            sb.AppendLine(string.Format(@"distance : {0}", distance));
            sb.AppendLine(string.Format(@"heart_rate : {0}", heart_rate));
            sb.AppendLine(string.Format(@"cadence : {0}", cadence));
            sb.AppendLine(string.Format(@"sensor : {0}", sensor));

            return sb.ToString();
        }
    }
}
