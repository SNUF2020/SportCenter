using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace SC
{
    // code based on solution #3 https://trainyourprogrammer.de/csharp-78-wetterdaten-abfragen-und-anzeigen.html from KawaiiShox 
    // API key from https://home.openweathermap.org/api_keys

    class GetWeather
    {        
        public static WeatherObject GetWetterByCoordTime(double _Lat, double _Lon, DateTime _Time, string _ApiKeyWeather)
        {
            try
            {
                Int32 unixTimestamp1 = (int)_Time.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                StringBuilder sb = new StringBuilder();
                sb.Append("https://api.openweathermap.org/data/3.0/onecall/timemachine?lat=" + _Lat.ToString() + "&lon=" + _Lon.ToString() + "&dt=");
                sb.Append(unixTimestamp1.ToString() + "&APPID=");
                sb.Append(_ApiKeyWeather);
                WebRequest request = WebRequest.Create(sb.ToString());
                request.Credentials = CredentialCache.DefaultCredentials;
                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                response.Close();
                return new JavaScriptSerializer().Deserialize<WeatherObject>(responseFromServer);
            }
            catch
            {
                MessageBox.Show("Error at remote-server (getting weather data)");
                return null;
            }
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }

    public class WeatherObject
    {
        public double lat { get; set; }
        public double lon { get; set; }
        public string timezone { get; set; }
        public List<WData> data { get; set; }
    }

    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }

    public class WData
    {
        public int dt { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
        public double temp { get; set; }
        public double feels_like { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
        public double dew_point { get; set; }
        public int clouds { get; set; }
        public int visibility { get; set; }
        public double wind_speed { get; set; }
        public int wind_deg { get; set; }
        public List<Weather> weather { get; set; }
    }
}
