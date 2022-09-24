using System;
using System.Text;
using System.Runtime.InteropServices;

using uint8 = System.Byte;
using uint16 = System.UInt16;

using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A1000.Interfaces;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A1002.DataTypes;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A1000.DataTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct D1009_Run_Type : IA1000
    {
        public uint16 track_index;          /* Index of associated track */
        public uint16 first_lap_index;      /* Index of first associated lap */
        public uint16 last_lap_index;       /* Index of last associated lap */
        public uint8 sport_type;            /* Same as D1000 */
        public uint8 program_type;          /* See below */
        public uint8 multisport;            /* See below */
        public uint8 unused1;               /* Unused. Set to 0. */
        public uint16 unused2;              /* Unused. Set to 0. */
        public D1009_Run_Type_Quick_Workout quick_workout;
        public D1008_Workout_Type workout;  /* Workout */

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public uint8[] unknown;

        /// <summary>
        /// Constructor to create a D1009_Run_Type type from its byte array representative.
        /// </summary>
        /// <param name="data">The byte array representation of the struct</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException">If the length of the data parameter is less than that of the minimum size of the struct</exception>
        public D1009_Run_Type(byte[] data)
        {
            const int typeSizeWithoutUnknownBytes = 681;  //TODO: Garmin Forerunner returns a data size of 684??

            if (data == null)
                throw new ArgumentNullException();

            if (data.Length < typeSizeWithoutUnknownBytes)
                throw new ArgumentException("TODO: (must localize) D1009_Run_Type. length of data cannot be less than size of data structure");

            int offset = 0;

            this.track_index = BitConverter.ToUInt16(data, offset);
            offset = offset + 2;

            this.first_lap_index = BitConverter.ToUInt16(data, offset);
            offset = offset + 2;

            this.last_lap_index = BitConverter.ToUInt16(data, offset);
            offset = offset + 2;

            this.sport_type = data[offset];
            offset = offset + 1;

            this.program_type = data[offset];
            offset = offset + 1;

            this.multisport = data[offset];
            offset = offset + 1;

            this.unused1 = data[offset];
            offset = offset + 1;

            this.unused2 = BitConverter.ToUInt16(data, offset);
            offset = offset + 2;

            int sizeOfD1009RuntypeQuickWorkout = Marshal.SizeOf(new D1009_Run_Type_Quick_Workout());
            byte[] buffer = new byte[sizeOfD1009RuntypeQuickWorkout];
            Buffer.BlockCopy(data, offset, buffer, 0, sizeOfD1009RuntypeQuickWorkout);
            this.quick_workout = new D1009_Run_Type_Quick_Workout(buffer);
            offset = offset + sizeOfD1009RuntypeQuickWorkout;

            int sizeOfD1008WorkoutType = Marshal.SizeOf(new D1008_Workout_Type());
            byte[] buffer1 = new byte[sizeOfD1008WorkoutType];
            Buffer.BlockCopy(data, offset, buffer1, 0, sizeOfD1008WorkoutType);
            this.workout = new D1008_Workout_Type(buffer1);
            offset = offset + sizeOfD1008WorkoutType;

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

            sb.AppendLine(string.Format(@"track_index : {0}", track_index));
            sb.AppendLine(string.Format(@"first_lap_index : {0}", first_lap_index));
            sb.AppendLine(string.Format(@"last_lap_index : {0}", last_lap_index));
            sb.AppendLine(string.Format(@"sport_type : {0}", sport_type));
            sb.AppendLine(string.Format(@"program_type : {0}", program_type));
            sb.AppendLine(string.Format(@"multisport : {0}", multisport));
            sb.AppendLine(string.Format(@"unused1 : {0}", unused1));
            sb.AppendLine(string.Format(@"unused2 : {0}", unused2));
            sb.AppendLine(quick_workout.ToString());
            sb.AppendLine(workout.ToString());

            return sb.ToString();
        }
    }
}
