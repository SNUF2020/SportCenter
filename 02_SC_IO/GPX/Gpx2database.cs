using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC
{
    class Gpx2database
    {
        public List<activity> WriteTrackData2database(GpxTrack track_, List<athlet> _SC_DataBase, int _active_athlet)
        {
            List<activity> newActivityList = new List<activity>();

            foreach (GpxTrackSegment _segment in track_.Segments)
            {
                activity new_activity = new activity();
                bool firstTrackPoint = true;

                if (_segment.TrackPoints != null)
                {
                    double dummy_lon_old = 0; // for distance calculation
                    double dummy_lat_old = 0;
                    double dummy_dist_old = 0;
                    int dummy_maxTime = 0;

                    foreach (GpxTrackPoint trackpoint_ in _segment.TrackPoints)
                    {
                        TrackPoint _newtrackpoint = new TrackPoint();

                        if (firstTrackPoint)
                        {
                            firstTrackPoint = false;
                            if (trackpoint_.Time != null)
                            {
                                new_activity.ActivityTime = (DateTime)trackpoint_.Time;
                                new_activity.MetaCreated = (DateTime)trackpoint_.Time;
                                new_activity.MetaModified = (DateTime)trackpoint_.Time;
                                new_activity.MetaSource = "From GPX-file";
                            }

                            _newtrackpoint.tm = 0;
                            _newtrackpoint.dist = 0;
                            _newtrackpoint.lat = trackpoint_.Latitude;
                            _newtrackpoint.lon = trackpoint_.Longitude;
                            if (trackpoint_.Elevation != null) _newtrackpoint.ele = trackpoint_.Elevation;
                            if (trackpoint_.HeartRate != null) _newtrackpoint.hr = trackpoint_.HeartRate;
                            if (trackpoint_.Cadence != null) _newtrackpoint.cadence = trackpoint_.Cadence;
                        }
                        else
                        {
                            _newtrackpoint.tm = ((int)Math.Round(((DateTime)trackpoint_.Time - new_activity.ActivityTime).TotalSeconds));

                            dummy_dist_old = dummy_dist_old + DatabaseHelpers.GetDistanceBetweenTwoPoints(dummy_lat_old, dummy_lon_old, trackpoint_.Latitude, trackpoint_.Longitude);
                            _newtrackpoint.dist = ((int)Math.Round(dummy_dist_old));

                            _newtrackpoint.lat = trackpoint_.Latitude;
                            _newtrackpoint.lon = trackpoint_.Longitude;
                            if (trackpoint_.Elevation != null) _newtrackpoint.ele = trackpoint_.Elevation;
                            if (trackpoint_.HeartRate != null) _newtrackpoint.hr = trackpoint_.HeartRate;
                            if (trackpoint_.Cadence != null) _newtrackpoint.cadence = trackpoint_.Cadence;
                        }

                        dummy_lat_old = trackpoint_.Latitude;
                        dummy_lon_old = trackpoint_.Longitude;
                        dummy_maxTime = (int)_newtrackpoint.tm;

                        new_activity.Track.Add(_newtrackpoint);
                    }

                    new_activity.Distance = Math.Round(dummy_dist_old);
                    new_activity.Duration = dummy_maxTime;

                    // -------------------------------------------------------------------------------
                    // Analysis section - getting avg- and max data from track data... 

                    double[] elevation = calc_Elevation(GetEle_from_List(new_activity.Track));

                    new_activity.total_Ascent = elevation[0]; // up
                    new_activity.total_Descent = elevation[1]; // down

                    // max speed... ???

                    new_activity.LocationName = DatabaseHelpers.Get_Location(_SC_DataBase, _active_athlet,
                       (double)new_activity.Track[0].lat, (double)new_activity.Track[0].lon);

                    // End of analysis section
                    // ------------------------------------------------------------------------------

                    // No further pre-definition: GPX could be hicking as well as biking
                }

                newActivityList.Add(new_activity);
            }

            return newActivityList;
        }

        private List<double> GetEle_from_List(List<TrackPoint> TrackPoint_List)
        {
            List<double> dummy_List = new List<double>();

            for (int i = 0; i < TrackPoint_List.Count; i++)
            {
                dummy_List.Add((double)TrackPoint_List[i].ele);
            }

            return dummy_List;
        }

        private double[] calc_Elevation(List<double> data_list)
        {
            List<double> _data_list = SavitzkyGolayFilter(data_list);
            double[] dummy_aray = { 0, 0 };

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
        
    }
}
