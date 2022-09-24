using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SC
{
    class FitlogWriter : IDisposable
    {
        private const string FITLOG_CREATOR = "SportCenter";
        private const string XMLSCHEMA_INSTANCE = "xsi";
        private const string XMLSCHEMA = "xsd";

        private XmlWriter Writer_;

        private List<athlet> _SC_DataBase;
        private int _active_athlet;
        private int _active_activity;

        public FitlogWriter(Stream stream, List<athlet> database, int active_athlet, int active_activity)
        {
            Writer_ = XmlWriter.Create(stream, new XmlWriterSettings { CloseOutput = true, Indent = true });
            Writer_.WriteStartDocument(false);
            Writer_.WriteStartElement("FitnessWorkbook", FitlogNamespaces.FITLOG_NAMESPACE);
            Writer_.WriteAttributeString("creator", FITLOG_CREATOR);
            Writer_.WriteAttributeString("xmlns", XMLSCHEMA_INSTANCE, null, FitlogNamespaces.XMLSCHEMA_INSTANCE);
            Writer_.WriteAttributeString("xmlns", XMLSCHEMA, null, FitlogNamespaces.XMLSCHEMA);

            _SC_DataBase = database;
            _active_athlet = active_athlet;
            _active_activity = active_activity;
        }

        public void Dispose()
        {
            Writer_.WriteEndElement();
            Writer_.Close();
        }

        public void Write_Ativity()
        {
            Writer_.WriteStartElement("AthleteLog");

            Writer_.WriteStartElement("Athlete");
            if (_SC_DataBase[_active_athlet].AthleteID != null) Writer_.WriteAttributeString("Id", _SC_DataBase[_active_athlet].AthleteID);
            if (_SC_DataBase[_active_athlet].AthleteName != null) Writer_.WriteAttributeString("Name", _SC_DataBase[_active_athlet].AthleteName);
            if (_SC_DataBase[_active_athlet].AthleteDoB != null) Writer_.WriteAttributeString("DateOfBirth", _SC_DataBase[_active_athlet].AthleteDoB.ToString());
            if (_SC_DataBase[_active_athlet].AthleteHeightcm != null) Writer_.WriteAttributeString("HeightCentimeters", _SC_DataBase[_active_athlet].AthleteHeightcm.ToString());
            Writer_.WriteEndElement();

            Writer_.WriteStartElement("Activity");

            if (_SC_DataBase[_active_athlet].Activities[_active_activity].ActivityTime != null) Writer_.WriteAttributeString("StartTime", _SC_DataBase[_active_athlet].Activities[_active_activity].ActivityTime.ToString());
            if (_SC_DataBase[_active_athlet].Activities[_active_activity].ActivityID != null) Writer_.WriteAttributeString("Id", _SC_DataBase[_active_athlet].Activities[_active_activity].ActivityID);

            if (_SC_DataBase[_active_athlet].Activities[_active_activity].MetaSource != null || _SC_DataBase[_active_athlet].Activities[_active_activity].MetaCreated != null || _SC_DataBase[_active_athlet].Activities[_active_activity].MetaModified != null)
            {
                Writer_.WriteStartElement("Metadata");
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].MetaSource != null) 
                    Writer_.WriteAttributeString("Source", _SC_DataBase[_active_athlet].Activities[_active_activity].MetaSource);
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].MetaCreated != null) 
                    Writer_.WriteAttributeString("Created", _SC_DataBase[_active_athlet].Activities[_active_activity].MetaCreated.ToString());
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].MetaModified != null) 
                    Writer_.WriteAttributeString("Modified", _SC_DataBase[_active_athlet].Activities[_active_activity].MetaModified.ToString());
                Writer_.WriteEndElement();
            }
            if (_SC_DataBase[_active_athlet].Activities[_active_activity].Duration >= 0)
            {
                Writer_.WriteStartElement("Duration");
                Writer_.WriteAttributeString("TotalSeconds", _SC_DataBase[_active_athlet].Activities[_active_activity].Duration.ToString().Replace(',', '.'));
                Writer_.WriteEndElement();
            }
            if (_SC_DataBase[_active_athlet].Activities[_active_activity].Distance >= 0)
            {
                Writer_.WriteStartElement("Distance");
                Writer_.WriteAttributeString("TotalMeters", _SC_DataBase[_active_athlet].Activities[_active_activity].Distance.ToString().Replace(',', '.'));
                Writer_.WriteEndElement();
            }
            if (_SC_DataBase[_active_athlet].Activities[_active_activity].total_Descent <= 0 || _SC_DataBase[_active_athlet].Activities[_active_activity].total_Ascent >= 0)
            {
                Writer_.WriteStartElement("Elevation");
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].total_Descent <= 0) 
                    Writer_.WriteAttributeString("DescendMeters", _SC_DataBase[_active_athlet].Activities[_active_activity].total_Descent.ToString().Replace(',', '.'));
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].total_Ascent >= 0) 
                    Writer_.WriteAttributeString("AscendMeters", _SC_DataBase[_active_athlet].Activities[_active_activity].total_Ascent.ToString().Replace(',', '.'));
                Writer_.WriteEndElement();
            }
            if (_SC_DataBase[_active_athlet].Activities[_active_activity].mean_HR >= 0 || _SC_DataBase[_active_athlet].Activities[_active_activity].max_HR >= 0)
            {
                Writer_.WriteStartElement("HeartRate");
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].mean_HR >= 0) 
                    Writer_.WriteAttributeString("AverageBPM", _SC_DataBase[_active_athlet].Activities[_active_activity].mean_HR.ToString().Replace(',', '.'));
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].max_HR >= 0) 
                    Writer_.WriteAttributeString("MaximumBPM", _SC_DataBase[_active_athlet].Activities[_active_activity].max_HR.ToString().Replace(',', '.'));
                Writer_.WriteEndElement();
            }
            if (_SC_DataBase[_active_athlet].Activities[_active_activity].Cadence_mean >= 0 || _SC_DataBase[_active_athlet].Activities[_active_activity].Cadence_max >= 0)
            {
                Writer_.WriteStartElement("Cadence");
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].Cadence_mean >= 0) 
                    Writer_.WriteAttributeString("AverageRPM", _SC_DataBase[_active_athlet].Activities[_active_activity].Cadence_mean.ToString().Replace(',', '.'));
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].Cadence_max >= 0) 
                    Writer_.WriteAttributeString("MaximumRPM", _SC_DataBase[_active_athlet].Activities[_active_activity].Cadence_max.ToString().Replace(',', '.'));
                Writer_.WriteEndElement();
            }
            if (_SC_DataBase[_active_athlet].Activities[_active_activity].Power_avrg >= 0 || _SC_DataBase[_active_athlet].Activities[_active_activity].Power_max >= 0)
            {
                Writer_.WriteStartElement("Power");
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].Power_avrg >= 0) Writer_.WriteAttributeString("AverageWatts", _SC_DataBase[_active_athlet].Activities[_active_activity].Power_avrg.ToString().Replace(',', '.'));
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].Power_max >= 0) Writer_.WriteAttributeString("MaximumWatts", _SC_DataBase[_active_athlet].Activities[_active_activity].Power_max.ToString().Replace(',', '.'));
                Writer_.WriteEndElement();
            }
            if (_SC_DataBase[_active_athlet].Activities[_active_activity].Calories_total >= 0)
            {
                Writer_.WriteStartElement("Calories");
                Writer_.WriteAttributeString("TotalCal", _SC_DataBase[_active_athlet].Activities[_active_activity].Calories_total.ToString().Replace(',', '.'));
                Writer_.WriteEndElement();
            }

            if (_SC_DataBase[_active_athlet].Activities[_active_activity].max_Speed >= 0)
            {
                Writer_.WriteStartElement("Speed");
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].max_Speed >= 0) Writer_.WriteAttributeString("MaxSpeed", Math.Round(_SC_DataBase[_active_athlet].Activities[_active_activity].max_Speed, 1).ToString().Replace(',', '.'));
                Writer_.WriteEndElement();
            }

            if (_SC_DataBase[_active_athlet].Activities[_active_activity].Info_Name != null || _SC_DataBase[_active_athlet].Activities[_active_activity].Info_Note != null)
            {
                Writer_.WriteStartElement("Info");
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].Info_Name != null) Writer_.WriteString(_SC_DataBase[_active_athlet].Activities[_active_activity].Info_Name);
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].Info_Note != null) Writer_.WriteString(_SC_DataBase[_active_athlet].Activities[_active_activity].Info_Note);
                Writer_.WriteEndElement();
            }

            if (_SC_DataBase[_active_athlet].Activities[_active_activity].Laps != null)
            {
                Writer_.WriteStartElement("Laps");

                foreach (LapData lap_ in _SC_DataBase[_active_athlet].Activities[_active_activity].Laps)
                {
                    Writer_.WriteStartElement("Lap");

                    if (lap_.lapattributes.StartTime != null) Writer_.WriteAttributeString("StartTime", lap_.lapattributes.StartTime.ToString());
                    if (lap_.lapattributes.DurationSeconds != null) Writer_.WriteAttributeString("DurationSeconds", lap_.lapattributes.DurationSeconds.ToString().Replace(',', '.'));
                    if (lap_.lapattributes.Notes != null) Writer_.WriteAttributeString("Notes", lap_.lapattributes.Notes);
                    if (lap_.lapattributes.Rest != null) Writer_.WriteAttributeString("Rest", lap_.lapattributes.Rest);

                    if (lap_.TotalMeters != null)
                    {
                        Writer_.WriteStartElement("Distance");
                        Writer_.WriteAttributeString("TotalMeters", lap_.TotalMeters.ToString().Replace(',', '.'));
                        Writer_.WriteEndElement();
                    }
                    if (lap_.TotalCal != null)
                    {
                        Writer_.WriteStartElement("Calories");
                        Writer_.WriteAttributeString("TotalCal", lap_.TotalCal.ToString().Replace(',', '.'));
                        Writer_.WriteEndElement();
                    }
                    if (lap_.HeartRate_avg != null)
                    {
                        Writer_.WriteStartElement("HeartRate");
                        Writer_.WriteAttributeString("HR_avg", lap_.HeartRate_avg.ToString().Replace(',', '.'));
                        Writer_.WriteAttributeString("HR_max", lap_.HeartRate_max.ToString().Replace(',', '.'));
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
                        Writer_.WriteAttributeString("Cad", lap_.Cadence.ToString().Replace(',', '.'));
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
                        Writer_.WriteAttributeString("Speed_max", lap_.Speed_max.ToString().Replace(',', '.'));
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

            if (_SC_DataBase[_active_athlet].Activities[_active_activity].Weather_Cond != null || _SC_DataBase[_active_athlet].Activities[_active_activity].Weather_Temp >= -100 || _SC_DataBase[_active_athlet].Activities[_active_activity].Weather_Note != null)
            {
                Writer_.WriteStartElement("Weather");
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].Weather_Cond != null) Writer_.WriteAttributeString("Conditions", _SC_DataBase[_active_athlet].Activities[_active_activity].Weather_Cond);
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].Weather_Temp >= -100) Writer_.WriteAttributeString("Temp", _SC_DataBase[_active_athlet].Activities[_active_activity].Weather_Temp.ToString());
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].Weather_Note != null) Writer_.WriteAttributeString("Notes", _SC_DataBase[_active_athlet].Activities[_active_activity].Weather_Note);
                Writer_.WriteEndElement();
            }

            if (_SC_DataBase[_active_athlet].Activities[_active_activity].CatID != null || _SC_DataBase[_active_athlet].Activities[_active_activity].CatName != null)
            {
                Writer_.WriteStartElement("Category");
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].CatID != null) Writer_.WriteAttributeString("Id", _SC_DataBase[_active_athlet].Activities[_active_activity].CatID);
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].CatName != null) Writer_.WriteAttributeString("Name", _SC_DataBase[_active_athlet].Activities[_active_activity].CatName);
                Writer_.WriteEndElement();
            }

            if (_SC_DataBase[_active_athlet].Activities[_active_activity].RouteID != null || _SC_DataBase[_active_athlet].Activities[_active_activity].RouteName != null)
            {
                Writer_.WriteStartElement("Route");
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].RouteID != null) Writer_.WriteAttributeString("ID", _SC_DataBase[_active_athlet].Activities[_active_activity].RouteID);
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].RouteName != null) Writer_.WriteAttributeString("Name", _SC_DataBase[_active_athlet].Activities[_active_activity].RouteName);
                Writer_.WriteEndElement();
            }

            if (_SC_DataBase[_active_athlet].Activities[_active_activity].LocationName != null || _SC_DataBase[_active_athlet].Activities[_active_activity].LocationID != null)
            {
                Writer_.WriteStartElement("Location");
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].LocationID != null) Writer_.WriteAttributeString("Id", _SC_DataBase[_active_athlet].Activities[_active_activity].LocationID);
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].LocationName != null) Writer_.WriteAttributeString("Name", _SC_DataBase[_active_athlet].Activities[_active_activity].LocationName);
                Writer_.WriteEndElement();
            }
            if (_SC_DataBase[_active_athlet].Activities[_active_activity].Equipment_ID != null || _SC_DataBase[_active_athlet].Activities[_active_activity].Equipment_Name != null)
            {
                Writer_.WriteStartElement("EquipmentUsed");
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].Equipment_ID != null) Writer_.WriteAttributeString("Id", _SC_DataBase[_active_athlet].Activities[_active_activity].Equipment_ID);
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].Equipment_Name != null) Writer_.WriteAttributeString("Name", _SC_DataBase[_active_athlet].Activities[_active_activity].Equipment_Name);
                Writer_.WriteEndElement();
            }

            if (_SC_DataBase[_active_athlet].Activities[_active_activity].Track != null)
            {
                Writer_.WriteStartElement("Track");
                 
                if (_SC_DataBase[_active_athlet].Activities[_active_activity].ActivityTime != null) Writer_.WriteAttributeString("StartTime", _SC_DataBase[_active_athlet].Activities[_active_activity].ActivityTime.ToString());

                foreach (TrackPoint trackpoint_ in _SC_DataBase[_active_athlet].Activities[_active_activity].Track)
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

                    Writer_.WriteEndElement();
                }

                Writer_.WriteEndElement();
            }

            if (_SC_DataBase[_active_athlet].Activities[_active_activity].Markers != null)
            {
                Writer_.WriteStartElement("DistanceMarkers");

                foreach (Marker marker in _SC_DataBase[_active_athlet].Activities[_active_activity].Markers)
                {
                    Writer_.WriteStartElement("Marker");

                    if (marker.dist != null) Writer_.WriteAttributeString("dist", marker.dist.ToString());

                    Writer_.WriteEndElement();
                }

                Writer_.WriteEndElement();
            }

            if (_SC_DataBase[_active_athlet].Activities[_active_activity].Pausen != null)
            {
                Writer_.WriteStartElement("TrackClock");

                foreach (Pause pause in _SC_DataBase[_active_athlet].Activities[_active_activity].Pausen)
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



    public void WriteAllAthletData(AthleteLogData data)
        {
            foreach (AthleteData athlete_ in data.athletedata)
            {
                Writer_.WriteStartElement("AthleteLog");

                Writer_.WriteStartElement("Athlete");
                if (athlete_.Id != null) Writer_.WriteAttributeString("Id", athlete_.Id);
                if (athlete_.Name != null) Writer_.WriteAttributeString("Name", athlete_.Name);
                if (athlete_.DoB != null) Writer_.WriteAttributeString("DateOfBirth", athlete_.Name);
                if (athlete_.Height != null) Writer_.WriteAttributeString("HeightCentimeters", athlete_.Name);
                if (athlete_.Weight != null) Writer_.WriteAttributeString("WeightKilograms", athlete_.Name);
                Writer_.WriteEndElement();

                foreach (ActivityData data_ in athlete_.activitydata_list)
                {
                    Writer_.WriteStartElement("Activity");

                    if (data_.activityattributes.StartTime != null) Writer_.WriteAttributeString("StartTime", data_.activityattributes.StartTime.ToString());
                    if (data_.activityattributes.Id != null) Writer_.WriteAttributeString("Id", data_.activityattributes.Id);

                    if (data_.metadataattributes.Source != null || data_.metadataattributes.Created != null || data_.metadataattributes.Modified != null)
                    {
                        Writer_.WriteStartElement("Metadata");
                        if (data_.metadataattributes.Source != null) Writer_.WriteAttributeString("Source", data_.metadataattributes.Source);
                        if (data_.metadataattributes.Created != null) Writer_.WriteAttributeString("Created", data_.metadataattributes.Created.ToString());
                        if (data_.metadataattributes.Modified != null) Writer_.WriteAttributeString("Modified", data_.metadataattributes.Modified.ToString());
                        Writer_.WriteEndElement();
                    }
                    if (data_.durationattributes.TotalSeconds != null)
                    {
                        Writer_.WriteStartElement("Duration");
                        Writer_.WriteAttributeString("TotalSeconds", data_.durationattributes.TotalSeconds.ToString());
                        Writer_.WriteEndElement();
                    }
                    if (data_.distanceattributes.TotalMeters != null)
                    {
                        Writer_.WriteStartElement("Distance");
                        Writer_.WriteAttributeString("TotalMeters", data_.distanceattributes.TotalMeters.ToString());
                        Writer_.WriteEndElement();
                    }
                    if (data_.elevationattributes.DescendMeters != null || data_.elevationattributes.AscendMeters != null)
                    {
                        Writer_.WriteStartElement("Elevation");
                        if (data_.elevationattributes.DescendMeters != null) Writer_.WriteAttributeString("DescendMeters", data_.elevationattributes.DescendMeters.ToString());
                        if (data_.elevationattributes.AscendMeters != null) Writer_.WriteAttributeString("AscendMeters", data_.elevationattributes.AscendMeters.ToString());
                        Writer_.WriteEndElement();
                    }
                    if (data_.heartrateattributes.AverageBPM != null || data_.heartrateattributes.MaximumBPM != null)
                    {
                        Writer_.WriteStartElement("HeartRate");
                        if (data_.heartrateattributes.AverageBPM != null) Writer_.WriteAttributeString("AverageBPM", data_.heartrateattributes.AverageBPM.ToString());
                        if (data_.heartrateattributes.MaximumBPM != null) Writer_.WriteAttributeString("MaximumBPM", data_.heartrateattributes.MaximumBPM.ToString());
                        Writer_.WriteEndElement();
                    }
                    if (data_.cadenceattributes.AverageRPM != null || data_.cadenceattributes.MaximumRPM != null)
                    {
                        Writer_.WriteStartElement("Cadence");
                        if (data_.cadenceattributes.AverageRPM != null) Writer_.WriteAttributeString("AverageRPM", data_.cadenceattributes.AverageRPM.ToString());
                        if (data_.cadenceattributes.MaximumRPM != null) Writer_.WriteAttributeString("MaximumRPM", data_.cadenceattributes.MaximumRPM.ToString());
                        Writer_.WriteEndElement();
                    }
                    if (data_.powerattributes.AverageWatts != null || data_.powerattributes.MaximumWatts != null)
                    {
                        Writer_.WriteStartElement("Power");
                        if (data_.powerattributes.AverageWatts != null) Writer_.WriteAttributeString("AverageWatts", data_.powerattributes.AverageWatts.ToString());
                        if (data_.powerattributes.MaximumWatts != null) Writer_.WriteAttributeString("MaximumWatts", data_.powerattributes.MaximumWatts.ToString());
                        Writer_.WriteEndElement();
                    }
                    if (data_.caloriesattributes != null)
                    {
                        Writer_.WriteStartElement("Calories");
                        Writer_.WriteAttributeString("TotalCal", data_.caloriesattributes.ToString());
                        Writer_.WriteEndElement();
                    }

                    if (data_.infoattributes.Notes != null)
                    {
                        Writer_.WriteStartElement("Notes");
                        Writer_.WriteString(data_.infoattributes.Notes);
                        Writer_.WriteEndElement();
                    }

                    if (data_.infoattributes.Name != null)
                    {
                        Writer_.WriteStartElement("Name");
                        Writer_.WriteString(data_.infoattributes.Name);
                        Writer_.WriteEndElement();
                    }

                    if (data_.lapdata_List != null)
                    {
                        Writer_.WriteStartElement("Laps");

                        foreach (LapData lap_ in data_.lapdata_List)
                        {
                            Writer_.WriteStartElement("Lap");

                            if (lap_.lapattributes.StartTime != null) Writer_.WriteAttributeString("StartTime", lap_.lapattributes.StartTime.ToString());
                            if (lap_.lapattributes.DurationSeconds != null) Writer_.WriteAttributeString("DurationSeconds", lap_.lapattributes.DurationSeconds.ToString());
                            if (lap_.lapattributes.Notes != null) Writer_.WriteAttributeString("Notes", lap_.lapattributes.Notes);
                            if (lap_.lapattributes.Rest != null) Writer_.WriteAttributeString("Rest", lap_.lapattributes.Rest);

                            if (lap_.TotalMeters != null)
                            {
                                Writer_.WriteStartElement("Distance");
                                Writer_.WriteAttributeString("TotalMeters", lap_.TotalMeters.ToString());
                                Writer_.WriteEndElement();
                            }
                            if (lap_.TotalCal != null)
                            {
                                Writer_.WriteStartElement("Calories");
                                Writer_.WriteAttributeString("TotalCal", lap_.TotalCal.ToString());
                                Writer_.WriteEndElement();
                            }
                            if (lap_.HeartRate_avg != null)
                            {
                                Writer_.WriteStartElement("HeartRate");
                                Writer_.WriteAttributeString("HR_avg", lap_.HeartRate_avg.ToString());
                                Writer_.WriteAttributeString("HR_max", lap_.HeartRate_max.ToString());
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
                                Writer_.WriteAttributeString("Cad", lap_.Cadence.ToString());
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
                                Writer_.WriteAttributeString("Speed_max", lap_.Speed_max.ToString());
                                Writer_.WriteEndElement();
                            }
                            if (lap_.lat_start != null)
                            {
                                Writer_.WriteStartElement("Start");
                                Writer_.WriteAttributeString("Lat", lap_.lat_start.ToString());
                                Writer_.WriteAttributeString("Lon", lap_.lon_start.ToString());
                                Writer_.WriteEndElement();
                            }
                            if (lap_.lat_end != null)
                            {
                                Writer_.WriteStartElement("End");
                                Writer_.WriteAttributeString("Lat", lap_.lat_end.ToString());
                                Writer_.WriteAttributeString("Lon", lap_.lon_end.ToString());
                                Writer_.WriteEndElement();
                            }
                            // Power and Elevation not implemented

                            Writer_.WriteEndElement();
                        }

                        Writer_.WriteEndElement();
                    }

                    if (data_.weatherattributes.Conditions != null || data_.weatherattributes.Temp != null || data_.weatherattributes.Notes != null)
                    {
                        Writer_.WriteStartElement("Weather");
                        if (data_.weatherattributes.Conditions != null) Writer_.WriteAttributeString("Conditions", data_.weatherattributes.Conditions);
                        if (data_.weatherattributes.Temp != null) Writer_.WriteAttributeString("Temp", data_.weatherattributes.Temp.ToString());
                        if (data_.weatherattributes.Notes != null) Writer_.WriteAttributeString("Notes", data_.weatherattributes.Notes);
                        Writer_.WriteEndElement();
                    }

                    if (data_.categoryattributes.Id != null || data_.categoryattributes.Name != null)
                    {
                        Writer_.WriteStartElement("Category");
                        if (data_.categoryattributes.Id != null) Writer_.WriteAttributeString("Id", data_.categoryattributes.Id);
                        if (data_.categoryattributes.Name != null) Writer_.WriteAttributeString("Name", data_.categoryattributes.Name);
                        Writer_.WriteEndElement();
                    }

                    if (data_.routeattributes.Id != null || data_.routeattributes.Name != null)
                    {
                        Writer_.WriteStartElement("Route");
                        if (data_.routeattributes.Id != null) Writer_.WriteAttributeString("ID", data_.routeattributes.Id);
                        if (data_.routeattributes.Name != null) Writer_.WriteAttributeString("Name", data_.routeattributes.Name);
                        Writer_.WriteEndElement();
                    }

                    if (data_.locationattributes.Name != null || data_.locationattributes.Id != null)
                    {
                        Writer_.WriteStartElement("Location");
                        if (data_.locationattributes.Id != null) Writer_.WriteAttributeString("Id", data_.locationattributes.Id);
                        if (data_.locationattributes.Name != null) Writer_.WriteAttributeString("Name", data_.locationattributes.Name);
                        Writer_.WriteEndElement();
                    }
                    if (data_.equipementitemattributes.Id != null || data_.equipementitemattributes.Name != null)
                    {
                        Writer_.WriteStartElement("EquipmentUsed");
                        if (data_.equipementitemattributes.Id != null) Writer_.WriteAttributeString("Id", data_.equipementitemattributes.Id);
                        if (data_.equipementitemattributes.Name != null) Writer_.WriteAttributeString("Name", data_.equipementitemattributes.Name);
                        Writer_.WriteEndElement();
                    }

                    if (data_.trackdate_list != null)
                    {
                        Writer_.WriteStartElement("Track");

                        if (data_.trackattributes.StartTime != null) Writer_.WriteAttributeString("StartTime", data_.trackattributes.StartTime.ToString());

                        foreach (TrackPoint trackpoint_ in data_.trackdate_list)
                        {
                            Writer_.WriteStartElement("pt");

                            if (trackpoint_.tm != null) Writer_.WriteAttributeString("tm", trackpoint_.tm.ToString());
                            if (trackpoint_.lat != null) Writer_.WriteAttributeString("lat", trackpoint_.lat.ToString());
                            if (trackpoint_.lon != null) Writer_.WriteAttributeString("lon", trackpoint_.lon.ToString());
                            if (trackpoint_.ele != null) Writer_.WriteAttributeString("ele", trackpoint_.ele.ToString());
                            if (trackpoint_.dist != null) Writer_.WriteAttributeString("dist", trackpoint_.dist.ToString());
                            if (trackpoint_.hr != null) Writer_.WriteAttributeString("hr", trackpoint_.hr.ToString());
                            if (trackpoint_.cadence != null) Writer_.WriteAttributeString("cadence", trackpoint_.cadence.ToString());
                            if (trackpoint_.power != null) Writer_.WriteAttributeString("power", trackpoint_.power.ToString());

                            Writer_.WriteEndElement();
                        }

                        Writer_.WriteEndElement();
                    }

                    if (data_.marker_list != null)
                    {
                        Writer_.WriteStartElement("DistanceMarkers");

                        foreach (Marker marker in data_.marker_list)
                        {
                            Writer_.WriteStartElement("Marker");

                            if (marker.dist != null) Writer_.WriteAttributeString("dist", marker.dist.ToString());

                            Writer_.WriteEndElement();
                        }

                        Writer_.WriteEndElement();
                    }

                    if (data_.pause_list != null)
                    {
                        Writer_.WriteStartElement("TrackClock");

                        foreach (Pause pause in data_.pause_list)
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
