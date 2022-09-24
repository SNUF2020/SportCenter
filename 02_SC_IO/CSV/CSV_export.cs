using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SC
{
    class CSV_export : IDisposable
    {
        private List<athlet> _SC_DataBase;
        private int _active_athlet;
        private int _active_activity;

        public CSV_export (List<athlet> database, int active_athlet, int active_activity)
        {
            _SC_DataBase = database;
            _active_athlet = active_athlet;
            _active_activity = active_activity;
        }

        public void Dispose()
        {
        }

        public void CSV_Export(string _selection)
        {
            string selection = _selection;


            // Displays a SaveFileDialog so the user can save the Data table
            using (SaveFileDialog sfDlg = new SaveFileDialog())
            {
                sfDlg.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                if (sfDlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // generate instance if stringbuilder for later writing of csv.file
                        StringBuilder sbOutput = new StringBuilder();

                        switch (selection)
                        {
                            case "summary":

                                int[] _act = get_activeActivity_List(_SC_DataBase[_active_athlet].Activities[_active_activity].ActivityTime.Date, DateTime.Now.Date);

                                // later: define number of datasets being saved
                                int Index = _act.GetLength(0);

                                // generate string w/ length = number of DataSets 
                                string[][] inaOutput = new string[Index][];

                                for (int i = 0; i < Index; i++) // number of DataSets
                                {
                                    inaOutput[i] = new string[15]; // 1 DataSet = 15 fields
                                    inaOutput[i][0] = _SC_DataBase[_active_athlet].Activities[_act[i]].ActivityTime.ToString("d", CultureInfo.GetCultureInfo("de-DE"));
                                    inaOutput[i][1] = _SC_DataBase[_active_athlet].Activities[_act[i]].ActivityTime.ToString("HH:mm");
                                    inaOutput[i][2] = Convert.ToString(_SC_DataBase[_active_athlet].Activities[_act[i]].CatName);
                                    inaOutput[i][3] = Convert.ToString(_SC_DataBase[_active_athlet].Activities[_act[i]].LocationName);
                                    inaOutput[i][4] = (_SC_DataBase[_active_athlet].Activities[_act[i]].Distance / 1000).ToString("#0.00"); //km
                                    inaOutput[i][5] = (_SC_DataBase[_active_athlet].Activities[_act[i]].total_Ascent).ToString("#0.0");  //
                                    inaOutput[i][6] = (_SC_DataBase[_active_athlet].Activities[_act[i]].total_Descent).ToString("#0.0");
                                    TimeSpan t = new TimeSpan(0, 0, (int)_SC_DataBase[_active_athlet].Activities[_act[i]].Duration);
                                    inaOutput[i][7] = t.ToString();
                                    inaOutput[i][8] = (_SC_DataBase[_active_athlet].Activities[_act[i]].Distance / _SC_DataBase[_active_athlet].Activities[_act[i]].Duration * 3.6).ToString("#0.0");
                                    inaOutput[i][9] = Convert.ToString(_SC_DataBase[_active_athlet].Activities[_act[i]].Calories_total);
                                    inaOutput[i][10] = (_SC_DataBase[_active_athlet].Activities[_act[i]].mean_HR).ToString("#0.0");
                                    inaOutput[i][11] = (_SC_DataBase[_active_athlet].Activities[_act[i]].Cadence_mean).ToString("#0.0");
                                    inaOutput[i][12] = (_SC_DataBase[_active_athlet].Activities[_act[i]].Power_avrg).ToString("#0.0");
                                    inaOutput[i][13] = Convert.ToString(_SC_DataBase[_active_athlet].Activities[_act[i]].Weather_Cond);
                                    inaOutput[i][14] = (_SC_DataBase[_active_athlet].Activities[_act[i]].Weather_Temp).ToString("#0.0");
                                    //inaOutput[i][15] = Convert.ToString(_SC_DataBase[_active_athlet].Activities[_act[i]].Weather_Note);
                                }
                                sbOutput.AppendLine("Date;Time;Cat;Location;Distance;Ascent;Descent;Duration;Speed;Calories;HR_avg;Cadence;Pow_avg;Weather;Temp");
                                for (int i = 0; i < inaOutput.GetLength(0); i++)
                                    sbOutput.AppendLine(string.Join(";", inaOutput[i]));

                                break;

                            case "track":

                                int _Index = _SC_DataBase[_active_athlet].Activities[_active_activity].Track.Count;

                                // generate string w/ length = number of DataSets 
                                string[][] _inaOutput = new string[_Index][];

                                for (int i = 0; i < _Index; i++) // number of DataSets
                                {
                                    _inaOutput[i] = new string[9]; // 1 DataSet = 9 fields

                                    _inaOutput[i][0] = _SC_DataBase[_active_athlet].Activities[_active_activity].Track[i].tm.ToString();
                                    _inaOutput[i][1] = _SC_DataBase[_active_athlet].Activities[_active_activity].Track[i].lat.ToString();
                                    _inaOutput[i][2] = _SC_DataBase[_active_athlet].Activities[_active_activity].Track[i].lon.ToString();
                                    _inaOutput[i][3] = _SC_DataBase[_active_athlet].Activities[_active_activity].Track[i].ele.ToString();
                                    _inaOutput[i][4] = _SC_DataBase[_active_athlet].Activities[_active_activity].Track[i].dist.ToString();
                                    _inaOutput[i][5] = _SC_DataBase[_active_athlet].Activities[_active_activity].Track[i].hr.ToString();
                                    _inaOutput[i][6] = _SC_DataBase[_active_athlet].Activities[_active_activity].Track[i].cadence.ToString();
                                    _inaOutput[i][7] = _SC_DataBase[_active_athlet].Activities[_active_activity].Track[i].power.ToString();
                                    _inaOutput[i][8] = _SC_DataBase[_active_athlet].Activities[_active_activity].Track[i].speed_device.ToString();
                                }

                                sbOutput.AppendLine("Time;Lat;Lon;Ele;Dist;Hr;Cad;Pow;Speed");
                                for (int i = 0; i < _inaOutput.GetLength(0); i++)
                                    sbOutput.AppendLine(string.Join(";", _inaOutput[i]));

                                break;
                        }

                        // Create and write the csv file
                        File.WriteAllText(sfDlg.FileName, sbOutput.ToString(), System.Text.Encoding.Default);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        } // Safe Data in csv-file

        private int[] get_activeActivity_List(DateTime _date, DateTime _dateEnd)
        {
            List<int> _activeActivity_List = new List<int>();
            DateTime dateEnd = _dateEnd.AddDays(1.0);

            for (DateTime date = _date; date < dateEnd; date = date.AddDays(1.0))
            {
                for (int act = 0; act < _SC_DataBase[_active_athlet].Activities.Count; act++)
                {
                    if (_SC_DataBase[_active_athlet].Activities[act].ActivityTime.Date == date.Date) _activeActivity_List.Add(act);
                }
            }

            return _activeActivity_List.ToArray();
        }

    }
}
