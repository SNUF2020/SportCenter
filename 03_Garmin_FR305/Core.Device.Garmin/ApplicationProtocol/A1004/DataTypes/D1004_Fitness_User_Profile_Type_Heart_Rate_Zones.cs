using System;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

using uint8 = System.Byte;
using uint16 = System.UInt16;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A1004.DataTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct D1004_Fitness_User_Profile_Type_Heart_Rate_Zones
    {
        public uint8 low_heart_rate;    /* In beats-per-minute, must be > 0 */
        public uint8 high_heart_rate;   /* In beats-per-minute, must be > 0 */
        public uint16 unused;           /* Unused. Set to 0. */

        public D1004_Fitness_User_Profile_Type_Heart_Rate_Zones(byte[] data)
        {
            const int typeSizeWithoutUnknownBytes = 4;

            if (data == null)
                throw new ArgumentNullException();

            if (data.Length < typeSizeWithoutUnknownBytes)
                throw new ArgumentException("TODO: (must localize) D1004_Fitness_User_Profile_Type_Heart_Rate_Zones. length of data cannot be less than size of data structure");

            int offset = 0;

            this.low_heart_rate = data[offset];
            offset = offset + 1;

            this.high_heart_rate = data[offset];
            offset = offset + 1;

            this.unused = BitConverter.ToUInt16(data, offset);
            offset = offset + 2;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format(@"low_heart_rate : {0}", low_heart_rate));
            sb.AppendLine(string.Format(@"high_heart_rate : {0}", high_heart_rate));
            sb.AppendLine(string.Format(@"unused : {0}", unused));

            return sb.ToString();
        }
    }
}
