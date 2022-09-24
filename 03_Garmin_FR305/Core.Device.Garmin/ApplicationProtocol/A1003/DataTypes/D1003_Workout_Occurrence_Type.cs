using System;
using System.Text;
using System.Runtime.InteropServices;

using time_type = System.UInt32;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A1003.DataTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct D1003_Workout_Occurrence_Type
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] workout_name;         /* Null-terminated workout name */
        public time_type day;               /* Day on which the workout falls */

        public D1003_Workout_Occurrence_Type(byte[] data)
        {
            const int typeSizeWithoutUnknownBytes = 20;

            if (data == null)
                throw new ArgumentNullException();

            if (data.Length < typeSizeWithoutUnknownBytes)
                throw new ArgumentException("TODO: (must localize) D1003_Workout_Occurrence_Type. Length of data cannot be less than size of data structure");

            int offset = 0;

            this.workout_name = new byte[16];
            Buffer.BlockCopy(data, offset, this.workout_name, 0, 16);
            offset = offset + 16;

            this.day = BitConverter.ToUInt32(data, offset);
            offset = offset + 4;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format(@"workout_name : {0}", Encoding.ASCII.GetString(workout_name).Split('\0')[0]));
            sb.AppendLine(string.Format(@"day : {0}", day));

            return sb.ToString();
        }

    }
}
