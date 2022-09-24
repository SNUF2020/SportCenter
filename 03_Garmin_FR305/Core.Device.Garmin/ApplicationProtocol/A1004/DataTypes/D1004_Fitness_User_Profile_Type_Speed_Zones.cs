using System;
using System.Text;
using System.Runtime.InteropServices;

using float32 = System.Single;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A1004.DataTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct D1004_Fitness_User_Profile_Type_Speed_Zones
    {
        public float32 low_speed;   /* In meters-per-second */
        public float32 high_speed;  /* In meters-per-second */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] name;         /* Null-terminated speed-zone name */

        public D1004_Fitness_User_Profile_Type_Speed_Zones(byte[] data)
        {
            const int typeSizeWithoutUnknownBytes = 24;

            if (data == null)
                throw new ArgumentNullException();

            if (data.Length < typeSizeWithoutUnknownBytes)
                throw new ArgumentException("TODO: (must localize) D1004_Fitness_User_Profile_Type_Speed_Zones. Length of data cannot be less than size of data structure");

            int offset = 0;

            this.low_speed = BitConverter.ToSingle(data, offset);
            offset = offset + 4;

            this.high_speed = BitConverter.ToSingle(data, offset);
            offset = offset + 4;

            this.name = new byte[16];
            Buffer.BlockCopy(data, offset, this.name, 0, 16);
            offset = offset + 16;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format(@"low_speed : {0}", low_speed));
            sb.AppendLine(string.Format(@"high_speed : {0}", high_speed));
            sb.AppendLine(string.Format(@"name : {0}", Encoding.ASCII.GetString(name).Split('\0')[0]));

            return sb.ToString();
        }
    }
}
