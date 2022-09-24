using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static SC.Tcx;

namespace SC
{
    class Tcx2database
    {
        public List<activity> WriteLapData2database(TcxActivities act_, List<athlet> _SC_DataBase, int _active_athlet)
        {
            List<activity> newActivityList = new List<activity>();
            activity new_activity = new activity();

            bool firstTrackPoint = true;

            if (act_._activityData.creator.Name != null) new_activity.MetaSource = act_._activityData.creator.Name;
            if (act_._activityData.Id != null) new_activity.MetaCreated = DateTime.Parse(act_._activityData.Id);
            if (act_._activityData.Id != null) new_activity.MetaModified = DateTime.Parse(act_._activityData.Id);
            if (act_._activityData.creator.Name != null) new_activity.MetaSource = act_._activityData.creator.Name;
            if (act_._activityData.activityattributes.Sport != null) new_activity.Info_Note = act_._activityData.activityattributes.Sport;
            //if (lap_.creator.ProductID != null) //Aktuell keine Verwendung
            //if (lap_.creator.UnitID != null) //Aktuell keine Verwendung

            foreach (TcxTrackPoint _tp in act_._activityData.lapdata.Track)
            {
                TrackPoint _newtrackpoint = new TrackPoint();

                if (firstTrackPoint)
                {

                    firstTrackPoint = false;
                    if (_tp.Time != null)
                    {
                        new_activity.ActivityTime = DateTime.Parse(_tp.Time);
                        _newtrackpoint.tm = 0;
                    }

                    if (_tp.Position.LatitudeDegrees != null) _newtrackpoint.lat = Convert.ToDouble(_tp.Position.LatitudeDegrees, CultureInfo.InvariantCulture);
                    if (_tp.Position.LongitudeDegrees != null) _newtrackpoint.lon = double.Parse(_tp.Position.LongitudeDegrees, CultureInfo.InvariantCulture);
                    if (_tp.AltitudeMeters != null) _newtrackpoint.ele = double.Parse(_tp.AltitudeMeters, CultureInfo.InvariantCulture);
                    if (_tp.DistanceMeters != null) _newtrackpoint.dist = double.Parse(_tp.DistanceMeters, CultureInfo.InvariantCulture);
                    if (_tp.HeartRateBpm != null) _newtrackpoint.hr = double.Parse(_tp.HeartRateBpm, CultureInfo.InvariantCulture);
                    if (_tp.Cadence != null) _newtrackpoint.cadence = double.Parse(_tp.Cadence, CultureInfo.InvariantCulture);
                    // Sensorstate wird nicht ausgewertet
                    if (_tp.Extensions.Speed != null) _newtrackpoint.speed_device = double.Parse(_tp.Extensions.Speed, CultureInfo.InvariantCulture);
                    if (_tp.Extensions.Watts != null) _newtrackpoint.power = double.Parse(_tp.Extensions.Watts, CultureInfo.InvariantCulture);
                    // RunCadence wird nicht ausgewertet
                }
                else
                {
                    _newtrackpoint.tm = ((int)Math.Round((DateTime.Parse(_tp.Time) - new_activity.ActivityTime).TotalSeconds));

                    if (_tp.Position.LatitudeDegrees != null) _newtrackpoint.lat = double.Parse(_tp.Position.LatitudeDegrees, CultureInfo.InvariantCulture);
                    if (_tp.Position.LongitudeDegrees != null) _newtrackpoint.lon = double.Parse(_tp.Position.LongitudeDegrees, CultureInfo.InvariantCulture);
                    if (_tp.AltitudeMeters != null) _newtrackpoint.ele = double.Parse(_tp.AltitudeMeters, CultureInfo.InvariantCulture);
                    if (_tp.DistanceMeters != null) _newtrackpoint.dist = double.Parse(_tp.DistanceMeters, CultureInfo.InvariantCulture);
                    if (_tp.HeartRateBpm != null) _newtrackpoint.hr = double.Parse(_tp.HeartRateBpm, CultureInfo.InvariantCulture);
                    if (_tp.Cadence != null) _newtrackpoint.cadence = double.Parse(_tp.Cadence, CultureInfo.InvariantCulture);
                    // Sensorstate wird nicht ausgewertet
                    if (_tp.Extensions.Speed != null) _newtrackpoint.speed_device = double.Parse(_tp.Extensions.Speed, CultureInfo.InvariantCulture);
                    //MessageBox.Show(_newtrackpoint.speed_device.ToString());
                    if (_tp.Extensions.Watts != null) _newtrackpoint.power = double.Parse(_tp.Extensions.Watts, CultureInfo.InvariantCulture);
                    // RunCadence wird nicht ausgewertet
                }
                new_activity.Track.Add(_newtrackpoint);
            }

            // -------------------------------------------------------------------------------
            // Analysis section - getting avg- and max data from track data... 

            double[] elevation = calc_Elevation(GetEle_from_List(new_activity.Track));

            new_activity.total_Ascent = elevation[0]; // up
            new_activity.total_Descent = elevation[1]; // down

            double[] avg_max = GetAvgMax_Values(new_activity.Track); // HR_avg, HR_max, Pow_avg, Pow_max, Cad_avg, Cad_max -> Pow_max and Cad_max will be not used -> see filtered approach

            new_activity.mean_HR = avg_max[0]; // HR_avg
            new_activity.max_HR = avg_max[1]; // HR_max
            new_activity.Power_avrg = avg_max[2]; // Pow_avg
            new_activity.Power_max = get_max_Value(GetPower_from_List(new_activity.Track)); // use filtered data for max value
            new_activity.Cadence_mean = avg_max[4]; // Cad_avg
            new_activity.Cadence_max = get_max_Value(GetCad_from_List(new_activity.Track)); // use filtered data for max value

            // max speed... ???

            new_activity.LocationName = DatabaseHelpers.Get_Location(_SC_DataBase, _active_athlet,
                       (double)new_activity.Track[0].lat, (double)new_activity.Track[0].lon);

            //new_activity.Weather_Cond = WeatherCondition.Clear.ToString();
            new_activity.Weather_Cond = "Indoor";
            new_activity.Weather_Temp = 20;
            new_activity.Weather_Note = "Indoor Cycling";
            new_activity.CatName = "smartMTB";

            // End of analysis section
            // ------------------------------------------------------------------------------

            if (act_._activityData.lapdata.TotalTimeSeconds != null) new_activity.Duration = Convert.ToDouble(act_._activityData.lapdata.TotalTimeSeconds, CultureInfo.InvariantCulture);
            if (act_._activityData.lapdata.DistanceMeters != null) new_activity.Distance = Convert.ToDouble(act_._activityData.lapdata.DistanceMeters);
            if (act_._activityData.lapdata.MaximumSpeed != null) new_activity.max_Speed = double.Parse(act_._activityData.lapdata.MaximumSpeed); // calculate!
            
            //if (act_._activityData.lapdata.Calories != null) new_activity.Calories_total = Convert.ToDouble(act_._activityData.lapdata.Calories);
            new_activity.Calories_total = Math.Round(new_activity.Power_avrg * 0.8598 * 4 * new_activity.Duration / 3600);
            // Bei gestückelten TCX-Aktivitäten (z.B. smartBike Tour wird auf zwei unterschiedliche Tage verteilt ergibt sich Fehler in der Kalorien-Berechnung
            
            if (act_._activityData.lapdata.Notes != null) new_activity.Info_Note = act_._activityData.lapdata.Notes;
            // Extension wird nicht ausgewertet - keine weiteren Informationen aktuell vorhanden

            // Tcx-data muss noch einer Lap in SC database zugeordnet werden! 
            LapData newlap = new LapData();
            if (act_._activityData.lapdata.lapattributes.StartTime != null) newlap.lapattributes.StartTime = DateTime.Parse(act_._activityData.lapdata.lapattributes.StartTime);
            if (act_._activityData.lapdata.TotalTimeSeconds != null) newlap.lapattributes.DurationSeconds = new_activity.Duration;
            if (act_._activityData.lapdata.Notes != null) newlap.lapattributes.Notes = act_._activityData.lapdata.Notes;
            if (act_._activityData.lapdata.DistanceMeters != null) newlap.TotalMeters = Convert.ToDouble(act_._activityData.lapdata.DistanceMeters);
            if (act_._activityData.lapdata.MaximumSpeed != null) newlap.Speed_max = double.Parse(act_._activityData.lapdata.MaximumSpeed);  // calculate!

            //if (act_._activityData.lapdata.Calories != null) newlap.TotalCal = Convert.ToDouble(act_._activityData.lapdata.Calories);
            newlap.TotalCal = Math.Round(new_activity.Power_avrg * 0.8598 * 4 * new_activity.Duration / 3600);
            // Bei gestückelten TCX-Aktivitäten (z.B. smartBike Tour wird auf zwei unterschiedliche Tage verteilt ergibt sich Fehler in der Kalorien-Berechnung
            
            if (act_._activityData.lapdata.Intensity != null) newlap.Intensity = GetIntensityValue(act_._activityData.lapdata.Intensity);
            if (act_._activityData.lapdata.TriggerMethod != null) newlap.Trigger = GetTriggerValue(act_._activityData.lapdata.TriggerMethod);

            newlap.HeartRate_avg = new_activity.mean_HR;  // calculated
            newlap.HeartRate_max = new_activity.max_HR;  // calculated
            newlap.Power = new_activity.Power_avrg; // calculated
            newlap.Power_max = new_activity.Power_max; // calculated
            newlap.Cadence = new_activity.Cadence_mean; // calculated
            newlap.Cadence_max = new_activity.Cadence_max; // calculated

            // Falls doch Originaldaten vom Sportgerät übertragen werden -> Überschreiben der Lap-Daten -> ist also nach der Analyse notwendig

            if (act_._activityData.lapdata.Cadence != null) newlap.Cadence = double.Parse(act_._activityData.lapdata.Cadence);  // if original data from sport unit avaialble
            if (act_._activityData.lapdata.AverageHeartRateBpm != null) newlap.HeartRate_avg = double.Parse(act_._activityData.lapdata.AverageHeartRateBpm);  // if original data from sport unit avaialble
            if (act_._activityData.lapdata.MaximumHeartRateBpm != null) newlap.HeartRate_max = double.Parse(act_._activityData.lapdata.MaximumHeartRateBpm);  // if original data from sport unit avaialble

            new_activity.Laps.Add(newlap);

            newActivityList.Add(new_activity);
            return newActivityList;
        }

        private double[] GetAvgMax_Values(List<TrackPoint> TrackPoint_List)
        {
            double[] dummy_aray = { 0, 0, 0, 0, 0, 0 }; // HR_avg, HR_max, Pow_avg, Pow_max, Cad_avg, Cad_max -> Pow_max and Cad_max will be not used -> see filtered approach

            for (int i = 0; i < TrackPoint_List.Count; i++)
            {
                if (TrackPoint_List[i].hr != null)
                {
                    dummy_aray[0] += (double)TrackPoint_List[i].hr;
                    if (dummy_aray[1] < (double)TrackPoint_List[i].hr) dummy_aray[1] = (double)TrackPoint_List[i].hr;
                }
                
                if (TrackPoint_List[i].power != null)
                {
                    dummy_aray[2] += (double)TrackPoint_List[i].power;
                    if (dummy_aray[3] < (double)TrackPoint_List[i].power) dummy_aray[3] = (double)TrackPoint_List[i].power;
                }

                if (TrackPoint_List[i].cadence != null)
                {
                    dummy_aray[4] += (double)TrackPoint_List[i].cadence;
                    if (dummy_aray[5] < (double)TrackPoint_List[i].cadence) dummy_aray[5] = (double)TrackPoint_List[i].cadence;
                }
            }

            dummy_aray[0] = dummy_aray[0] / TrackPoint_List.Count;
            dummy_aray[2] = dummy_aray[2] / TrackPoint_List.Count;
            dummy_aray[4] = dummy_aray[4] / TrackPoint_List.Count;

            return dummy_aray;
        }

        private List<double> GetEle_from_List(List<TrackPoint> TrackPoint_List)
        {
            List<double> dummy_List = new List<double>();

            for (int i = 0; i < TrackPoint_List.Count; i++)
            {
                if (TrackPoint_List[i].ele != null) dummy_List.Add((double)TrackPoint_List[i].ele);
            }

            return dummy_List;
        }
        private List<double> GetPower_from_List(List<TrackPoint> TrackPoint_List)
        {
            List<double> dummy_List = new List<double>();

            for (int i = 0; i < TrackPoint_List.Count; i++)
            {
                if (TrackPoint_List[i].power != null) dummy_List.Add((double)TrackPoint_List[i].power);
            }

            return dummy_List;
        }

        private List<double> GetCad_from_List(List<TrackPoint> TrackPoint_List)
        {
            List<double> dummy_List = new List<double>();

            for (int i = 0; i < TrackPoint_List.Count; i++)
            {
                if (TrackPoint_List[i].cadence != null) dummy_List.Add((double)TrackPoint_List[i].cadence);
            }

            return dummy_List;
        }

        private double get_max_Value (List<double> data_list)
        {
            List<double> _data_list = SavitzkyGolayFilter(data_list);
            double dummy_value = 0;

            for (int i = 1; i < _data_list.Count; i++)
            {
                if (dummy_value < _data_list[i]) dummy_value = _data_list[i];

            }

            return dummy_value;
        }

        private double[] calc_Elevation(List<double> data_list)
        {
            List<double> _data_list = SavitzkyGolayFilter(data_list);
            double[] dummy_aray = { 0, 0};

            double old_value = _data_list[0];

            for (int i = 1; i < _data_list.Count; i++)
            {
                double diff = _data_list[i] - old_value;
                old_value = _data_list[i];
                if (diff >= 0)
                {
                    dummy_aray[0] += diff; // up
                }
                else
                {
                    dummy_aray[1] += diff; // down
                }
            }

            return dummy_aray;
        }

        private static List<double> SavitzkyGolayFilter(List<double> data_list)
        {
            List<double> Output = new List<double>(data_list);

            for (int i = 0; i < data_list.Count; i++)
            {
                if (i >= 12 & i < data_list.Count - 12)
                {
                    Output[i] =
                    (data_list[i - 12] * -253 + data_list[i - 11] * -138
                    + data_list[i - 10] * -33 + data_list[i - 9] * 62
                    + data_list[i - 8] * 147 + data_list[i - 7] * 222
                    + data_list[i - 6] * 287 + data_list[i - 5] * 343
                    + data_list[i - 4] * 387 + data_list[i - 3] * 422
                    + data_list[i - 2] * 447 + data_list[i - 1] * 462
                    + data_list[i] * 467
                    + data_list[i + 1] * 462 + data_list[i + 2] * 447
                    + data_list[i + 3] * 422 + data_list[i + 4] * 387
                    + data_list[i + 5] * 343 + data_list[i + 6] * 287
                    + data_list[i + 7] * 222 + data_list[i + 8] * 147
                    + data_list[i + 9] * 62 + data_list[i + 10] * -33
                    + data_list[i + 11] * -138 + data_list[i + 12] * -253) / 5175;
                }
                else
                {
                    if (i >= 8 & i < data_list.Count - 8)
                    {
                        Output[i] = (data_list[i - 8] * -21
                            + data_list[i - 7] * -6
                            + data_list[i - 6] * 7
                            + data_list[i - 5] * 18
                            + data_list[i - 4] * 27
                            + data_list[i - 3] * 34
                            + data_list[i - 2] * 39
                            + data_list[i - 1] * 42
                            + data_list[i] * 43
                            + data_list[i + 1] * 42
                            + data_list[i + 2] * 39
                            + data_list[i + 3] * 34
                            + data_list[i + 4] * 27
                            + data_list[i + 5] * 18
                            + data_list[i + 6] * 7
                            + data_list[i + 7] * -6
                            + data_list[i + 8] * -21) / 323;
                    }
                    else
                    {
                        if (i >= 4 & i < data_list.Count - 4)
                        {
                            Output[i] =
                                (data_list[i - 4] * -21
                                + data_list[i - 3] * 14
                                + data_list[i - 2] * 39
                                + data_list[i - 1] * 54
                                + data_list[i] * 59
                                + data_list[i + 1] * 54
                                + data_list[i + 2] * 39
                                + data_list[i + 3] * 14
                                + data_list[i + 4] * -21) / 231;
                        }
                        else
                        {
                            if (i >= 2 & i < data_list.Count - 2)
                            {
                                Output[i] =
                                    (data_list[i - 2] * -3
                                    + data_list[i - 1] * 12
                                    + data_list[i] * 17
                                    + data_list[i + 1] * 12
                                    + data_list[i + 2] * -3) / 35;
                            }
                        }
                    }
                }
            }

            return Output;
        }

        private double GetIntensityValue(string IntensitName)
        {
            double intensity_value = 0;

            switch (IntensitName)
            {
                case "Active":
                    intensity_value = 0;
                    break;
                case "active":
                    intensity_value = 0;
                    break;
                case "rest":
                    intensity_value = 1;
                    break;
                case "Rest":
                    intensity_value = 1;
                    break;
            }

            return intensity_value;
        }
        private double GetTriggerValue(string TriggerMethod)
        {
            double trigger_value = 0;

            switch (TriggerMethod)
            {
                case "manual":
                    trigger_value = 0;
                    break;
                case "distance":
                    trigger_value = 1;
                    break;
                case "location":
                    trigger_value = 2;
                    break;
                case "time":
                    trigger_value = 3;
                    break;
                case "heart_rate":
                    trigger_value = 4;
                    break;
            }

            return trigger_value;
        }
    }
}
