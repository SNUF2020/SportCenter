using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A30x.DataTypes;
using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A906.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SC
{
    public static class FitlogNamespaces
    {
        public const string FITLOG_NAMESPACE = "http://www.zonefivesoftware.com/xmlschemas/FitnessLogbook/v2/fitnesslog2.xsd";
        public const string XMLSCHEMA_INSTANCE = "http://www.w3.org/2001/XMLSchema-instance";
        public const string XMLSCHEMA = "http://www.w3.org/2001/XMLSchema";
    }

    public class FitlogAttributes
    {
        public string Version { get; set; }
        public string Creator { get; set; }
    }

    public class AthleteLogData
    {
        // hier muss die Sache als Liste beschrieben werden: Logik ist dass für jeden Athleten eine Anzahl von Aktivitäten gelistet sein kann
        // Somit kann es mehere Athleten und für diese wiederum meherere Aktivitäten geben...
        public List<AthleteData> athletedata = new List<AthleteData>();
    }

    public class AthleteData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime DoB { get; set; }
        public int? Height { get; set; }
        public double? Weight { get; set; }


        //public List<AthletAttributes> athleteattributes_list = new List<AthletAttributes>();

        public List<ActivityData> activitydata_list = new List<ActivityData>();
    }

    public class ActivityData
    {
        public ActivityAttributes activityattributes = new ActivityAttributes();
        public MetaDataAttributes metadataattributes = new MetaDataAttributes();
        public DurationAttributes durationattributes = new DurationAttributes();
        public DistanceAttributes distanceattributes = new DistanceAttributes();
        public ElevationAttributes elevationattributes = new ElevationAttributes();
        public HeartrateAttributes heartrateattributes = new HeartrateAttributes();
        public CadenceAttributes cadenceattributes = new CadenceAttributes();
        public PowerAttributes powerattributes = new PowerAttributes();
        public InfoAttributes infoattributes = new InfoAttributes();
        public CaloriesAttributes caloriesattributes = new CaloriesAttributes();
        public SpeedAttributes speedattributes = new SpeedAttributes();
        public IntensityAttributes intensityattributes = new IntensityAttributes();
        public WeatherAttributes weatherattributes = new WeatherAttributes();
        public CategoryAttributes categoryattributes = new CategoryAttributes();
        public RouteAttributes routeattributes = new RouteAttributes();
        public LocationAttributes locationattributes = new LocationAttributes();
        public EquipementItemAttributes equipementitemattributes = new EquipementItemAttributes();
        public TrackAttributes trackattributes = new TrackAttributes();
        public List<TrackPoint> trackdate_list = new List<TrackPoint>();
        public List<LapData> lapdata_List = new List<LapData>();
        public List<Pause> pause_list = new List<Pause>();
        public List<Marker> marker_list = new List<Marker>();
    }

    public class ActivityAttributes
    {
        public DateTime StartTime { get; set; }
        public string Id { get; set; }
    }

    public class LapData
    {
        public LapAttributes lapattributes = new LapAttributes();

        public double? TotalMeters { get; set; }
        public double? TotalCal { get; set; }
        public double? HeartRate_avg { get; set; }
        public double? HeartRate_max { get; set; }
        public double? Intensity { get; set; }
        public double? Cadence { get; set; }
        public double? Cadence_max { get; set; }
        public double? Trigger { get; set; }
        public double? Speed_max { get; set; }
        public double? lat_start { get; set; }
        public double? lon_start { get; set; }
        public double? lat_end { get; set; }
        public double? lon_end { get; set; }
        public double? Power { get; set; }
        public double? Power_max { get; set; }
        public double? Elevation { get; set; }

        public LapData ()
        {

        }

        public LapData(D1015_Lap_Type _lap)
        {
            // Analog zu Trackpoint Variante: Zuordnung der Lap-Daten zu LapData...
            // this.lapattributes.StartTime = DateTime.Now; // muss nachbaearbeitet werden
            this.lapattributes.DurationSeconds = _lap.total_time/100; // totaltime = duration of lap in hundreths of seconds
            // newlap.lapattributes.Notes; // -> no Garmin FR 305 field
            // newlap.lapattributes.Rest; // -> no Garmin FR 305 field
            this.TotalMeters = _lap.total_dist;
            // newlap.Elevation; // -> no Garmin FR 305 field
            this.TotalCal = _lap.calories;
            this.HeartRate_avg = _lap.avg_heart_rate;
            this.HeartRate_max = _lap.max_heart_rate;
            this.Intensity = _lap.intensity;
            this.Cadence = _lap.avg_cadence;
            this.Trigger = _lap.trigger_method;
            this.Speed_max = _lap.max_speed * 3.6; // Umrechnung von m/s auf km/s
            this.lat_start = transf_Pos(_lap.begin.lat); 
            this.lon_start = transf_Pos(_lap.begin.lon);
            this.lat_end = transf_Pos(_lap.end.lat);
            this.lon_end = transf_Pos(_lap.end.lon);
            // newlap.Power; // -> no Garmin FR 305 field
        }
        private double transf_Pos(int _Pos)
        {
            return (double)_Pos * 180 / 2147483648;
        }
    }

    public class LapAttributes
    {
        public DateTime StartTime { get; set; }
        public double? DurationSeconds { get; set; }
        public string Notes { get; set; }
        public string Rest { get; set; }
    }

    public class TrackPoint
    {
        public TrackAttributes trackattributes = new TrackAttributes();
        public int? tm { get; set; }
        public double? lat { get; set; }
        public double? lon { get; set; }
        public double? ele { get; set; }
        public double? dist { get; set; }
        public double? hr { get; set; }
        public double? cadence { get; set; }
        public double? power { get; set; }
        public double? speed_device { get; set; } // Warum string?

        public TrackPoint()
        { }

        public TrackPoint(D304_Trk_Point_Type _tp)
        {
            this.tm = (int)_tp.time;

            if (_tp.posn.lat != Int32.MaxValue)
                this.lat = transf_Pos(_tp.posn.lat);
            else this.lat = null;

            if (_tp.posn.lon != Int32.MaxValue) this.lon = transf_Pos(_tp.posn.lon);
            //else this.lon = null;
            
            if (_tp.alt != 1E+25) this.ele = _tp.alt;
            else this.ele = null;
            
            if (_tp.distance != 1E+25) this.dist = _tp.distance;
            else this.dist = null;
            
            if (_tp.heart_rate != 0) this.hr = _tp.heart_rate;
            else this.hr = null;
            
            if (_tp.cadence != 255) this.cadence = _tp.cadence;
            else this.cadence = null;
            
            // sensor not transfered -> bool information if a wheel sensor is present, well...
        }
        private double transf_Pos(int _Pos)
        {
            return (double)_Pos * 180 / 2147483648; // 2^31
        }
    }
    public class Marker
    {
        public double? dist { get; set; }
    }

    public class Pause
    {
        public DateTime EndTime { get; set; }
        public DateTime StartTime { get; set; }
    }

    public class MetaDataAttributes
    {
        public string Source { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }

    }
    public class DurationAttributes
    {
        public double? TotalSeconds { get; set; }

    }
    public class DistanceAttributes
    {
        public double? TotalMeters { get; set; }

    }
    public class ElevationAttributes
    {
        public double? DescendMeters { get; set; }
        public double? AscendMeters { get; set; }
    }
    public class HeartrateAttributes
    {
        public double? AverageBPM { get; set; }
        public double? MaximumBPM { get; set; }
    }
    public class CadenceAttributes
    {
        public double? AverageRPM { get; set; }
        public double? MaximumRPM { get; set; }
    }
    public class PowerAttributes
    {
        public double? AverageWatts { get; set; }
        public double? MaximumWatts { get; set; }
    }

    public class CaloriesAttributes
    {
        public double? TotalCal { get; set; }
    }

    public class SpeedAttributes
    {
        public double? MaxSpeed { get; set; }
    }

    public class InfoAttributes
    {
        public string Notes { get; set; }
        public string Name { get; set; }
    }

    public class IntensityAttributes
    {
        public string Value { get; set; }
    }
    public class WeatherAttributes
    {
        public string Conditions { get; set; }
        public double? Temp { get; set; }
        public string Notes { get; set; }
    }
    public class CategoryAttributes
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class LocationAttributes
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class RouteAttributes
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class EquipementItemAttributes
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class TrackAttributes
    {
        public DateTime StartTime { get; set; }
    }

    public class HistoryAttributes
    {
        public DateTime Date { get; set; }
        public double WeightKilograms { get; set; }
        public double BodyFatPercentage { get; set; }
        public double RestingHeartRateBPM { get; set; }
        public double MaximumHeartRateBPM { get; set; }
        public double SystolicBloodPressure { get; set; }
        public double DiastolicBloodPressure { get; set; }
        public double CaloriesConsumed { get; set; }
        public string Mood { get; set; }
        public double SleepHours { get; set; }
        public string SleepQuality { get; set; }
        public string Injured { get; set; }
        public string InjuredText { get; set; }
        public string Sick { get; set; }
        public string SickText { get; set; }
        public string MissedWorkout { get; set; }
        public string MissedWorkoutText { get; set; }
        public string Notes { get; set; }
    }
}
