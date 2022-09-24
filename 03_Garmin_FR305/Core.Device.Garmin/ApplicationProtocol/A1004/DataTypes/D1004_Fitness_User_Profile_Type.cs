using System;
using System.Text;
using System.Runtime.InteropServices;

using uint8 = System.Byte;
using uint16 = System.UInt16;
using float32 = System.Single;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A1004.DataTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct D1004_Fitness_User_Profile_Type
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public D1004_Fitness_User_Profile_Type_Activities[] activities;
        public float32 weight;      /* User’s weight, in kilograms */
        public uint16 birth_year;   /* No base value (i.e. 1990 means 1990) */
        public uint8 birth_month;   /* 1 = January, etc. */
        public uint8 birth_day;     /* 1 = first day of month, etc. */
        public uint8 gender;        /* See below */

        public D1004_Fitness_User_Profile_Type(byte[] data)
        {
            const int typeSizeWithoutUnknownBytes = 0;

            if (data == null)
                throw new ArgumentNullException();

            if (data.Length < typeSizeWithoutUnknownBytes)
                throw new ArgumentException("TODO: (must localize) D1004_Fitness_User_Profile_Type. Length of data cannot be less than size of data structure");

            int offset = 0;

            this.activities = new D1004_Fitness_User_Profile_Type_Activities[3];
            for (int i = 0; i < 3; i++)
            {
                int sizeOfD1004FitnessUserProfileTypeActivities = Marshal.SizeOf(new D1004_Fitness_User_Profile_Type_Activities());
                byte[] buffer = new byte[sizeOfD1004FitnessUserProfileTypeActivities];
                Buffer.BlockCopy(data, offset, buffer, 0, sizeOfD1004FitnessUserProfileTypeActivities);

                this.activities[i] = new D1004_Fitness_User_Profile_Type_Activities(buffer);
                offset = offset + sizeOfD1004FitnessUserProfileTypeActivities;
            }

            this.weight = BitConverter.ToSingle(data, offset);
            offset = offset + 4;

            this.birth_year = BitConverter.ToUInt16(data, offset);
            offset = offset + 2;

            this.birth_month = data[offset];
            offset = offset + 1;

            this.birth_day = data[offset];
            offset = offset + 1;

            this.gender = data[offset];
            offset = offset + 1;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (D1004_Fitness_User_Profile_Type_Activities up in activities)
            {
                sb.Append(up.ToString());
            }
            sb.AppendLine(string.Format(@"weight : {0}", weight));
            sb.AppendLine(string.Format(@"birth_year : {0}", birth_year));
            sb.AppendLine(string.Format(@"birth_month : {0}", birth_month));
            sb.AppendLine(string.Format(@"birth_day : {0}", birth_day));
            sb.AppendLine(string.Format(@"gender : {0}", gender));

            return sb.ToString();
        }

    }
}
