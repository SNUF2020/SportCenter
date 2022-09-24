using System;
using System.Runtime.InteropServices;
using System.Text;

using uint8 = System.Byte;
using uint16 = System.UInt16;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A600.DataTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct D600_Date_Time_Type
    {
        public uint8 month;            /* month  (1-12)                  */
        public uint8 day;              /* day    (1-31)                  */
        public uint16 year;            /* year   (1990 means 1990)       */
        public uint16 hour;            /* hour   (0-23)                  */
        public uint8 minute;           /* minute (0-59)                  */
        public uint8 second;           /* second (0-59)                  */

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(String.Format(@"month :  {0}", this.month));
            sb.AppendLine(String.Format(@"day : {0}", this.day));
            sb.AppendLine(String.Format(@"year : {0}", this.year));
            sb.AppendLine(String.Format(@"hour :  {0}", this.hour));
            sb.AppendLine(String.Format(@"minute : {0}", this.minute));
            sb.AppendLine(String.Format(@"second : {0}", this.second));

            return sb.ToString();
        }
    }
}
