using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC
{
    public class Tcx
    {
        public static class TcxNamespaces
        {
            public const string tcx_NAMESPACE = "https://www8.garmin.com/xmlschemas/TrainingCenterDatabasev2.xsd";
            public const string tcx_ACTIVITYExtensionSCHEMA = "https://www8.garmin.com/xmlschemas/ActivityExtensionv2.xsd";
            public const string XMLSCHEMA_INSTANCE = "http://www.w3.org/2001/XMLSchema-instance";
            public const string XMLSCHEMA = "http://www.w3.org/2001/XMLSchema";
        }

        public class TcxAttributes
        {
            public string Version { get; set; }
            public string Creator { get; set; }
        }

        public class TcxActivities
        {
            public TcxActivityData _activityData = new TcxActivityData();
            public MultiSport _multisport = new MultiSport();
        }

        public class TcxActivityData
        {
            public ActivityAttributes activityattributes = new ActivityAttributes();
            public string Id { get; set; }
            public TcxLapData lapdata = new TcxLapData();
            public CreatorData creator = new CreatorData();
        }

        public class MultiSport
        {
            // wird nicht weiter ausgearbeitet, da von TACX nicht verwendet
        }

        public class ActivityAttributes
        {
            public string Sport { get; set; }
        }

        public class TcxLapData
        {
            public LapAttributes lapattributes = new LapAttributes();

            public string TotalTimeSeconds { get; set; }
            public string DistanceMeters { get; set; }
            public string MaximumSpeed { get; set; }
            public string Calories { get; set; }
            public string AverageHeartRateBpm { get; set; }
            public string MaximumHeartRateBpm { get; set; }
            public string Intensity { get; set; }
            public string Cadence { get; set; }
            public string TriggerMethod { get; set; }

            public List<TcxTrackPoint> Track = new List<TcxTrackPoint>();
            public string Notes { get; set; }
            public string Extensions { get; set; }
        }

        public class LapAttributes
        {
            public string StartTime { get; set; }
        }

        public class TcxTrackPoint
        {
            public string Time { get; set; }
            public PositionData Position = new PositionData();
            public string AltitudeMeters { get; set; }
            public string DistanceMeters { get; set; }
            public string HeartRateBpm { get; set; }
            public string Cadence { get; set; } // Bike Cadence
            public string SensorState { get; set; }
            public TrackExtensionData Extensions = new TrackExtensionData();
        }

        public class PositionData
        {
            public string LatitudeDegrees { get; set; }
            public string LongitudeDegrees { get; set; }
        }

        public class TrackExtensionData
        {
            public string Speed { get; set; }
            public string RunCadence { get; set; }
            public string Watts { get; set; }
        }

        public class CreatorData
        {
            // public CreatorAttributes lapattributes = new CreatorAttributes(); // nicht umgesetzt - keine für mich relevanten Daten

            public string Name { get; set; }
            public string UnitId { get; set; }
            public string ProductID { get; set; }
            // public List<VersionData> Version { get; set; } // nicht umgesetzt - keine für mich relevanten Daten
        }

    }
}
