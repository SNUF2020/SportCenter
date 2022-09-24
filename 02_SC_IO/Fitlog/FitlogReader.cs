using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SC
{
    public enum WeatherCondition { Clear, ScatterClouds, PartClouds, Overcast, MostClouds, Clouds, ChanceRain, LightDrizzle, LightRain, Rain, HeavyRain, ChanceThunder, Thunder, Snow, Haze };

    public class FitlogReader : IDisposable
    {
        private XmlReader Reader_;

        public GpxObjectType ObjectType { get; private set; }
        public FitlogAttributes Attributes { get; private set; }
        public AthleteLogData AthletLogdata { get; set; }

        public FitlogReader(Stream stream)
        {
            Reader_ = XmlReader.Create(stream, new XmlReaderSettings { CloseInput = true });

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:
                        if (Reader_.Name != "FitnessWorkbook") throw new FormatException(Reader_.Name); // vorher: "GPX"
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

        private FitlogAttributes ReadGpxAttribures()
        {
            FitlogAttributes attributes = new FitlogAttributes();

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
                            case "AthleteLog":
                                AthletLogdata = ReadAthleteLogData();
                                ObjectType = GpxObjectType.AthletLogData;
                                return true;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != "FitnessWorkbook" & Reader_.Name != "AthleteLog") throw new FormatException(Reader_.Name); // vorher "GPX"
                        ObjectType = GpxObjectType.None;
                        return false;
                }
            }

            ObjectType = GpxObjectType.None;
            return false;
        }

        private AthleteLogData ReadAthleteLogData()
        {
            string elementName = Reader_.Name;
            bool isEmptyElement = Reader_.IsEmptyElement;

            AthleteLogData logdata = new AthleteLogData();
            AthleteData athletedata = new AthleteData();

            if (isEmptyElement) return logdata;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "Athlete":
                                athletedata.Id = Reader_.GetAttribute("Id");
                                athletedata.Name = Reader_.GetAttribute("Name");
                                athletedata.DoB = Convert.ToDateTime(Reader_.GetAttribute("DateOfBirth"));
                                athletedata.Height = Convert.ToInt32(Reader_.GetAttribute("HeightCentimeters"));
                                athletedata.Weight = Convert.ToDouble(Reader_.GetAttribute("WeightKilograms"));
                                break;
                            case "Activity":
                                athletedata.activitydata_list.Add(ReadActivity());
                                break;
                            //case "History": weiteres fitlog Element -> besitzt nur Attribute!
                            //athletedata.historydata = ReadHistory();
                            //break
                            // Integration History??? -> file generieren bei dem History abgebildet ist. Funktioniert net :-/
                            default:
                                SkipElement();
                                break;
                        }
                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        logdata.athletedata.Add(athletedata);
                        return logdata;
                }
            }

            throw new FormatException(elementName);
        }

        private ActivityData ReadActivity()
        {
            ActivityData activitydata = new ActivityData();
            if (Reader_.IsEmptyElement) return activitydata;

            activitydata.activityattributes.StartTime = Convert.ToDateTime(Reader_.GetAttribute("StartTime"));
            activitydata.activityattributes.Id = Reader_.GetAttribute("Id");

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "Metadata":
                                activitydata.metadataattributes = ReadMethaDataAttributes();
                                break;
                            case "Duration":
                                activitydata.durationattributes = ReadDurationAttributes();
                                break;
                            case "Distance":
                                activitydata.distanceattributes = ReadDistanceAttributes();
                                break;
                            case "Elevation":
                                activitydata.elevationattributes = ReadElevationAttributes();
                                break;
                            case "HeartRate":
                                activitydata.heartrateattributes = ReadHeartRateAttributes();
                                break;
                            case "Cadence":
                                activitydata.cadenceattributes = ReadCadenceAttributes();
                                break;
                            case "Power":
                                activitydata.powerattributes = ReadPowerAttributes();
                                break;
                            case "Calories":
                                activitydata.caloriesattributes = ReadCaloriesAttributes();
                                break;
                            case "Notes":
                                activitydata.infoattributes.Notes = Reader_.ReadInnerXml();
                                break;
                            case "Name":
                                activitydata.infoattributes.Name = Reader_.ReadInnerXml();
                                break;
                            case "Laps":
                                activitydata.lapdata_List = ReadLapData();
                                break;
                            case "Weather":
                                activitydata.weatherattributes = ReadWeatherAttributes();
                                break;
                            case "Category":
                                activitydata.categoryattributes = ReadCategoryAttributes();
                                break;
                            case "Location":
                                activitydata.locationattributes = ReadLocationAttributes();
                                break;
                            case "Route":
                                activitydata.routeattributes = ReadRouteattributes();
                                break;
                            case "EquipmentUsed":
                                activitydata.equipementitemattributes = ReadEquipementIttemAttributes();
                                break;
                            case "Track":
                                activitydata.trackattributes.StartTime = Convert.ToDateTime(Reader_.GetAttribute("StartTime"));
                                activitydata.trackdate_list = ReadTrack();
                                break;
                            case "DistanceMarkers":
                                activitydata.marker_list = ReadMarker();
                                break;
                            case "TrackClock":
                                activitydata.pause_list = ReadPause();
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return activitydata;
                }
            }

            throw new FormatException(elementName);
        }

        private List<Marker> ReadMarker()
        {
            List<Marker> marker_list = new List<Marker>();
            if (Reader_.IsEmptyElement) return marker_list;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "Marker":
                                marker_list.Add(getMarkerInfo());
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return marker_list;
                }
            }

            throw new FormatException(elementName);
        }
        private Marker getMarkerInfo()
        {
            Marker Marker_attributes = new Marker();

            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "dist":
                        Marker_attributes.dist = Convert.ToDouble(Reader_.Value);
                        break;
                }
            }

            return Marker_attributes;
        }
        private EquipementItemAttributes ReadEquipementIttemAttributes()
        {
            EquipementItemAttributes attributes = new EquipementItemAttributes();

            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "Id":
                        attributes.Id = Reader_.Value;
                        break;
                    case "Name":
                        attributes.Name = Reader_.Value;
                        break;
                }
            }

            return attributes;
        }
        private List<Pause> ReadPause()
        {
            List<Pause> pause_list = new List<Pause>();
            if (Reader_.IsEmptyElement) return pause_list;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "Pause":
                                pause_list.Add(getPauseInfo());
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return pause_list;
                }
            }

            throw new FormatException(elementName);
        }
        private Pause getPauseInfo()
        {
            Pause Pause_attributes = new Pause();

            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "EndTime":
                        Pause_attributes.EndTime = Convert.ToDateTime(Reader_.Value);
                        break;
                    case "StartTime":
                        Pause_attributes.StartTime = Convert.ToDateTime(Reader_.Value);
                        break;
                }
            }

            return Pause_attributes;
        }
        private List<TrackPoint> ReadTrack()
        {
            List<TrackPoint> trackdata_list = new List<TrackPoint>();
            if (Reader_.IsEmptyElement) return trackdata_list;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "Metadata":
                                //trackdata_list.Add(readMetaData()); -> Keine Weitere Def. in zonefive format -> Eventuell identische Struktur wie Metadata aus Activity
                                break;
                            case "pt":
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

        private List<LapData> ReadLapData()
        {
            List<LapData> data_list = new List<LapData>();
            if (Reader_.IsEmptyElement) return data_list;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "Lap":
                                data_list.Add(getLap());
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return data_list;
                }
            }

            throw new FormatException(elementName);
        }

        private LapData getLap()
        {
            LapData data = new LapData();
            data.lapattributes.StartTime = Convert.ToDateTime(Reader_.GetAttribute("StartTime"));
            data.lapattributes.DurationSeconds = Convert.ToDouble(Reader_.GetAttribute("DurationSeconds"), CultureInfo.InvariantCulture);
            data.lapattributes.Notes = Reader_.GetAttribute("Notes");
            data.lapattributes.Rest = Reader_.GetAttribute("Rest");

            if (Reader_.IsEmptyElement) return data;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "Distance":
                                data.TotalMeters = Convert.ToDouble(Reader_.GetAttribute("TotalMeters"));
                                break;
                            case "Calories":
                                data.TotalCal = Convert.ToDouble(Reader_.GetAttribute("TotalCal"));
                                break;
                            case "HeartRate":
                                data.HeartRate_avg = Convert.ToDouble(Reader_.GetAttribute("HR_avg"), CultureInfo.InvariantCulture);
                                data.HeartRate_max = Convert.ToDouble(Reader_.GetAttribute("HR_max"), CultureInfo.InvariantCulture);
                                break;
                            case "Intensity":
                                data.Intensity = Convert.ToDouble(Reader_.GetAttribute("Int"));
                                break;
                            case "Cadence":
                                data.Cadence = Convert.ToDouble(Reader_.GetAttribute("Cadence"));
                                break;
                            case "Trigger":
                                data.Trigger = Convert.ToDouble(Reader_.GetAttribute("Trig"));
                                break;
                            case "Speed":
                                data.Speed_max = Convert.ToDouble(Reader_.GetAttribute("Speed_max"));
                                break;
                            case "Start":
                                data.lat_start = Convert.ToDouble(Reader_.GetAttribute("Lat"), CultureInfo.InvariantCulture);
                                data.lon_start = Convert.ToDouble(Reader_.GetAttribute("Lon"), CultureInfo.InvariantCulture);
                                break;
                            case "End":
                                data.lat_end = Convert.ToDouble(Reader_.GetAttribute("Lat"), CultureInfo.InvariantCulture);
                                data.lon_end = Convert.ToDouble(Reader_.GetAttribute("Lon"), CultureInfo.InvariantCulture);
                                break;
                            case "Power":
                                data.Power = Convert.ToDouble(Reader_.GetAttribute("Power"));
                                break;
                            case "Elevation":
                                data.Elevation = Convert.ToDouble(Reader_.GetAttribute("Elevation"), CultureInfo.InvariantCulture);
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

        private TrackPoint getTrackPoint()
        {
            TrackPoint ptattributes = new TrackPoint();

            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "tm":
                        ptattributes.tm = Convert.ToInt32(Reader_.Value, CultureInfo.InvariantCulture);
                        break;
                    case "lat":
                        ptattributes.lat = Convert.ToDouble(Reader_.Value, CultureInfo.InvariantCulture);
                        break;
                    case "lon":
                        ptattributes.lon = Convert.ToDouble(Reader_.Value, CultureInfo.InvariantCulture);
                        break;
                    case "ele":
                        ptattributes.ele = Convert.ToDouble(Reader_.Value, CultureInfo.InvariantCulture);
                        break;
                    case "dist":
                        ptattributes.dist = Convert.ToDouble(Reader_.Value, CultureInfo.InvariantCulture);
                        break;
                    case "hr":
                        ptattributes.hr = Convert.ToDouble(Reader_.Value, CultureInfo.InvariantCulture);
                        break;
                    case "cadence":
                        ptattributes.cadence = Convert.ToDouble(Reader_.Value, CultureInfo.InvariantCulture);
                        break;
                    case "power":
                        ptattributes.power = Convert.ToDouble(Reader_.Value, CultureInfo.InvariantCulture);
                        break;
                }
            }

            return ptattributes;
        }

        private MetaDataAttributes ReadMethaDataAttributes()
        {
            MetaDataAttributes metaattributes = new MetaDataAttributes();

            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "Source":
                        metaattributes.Source = Reader_.Value;
                        break;
                    case "Created":
                        metaattributes.Created = Convert.ToDateTime(Reader_.Value);
                        break;
                    case "Modified":
                        metaattributes.Modified = Convert.ToDateTime(Reader_.Value);
                        break;
                }
            }

            return metaattributes;
        }
        private DurationAttributes ReadDurationAttributes()
        {
            DurationAttributes attributes = new DurationAttributes();

            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "TotalSeconds":
                        attributes.TotalSeconds = Convert.ToDouble(Reader_.Value, CultureInfo.InvariantCulture);
                        break;
                }
            }

            return attributes;
        }
        private DistanceAttributes ReadDistanceAttributes()
        {
            DistanceAttributes attributes = new DistanceAttributes();

            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "TotalMeters":
                        attributes.TotalMeters = Convert.ToDouble(Reader_.Value, CultureInfo.InvariantCulture);
                        break;
                }
            }

            return attributes;
        }
        private ElevationAttributes ReadElevationAttributes()
        {
            ElevationAttributes attributes = new ElevationAttributes();

            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {         
                    case "DescendMeters":
                        attributes.DescendMeters = Convert.ToDouble(Reader_.Value, CultureInfo.InvariantCulture);
                        break;
                    case "AscendMeters":
                        attributes.AscendMeters = Convert.ToDouble(Reader_.Value, CultureInfo.InvariantCulture);
                        break;
                }
            }

            return attributes;
        }
        private HeartrateAttributes ReadHeartRateAttributes()
        {
            HeartrateAttributes attributes = new HeartrateAttributes();

            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "AverageBPM":
                        attributes.AverageBPM = Convert.ToDouble(Reader_.Value, CultureInfo.InvariantCulture);
                        break;
                    case "MaximumBPM":
                        attributes.MaximumBPM = Convert.ToDouble(Reader_.Value, CultureInfo.InvariantCulture);
                        break;
                }
            }

            return attributes;
        }
        private CadenceAttributes ReadCadenceAttributes()
        {
            CadenceAttributes attributes = new CadenceAttributes();

            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "AverageRPM":
                        attributes.AverageRPM = Convert.ToDouble(Reader_.Value, CultureInfo.InvariantCulture);
                        break;
                    case "MaximumRPM":
                        attributes.MaximumRPM = Convert.ToDouble(Reader_.Value, CultureInfo.InvariantCulture);
                        break;
                }
            }

            return attributes;
        }
        private PowerAttributes ReadPowerAttributes()
        {
            PowerAttributes attributes = new PowerAttributes();

            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "AverageWatts":
                        attributes.AverageWatts = Convert.ToDouble(Reader_.Value, CultureInfo.InvariantCulture);
                        break;
                    case "MaximumWatts":
                        attributes.MaximumWatts = Convert.ToDouble(Reader_.Value, CultureInfo.InvariantCulture);
                        break;
                }
            }

            return attributes;
        }
        private CaloriesAttributes ReadCaloriesAttributes()
        {
            CaloriesAttributes attributes = new CaloriesAttributes();

            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "TotalCal":
                        attributes.TotalCal = Convert.ToDouble(Reader_.Value);
                        break;
                }
            }

            return attributes;
        }
        private WeatherAttributes ReadWeatherAttributes()
        {
            WeatherAttributes attributes = new WeatherAttributes();

            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "Conditions":
                        attributes.Conditions = Reader_.Value;
                        break;
                    case "Temp":
                        attributes.Temp = Convert.ToDouble(Reader_.Value);
                        break;
                    case "Notes":
                        attributes.Notes = Reader_.Value;
                        break;
                }
            }

            return attributes;
        }
        private CategoryAttributes ReadCategoryAttributes()
        {
            CategoryAttributes attributes = new CategoryAttributes();

            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "Id":
                        attributes.Id = Reader_.Value;
                        break;
                    case "Name":
                        attributes.Name = Reader_.Value;
                        break;
                }
            }

            return attributes;
        }
        private LocationAttributes ReadLocationAttributes()
        {
            LocationAttributes attributes = new LocationAttributes();

            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "Id":
                        attributes.Id = Reader_.Value;
                        break;
                    case "Name":
                        attributes.Name = Reader_.Value;
                        break;
                }
            }

            return attributes;
        }

        private RouteAttributes ReadRouteattributes()
        {
            RouteAttributes attributes = new RouteAttributes();

            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "Id":
                        attributes.Id = Reader_.Value;
                        break;
                    case "Name":
                        attributes.Name = Reader_.Value;
                        break;
                }
            }

            return attributes;
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
