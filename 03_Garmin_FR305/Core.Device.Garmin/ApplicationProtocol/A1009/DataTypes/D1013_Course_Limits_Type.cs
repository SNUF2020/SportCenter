using System;
using System.Text;
using System.Runtime.InteropServices;

using uint32 = System.UInt32;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A1009.DataTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct D1013_Course_Limits_Type
    {
        public uint32 max_courses;          /* Maximum courses */
        public uint32 max_course_laps;      /* Maximum course laps */
        public uint32 max_course_pnt;       /* Maximum course points */
        public uint32 max_course_trk_pnt;   /* Maximum course track points */

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format(@"max_courses : {0}", max_courses));
            sb.AppendLine(string.Format(@"max_course_laps : {0}", max_course_laps));
            sb.AppendLine(string.Format(@"max_course_pnt : {0}", max_course_pnt));
            sb.AppendLine(string.Format(@"max_course_trk_pnt : {0}", max_course_trk_pnt));

            return sb.ToString();
        }
    }
}
