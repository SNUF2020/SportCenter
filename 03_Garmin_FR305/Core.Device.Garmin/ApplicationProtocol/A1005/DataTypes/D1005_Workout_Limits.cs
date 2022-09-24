using System;
using System.Text;
using System.Runtime.InteropServices;

using uint32 = System.UInt32;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A1005.DataTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct D1005_Workout_Limits
    {
        public uint32 max_workouts;                /* Maximum workouts */
        public uint32 max_unscheduled_workouts;    /* Maximum unscheduled workouts */
        public uint32 max_occurrences;             /* Maximum workout occurrences */

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(String.Format(@"max_workouts :  {0}", this.max_workouts));
            sb.AppendLine(String.Format(@"max_unscheduled_workouts : {0}", this.max_unscheduled_workouts));
            sb.AppendLine(String.Format(@"max_occurrences : {0}", this.max_occurrences));

            return sb.ToString();
        }
    }
}
