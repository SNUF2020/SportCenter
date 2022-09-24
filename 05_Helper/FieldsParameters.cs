using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC
{
    class FieldsParameters
    {
        // -----------------------------------------------------------------------------------------------------
        // General fields
        public static string API_key_Landscape { get; set; } = "";
        public static string API_key_Weather { get; set; } = "";

        public static string Tracks_Dir { get; set; } = "";
        public static string SRTM_Data_Dir { get; set; } = "";

        // initialer startpunkt = BW, Goldregenweg 15 
        public static double Initial_Lon { get; set; } = 9.10477057099342;
        public static double Initial_Lat { get; set; } = 48.7203868478537;
        public static double Initial_Zoom { get; set; } = 305.748113086f;

        // ----------------------------------------------------------------------------------------------------------
        // DB fields
        public static bool database_changed = false; // if yes -> save DB to Db file at end of program

        public static int active_athlet = 0; // Athlet #0 = first athlet in DB
        public static int active_activity; // actual Activity - if <0 = not defined


        // ----------------------------------------------------------------------------------------------------------
        // Map Control fields
        public static bool showStartPoints = true; // plot only starting points
        public static bool showAllTracks = false; // plot only starting points
        public static bool autoamtic_center_track = true;
        public static bool program_start = true;

        // -------------------------------------------------------------------------------------------------------------
        // Chart ele fields
        public static double minX_Value = 1000;
        public static double maxX_Value = 1001;

        public static double maxAlt = -1000;
        public static double minAlt = 10000;

        public static double maxX_Alt;
        public static double minX_Alt;

        public static bool mouseDown = false;

        // -------------------------------------------------------------------------------------------------------------
        // Filter parameters
        public static string Filter_Month;
        public static string Filter_Year;
        public static string Filter_Cat;
        public static string Filter_Loc;

        // -------------------------------------------------------------------------------------------------------------
        // Analysis-Panel Fields
        public static double week_distance;
        public static double week_duration;
        public static double week_calories;
        public static double week_ascent;
        public static double week_descent;

        public static double week_min1_distance;
        public static double week_min1_duration;
        public static double week_min1_calories;
        public static double week_min1_ascent;
        public static double week_min1_descent;

        public static double month_distance;
        public static double month_duration;
        public static double month_calories;
        public static double month_ascent;
        public static double month_descent;

        public static double month_min1_distance;
        public static double month_min1_duration;
        public static double month_min1_calories;
        public static double month_min1_ascent;
        public static double month_min1_descent;
    }
}
