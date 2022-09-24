using System;
using System.Text;
using System.Runtime.InteropServices;

using uint16 = System.UInt16;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A30x.Interfaces;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A30x.DataTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct D311_Trk_Hdr_Type : ITrkHdrType
    {
        public uint16 index;   /* unique among all tracks received from device */

        /// <summary>
        /// Constructor to create a D311 type from its byte array representative.
        /// Note. On a Forerunner 305 version 2.90 of the software, 4 bytes are returned. The last 2 bytes are ignored
        /// </summary>
        /// <param name="data">The byte array representation of the struct</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException">If the length of the data parameter is less than that of the minimum size of the struct</exception>
        public D311_Trk_Hdr_Type(byte[] data)
        {
            const int typeSizeWithoutUnknownBytes = 2;

            if (data == null)
                throw new ArgumentNullException();

            if (data.Length < typeSizeWithoutUnknownBytes)
                throw new ArgumentException("TODO: (must localize) D311_Trk_Hdr_Type. length of data cannot be less than size of data structure");

            int offset = 0;

            index = BitConverter.ToUInt16(data, offset);
            offset = offset + 2;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format(@"index : {0}", index));

            return sb.ToString();
        }
    }
}
