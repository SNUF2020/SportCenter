using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC
{
    class AnalysisHelpers
    {
        public static DateTime GetFirstDayOfWeek(DateTime dateTime)
        {
            while (dateTime.DayOfWeek != DayOfWeek.Monday)
                dateTime = dateTime.Subtract(new TimeSpan(1, 0, 0, 0));
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
        } // snipplet from Jan Welker https://dotnet-snippets.de/snippet/ersten-und-letzten-tag-der-woche-berechnen/617

        // Kalenderwoche für die Überschrift des Panel_week berechenen...
        // https://mycsharp.de/forum/threads/83188/datetime-nummer-der-kalenderwoche-ermitteln

        public static DateTime GetFirstDayOfMonth(DateTime givenDate)
        {
            return new DateTime(givenDate.Year, givenDate.Month, 1);
        } // snipplet from Jan Welker https://dotnet-snippets.de/snippet/ersten-und-letzten-tag-im-monat-berechnen/616

        public static DateTime GetTheLastDayOfMonth(DateTime givenDate)
        {
            return GetFirstDayOfMonth(givenDate).AddMonths(1).Subtract(new TimeSpan(1, 0, 0, 0, 0));
        } // snipplet from Jan Welker https://dotnet-snippets.de/snippet/ersten-und-letzten-tag-im-monat-berechnen/616

        public static IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        } // sniplet from mqp -> see https://stackoverflow.com/questions/1847580/how-do-i-loop-through-a-date-range

        public static string Time2DurationStrin(double _duration)
        {
            TimeSpan t = TimeSpan.FromSeconds(_duration);
            return string.Format("{00:D2}h:{1:D2}", t.Hours, t.Minutes);
        }

    }
}
