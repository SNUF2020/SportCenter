using System;
using System.Text;
using System.Runtime.InteropServices;

using uint32 = System.UInt32;
using float32 = System.Single;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A1000.DataTypes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct D1009_Run_Type_Quick_Workout
    {
        public uint32 time;         /* Time result of quick workout */
        public float32 distance;    /* Distance result of quick workout */

        /// <summary>
        /// Constructor to create a D1009_Run_Type_Quick_Workout type from its byte array representative.
        /// </summary>
        /// <param name="data">The byte array representation of the struct</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException">If the length of the data parameter is less than that of the minimum size of the struct</exception>
        public D1009_Run_Type_Quick_Workout(byte[] data)
        {
            const int typeSizeWithoutUnknownBytes = 8;

            if (data == null)
                throw new ArgumentNullException();

            if (data.Length < typeSizeWithoutUnknownBytes)
                throw new ArgumentException("TODO: (must localize) D1009_Run_Type_Quick_Workout. length of data cannot be less than size of data structure");

            int offset = 0;

            time = BitConverter.ToUInt16(data, offset);
            offset = offset + 4;

            distance = BitConverter.ToSingle(data, offset);
            offset = offset + 4;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format(@"time : {0}", time));
            sb.AppendLine(string.Format(@"distance : {0}", distance));

            return sb.ToString();
        }
    }
}
