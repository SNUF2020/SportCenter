using System;
using System.Text;
using System.Runtime.InteropServices;

using uint8 = System.Byte;
using uint16 = System.UInt16;
using float32 = System.Single;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A1002.DataTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct D1008_Workout_Type_Step
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] custom_name;                 /* Null-terminated step name */
        public float32 target_custom_zone_low;     /* See below */
        public float32 target_custom_zone_high;    /* See below */
        public uint16 duration_value;              /* Same as D1002 */
        public uint8 intensity;                    /* Same as D1001 */
        public uint8 duration_type;                /* Same as D1002 */
        public uint8 target_type;                  /* See below */
        public uint8 target_value;                 /* See below */
        public uint16 unused;                      /* Unused. Set to 0. */

        public D1008_Workout_Type_Step(byte[] data)
        {
            const int typeSizeWithoutUnknownBytes = 32;

            if (data == null)
                throw new ArgumentNullException();

            if (data.Length < typeSizeWithoutUnknownBytes)
                throw new ArgumentException("TODO: (must localize) D1008_Workout_Type_Step. length of data cannot be less than size of data structure");

            int offset = 0;

            this.custom_name = new byte[16];
            Buffer.BlockCopy(data, offset, this.custom_name, 0, 16);
            offset = offset + 16;

            this.target_custom_zone_low = BitConverter.ToSingle(data, offset);
            offset = offset + 4;

            this.target_custom_zone_high = BitConverter.ToSingle(data, offset);
            offset = offset + 4;

            this.duration_value = BitConverter.ToUInt16(data, offset);
            offset = offset + 2;

            this.intensity = data[offset];
            offset = offset + 1;

            this.duration_type = data[offset];
            offset = offset + 1;

            this.target_type = data[offset];
            offset = offset + 1;

            this.target_value = data[offset];
            offset = offset + 1;

            this.unused = BitConverter.ToUInt16(data, offset);
            offset = offset + 2;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            //TODO: Should scan for the first occurence ofa null terminator and return the text before that
            sb.AppendLine(string.Format(@"custom_name : {0}", Encoding.ASCII.GetString(custom_name).Split('\0')[0]));
            sb.AppendLine(string.Format(@"target_custom_zone_low : {0}", target_custom_zone_low));
            sb.AppendLine(string.Format(@"target_custom_zone_high : {0}", target_custom_zone_high));
            sb.AppendLine(string.Format(@"duration_value : {0}", duration_value));
            sb.AppendLine(string.Format(@"intensity : {0}", intensity));
            sb.AppendLine(string.Format(@"duration_type : {0}", duration_type));
            sb.AppendLine(string.Format(@"target_type : {0}", target_type));
            sb.AppendLine(string.Format(@"target_value : {0}", target_value));
            sb.AppendLine(string.Format(@"unused : {0}", unused));

            return sb.ToString();
        }
    }
}
