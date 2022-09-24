using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SC
{
    class Fitlog2database
    {
        // Umwandlung string -> datetime: https://www.google.com/search?client=firefox-b-e&q=c%23+string+to+datetime

        public Fitlog2database()
        {
        }

        public List<athlet> WriteAllAthletData2database(AthleteLogData data)
        {
            List<athlet> database_update = new List<athlet>();

            foreach (AthleteData athlete_ in data.athletedata)
            {
                athlet new_athlet = new athlet();

                if (athlete_.Id != null) new_athlet.AthleteID = athlete_.Id;
                if (athlete_.Name != null) new_athlet.AthleteName = athlete_.Name;
                if (athlete_.DoB != null) new_athlet.AthleteDoB = athlete_.DoB;
                if (athlete_.Height != null) new_athlet.AthleteHeightcm = athlete_.Height;

                foreach (ActivityData data_ in athlete_.activitydata_list)
                {
                    activity new_db_element = new activity();
                    fitness new_fitnes = new fitness();

                    if (athlete_.Weight != null) new_fitnes.WeightKilograms = athlete_.Weight.ToString();
                    if (athlete_.Weight != null) new_fitnes.Date = data_.metadataattributes.Modified;

                    if (data_.activityattributes.StartTime != null) new_db_element.ActivityTime = data_.activityattributes.StartTime;
                    if (data_.activityattributes.Id != null) new_db_element.ActivityID = data_.activityattributes.Id;

                    if (data_.metadataattributes.Source != null) new_db_element.MetaSource = data_.metadataattributes.Source;
                    if (data_.metadataattributes.Created != null) new_db_element.MetaCreated = data_.metadataattributes.Created;
                    if (data_.metadataattributes.Modified != null) new_db_element.MetaModified = data_.metadataattributes.Modified;

                    if (data_.durationattributes.TotalSeconds != null) new_db_element.Duration = (double)data_.durationattributes.TotalSeconds;
                    if (data_.distanceattributes.TotalMeters != null) new_db_element.Distance = (double)data_.distanceattributes.TotalMeters;
                    if (data_.elevationattributes.DescendMeters != null) new_db_element.total_Descent = (double)data_.elevationattributes.DescendMeters;
                    if (data_.elevationattributes.AscendMeters != null) new_db_element.total_Ascent = (double)data_.elevationattributes.AscendMeters;

                    if (data_.heartrateattributes.AverageBPM != null) new_db_element.mean_HR = (double)data_.heartrateattributes.AverageBPM;
                    if (data_.heartrateattributes.MaximumBPM != null) new_db_element.max_HR = (double)data_.heartrateattributes.MaximumBPM;
                    if (data_.cadenceattributes.AverageRPM != null) new_db_element.Cadence_mean = (double)data_.cadenceattributes.AverageRPM;
                    if (data_.cadenceattributes.MaximumRPM != null) new_db_element.Cadence_max = (double)data_.cadenceattributes.MaximumRPM;
                    if (data_.powerattributes.AverageWatts != null) new_db_element.Power_avrg = (double)data_.powerattributes.AverageWatts;
                    if (data_.powerattributes.MaximumWatts != null) new_db_element.Power_max = (double)data_.powerattributes.MaximumWatts;
                    if (data_.caloriesattributes.TotalCal != null) new_db_element.Calories_total = (int)data_.caloriesattributes.TotalCal;
                    if (data_.speedattributes.MaxSpeed != null) new_db_element.max_Speed = (double)data_.speedattributes.MaxSpeed;

                    if (data_.infoattributes.Notes != null) new_db_element.Info_Note = data_.infoattributes.Notes;
                    if (data_.infoattributes.Name != null) new_db_element.Info_Name = data_.infoattributes.Name;

                    if (data_.weatherattributes.Conditions != null) new_db_element.Weather_Cond = data_.weatherattributes.Conditions;
                    if (data_.weatherattributes.Temp != null) new_db_element.Weather_Temp = (int)data_.weatherattributes.Temp;
                    if (data_.weatherattributes.Notes != null) new_db_element.Weather_Note = data_.weatherattributes.Notes;

                    if (data_.categoryattributes.Id != null) new_db_element.CatID = data_.categoryattributes.Id;
                    if (data_.categoryattributes.Name != null) new_db_element.CatName = data_.categoryattributes.Name;
                    if (data_.routeattributes.Id != null) new_db_element.RouteID = data_.routeattributes.Id;
                    if (data_.routeattributes.Name != null) new_db_element.RouteName = data_.routeattributes.Name;
                    if (data_.locationattributes.Id != null) new_db_element.LocationID = data_.locationattributes.Id;
                    if (data_.locationattributes.Name != null) new_db_element.LocationName = data_.locationattributes.Name;

                    if (data_.equipementitemattributes.Id != null) new_db_element.Equipment_ID = data_.equipementitemattributes.Id;
                    if (data_.equipementitemattributes.Name != null) new_db_element.Equipment_Name = data_.equipementitemattributes.Name;

                    if (data_.lapdata_List != null)
                        foreach (LapData lap_ in data_.lapdata_List)
                        {
                            LapData _newLap = new LapData();

                            if (lap_.lapattributes.StartTime != null) _newLap.lapattributes.StartTime = lap_.lapattributes.StartTime;
                            if (lap_.lapattributes.DurationSeconds != null) _newLap.lapattributes.DurationSeconds = lap_.lapattributes.DurationSeconds;
                            if (lap_.lapattributes.Notes != null) _newLap.lapattributes.Notes = lap_.lapattributes.Notes;
                            if (lap_.lapattributes.Rest != null) _newLap.lapattributes.Rest = lap_.lapattributes.Rest;

                            if (lap_.TotalMeters != null) _newLap.TotalMeters = lap_.TotalMeters;
                            if (lap_.TotalCal != null) _newLap.TotalCal = lap_.TotalCal;
                            if (lap_.HeartRate_avg != null)
                            {
                                _newLap.HeartRate_avg = lap_.HeartRate_avg;
                                _newLap.HeartRate_max = lap_.HeartRate_max;
                            }
                            if (lap_.Intensity != null) _newLap.Intensity = lap_.Intensity;
                            if (lap_.Cadence != null) _newLap.Cadence = lap_.Cadence;
                            if (lap_.Trigger != null) _newLap.Trigger = lap_.Trigger;
                            if (lap_.Speed_max != null) _newLap.Speed_max = lap_.Speed_max;
                            if (lap_.lat_start != null)
                            {
                                _newLap.lat_start = lap_.lat_start;
                                _newLap.lon_start = lap_.lon_start;
                            }
                               
                            if (lap_.lat_end != null)
                            {
                                _newLap.lat_end = lap_.lat_end;
                                _newLap.lon_end = lap_.lon_end;
                            }
                            // Power and Elevation not implemented

                            new_db_element.Laps.Add(_newLap);
                        }
                    //if (data_.activityattributes.Id == "a314ac3e-0907-46b3-800f-41d91fcd4fd7") MessageBox.Show(data_.activityattributes.StartTime.ToString());

                    if (data_.trackdate_list != null)
                    {
                        bool firstTrackPoint = true;

                        double dummy_lon_old = 0; // for distance calculation
                        double dummy_lat_old = 0;
                        double dummy_dist_old = 0;

                        foreach (TrackPoint trackpoint_ in data_.trackdate_list)
                        {

                            TrackPoint _newtrackpoint = new TrackPoint();

                            if (trackpoint_.lat != null)
                                {
                                if (firstTrackPoint)
                                {
                                    firstTrackPoint = false;

                                    _newtrackpoint.tm = 0;
                                    _newtrackpoint.dist = 0;
                                    if (trackpoint_.lat != null) _newtrackpoint.lat = trackpoint_.lat;
                                    if (trackpoint_.lon != null) _newtrackpoint.lon = trackpoint_.lon;
                                    if (trackpoint_.ele != null) _newtrackpoint.ele = trackpoint_.ele;
                                    if (trackpoint_.hr != null) _newtrackpoint.hr = trackpoint_.hr;
                                    if (trackpoint_.cadence != null) _newtrackpoint.cadence = trackpoint_.cadence;
                                    if (trackpoint_.power != null) _newtrackpoint.power = trackpoint_.power;
                                }
                                else
                                {

                                    if (trackpoint_.tm != null) _newtrackpoint.tm = trackpoint_.tm;
                                    if (trackpoint_.lat != null) _newtrackpoint.lat = trackpoint_.lat;
                                    if (trackpoint_.lon != null) _newtrackpoint.lon = trackpoint_.lon;
                                    if (trackpoint_.ele != null) _newtrackpoint.ele = trackpoint_.ele;
                                    if (trackpoint_.hr != null) _newtrackpoint.hr = trackpoint_.hr;
                                    if (trackpoint_.cadence != null) _newtrackpoint.cadence = trackpoint_.cadence;
                                    if (trackpoint_.power != null) _newtrackpoint.power = trackpoint_.power;

                                    dummy_dist_old = dummy_dist_old + DatabaseHelpers.GetDistanceBetweenTwoPoints(dummy_lat_old, dummy_lon_old, (double)trackpoint_.lat, (double)trackpoint_.lon);
                                    _newtrackpoint.dist = ((int)Math.Round(dummy_dist_old));

                                }
                                dummy_lat_old = (double)trackpoint_.lat;
                                dummy_lon_old = (double)trackpoint_.lon;

                                new_db_element.Track.Add(_newtrackpoint);
                            }

                                    
                        }
                    }
                    //if (data_.activityattributes.Id == "a314ac3e-0907-46b3-800f-41d91fcd4fd7") MessageBox.Show(data_.activityattributes.StartTime.ToString());

                    if (data_.marker_list != null)

                        foreach (Marker marker in data_.marker_list)
                        {
                            Marker _newmarker = new Marker();

                            if (marker.dist != null) _newmarker.dist = marker.dist;

                            new_db_element.Markers.Add(_newmarker);
                        }

                    if (data_.pause_list != null)

                    

                        foreach (Pause pause in data_.pause_list)
                        {
                            Pause _newpause = new Pause();

                            if (pause.EndTime != null) _newpause.EndTime = pause.EndTime;
                            if (pause.StartTime != null) _newpause.StartTime = pause.StartTime;

                            new_db_element.Pause_total += (pause.EndTime - pause.StartTime);

                            new_db_element.Pausen.Add(_newpause);
                        }
          
                    new_athlet.Activities.Add(new_db_element);

                    if (athlete_.Weight != null) new_athlet.History.Add(new_fitnes);
                }
                database_update.Add(new_athlet);
            }
            return database_update;
        }
    }
}
