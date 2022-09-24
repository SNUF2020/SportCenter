using System;
using System.Text;
using System.Runtime.InteropServices;

using uint8 = System.Byte;
using uint16 = System.UInt16;
using float32 = System.Single;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A1004.DataTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct D1004_Fitness_User_Profile_Type_Activities
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public D1004_Fitness_User_Profile_Type_Heart_Rate_Zones[] heart_rate_zones;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public D1004_Fitness_User_Profile_Type_Speed_Zones[] speed_zones;

        public float32 gear_weight;     /* Weight of equipment in kilograms */
        public uint8 max_heart_rate;    /* In beats-per-minute, must be > 0 */
        public uint8 unused1;           /* Unused. Set to 0. */
        public uint16 unused2;          /* Unused. Set to 0. */

        public D1004_Fitness_User_Profile_Type_Activities(byte[] data)
        {
            const int typeSizeWithoutUnknownBytes = 0;

            if (data == null)
                throw new ArgumentNullException();

            if (data.Length < typeSizeWithoutUnknownBytes)
                throw new ArgumentException("TODO: (must localize) D1004_Fitness_User_Profile_Type_Heart_Rate_Zones. length of data cannot be less than size of data structure");

            int offset = 0;

            this.heart_rate_zones = new D1004_Fitness_User_Profile_Type_Heart_Rate_Zones[5];
            for (int i = 0; i < 5; i++)
            {
                int sizeOfD1004FitnessUserProfileTypeHeartRateZones = Marshal.SizeOf(new D1004_Fitness_User_Profile_Type_Heart_Rate_Zones());
                byte[] buffer = new byte[sizeOfD1004FitnessUserProfileTypeHeartRateZones];
                Buffer.BlockCopy(data, offset, buffer, 0, sizeOfD1004FitnessUserProfileTypeHeartRateZones);

                this.heart_rate_zones[i] = new D1004_Fitness_User_Profile_Type_Heart_Rate_Zones(buffer);
                offset = offset + sizeOfD1004FitnessUserProfileTypeHeartRateZones;
            }

            this.speed_zones = new D1004_Fitness_User_Profile_Type_Speed_Zones[10];
            for (int i = 0; i < 10; i++)
            {
                int sizeOfD1004FitnessUserProfileTypeSpeedZones = Marshal.SizeOf(new D1004_Fitness_User_Profile_Type_Speed_Zones());
                byte[] buffer = new byte[sizeOfD1004FitnessUserProfileTypeSpeedZones];
                Buffer.BlockCopy(data, offset, buffer, 0, sizeOfD1004FitnessUserProfileTypeSpeedZones);

                this.speed_zones[i] = new D1004_Fitness_User_Profile_Type_Speed_Zones(buffer);
                offset = offset + sizeOfD1004FitnessUserProfileTypeSpeedZones;
            }

            this.gear_weight = BitConverter.ToSingle(data, offset);
            offset = offset + 4;

            this.max_heart_rate = data[offset];
            offset = offset + 1;

            this.unused1 = data[offset];
            offset = offset + 1;

            this.unused2 = BitConverter.ToUInt16(data, offset);
            offset = offset + 2;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (D1004_Fitness_User_Profile_Type_Heart_Rate_Zones hrz in heart_rate_zones)
            {
                sb.Append(hrz.ToString());
            }

            foreach (D1004_Fitness_User_Profile_Type_Speed_Zones sz in speed_zones)
            {
                sb.Append(sz.ToString());
            }
            sb.AppendLine(string.Format(@"gear_weight : {0}", gear_weight));
            sb.AppendLine(string.Format(@"max_heart_rate : {0}", max_heart_rate));
            sb.AppendLine(string.Format(@"unused1 : {0}", unused1));
            sb.AppendLine(string.Format(@"unused2 : {0}", unused2));

            return sb.ToString();
        }
    }
}
