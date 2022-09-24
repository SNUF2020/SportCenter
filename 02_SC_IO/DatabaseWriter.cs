using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SC
{
    class DatabaseWriter : IDisposable
    {
        private const string FITLOG_CREATOR = "SNuf2021";
        private const string XMLSCHEMA_INSTANCE = "xsi";
        private const string XMLSCHEMA = "xsd";

        private XmlWriter Writer_;
        private List<athlet> _database;
        private bool trackzipping = true;

        public DatabaseWriter(Stream stream, List<athlet> database)
        {
            Writer_ = XmlWriter.Create(stream, new XmlWriterSettings { CloseOutput = true, Indent = true });
            Writer_.WriteStartDocument(false);
            Writer_.WriteStartElement("SportCenter_DataBase", activityNamespaces.Activity_NAMESPACE);
            Writer_.WriteAttributeString("creator", FITLOG_CREATOR);
            Writer_.WriteAttributeString("xmlns", XMLSCHEMA_INSTANCE, null, activityNamespaces.XMLSCHEMA_INSTANCE);
            Writer_.WriteAttributeString("xmlns", XMLSCHEMA, null, activityNamespaces.XMLSCHEMA);

            _database = database;
        }

        public void Dispose()
        {
            Writer_.WriteEndElement();
            Writer_.Close();
        }

        public void WriteAllAthletData()
        {
            foreach (athlet _athlet in _database)
            {
                Writer_.WriteStartElement("AthleteLog");

                Writer_.WriteStartElement("Athlete");
                if (_athlet.AthleteID != null) Writer_.WriteAttributeString("Id", _athlet.AthleteID);
                if (_athlet.AthleteName != null) Writer_.WriteAttributeString("Name", _athlet.AthleteName);
                if (_athlet.AthleteDoB != null) Writer_.WriteAttributeString("DateOfBirth", _athlet.AthleteDoB.ToString());
                if (_athlet.AthleteHeightcm != null) Writer_.WriteAttributeString("HeightCentimeters", _athlet.AthleteHeightcm.ToString());
                Writer_.WriteEndElement();

                foreach (fitness fitness_ in _athlet.History)
                {
                    Writer_.WriteStartElement("Fittness");
                    if (fitness_.Date != null) Writer_.WriteAttributeString("Date", fitness_.Date.ToString());
                    if (fitness_.WeightKilograms != null) Writer_.WriteAttributeString("WeightKilograms", fitness_.WeightKilograms);
                    Writer_.WriteEndElement();
                }

                foreach (activity activity_ in _athlet.Activities)
                {
                    Writer_.WriteStartElement("Activity");

                    if (activity_.ActivityTime != null) Writer_.WriteAttributeString("StartTime", activity_.ActivityTime.ToString());
                    if (activity_.ActivityID != null) Writer_.WriteAttributeString("Id", activity_.ActivityID);

                    if (activity_.MetaSource != null || activity_.MetaCreated != null || activity_.MetaModified != null)
                    {
                        Writer_.WriteStartElement("Metadata");
                        if (activity_.MetaSource != null) Writer_.WriteAttributeString("Source", activity_.MetaSource);
                        if (activity_.MetaCreated != null) Writer_.WriteAttributeString("Created", activity_.MetaCreated.ToString());
                        if (activity_.MetaModified != null) Writer_.WriteAttributeString("Modified", activity_.MetaModified.ToString());
                        Writer_.WriteEndElement();
                    }
                    if (activity_.Duration >= 0)
                    {
                        Writer_.WriteStartElement("Duration");
                        Writer_.WriteAttributeString("TotalSeconds", Math.Round(activity_.Duration, 1).ToString().Replace(',', '.')); // round duration to next int number
                        Writer_.WriteEndElement();
                    }
                    if (activity_.Distance >= 0)
                    {
                        Writer_.WriteStartElement("Distance");
                        Writer_.WriteAttributeString("TotalMeters", Math.Round(activity_.Distance, 1).ToString().Replace(',', '.'));
                        Writer_.WriteEndElement();
                    }
                    if (activity_.total_Descent >= 0 || activity_.total_Ascent >= 0) // DAS MÜSSTE EIN FEHLER SEIN
                    {
                        Writer_.WriteStartElement("Elevation");
                        if (activity_.total_Descent <= 0) Writer_.WriteAttributeString("DescendMeters", Math.Round(activity_.total_Descent, 1).ToString().Replace(',', '.'));
                        if (activity_.total_Ascent >= 0) Writer_.WriteAttributeString("AscendMeters", Math.Round(activity_.total_Ascent, 1).ToString().Replace(',', '.'));
                        Writer_.WriteEndElement();
                    }
                    if (activity_.mean_HR >= 0 || activity_.max_HR >= 0)
                    {
                        Writer_.WriteStartElement("HeartRate");
                        if (activity_.mean_HR >= 0) Writer_.WriteAttributeString("AverageBPM", Math.Round(activity_.mean_HR, 1).ToString().Replace(',', '.'));
                        if (activity_.max_HR >= 0) Writer_.WriteAttributeString("MaximumBPM", Math.Round(activity_.max_HR, 1).ToString().Replace(',', '.'));
                        Writer_.WriteEndElement();
                    }
                    if (activity_.Cadence_mean >= 0 || activity_.Cadence_max >= 0)
                    {
                        Writer_.WriteStartElement("Cadence");
                        if (activity_.Cadence_mean >= 0) Writer_.WriteAttributeString("AverageRPM", Math.Round(activity_.Cadence_mean, 1).ToString().Replace(',', '.'));
                        if (activity_.Cadence_max >= 0) Writer_.WriteAttributeString("MaximumRPM", Math.Round(activity_.Cadence_max, 1).ToString().Replace(',', '.'));
                        Writer_.WriteEndElement();
                    }
                    if (activity_.Power_avrg >= 0 || activity_.Power_max >= 0)
                    {
                        Writer_.WriteStartElement("Power");
                        if (activity_.Power_avrg >= 0) Writer_.WriteAttributeString("AverageWatts", Math.Round(activity_.Power_avrg, 1).ToString().Replace(',', '.'));
                        if (activity_.Power_max >= 0) Writer_.WriteAttributeString("MaximumWatts", Math.Round(activity_.Power_max, 1).ToString().Replace(',', '.'));
                        Writer_.WriteEndElement();
                    }
                    if (activity_.Calories_total >= 0)
                    {
                        Writer_.WriteStartElement("Calories");
                        Writer_.WriteAttributeString("TotalCal", Math.Round(activity_.Calories_total, 1).ToString().Replace(',', '.'));
                        Writer_.WriteEndElement();
                    }

                    if (activity_.max_Speed >= 0)
                    {
                        Writer_.WriteStartElement("Speed");
                        Writer_.WriteAttributeString("MaxSpeed", Math.Round(activity_.max_Speed, 1).ToString().Replace(',', '.'));
                        Writer_.WriteEndElement();
                    }

                    if (activity_.Info_Note != null || activity_.Info_Name != null)
                    {
                        Writer_.WriteStartElement("Info");
                        if (activity_.Info_Note != null) Writer_.WriteAttributeString("Notes", activity_.Info_Note);
                        if (activity_.Info_Name != null) Writer_.WriteAttributeString("Name", activity_.Info_Name);
                        Writer_.WriteEndElement();
                    }

                    if (activity_.Laps != null)
                    {
                        Writer_.WriteStartElement("Laps");

                        foreach (LapData lap_ in activity_.Laps)
                        {
                            Writer_.WriteStartElement("Lap");
                            if (lap_.lapattributes.StartTime != null) Writer_.WriteAttributeString("StartTime", lap_.lapattributes.StartTime.ToString());
                            if (lap_.lapattributes.DurationSeconds != null) Writer_.WriteAttributeString("DurationSeconds", (Math.Round((double)lap_.lapattributes.DurationSeconds, 1)).ToString().Replace(',', '.'));
                            if (lap_.lapattributes.Notes != null) Writer_.WriteAttributeString("Notes", lap_.lapattributes.Notes);
                            if (lap_.lapattributes.Rest != null) Writer_.WriteAttributeString("Rest", lap_.lapattributes.Rest);
                            
                            if (lap_.TotalMeters != null)
                            {
                                Writer_.WriteStartElement("Distance");
                                Writer_.WriteAttributeString("TotalMeters", (Math.Round((double)lap_.TotalMeters, 1)).ToString().Replace(',', '.'));
                                Writer_.WriteEndElement();
                            }
                            if (lap_.TotalCal != null)
                            {
                                Writer_.WriteStartElement("Calories");
                                Writer_.WriteAttributeString("TotalCal", (Math.Round((double)lap_.TotalCal, 1)).ToString().Replace(',', '.'));
                                Writer_.WriteEndElement();
                            }
                            if (lap_.HeartRate_avg != null)
                            {
                                Writer_.WriteStartElement("HeartRate");
                                Writer_.WriteAttributeString("HR_avg", Math.Round((double)lap_.HeartRate_avg, 1).ToString().Replace(',', '.'));
                                Writer_.WriteAttributeString("HR_max", Math.Round((double)lap_.HeartRate_max, 1).ToString().Replace(',', '.'));
                                Writer_.WriteEndElement();
                            }
                            if (lap_.Intensity != null)
                            {
                                Writer_.WriteStartElement("Intensity");
                                Writer_.WriteAttributeString("Int", lap_.Intensity.ToString());
                                Writer_.WriteEndElement();
                            }
                            if (lap_.Cadence != null)
                            {
                                Writer_.WriteStartElement("Cadence");
                                Writer_.WriteAttributeString("Cad", Math.Round((double)lap_.Cadence, 1).ToString().Replace(',', '.'));
                                Writer_.WriteEndElement();
                            }
                            if (lap_.Trigger != null)
                            {
                                Writer_.WriteStartElement("Trigger");
                                Writer_.WriteAttributeString("Trig", lap_.Trigger.ToString());
                                Writer_.WriteEndElement();
                            }
                            if (lap_.Speed_max != null)
                            {
                                Writer_.WriteStartElement("Speed");
                                Writer_.WriteAttributeString("Speed_max", Math.Round((double)lap_.Speed_max, 1).ToString().Replace(',', '.'));
                                Writer_.WriteEndElement();
                            }
                            if (lap_.lat_start != null)
                            {
                                Writer_.WriteStartElement("Start");
                                Writer_.WriteAttributeString("Lat", lap_.lat_start.ToString().Replace(',', '.'));
                                Writer_.WriteAttributeString("Lon", lap_.lon_start.ToString().Replace(',', '.'));
                                Writer_.WriteEndElement();
                            }
                            if (lap_.lat_end != null)
                            {
                                Writer_.WriteStartElement("End");
                                Writer_.WriteAttributeString("Lat", lap_.lat_end.ToString().Replace(',', '.'));
                                Writer_.WriteAttributeString("Lon", lap_.lon_end.ToString().Replace(',', '.'));
                                Writer_.WriteEndElement();
                            }
                            // Power and Elevation not implemented
                            Writer_.WriteEndElement();
                        }

                        Writer_.WriteEndElement();
                    }

                    if (activity_.Weather_Cond != null || activity_.Weather_Temp >= -100 || activity_.Weather_Note != null)
                    {
                        Writer_.WriteStartElement("Weather");
                        if (activity_.Weather_Cond != null) Writer_.WriteAttributeString("Conditions", activity_.Weather_Cond);
                        if (activity_.Weather_Temp >= -100) Writer_.WriteAttributeString("Temp", activity_.Weather_Temp.ToString());
                        if (activity_.Weather_Note != null) Writer_.WriteAttributeString("Notes", activity_.Weather_Note);
                        Writer_.WriteEndElement();
                    }

                    if (activity_.CatID != null || activity_.CatName != null)
                    {
                        Writer_.WriteStartElement("Category");
                        if (activity_.CatID != null) Writer_.WriteAttributeString("Id", activity_.CatID);
                        if (activity_.CatName != null) Writer_.WriteAttributeString("Name", activity_.CatName);
                        Writer_.WriteEndElement();
                    }

                    if (activity_.RouteID != null || activity_.RouteName != null)
                    {
                        Writer_.WriteStartElement("Route");
                        if (activity_.RouteID != null) Writer_.WriteAttributeString("ID", activity_.RouteID);
                        if (activity_.RouteName != null) Writer_.WriteAttributeString("Name", activity_.RouteName);
                        Writer_.WriteEndElement();
                    }

                    if (activity_.LocationName != null || activity_.LocationID != null)
                    {
                        Writer_.WriteStartElement("Location");
                        if (activity_.LocationID != null) Writer_.WriteAttributeString("Id", activity_.LocationID);
                        if (activity_.LocationName != null) Writer_.WriteAttributeString("Name", activity_.LocationName);
                        Writer_.WriteEndElement();
                    }
                    if (activity_.Equipment_ID != null || activity_.Equipment_Name != null)
                    {
                        Writer_.WriteStartElement("EquipmentUsed");
                        if (activity_.Equipment_ID != null) Writer_.WriteAttributeString("Id", activity_.Equipment_ID);
                        if (activity_.Equipment_Name != null) Writer_.WriteAttributeString("Name", activity_.Equipment_Name);
                        Writer_.WriteEndElement();
                    }

                    if (activity_.Track != null)
                    {
                        Writer_.WriteStartElement("Track");
                        if (activity_.ActivityTime != null) Writer_.WriteAttributeString("StartTime", activity_.ActivityTime.ToString());

                        if (trackzipping)
                        {
                            StringBuilder track_string = new StringBuilder();
                            string tp_string0 = "<zip-file> <zip> ";
                            track_string.AppendLine(tp_string0);

                            foreach (TrackPoint trackpoint_ in activity_.Track)
                            {
                                string tp_string = "<pt ";
                                if (trackpoint_.tm != null) tp_string += "tm=" + "\"" + trackpoint_.tm.ToString().Replace(',', '.') + "\" ";
                                if (trackpoint_.lat != null) tp_string += "lat=" + "\"" + trackpoint_.lat.ToString().Replace(',', '.') + "\" ";
                                if (trackpoint_.lon != null) tp_string += "lon=" + "\"" + trackpoint_.lon.ToString().Replace(',', '.') + "\" ";
                                if (trackpoint_.ele != null) tp_string += "ele=" + "\"" + trackpoint_.ele.ToString().Replace(',', '.') + "\" ";
                                if (trackpoint_.dist != null) tp_string += "dist=" + "\"" + trackpoint_.dist.ToString().Replace(',', '.') + "\" ";
                                if (trackpoint_.hr != null) tp_string += "hr=" + "\"" + trackpoint_.hr.ToString().Replace(',', '.') + "\" ";
                                if (trackpoint_.cadence != null) tp_string += "cadence=" + "\"" + trackpoint_.cadence.ToString().Replace(',', '.') + "\" ";
                                if (trackpoint_.power != null) tp_string += "power=" + "\"" + trackpoint_.power.ToString().Replace(',', '.') + "\" ";

                                tp_string += "/>";
                                track_string.AppendLine(tp_string);
                            }

                            string tp_stringEnd = "</zip> </zip-file> ";
                            track_string.AppendLine(tp_stringEnd);

                            byte[] zip_track = DatabaseHelpers.Zip(track_string.ToString());

                            Writer_.WriteCData(Convert.ToBase64String(zip_track));
                        }
                        else
                        {
                            foreach (TrackPoint trackpoint_ in activity_.Track)
                            {
                                Writer_.WriteStartElement("pt");

                                if (trackpoint_.tm != null) Writer_.WriteAttributeString("tm", trackpoint_.tm.ToString().Replace(',', '.'));
                                if (trackpoint_.lat != null) Writer_.WriteAttributeString("lat", trackpoint_.lat.ToString().Replace(',', '.'));
                                if (trackpoint_.lon != null) Writer_.WriteAttributeString("lon", trackpoint_.lon.ToString().Replace(',', '.'));
                                if (trackpoint_.ele != null) Writer_.WriteAttributeString("ele", trackpoint_.ele.ToString().Replace(',', '.'));
                                if (trackpoint_.dist != null) Writer_.WriteAttributeString("dist", trackpoint_.dist.ToString().Replace(',', '.'));
                                if (trackpoint_.hr != null) Writer_.WriteAttributeString("hr", trackpoint_.hr.ToString().Replace(',', '.'));
                                if (trackpoint_.cadence != null) Writer_.WriteAttributeString("cadence", trackpoint_.cadence.ToString().Replace(',', '.'));
                                if (trackpoint_.power != null) Writer_.WriteAttributeString("power", trackpoint_.power.ToString().Replace(',', '.'));
                                // speed fehlt!!!

                                Writer_.WriteEndElement();
                            }
                        }

                        Writer_.WriteEndElement(); //Track
                
                    }

                if (activity_.Markers != null)
                    {
                        Writer_.WriteStartElement("DistanceMarkers");

                        foreach (Marker marker in activity_.Markers)
                        {
                            Writer_.WriteStartElement("Marker");

                            if (marker.dist != null) Writer_.WriteAttributeString("dist", marker.dist.ToString());

                            Writer_.WriteEndElement();
                        }

                        Writer_.WriteEndElement();
                    }

                    if (activity_.Pausen != null)
                    {
                        Writer_.WriteStartElement("TrackClock");

                        foreach (Pause pause in activity_.Pausen)
                        {
                            Writer_.WriteStartElement("Pause");

                            if (pause.EndTime != null) Writer_.WriteAttributeString("EndTime", pause.EndTime.ToString());
                            if (pause.StartTime != null) Writer_.WriteAttributeString("StartTime", pause.StartTime.ToString());

                            Writer_.WriteEndElement();
                        }

                        Writer_.WriteEndElement();
                    }

                    Writer_.WriteEndElement();

                }

                Writer_.WriteEndElement();
            }

        }

    }
}
