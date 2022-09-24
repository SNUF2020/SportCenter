using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SC._02_SC_IO
{
    class ZipReader : IDisposable
    {
        private XmlReader ZipReader_;

        public List<TrackPoint> trackdata_list_zip = new List<TrackPoint>();

        public enum ZipObjectType { None, Zip };

        public ZipObjectType ObjectType { get; private set; }


        public ZipReader(string zip)
        {
            ZipReader_ = XmlReader.Create(new StringReader(zip)); // zip = Unzip-file

            while (ZipReader_.Read())
            {
                switch (ZipReader_.NodeType)
                {
                    case XmlNodeType.Element:
                        if (ZipReader_.Name != "zip-file") throw new FormatException(ZipReader_.Name); // Testing if this file is a zip file
                        ObjectType = ZipObjectType.Zip;
                        return;
                }
            }

            throw new FormatException();
        }

        public void Dispose()
        {
            ZipReader_.Close();
        }

        public bool Read()
        {
            if (ObjectType == ZipObjectType.None) return false;

            while (ZipReader_.Read())
            {
                switch (ZipReader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (ZipReader_.Name)
                        {
                            case "zip":
                                trackdata_list_zip = ReadTrack_zip();
                                return true;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (ZipReader_.Name != "zip-file" & ZipReader_.Name != "zip") throw new FormatException(ZipReader_.Name);
                        ObjectType = ZipObjectType.None;
                        return false;
                }
            }
            return false;
        }

        private List<TrackPoint> ReadTrack_zip()
        {

            List<TrackPoint> trackdata_list_zip = new List<TrackPoint>();
            if (ZipReader_.IsEmptyElement) return trackdata_list_zip;

            string elementName = ZipReader_.Name;

            while (ZipReader_.Read())
            {
                //Console.WriteLine("Hurra2!"); // Hier kommt der zip_Reader nicht hin!!!
                switch (ZipReader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (ZipReader_.Name)
                        {
                            case "pt":
                                trackdata_list_zip.Add(getTrackPoint());
                                break;

                        }
                        break;

                    case XmlNodeType.EndElement:
                        if (ZipReader_.Name != elementName) throw new FormatException(ZipReader_.Name);
                        return trackdata_list_zip;
                }
            }
            throw new FormatException(elementName);
        }

        private TrackPoint getTrackPoint()
        {
            TrackPoint ptattributes = new TrackPoint();

            while (ZipReader_.MoveToNextAttribute())
            {
                switch (ZipReader_.Name)
                {
                    case "tm":
                        ptattributes.tm = Convert.ToInt32(ZipReader_.Value, CultureInfo.InvariantCulture);
                        break;
                    case "lat":
                        ptattributes.lat = Convert.ToDouble(ZipReader_.Value, CultureInfo.InvariantCulture);
                        break;
                    case "lon":
                        ptattributes.lon = Convert.ToDouble(ZipReader_.Value, CultureInfo.InvariantCulture);
                        break;
                    case "ele":
                        ptattributes.ele = Convert.ToDouble(ZipReader_.Value, CultureInfo.InvariantCulture);
                        break;
                    case "dist":
                        ptattributes.dist = Convert.ToDouble(ZipReader_.Value, CultureInfo.InvariantCulture);
                        break;
                    case "hr":
                        ptattributes.hr = Convert.ToDouble(ZipReader_.Value, CultureInfo.InvariantCulture);
                        break;
                    case "cadence":
                        ptattributes.cadence = Convert.ToDouble(ZipReader_.Value, CultureInfo.InvariantCulture);
                        break;
                    case "power":
                        ptattributes.power = Convert.ToDouble(ZipReader_.Value, CultureInfo.InvariantCulture);
                        break;
                        // speed is missing
                }
            }

            return ptattributes;
        }
    }
}
