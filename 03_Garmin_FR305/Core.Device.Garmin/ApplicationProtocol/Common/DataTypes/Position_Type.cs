using System;
using System.Text;
using System.Runtime.InteropServices;

using sint32 = System.Int32;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.Common.DataTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct position_type
    {
        public sint32 lat;  /* latitude in semicircles */
        public sint32 lon;  /* longitude in semicircles */

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format(@"lat : {0}", lat));
            sb.AppendLine(string.Format(@"lon : {0}", lon));

            return sb.ToString();
        }
    }
}
