using System;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.LinkProtocol.Enumerations
{
    public enum L001_packet_id
    {
        Pid_Command_Data = 10,
        Pid_Xfer_Cmplt = 12,
        Pid_Date_Time_Data = 14,
        Pid_Position_Data = 17,
        Pid_Prx_Wpt_Data = 19,
        Pid_Records = 27,
        Pid_Rte_Hdr = 29,
        Pid_Rte_Wpt_Data = 30,
        Pid_Almanac_Data = 31,
        Pid_Trk_Data = 34,
        Pid_Wpt_Data = 35,
        Pid_Pvt_Data = 51,
        Pid_Rte_Link_Data = 98,
        Pid_Trk_Hdr = 99,
        Pid_FlightBook_Record = 134,
        Pid_Lap = 149,
        Pid_Wpt_Cat = 152,
        Pid_Run = 990,
        Pid_Workout = 991,
        Pid_Workout_Occurrence = 992,
        Pid_Fitness_User_Profile = 993,
        Pid_Workout_Limits = 994,
        Pid_Course = 1061,
        Pid_Course_Lap = 1062,
        Pid_Course_Point = 1063,
        Pid_Course_Trk_Hdr = 1064,
        Pid_Course_Trk_Data = 1065,
        Pid_Course_Limits = 1066
    }
}
