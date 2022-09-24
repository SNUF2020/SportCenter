using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC
{
    public class Data
    {
        public List<Track4Selection> track { get; set; }
        public List<int> sel_tracks { get; set; }

        public Data()
        {
            this.track = new List<Track4Selection>();
            this.sel_tracks = new List<int>();
        }
    }

    public class Track4Selection
    { // Müssen das properties sein??
        public DateTime ActivityTime { get; set; }
        public double Duration { get; set; }
        public double Distance { get; set; }
        public string CatName { get; set; }
        public bool Check { get; set; } = true;
    }

    class athlet
    {
        public string AthleteID { get; set; }
        public string AthleteName { get; set; }
        public DateTime AthleteDoB { get; set; }
        public int? AthleteHeightcm { get; set; }

        public List<activity> Activities { get; set; }
        public List<fitness> History { get; set; }

        public athlet()
        {
            this.Activities = new List<activity>();
            this.History = new List<fitness>();
        }
    }

    public class activity
    {
        public string ActivityID { get; set; }
        public DateTime ActivityTime { get; set; }
        public string MetaSource { get; set; }
        public DateTime MetaCreated { get; set; }
        public DateTime MetaModified { get; set; }

        // -----------------------------------------------------------------------------------------------

        public double Duration { get; set; }
        public double Distance { get; set; }
        public double total_Descent { get; set; }
        public double total_Ascent { get; set; }

        // ------------------------------------------------------------------------------------------------

        public double mean_HR { get; set; }
        public double max_HR { get; set; }
        public double Cadence_mean { get; set; }
        public double Cadence_max { get; set; }
        public double Power_avrg { get; set; }
        public double Power_max { get; set; }
        public double Calories_total { get; set; }
        public double max_Speed { get; set; }

        // ------------------------------------------------------------------------------------------------

        public string Info_Note { get; set; }
        public string Info_Name { get; set; }

        // ------------------------------------------------------------------------------------------------

        public string Weather_Cond { get; set; }
        public int Weather_Temp { get; set; }
        public string Weather_Note { get; set; }
        public string CatID { get; set; }
        public string CatName { get; set; }
        public string RouteID { get; set; }
        public string RouteName { get; set; }
        public string LocationID { get; set; }
        public string LocationName { get; set; }
        public string Equipment_ID { get; set; }
        public string Equipment_Name { get; set; }

        // ------------------------------------------------------------------------------------------------

        public List<LapData> Laps { get; set; }
        public List<TrackPoint> Track { get; set; }
        public List<Marker> Markers { get; set; }
        public List<Pause> Pausen { get; set; }
        public TimeSpan Pause_total { get; set; }

        public activity()
        {
            initialize_activity();
        }

        private void initialize_activity() // optimized for Garmin FR305 upload
        {
            this.Laps = new List<LapData>();
            this.Track = new List<TrackPoint>();
            this.Pausen = new List<Pause>();
            this.Markers = new List<Marker>();

            ActivityID = Guid.NewGuid().ToString(); // Create the value of a GUID (Globally Unique Identifier, globaler eindeutiger Bezeichner)
            ActivityTime = DateTime.Now;

            MetaCreated = DateTime.Now;
            MetaModified = DateTime.Now;
            
            CatID = "";
            CatName = "";

            Weather_Cond = "";

            Info_Note = "";

            Pause_total = TimeSpan.Zero;
        }
    }

    public class Track4Map
    {
        public DateTime ActivityTime { get; set; }
        public double Duration { get; set; }
        public double Distance { get; set; }
        public double total_Descent { get; set; }
        public double total_Ascent { get; set; }
        public string CatName { get; set; }
        public string LocName { get; set; }
        public List<TrackPoint> Track { get; set; }

        public Track4Map(activity _activity)
        {
            this.ActivityTime = _activity.ActivityTime;
            this.CatName = _activity.CatName;
            this.LocName = _activity.LocationName;
            this.Distance = _activity.Distance;
            this.Duration = _activity.Duration;
            this.total_Ascent = _activity.total_Ascent;
            this.total_Descent = _activity.total_Descent;
            this.Track = _activity.Track;
        }
    }

    public static class activityNamespaces
    {
        public const string Activity_NAMESPACE = "";
        public const string XMLSCHEMA_INSTANCE = "http://www.w3.org/2001/XMLSchema-instance";
        public const string XMLSCHEMA = "http://www.w3.org/2001/XMLSchema";
    }

    class fitness
    {
        //public string AthleteWeight { get; set; }
        public DateTime Date { get; set; }
        public string WeightKilograms { get; set; }
        public string BodyFatPercentage { get; set; }
        public string RestingHeartRateBPM { get; set; }
        public string MaximumHeartRateBPM { get; set; }
        public string SystolicBloodPressure { get; set; }
        public string DiastolicBloodPressure { get; set; }
        public string CaloriesConsumed { get; set; }
        public string Mood { get; set; }
        public string SleepHours { get; set; }
        public string SleepQuality { get; set; }
        public string Injured { get; set; }
        public string InjuredText { get; set; }
        public string Sick { get; set; }
        public string SickText { get; set; }
        public string MissedWorkout { get; set; }
        public string MissedWorkoutText { get; set; }
        public string Notes { get; set; }
    }

    public class configFile
    {
        public string ApiKey_Landscape { get; set; }
        public string ApiKey_Satelite { get; set; }
        public string ApiKey_Hybride { get; set; }
        public string ApiKey_Weather { get; set; }
        public string WorkDir { get; set; }
        public string RoutesDir { get; set; }
        public string SRTMDir { get; set; }
        public string StartPoint_Lon { get; set; } 
        public string StartPoint_Lat { get; set; }
        public string StartPoint_Zoom { get; set; }
        public List<string> Categories { get; set; }
        public List<string> Weather { get; set; }

    }
}
