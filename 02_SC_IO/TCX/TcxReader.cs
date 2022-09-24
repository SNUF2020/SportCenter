using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static SC.Tcx;

namespace SC
{
    public class TcxReader : IDisposable
    {
        private XmlReader Reader_;

        public GpxObjectType ObjectType { get; private set; }
        public TcxAttributes Attributes { get; private set; }
        public TcxActivities TcxActivities { get; set; }

        public TcxReader(Stream stream)
        {
            Reader_ = XmlReader.Create(stream, new XmlReaderSettings { CloseInput = true });

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:
                        if (Reader_.Name != "TrainingCenterDatabase") throw new FormatException(Reader_.Name);
                        Attributes = ReadGpxAttribures();
                        ObjectType = GpxObjectType.Attributes;
                        return;
                }
            }

            throw new FormatException();
        }
        public void Dispose()
        {
            Reader_.Close();
        }

        private TcxAttributes ReadGpxAttribures()
        {
            TcxAttributes attributes = new TcxAttributes();

            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "version":
                        attributes.Version = Reader_.Value;
                        break;
                    case "creator":
                        attributes.Creator = Reader_.Value;
                        break;
                }
            }

            return attributes;
        }

        public bool Read()
        {
            if (ObjectType == GpxObjectType.None) return false;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "Activities":
                                TcxActivities = CheckActivities();
                                ObjectType = GpxObjectType.TcxActivities;
                                return true;
                            /*case "Folders": // in TACX-files nicht vorhanden
                                return true;
                            case "Workouts": // in TACX-files nicht vorhanden
                                return true;
                            case "Courses": // in TACX-files nicht vorhanden
                                return true;
                            case "Author": // in TACX-files nicht vorhanden
                                return true;
                            case "Extension": // in TACX-files nicht vorhanden
                                return true;*/
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != "TrainingCenterDatabase" & Reader_.Name != "Activities") throw new FormatException(Reader_.Name);
                        ObjectType = GpxObjectType.None;
                        return false;
                }
            }

            ObjectType = GpxObjectType.None;
            return false;
        }

        private TcxActivities CheckActivities()
        {
            TcxActivities _tcxActivities = new TcxActivities();
            if (Reader_.IsEmptyElement) return _tcxActivities;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "Activity":
                                _tcxActivities._activityData = ReadActivity();
                                break;
                            case "MultiSportSession": // not covered in this code
                                SkipElement();
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName & Reader_.Name != "Activity") throw new FormatException(Reader_.Name);

                        return _tcxActivities;
                }

            }

            throw new FormatException(elementName);
        }

        private TcxActivityData ReadActivity()
        {
            TcxActivityData activitydata = new TcxActivityData();
            if (Reader_.IsEmptyElement) return activitydata;

            activitydata.activityattributes.Sport = Reader_.GetAttribute("Sport");

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "Id":
                                activitydata.Id = ReadContentAsString();
                                // https://zetcode.com/csharp/xmlreader/
                                break;
                            case "Lap":
                                activitydata.lapdata = getLap();
                                break;
                            case "Notes": // in TACX-files nicht vorhanden
                                break;
                            case "Training": // in TACX-files nicht vorhanden
                                break;
                            case "Creator":
                                activitydata.creator = ReadCreatorData();
                                break;
                            case "Extensions": // in TACX-files nicht vorhanden
                                break;
                            default:
                                SkipElement();
                                break;
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName & Reader_.Name != "Lap") throw new FormatException(Reader_.Name);
                        return activitydata;
                }
            }
            throw new FormatException(elementName);
        }

        private CreatorData ReadCreatorData()
        {
            CreatorData _CreatorData = new CreatorData();
            if (Reader_.IsEmptyElement) return _CreatorData;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "Name":
                                _CreatorData.Name = ReadContentAsString();
                                break;
                            case "UnitId":
                                _CreatorData.UnitId = ReadContentAsString();
                                break;
                            case "ProductID":
                                _CreatorData.ProductID = ReadContentAsString();
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return _CreatorData;
                }
            }

            throw new FormatException(elementName);
        }



        private TcxLapData getLap()
        {
            TcxLapData data = new TcxLapData();
            data.lapattributes.StartTime = Reader_.GetAttribute("StartTime");

            if (Reader_.IsEmptyElement) return data;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "TotalTimeSeconds":
                                data.TotalTimeSeconds = ReadContentAsString();
                                break;
                            case "DistanceMeters":
                                data.DistanceMeters = ReadContentAsString();
                                break;
                            case "MaximumSpeed":
                                data.MaximumSpeed = ReadContentAsString();
                                break;
                            case "Calories":
                                data.Calories = ReadContentAsString();
                                break;
                            case "AverageHeartRateBpm":
                                data.AverageHeartRateBpm = ReadContentAsString();
                                break;
                            case "MaximumHeartRateBpm":
                                data.MaximumHeartRateBpm = ReadContentAsString();
                                break;
                            case "Intensity":
                                data.Intensity = ReadContentAsString();
                                break;
                            case "Cadence":
                                data.Cadence = ReadContentAsString();
                                break;
                            case "TriggerMethod":
                                data.TriggerMethod = ReadContentAsString();
                                break;
                            case "Track":
                                data.Track = ReadTcxTrack();
                                break;
                            case "Notes":
                                data.Notes = ReadContentAsString();
                                break;
                            case "Extensions":
                                data.Extensions = ReadContentAsString();
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return data;
                }
            }

            throw new FormatException(elementName);
        }

        private List<TcxTrackPoint> ReadTcxTrack()
        {
            List<TcxTrackPoint> trackdata_list = new List<TcxTrackPoint>();
            if (Reader_.IsEmptyElement) return trackdata_list;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "Trackpoint":
                                trackdata_list.Add(getTrackPoint());
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return trackdata_list;
                }
            }

            throw new FormatException(elementName);
        }

        private TcxTrackPoint getTrackPoint()
        {
            TcxTrackPoint tcxTP = new TcxTrackPoint();
            if (Reader_.IsEmptyElement) return tcxTP;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "Time":
                                tcxTP.Time = ReadContentAsString();
                                break;
                            case "Position":
                                tcxTP.Position = getTPPositon();
                                break;
                            case "AltitudeMeters":
                                tcxTP.AltitudeMeters = ReadContentAsString();
                                break;
                            case "DistanceMeters":
                                tcxTP.DistanceMeters = ReadContentAsString();
                                break;
                            case "HeartRateBpm":
                                tcxTP.HeartRateBpm = getHR();
                                break;
                            case "Cadence":
                                tcxTP.Cadence = ReadContentAsString();
                                break;
                            case "SensorState":
                                tcxTP.SensorState = ReadContentAsString();
                                break;
                            case "Extensions":
                                tcxTP.Extensions = getTPExtension();
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return tcxTP;
                }
            }

            throw new FormatException(elementName);
        }

        private string getHR()
        {
            string _HR = null;
            if (Reader_.IsEmptyElement) return _HR;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "Value":
                                _HR = ReadContentAsString();
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return _HR;
                }
            }

            throw new FormatException(elementName);
        }

        private PositionData getTPPositon()
        {
            PositionData newPosition = new PositionData();
            if (Reader_.IsEmptyElement) return newPosition;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "LatitudeDegrees":
                                newPosition.LatitudeDegrees = ReadContentAsString();
                                break;
                            case "LongitudeDegrees":
                                newPosition.LongitudeDegrees = ReadContentAsString();
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return newPosition;
                }
            }

            throw new FormatException(elementName);
        }

        private TrackExtensionData getTPExtension()
        {
            TrackExtensionData newTPExtension = new TrackExtensionData();
            if (Reader_.IsEmptyElement) return newTPExtension;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "ns3:TPX":
                                newTPExtension = getTPP();
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return newTPExtension;
                }
            }

            throw new FormatException(elementName);
        }

        private TrackExtensionData getTPP()
        {
            TrackExtensionData newTPExtension = new TrackExtensionData();
            if (Reader_.IsEmptyElement) return newTPExtension;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "ns3:Speed":
                                newTPExtension.Speed = ReadContentAsString();
                                break;
                            case "ns3:RunCadence":
                                newTPExtension.RunCadence = ReadContentAsString();
                                break;
                            case "ns3:Watts":
                                newTPExtension.Watts = ReadContentAsString();
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return newTPExtension;
                }
            }

            throw new FormatException(elementName);
        }
        private string ReadContentAsString()
        {
            if (Reader_.IsEmptyElement) throw new FormatException(Reader_.Name);

            string elementName = Reader_.Name;
            string result = string.Empty;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Text:
                        result = Reader_.Value;
                        break;

                    case XmlNodeType.EndElement:
                        return result;

                    case XmlNodeType.Element:
                        throw new FormatException(elementName);
                }
            }

            throw new FormatException(elementName);
        }

        private int ReadContentAsInt()
        {
            string value = ReadContentAsString();
            return int.Parse(value, CultureInfo.InvariantCulture);
        }

        private double ReadContentAsDouble()
        {
            string value = ReadContentAsString();
            return double.Parse(value, CultureInfo.InvariantCulture);
        }

        private DateTime ReadContentAsDateTime()
        {
            string value = ReadContentAsString();
            return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        }

        private void SkipElement()
        {
            if (Reader_.IsEmptyElement) return;

            string elementName = Reader_.Name;
            int depth = Reader_.Depth;

            while (Reader_.Read())
            {
                if (Reader_.NodeType == XmlNodeType.EndElement)
                {
                    if (Reader_.Depth == depth && Reader_.Name == elementName) return;
                }
            }

            throw new FormatException(elementName);
        }
    }
}
