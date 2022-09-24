using System;
using System.Runtime.InteropServices;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A000.Datatypes
{
    //TODO: convert to class
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct Ext_Product_Data_Type
    {
        /* ...  zero or more additional null-terminated strings */
        public char[] value;
    }
}
