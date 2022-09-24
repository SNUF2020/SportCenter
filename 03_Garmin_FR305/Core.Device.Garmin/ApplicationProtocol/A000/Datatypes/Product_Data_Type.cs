using System;
using System.Text;
using System.Runtime.InteropServices;

using uint16 = System.UInt16;
using sint16 = System.Int16;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A000.Datatypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    //public unsafe struct Product_Data_Type
    public struct Product_Data_Type
    {
        public uint16 product_ID;
        public sint16 software_version;
        // <summary>
        //  7.3.3(SDK) A variable-length string is a null terminated string that can be any size
        // as long as it does not cause a data packet to become larger than the max allowable data
        // packet size. .. Whenever possible the variable length strings are placed at the end
        // of the data structure to minimize need for run-time offset calculations.
        /// So using this the first 2 fields are 4 bytes in total so we can assume the variable
        // length string can be a max of 64-4=60 bytes long.
        // </summary>
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst=60)]
        public char[] product_description; /* null-terminated string */
        /* char **         additional_data;*/
        /*...  zero or more additional null-terminated strings - MM Can be ignored*/

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format(@"product_ID : {0}", product_ID));
            sb.AppendLine(string.Format(@"software_version : {0}", software_version));
            sb.AppendLine(string.Format(@"product_description : {0}", new string(product_description)));

            return sb.ToString();
        }
    }
}
