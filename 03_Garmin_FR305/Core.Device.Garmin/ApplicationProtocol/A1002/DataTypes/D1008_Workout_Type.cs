using System;
using System.Text;
using System.Runtime.InteropServices;

using uint8 = System.Byte;
using uint32 = System.UInt32;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A1002.Interfaces;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A1002.DataTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct D1008_Workout_Type : IA1002
    {
        public uint32 num_valid_steps;  /* Number of valid steps (1-20) */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public D1008_Workout_Type_Step[] steps;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] name;             /* Null-terminated workout name */
        public uint8 sport_type;        /* Same as D1000 */

        public D1008_Workout_Type(byte[] data)
        {
            const int typeSizeWithoutUnknownBytes = 661;

            if (data == null)
                throw new ArgumentNullException();

            if (data.Length < typeSizeWithoutUnknownBytes)
                throw new ArgumentException("TODO: (must localize) D1008_Workout_Type. length of data cannot be less than size of data structure");

            int offset = 0;

            this.num_valid_steps = BitConverter.ToUInt32(data, offset);
            offset = offset + 4;

            this.steps = new D1008_Workout_Type_Step[20];
            for (int i = 0; i < 20; i++)
            {
                int sizeOfD1008WorkoutTypeStep = Marshal.SizeOf(new D1008_Workout_Type_Step());
                byte[] buffer = new byte[sizeOfD1008WorkoutTypeStep];
                Buffer.BlockCopy(data, offset, buffer, 0, sizeOfD1008WorkoutTypeStep);

                this.steps[i] = new D1008_Workout_Type_Step(buffer);
                offset = offset + sizeOfD1008WorkoutTypeStep;
            }

            this.name = new byte[16];
            Buffer.BlockCopy(data, offset, this.name, 0, 16);
            offset = offset + 16;

            this.sport_type = data[offset];
            offset = offset + 1;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format(@"num_valid_steps : {0}", num_valid_steps));
            for (int i = 0; i < num_valid_steps; i++)
            {
                sb.AppendLine(string.Format(@" --- Step {0} --- ", i));
                sb.Append(steps[i]);
            }

            sb.AppendLine();
            sb.AppendLine(string.Format(@"name : {0}", Encoding.ASCII.GetString(name).Split('\0')[0]));
            sb.AppendLine(string.Format(@"sport_type : {0}", sport_type));

            return sb.ToString();
        }
    }
}
