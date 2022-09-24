using SC._01_SRTM_Data;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A30x.DataTypes;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A30x.Interfaces;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A906.DataTypes;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A906.Interfaces;
using SC._03_Garmin_FR305.Core.Device.Garmin.PhysicalProtocol;
using SC._03_Garmin_FR305.Core.Device.Garmin.PhysicalProtocol.Exceptions;
using SRTM.Sources.USGS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SC
{
    class Garmin_Reader : IDisposable
    {
        private List<athlet> _SC_DataBase;
        private int _active_athlet;

        public void Dispose()
        {
        }

        public Garmin_Reader(List<athlet> database, int active_athlet)
        {
            _SC_DataBase = database;
            _active_athlet = active_athlet;
        }

        public List<activity> Import_Garmin_FR305(string _SRTM_Data_Dir, string _ApiKeyWeather)
        {
            // Based on original code (from https://sourceforge.net/projects/opensportsman/) data from USB device (Garmin Forerunner 305) will be fetched (track and lap data)
            // Original code from  mauricemarinus under GNU General Public License version 2.0 (GPLv2)

            // by writing data to track-list lon and lat position is transfered from semicircles into degrees (see Garmin documantation page 26) 
            // For time UTC-zone difference (to initial time reference) and DaylightSavingTime is checked (if true = +1h)

            GarminDeviceDiscoverer gdd = new GarminDeviceDiscoverer();

            List<activity> GarminDeviceTrackList = new List<activity>();

            // get in contact with Garmin device
            try
            {
                gdd.Initialize();
                gdd.FindDevices();

                if (gdd.Devices.Count < 1)
                {
                    MessageBox.Show(gdd.Devices.Count.ToString() + " @ Garmin device detection"); // eventuell nicht notwendig, da unten der Fehlercode ausgegebene wird.
                    return null; // eventuell besser die GarminDeviceTrackList zurückzugeben???
                }

                gdd.Devices[0].StartSession();
            }
            catch (GarminDeviceNotFoundException ex)
            {
                MessageBox.Show("Oops... " + ex.Message);
                return null; // eventuell besser die GarminDeviceTrackList zurückzugeben???
            }

            // straight forward programming - expecting ONE Garmin device detected
            try
            {
                GarminDevice gd = (GarminDevice)gdd.Devices[0];

                List<IA302<ITrkHdrType, ITrkPointType>> tracks = gd.FetchTracks();
                List<IA906> laps = gd.FetchLaps();

                DateTime initial_Date = new DateTime(1989, 12, 31);
                DateTime current_Date = DateTime.Now;

                TimeZone localTimeZone = TimeZone.CurrentTimeZone;
                TimeSpan currentOffset = localTimeZone.GetUtcOffset(current_Date);

                DateTime initial_localDate = initial_Date + currentOffset;
                DateTime date_Track = DateTime.Now;

                // New form for track selection (tracks, not in SC_DB will be automatically checked)
                Data select = new Data();

                foreach (IA302<ITrkHdrType, ITrkPointType> track in tracks)
                {
                    D304_Trk_Point_Type tp_first = (D304_Trk_Point_Type)track.TrackPoints[0];
                    D304_Trk_Point_Type tp_last = (D304_Trk_Point_Type)track.TrackPoints[track.TrackPoints.Count - 3]; // last and seconde last point from FR305 can be invalide!

                    Track4Selection new_track = new Track4Selection();

                    // starting time:
                    new_track.ActivityTime = initial_localDate.AddSeconds(tp_first.time);
                    if (localTimeZone.IsDaylightSavingTime(date_Track)) // summer time shift?
                    {
                        //new_track.ActivityTime = new_track.ActivityTime.AddHours(1); //  summer time shift correction
                    }

                    // duration:
                    new_track.Duration = tp_last.time - tp_first.time;
                    
                    // distance:
                    new_track.Distance = tp_last.distance;
                    
                    // Proof if track is in SC_DB -> flag: checked / unchecked
                    foreach (activity activity_DB in _SC_DataBase[_active_athlet].Activities)
                    {
                        if (new_track.ActivityTime.ToString() == activity_DB.ActivityTime.ToString())
                        {
                            new_track.Check = false;
                        }
                    } 
                    select.track.Add(new_track);
                }

                // new Form for track selection
                Form_TrackSelection ts = new Form_TrackSelection(tracks.Count, select);
                ts.ShowDialog();
               
                foreach (int i in select.sel_tracks) // every selected track will be loaded to SC_DB
                {
                    bool firstTP = true;
                    bool newActivity = true;
                    bool newPause = true;
                    DateTime pause_dummy = DateTime.Now;
                    int StartTime = 0;
                    activity new_activity = new activity();

                    foreach (ITrkPointType item in tracks[i].TrackPoints)
                    {
                        D304_Trk_Point_Type tp = (D304_Trk_Point_Type)item;
                        TrackPoint NewTrackpoint = new TrackPoint(tp);

                        if (tp.posn.lat == Int32.MaxValue && tp.posn.lon == Int32.MaxValue && tp.heart_rate == 0 && tp.alt > 100000000) // trkpt not valide -> Pause
                        // Two consecutive track points with invalid position, invalid altitude, invalid heart rate, invalid distance and invalid
                        // cadence indicate a pause in track point recording during the time between the two points.
                        {
                            if (newPause)
                            {
                                newPause = false;
                                pause_dummy = initial_localDate.AddSeconds(tp.time); // first datetime of pause 
                            }
                            else
                            {
                                newPause = true;
                                Pause newPauseItem = new Pause();
                                newPauseItem.StartTime = pause_dummy;
                                newPauseItem.EndTime = initial_localDate.AddSeconds(tp.time);

                                // Hier muss Prüfung rein: PauseItem >4h -> Spalte Track auf f!!!!
                                // Strategie -> Merke Dir hier an welche(n) Stelle(n) die >4h Pause sitzt(en) -> Zusätzliche Routine am Ende bei "Add-Track"
                                //(xxx

                                new_activity.Pausen.Add(newPauseItem);

                                
                                new_activity.Pause_total += (newPauseItem.EndTime - newPauseItem.StartTime); //sum-up all pause times
                            }
                        }
                        else
                        {

                            if (firstTP)
                            {
                                firstTP = false;
                                StartTime = (int)NewTrackpoint.tm;

                                date_Track = initial_localDate.AddSeconds(tp.time); // starting date of Garmin track
                                if (localTimeZone.IsDaylightSavingTime(date_Track)) // summer time shift?
                                {
                                    //date_Track = date_Track.AddHours(1); //  falsch, da FR305 dies schon in seiner Zeit berücksichtigt
                                }

                                new_activity.ActivityTime = date_Track;

                                foreach (activity activity_DB in _SC_DataBase[_active_athlet].Activities)
                                {
                                    if (new_activity.ActivityTime.ToString() == activity_DB.ActivityTime.ToString())
                                    {
                                        MessageBox.Show("Track skiped (Activity already in SC-Database)");
                                        newActivity = false;
                                    }
                                } // if Track-date exisits in DB -> no chance to introduce track (TO-DO: replace method)
                            }

                            NewTrackpoint.tm -= StartTime; // getting the net-time

                            if (tp.posn.lat != Int32.MaxValue && tp.posn.lon != Int32.MaxValue)
                            {
                                new_activity.Track.Add(NewTrackpoint);
                            }
                        }
                    }

                    bool first_lap = true;
                    double dummy_distance = 0;
                    double dummy_duration = 0;
                    double dummy_HR_avg = 0;
                    double dummy_HR_max = 0;
                    double dummy_cadence_avg = 0;
                    double dummy_calories = 0;
                    double dummy_maxSpeed = 0;

                    if (new_activity.Track.Any()) //check: is there any data in?
                    {
                        foreach (IA906 lap in laps)
                        {
                            D1015_Lap_Type l = (D1015_Lap_Type)lap;
                            if (l.start_time >= StartTime && l.start_time < (StartTime + new_activity.Track[new_activity.Track.Count - 1].tm))
                            {
                                if (!first_lap)
                                {
                                    LapData newlap = new LapData(l);
                                    newlap.lapattributes.StartTime = initial_localDate.AddSeconds(l.start_time);
                                    new_activity.Laps.Add(newlap);

                                    Marker new_marker = new Marker();
                                    new_marker.dist = dummy_distance;

                                    if (l.max_heart_rate > dummy_HR_max) dummy_HR_max = l.max_heart_rate;
                                    dummy_HR_avg = ((dummy_HR_avg * dummy_duration) + (l.avg_heart_rate * (l.total_time / 100))) / (dummy_duration + l.total_time / 100);
                                    dummy_cadence_avg = ((dummy_cadence_avg * dummy_duration) + (l.avg_cadence * (l.total_time / 100))) / (dummy_duration + l.total_time / 100);

                                    dummy_calories += l.calories;
                                    if (l.max_speed > dummy_maxSpeed) dummy_HR_max = l.max_speed;

                                    dummy_distance += l.total_dist;
                                    dummy_duration += l.total_time / 100;
                                }

                                if (first_lap)
                                {
                                    LapData newlap = new LapData(l); // All data from Garmin Lap go directly to LapData (in SC data base)
                                    newlap.lapattributes.StartTime = initial_localDate.AddSeconds(l.start_time);
                                    new_activity.Laps.Add(newlap);

                                    dummy_distance += l.total_dist;
                                    dummy_duration += l.total_time / 100;
                                    dummy_HR_max = l.max_heart_rate;
                                    dummy_HR_avg = l.avg_heart_rate;
                                    dummy_cadence_avg = l.avg_cadence;
                                    dummy_calories = l.calories;
                                    dummy_maxSpeed = l.max_speed;
                                    first_lap = false;
                                }
                            }
                        }
                    }

                    new_activity.MetaSource = "Importiert von Garmin - Forerunner 305 [USB]";
                    new_activity.Duration = dummy_duration;
                    new_activity.Distance = dummy_distance;

                    new_activity.mean_HR = dummy_HR_avg;
                    new_activity.max_HR = dummy_HR_max;
                    new_activity.Cadence_mean = dummy_cadence_avg;
                    // max cadence: Auswertung über den gesamten track -> get_maxCadence()
                    // Pausetotal
                    // Power: no Garmin FR305 field 

                    // ----------------------------------------------------------------------------------------------
                    // start of analysis section
                    new_activity.LocationName = DatabaseHelpers.Get_Location(_SC_DataBase, _active_athlet, 
                        (double)new_activity.Laps[0].lat_start, (double)new_activity.Laps[0].lon_start); // Hier liegt der FEHLER (Index out of range)

                    // Elevation Data from SRTM is standard - if no elevation SRTM-data is avaialable GARMIN data will be used 
                    if (new_activity.Track.Any())
                    {
                        var srtmData = new SRTM.SRTMData(_SRTM_Data_Dir, new USGSSource());
                        foreach (TrackPoint tpt in new_activity.Track)
                        {
                            tpt.ele = (double)TrackHelpers.check4NegativeValue(srtmData.GetElevation((double)tpt.lat, (double)tpt.lon));
                            // if SRTM data is available ele data from GARMIN will be overridden 
                        }
                    }
                    
                    // analysis of up and down meters
                    double[] elev = EleHelpers.Calc_Elevation(EleHelpers.GetEleData(new_activity.Track));
                    new_activity.Info_Note += "Elevation source: SRTM";
                    new_activity.total_Ascent = elev[0]; // up!
                    new_activity.total_Descent = elev[1]; // down!

                    // get weather data
                    WeatherObject weather = GetWeather.GetWetterByCoordTime((double)new_activity.Laps[0].lat_start, (double)new_activity.Laps[0].lon_start,
                       new_activity.ActivityTime, _ApiKeyWeather);
                    if (weather != null)
                    {
                        new_activity.Weather_Cond = weather.data[0].weather[0].main;
                        new_activity.Weather_Temp = (int)(weather.data[0].temp - 273);    
                        new_activity.Weather_Note = "ID: " + weather.data[0].weather[0].id.ToString() + " / Main: " + weather.data[0].weather[0].description + " / Icon: " + weather.data[0].weather[0].icon;
                    }

                    // ------------------------------------------------------------------------------------------------
                    // end of analysis section

                    //
                    // Power calculation section
                    //
                    // Grundstategie: Berechne die Leistung: P_ges = P_hub + P_Roll + P_Luft
                    // https://www.michael-konczer.com/de/training/rechner/rennrad-leistung-berechnen

                    // Daraus lässt sich dan einfach die mittlere Arbeit W_avg berechnen: W_ges = P_ges / Wirkungsgrad_Mensch * Umrechenfaktor Wh/kcal
                    // Wirkungsgrad_Mensch = 25% (nur 25% der Umgesetzten Energie geht in die Leistung)
                    // Umrechnungsfaktor kcal = 0,8598

                    // P_Hub = m * g * Höhenmeter_delta (nur positiv!) / 1sec (ist durch das Zeitraster bereits vorgegeben)
                    // P_Luft = 0,5 * c_w * A * Rho * v^3
                    //        = 0,5 * 0,3     * 1,2 * v^3
                    // P_Roll = m * g    * µ_R    * v
                    //        = m * 9,81 * 0,008 * v

                    // m = 90 (zunächst statisch, kann erweitert werden wenn Sportler-Tagebuch mit Tagesgewicht vorliegt
                    // g = 9,81

                    double _power_up = 90 * 9.81 * new_activity.total_Ascent / new_activity.Duration;
                    double _power_v = 0.5 * 0.3 * 1.2 * Math.Pow(new_activity.Distance / new_activity.Duration, 3);
                    double _power_roll = 90 * 9.81 * 0.008 * new_activity.Distance / new_activity.Duration;

                    new_activity.Power_avrg = _power_up + _power_v + _power_roll;
                    //
                    // end of power calculation section
                    //

                    // Consistent calories calculation. Lap calories still from Garmin device
                    new_activity.Calories_total = Math.Round(new_activity.Power_avrg * 0.8598 * 4 * new_activity.Duration / 3600);
                    //
                    // new_activity.Calories_total = dummy_calories;
                    //
                    
                    new_activity.max_Speed = Math.Round(dummy_maxSpeed * 3.6, 1);

                    if (new_activity.Track.Count < 10) newActivity = false; // if there is no track I do not want this activity from Garmin

                    // Abfrage: Pause >4h -> Dann bitte die Absplitten und Prozedur neu starten

                    // DeviceUnitId : 3821696846 = CycloCross Garmin
                    if (gd.DeviceUnitId == 3821696846) new_activity.CatName = "CycloCross";
                    else new_activity.CatName = "Rennrad";

                    if (newActivity) GarminDeviceTrackList.Add(new_activity);
                }
                
                gdd.FinalizeAll();

                MessageBox.Show("Import(s) from Garmin FR305 finished!");
                    
                return GarminDeviceTrackList;
            }
            catch (GarminDeviceNotFoundException ex)
            {
                MessageBox.Show(ex.Message + " Garmin FR305 Load");
                return null;
            }
        }
    }
}
