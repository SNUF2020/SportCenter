using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace SC
{
    static class DatabaseHelpers
    {

        public static void calc_PowerCalories_Hiking(activity act)
        {
            // Grundstategie: Berechne zunächst die Arbeit in kcal: W_ges = W_hub + W_v
            // Daraus lässt sich dan einfach die mittlere P_avg berechnen
            // 
            // W_hub = P_Hub / Wirkungsgrad_Mensch * Umrechenfaktor Wh/kcal
            // W_v = P_v * t [in h] / Wirkungsgrad_Mensch * Umrechenfaktor Wh/kcal
            //
            // https://www.leifiphysik.de/mechanik/arbeit-energie-und-leistung/aufgabe/wirkungsgrad-beim-radfahren
            // Wirkungsgrad_Mensch = 25% (nur 25% der Umgesetzten Energie geht in die Leistung)
            // Umrechnungsfaktor kcal = 0,8598
            //
            // P_Hub = m * g * Höhenmeter_total / zeit_aufstieg
            //
            // m = 90 (zunächst statisch, kann erweitert werden wenn Sportler-Tagebuch mit Tagesgewicht vorliegt
            // g = ca. 10 und mittlere Steigleistung 360 hm/h -> Zeit_anstieg = Höhenmeter_total / 360
            // Somit ergibt sich eine sehr einfache Formel:
            // P_Hub = 90 * (Meters_up / 360)
            // 
            // P_v = F * v
            //
            // F: Typisches Verhältnis Normal- zu Quer-Kraftkomponente: 10:1 - Für die Normalkraft kann die Gewichtskraft angenommen werden -> m * g * 10% = KG 
            // siehe dazu zB. http://www.thomas-wilhelm.net/veroeffentlichung/Gehen.pdf
            // F = 10% von (m*g) / 1,31 (so rechnet ST) -> ergibt im Flachen sehr gute Übereinstimmung mit Werten aus Internet
            // (z.B. https://www.fitforfun.de/sport/outdoor/trendsport-wandern_aid_4845.html)
            // ST-faktor 1,31 kann nicht direkt nachvollzogen werden - erscheint als empirisch abgeleiteter Faktor
            // P_v = m / 1,31 * v [in m/s]
            // W_v = m / 1,31 * Dis[m] / Dur [sec] * Dur [h] / Wirkungsgrad_Mensch * Umrechenfaktor Wh/kcal
            // Somit W_v = m / 1,31 * Dis[m] / 3600 / Wirkungsgrad_Mensch * Umrechenfaktor Wh/kcal

            double W_hub = 90 * act.total_Ascent / 360 * 0.8598 * 4;
            double W_v = 90 / 1.31 * act.Distance / 3600 * 0.8598 * 4;

            act.Calories_total = Math.Round(W_hub + W_v);
            act.Power_avrg = act.Calories_total / 4 / 0.8598 / act.Duration * 3600;
            act.MetaModified = DateTime.Now;
        }

        public static void calc_Calories(activity act)
        {
            // Umrechnung Leistung in Kalorien
            // W = P * t [in h] / Wirkungsgrad_Mensch * Umrechenfaktor Wh/kcal
            // Wirkungsgrad_Mensch = 25% (nur 25% der Umgesetzten Energie geht in die Leistung)
            // Umrechnungsfaktor kcal = 0,8598

            act.Calories_total = Math.Round(act.Power_avrg * 0.8598 * 4 * act.Duration / 3600);
            act.MetaModified = DateTime.Now;
        }

        public static void calc_CaloriesPower_Swimming(activity act)
        {
            // Zu Schwimmen habe ich keine physikalische Beschreibung gefunden (Starke Abhängigkeit von Schwimmstil zu erwarten)
            // Grundsätzliche Überlegung: P_ges = P_Hub (der Arme und Beine) + P_Wasserwiderstand
            // Zu beachten: Wasserwiderstand und Wasserauftrieb -> Hub muss unterteilt werden in Hub_Wasser und Hub_Luft
            // 
            // Da kein wesentlicher Bestandteil meiner Sport-Datenbank nehme ich die männlichen Werte aus Tabelle
            // https://www.stuttgarter-nachrichten.de/inhalt.kalorienverbrauch-schwimmen-mhsd.589a22bd-00c7-44d1-8628-f83f9da14c4e.html
            // 60kg -> 666 kcal pro Stunde
            // 100kg -> 896 kcal pro Stunde
            // und paramteriesiere diese:
            // W = 666 kcal + (m[kg] - 60) * 5,75 kcal

            act.Calories_total = Math.Round(((666 + (90 - 60) * 5.75)) * act.Duration / 3600);
            act.Power_avrg = Math.Round(act.Calories_total / act.Duration * 3600 / 0.8598 / 4);
            act.MetaModified = DateTime.Now;
        }

        // Berechnung der Leistung beim Radfahren:
        
        // Grundstategie: Berechne die Leistung: P_ges = P_hub + P_Roll + P_Luft
        // https://www.michael-konczer.com/de/training/rechner/rennrad-leistung-berechnen

        // Daraus lässt sich dan einfach die mittlere Arbeit W_avg berechnen: W_ges = P_ges / Wirkungsgrad_Mensch * Umrechenfaktor Wh/kcal
        // Wirkungsgrad_Mensch = 25% (nur 25% der Umgesetzten Energie geht in die Leistung)
        // Umrechnungsfaktor kcal = 0,8598

        // Rollwiderstand -> Reibugsfaktor
        // https://www.leifiphysik.de/mechanik/reibung-und-fortbewegung/ausblick/reibungskraefte-beim-fahrradfahren
        // Slik-Reifen mit >5bar: 0,003 bis 0,004
        // Einfluss Untergrund: Asphalt zu festem Sand = Faktor 4 höherer Reibungsfaktor
        // https://de.wikipedia.org/wiki/Rollwiderstand
        // PKW-Reifen auf Asphalt: 0,011 bis 0,015 -> Auf Schotter: 0,02
        // http://www.wolfgang-menn.de/motion_d.htm
        // Rennrad: 0,003 bis 0,006

        // Luftwiderstand -> C_w  * A Faktor:
        // http://www.wolfgang-menn.de/motion_d.htm
        // Für Bremsschalter-Haltung, grob gemittelt (= meine Rennradhaltung): 0,3
        // Für OberLenkerPosition (= Aufrechte Sitzhaltung, also auch MTB): 0,35 

        // Rennrad: m_Rad = 9kg / µ_R = 0,006 / c_W = 0,3 (Werte abgeleitet von Ergometer mit Rennrad-Einstellung)
        //
        // CycloCross: m_Rad = 9kg / µ_R = 0,009 / c_W = 0,3 (Roll-Widerstand zwischen MTB und Race)
        //
        // MTB: m_Rad = 12kg / µ_R = 0,018 / c_w * A = 0,35 (Werte abgeleitet von Ergometer mit MTB-Einstellung)

        // P_Hub = m * g * Höhenmeter_delta (nur positiv!) / 1sec (ist durch das Zeitraster bereits vorgegeben)
        // Für alle Bike-Kategorien gleich 

        // P_Luft = 0,5 * c_w * A * Rho * v^3
        //        = 0,5 * 0,3     * 1,2 * v^3 (für RaceBike, ErgoRace, CrossBike)
        //        = 0,5 * 0,35     * 1,2 * v^3 (für MTB, ErgoMTB)

        // P_Roll = m * g    * µ_R    * v
        //        = m * 9,81 * 0,006 * v (für RaceBike, ErgoRace)
        //        = m * 9,81 * 0,009  * v (für CrossBike)
        //        = m * 9,81 * 0,018  * v (für MTB, ErgoMTB)

        // Allgemein:
        // Wirkungsgrad Kraftübertragung Kette: 98%
        // m = 90 (zunächst statisch, kann erweitert werden wenn Sportler-Tagebuch mit Tagesgewicht vorliegt
        // g = 9,81

        public static void calc_Power_RaceBike(activity act)
        {

            double _power_up = 90 * 9.81 * act.total_Ascent / act.Duration;
            double _power_v = 0.5 * 0.3 * 1.2 * Math.Pow(act.Distance / act.Duration, 3);
            double _power_roll = 90 * 9.81 * 0.006 * act.Distance / act.Duration;

            act.Power_avrg = (_power_up + _power_v + _power_roll) / 0.98;

            calc_Calories(act);
        }

        public static void calc_Power_CrossBike(activity _act)
        {
            double _power_up = 90 * 9.81 * _act.total_Ascent / _act.Duration;
            double _power_v = 0.5 * 0.3 * 1.2 * Math.Pow(_act.Distance / _act.Duration, 3);
            double _power_roll = 90 * 9.81 * 0.009 * _act.Distance / _act.Duration;

            _act.Power_avrg = (_power_up + _power_v + _power_roll) / 0.98;

            calc_Calories(_act);
        }

        public static void calc_Power_MTBBike(activity _act) // also for ErgoMTB
        {
            double _power_up = 90 * 9.81 * _act.total_Ascent / _act.Duration;
            double _power_v = 0.5 * 0.35 * 1.2 * Math.Pow(_act.Distance / _act.Duration, 3);
            double _power_roll = 90 * 9.81 * 0.018 * _act.Distance / _act.Duration;

            _act.Power_avrg = (_power_up + _power_v + _power_roll) / 0.98;

            calc_Calories(_act);
        }

        public static string Get_Location(List<athlet> _SC_DataBase, int _active_athlet, double _lat, double _lon)
        {
            string _location = "";

            for (int i = 0; i < _SC_DataBase[_active_athlet].Activities.Count; i++)
            {
                if (_SC_DataBase[_active_athlet].Activities[i].Track.Any())
                {
                    if (GetDistanceBetweenTwoPoints((double)_SC_DataBase[_active_athlet].Activities[i].Track[0].lat,
                    (double)_SC_DataBase[_active_athlet].Activities[i].Track[0].lon, _lat, _lon) < 1000)
                        _location = _SC_DataBase[_active_athlet].Activities[i].LocationName;
                }  
            }

            return _location;
        }

        /// <summary>
        /// Gets the distance between two coordinates in meters.
        /// </summary>
        /// <param name="latitude1">The latitude1.</param>
        /// <param name="longitude1">The longitude1.</param>
        /// <param name="latitude2">The latitude2.</param>
        /// <param name="longitude2">The longitude2.</param>
        /// <returns></returns>
        
        public static double GetDistanceBetweenTwoPoints(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            if (latitude1 != latitude2 || longitude1 != longitude2)
            {
                double theta = longitude1 - longitude2;
                double distance = Math.Sin(ConvertDecimalDegreesToRadians(latitude1)) *
                                  Math.Sin(ConvertDecimalDegreesToRadians(latitude2)) +
                                  Math.Cos(ConvertDecimalDegreesToRadians(latitude1)) *
                                  Math.Cos(ConvertDecimalDegreesToRadians(latitude2)) *
                                  Math.Cos(ConvertDecimalDegreesToRadians(theta));

                distance = Math.Acos(distance);
                distance = ConvertRadiansToDecimalDegrees(distance);
                distance = distance * 60 * 1.1515;
                // convert to meters
                return (distance * 1.609344 * 1000);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Converts the decimal degrees to radians.
        /// </summary>
        /// <param name="degree">The degree.</param>
        /// <returns></returns>
        private static double ConvertDecimalDegreesToRadians(double degree)
        {
            return (degree * Math.PI / 180.0);
        }

        /// <summary>
        /// Converts the radians to decimal degrees.
        /// </summary>
        /// <param name="radian">The radian.</param>
        /// <returns></returns>
        private static double ConvertRadiansToDecimalDegrees(double radian)
        {
            return (radian / Math.PI * 180.0);
        }

        // ---------------------------------------------------------------------------------------------
        // zip section

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        public static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
    }
}
