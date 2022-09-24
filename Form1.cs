using BruTile.Predefined;
using BruTile.Web;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Globalization;
using SC._01_SRTM_Data;
using System.IO;
using System.Drawing;
using BruTile.Cache;
using BruTile;
using SRTM.Sources.USGS;
using System.Windows.Forms.DataVisualization.Charting;

// Code by S.Nuf.2022 (Code under MIT Licence)
// SC = Sport Center, Version1.0
// After shut down of SportTracks I creat my own "SportTracks"
// SC integrates all my sport activities into one data base with geo-graphical interface (using BruTile Mapcontrol)

// STRUCTURE of CODE:
// - Fields (DataBase, MapControl) -> rest in Helper_FieldsParameters
// - General/Form1 (Methods, Events)
// - DataBase (Methods, Events)
// - MapControl (Methods, Events)
// - ControlPanel (Methods, Events)
// - Elevation Chart
// - IO

// GARMIN DATA:
// Based on code (from https://sourceforge.net/projects/opensportsman/) data from USB device (Garmin Forerunner 305) will be fetched (track and lap data)
// Original code from  mauricemarinus under GNU General Public License version 2.0 (GPLv2)

// ELEVATION DATA SOURCE:
// Embedding of elevation data: https://github.com/itinero/srtm 
// Elevation data from http://viewfinderpanoramas.org/dem3.html (90m-resolution, Alpes 30m) and (better): 
//   from https://e4ftl01.cr.usgs.gov/MEASURES/SRTMGL1.003/2000.02.11/ (NASA, world-wide with 30m-resolution)
//   Problem at both servers: Downloading of data at running program does not work -> Since app. 2014 from server-side disabled 
//   -> Data will be downloaded to local disc (at NASA server password required)
//   All elevation data are stored in SRTM_Data at SRTM-folder
// Specific:
//  smartBike (Tacx unit w/ KinoMap software): No SRTM data will be used -> use of smartBike elevation data (use of SRTM routine leads to higher noise)
//  GARMIN (FR305): elevation data from SRTM is standard - if no elevation SRTM-data is avaialable GARMIN data will be used
//  Hiking (GPX files from program Routen Planer Sport, RPS): SRTM will be used (no other option possibel) 

// WEATHER DATA SOURCE:
// Weather history data up to 40 years into the past:
// https://openweathermap.org/api/one-call-3#history
// With API key "One Call API 3.0"

// MAP DATA SOURCE:
//  Map data: Thunderforest (API key necessary) or OSM and Bing (w/o API key)
//
// GENERAL POINTS:
//  Implementation of GpxReader.cs etc.: Source code based on dlg.krakow.pl code (copyright (c) 2011-2016, dlg.krakow.pl) 
//  Doku GPX-Writer/Reader: https://github.com/macias/Gpx/blob/master/Gpx/Implementation/GpxWriter.cs
//
// Implementation of config-file (see class ConfigFileReader)
//
// Database w/ data compression
//   see https://stackoverflow.com/questions/7343465/compression-decompression-string-with-c-sharp -> first solution from https://stackoverflow.com/users/613130/xanatos
//   -> DatabaseHelpers (zip, unzip)
//   ZipReader -> separat XMLReader-class

namespace SC
{
    public partial class Form1 : Form
    {
        // DB Fields
        //
        private List<athlet> SC_DataBase = new List<athlet>(); // complete data base 
        private List<Track4Map> Tracks4Map = new List<Track4Map>(); // all tracks which should be shown in map

        // MapControl Fields
        //
        private Fetcher<Image> _fetcher;
        private Renderer _renderer;
        private readonly MemoryCache<Tile<Image>> _tileCache = new MemoryCache<Tile<Image>>(200, 300);
        private ITileSource _tileSource;
        private Point _previousMousePosition;
        private Viewport _viewport = null;

        private bool _down = false;

        private Bitmap _buffer;
        private Bitmap _route;

        //
        // --------------------------------------------------------------------------------------------------------------
        //
        // Form1: General Constructor
        //
        public Form1()
        {
            InitializeComponent();
            ConfigFile_Load();
            InitializeMapControl();

            // Defined closing event for Form1 
            this.SuspendLayout();
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
        }

        //
        // Form1: General Methods
        //
        private void ConfigFile_Load() 
        {
            try
            {
                using (ConfigFileReader2 reader = new ConfigFileReader2(new FileStream("SC_ConfigFile.txt", FileMode.Open)))
                {
                    FieldsParameters.API_key_Landscape = reader.ConfigContent.ApiKey_Landscape;
                    FieldsParameters.API_key_Weather = reader.ConfigContent.ApiKey_Weather;
                    // 
                    FieldsParameters.Tracks_Dir = reader.ConfigContent.RoutesDir;
                    FieldsParameters.SRTM_Data_Dir = reader.ConfigContent.SRTMDir;
                    //
                    FieldsParameters.Initial_Lon = Convert.ToDouble(reader.ConfigContent.StartPoint_Lon, CultureInfo.InvariantCulture);
                    FieldsParameters.Initial_Lat = Convert.ToDouble(reader.ConfigContent.StartPoint_Lat, CultureInfo.InvariantCulture);
                    FieldsParameters.Initial_Zoom = Convert.ToDouble(reader.ConfigContent.StartPoint_Zoom, CultureInfo.InvariantCulture);
                    //
                    if (reader.ConfigContent.Categories.Any())
                    {
                        foreach (string s in reader.ConfigContent.Categories)
                            comboBox_Cat.Items.Add(s);
                    }
                    //
                    if (reader.ConfigContent.Weather.Any())
                    {
                        foreach (string w in reader.ConfigContent.Weather)
                            comboBox_Weather.Items.Add(w);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " ConfigFile_Load");
            }
        } // Initial load of config file

        private void Form1_Load(object sender, EventArgs e)
        {
            Initial_SC_DB_Load();

            LoadAllTracks4Map();
            
            if (SC_DataBase.Any()) //prevent IndexOutOfRangeException for empty list - code snippet from code.grepper.com
            {
                FieldsParameters.active_activity = SC_DataBase[FieldsParameters.active_athlet].Activities.Count - 1;
                InitializePanel1();
                Update_panel1(FieldsParameters.active_activity);
                InitializeFilter(); // adding all years with activities in the database to the dropdown_combobox
            }
        }

        //
        // --------------------------------------------------------------------------------------------------------------
        //
        // Form1: General Events 
        //
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (FieldsParameters.database_changed)
            {
                DialogResult result = MessageBox.Show("Änderungen im Logbuch speichern?", "Speicherbestätigung Logbuch", 
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

                switch (result)
                {
                    case DialogResult.Yes:
                        Save_SC_DataBase();
                        break;
                    case DialogResult.No:
                        break;
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
        }

        // --------------------------------------------------------------------------------------------------------------
        //
        // DB code
        //
        // DB Methods
        //
        private void Initial_SC_DB_Load() 
        {
            try
            {
                Cursor = Cursors.WaitCursor; // Wait-Cursor will be shown for time of writing process

                using (DatabaseReader reader = new DatabaseReader(new FileStream("SC.DataBase", FileMode.Open)))
                {
                    Fitlog2database writer = new Fitlog2database();

                    while (reader.Read())
                    {
                        switch (reader.ObjectType)
                        {
                            case DatabaseReader.GpxObjectType.AthletLogData:
                                SC_DataBase = writer.WriteAllAthletData2database(reader.AthletLogdata);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " initial_SC_DB_Load");
            }

            finally
            {
                Cursor = Cursors.Default;
            }
        } // Initial load of DB

        private void Delete_activity()
        {
            if (SC_DataBase.Any()) //prevent IndexOutOfRangeException for empty list - code snippet from code.grepper.com
            {
                if (SC_DataBase[FieldsParameters.active_athlet].Activities.Count > 1) SC_DataBase[FieldsParameters.active_athlet].Activities.RemoveAt(FieldsParameters.active_activity);
                if (FieldsParameters.active_activity >= 1 & FieldsParameters.active_activity > SC_DataBase.Count - 1) FieldsParameters.active_activity -= 1;
                
                LoadAllTracks4Map(); // update track list
                UpdateCalendar();
                panel1.Invalidate();
                Center_Track(GetActualTrack(Tracks4Map, FieldsParameters.active_activity));
                Invalidate();
            }
        } // DB can be deleted down to one element

        private void Create_activity()
        {
            activity new_activity = new activity();

            SC_DataBase[FieldsParameters.active_athlet].Activities.Add(new_activity);

            FieldsParameters.database_changed = true;
        }

        private void Save_SC_DataBase()
        {
            if (SC_DataBase.Any()) //prevent IndexOutOfRangeException for empty list - code snippet from code.grepper.com
            {
                try
                {
                    Cursor = Cursors.WaitCursor; // Wait-Cursor will be shown for time of writing process

                    using (DatabaseWriter writer = new DatabaseWriter(new FileStream("SC.DataBase", FileMode.Create), SC_DataBase))
                    {
                        writer.WriteAllAthletData();
                    }
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        } // safe SC data base

        private void LoadAllTracks4Map()
        {
            Tracks4Map.Clear();

            for (int t = 0; t < SC_DataBase[FieldsParameters.active_athlet].Activities.Count; t++)
            {                
                Tracks4Map.Add(new Track4Map(SC_DataBase[FieldsParameters.active_athlet].Activities[t]));
            } // Hand-over to map of all tracks in DB 
            
            //Invalidate();
        }

        private DateTime[] Get_activityDates()
        {
            DateTime[] activityDates = new DateTime[SC_DataBase[FieldsParameters.active_athlet].Activities.Count];

            for (int act = 0; act < SC_DataBase[FieldsParameters.active_athlet].Activities.Count; act++)
            {
                activityDates[act] = SC_DataBase[FieldsParameters.active_athlet].Activities[act].ActivityTime;
            }

            return activityDates;
        }

        private int Get_Activity_byDate_MonthCal(DateTime _date, int activeActivity)
        { 
            int _activeActivity = activeActivity;

            for (int act = 0; act < SC_DataBase[FieldsParameters.active_athlet].Activities.Count; act++)
            {
                if (SC_DataBase[FieldsParameters.active_athlet].Activities[act].ActivityTime.Date == _date.Date) _activeActivity = act;
            }

            return _activeActivity;
        }

        private int Get_Activity_byDate_WeekPanel(DateTime _date, int activeActivity)
        {
            int _activeActivity = activeActivity;

            for (int act = 0; act < SC_DataBase[FieldsParameters.active_athlet].Activities.Count; act++)
            {
                if (SC_DataBase[FieldsParameters.active_athlet].Activities[act].ActivityTime == _date) _activeActivity = act;
            }

            return _activeActivity;
        } // getting specific activity

        private int Get_Activity_byLocation(PointF _loc, int activeActivity)
        {
            int _activeActivity = activeActivity;
            double max_dist = 100;

            for (int act = 0; act < SC_DataBase[FieldsParameters.active_athlet].Activities.Count; act++)
            {
                if (SC_DataBase[FieldsParameters.active_athlet].Activities[act].Track.Any())
                {
                    double dist = TrackHelpers.GetDistanceBetweenTwoPoints(_viewport.YToLatitude(_loc.Y), _viewport.XToLongituide(_loc.X),
                    (double)SC_DataBase[FieldsParameters.active_athlet].Activities[act].Track[0].lat,
                    (double)SC_DataBase[FieldsParameters.active_athlet].Activities[act].Track[0].lon);
                   
                    if (dist < max_dist)
                    {
                        _activeActivity = act;
                        max_dist = dist;
                    }
                }
            }

            return _activeActivity;
        }

        private int Get_youngest_Activity()
        {
            int _activeActivity = 0;
            DateTime _date = SC_DataBase[FieldsParameters.active_athlet].Activities[0].ActivityTime;

            for (int act = 1; act < SC_DataBase[FieldsParameters.active_athlet].Activities.Count; act++)
            {
                if (SC_DataBase[FieldsParameters.active_athlet].Activities[act].ActivityTime > _date)
                {
                    _date = SC_DataBase[FieldsParameters.active_athlet].Activities[act].ActivityTime;
                    _activeActivity = act;
                }
            }

            return _activeActivity;
        }
        private int Get_oldest_Activity()
        {
            int _activeActivity = 0;
            DateTime _date = SC_DataBase[FieldsParameters.active_athlet].Activities[0].ActivityTime;

            for (int act = 1; act < SC_DataBase[FieldsParameters.active_athlet].Activities.Count; act++)
            {
                if (SC_DataBase[FieldsParameters.active_athlet].Activities[act].ActivityTime < _date)
                {
                    _date = SC_DataBase[FieldsParameters.active_athlet].Activities[act].ActivityTime;
                    _activeActivity = act;
                }
            }

            return _activeActivity;
        }

        private int Get_last_added_Activity()
        {
            int _activeActivity = SC_DataBase[FieldsParameters.active_athlet].Activities.Count - 1;

            return _activeActivity;
        }

        private List<List<TrackPoint>> GetActualTrack(List<Track4Map> _tracks, int _activeActivity)
        {
            List<List<TrackPoint>> _track = new List<List<TrackPoint>>();
               
            _track.Add(_tracks[_activeActivity].Track);

            return _track;
        }

        private List<List<TrackPoint>> GetALLTracks(List<Track4Map> _tracks)
        {
            List<List<TrackPoint>> _track = new List<List<TrackPoint>>();

            foreach(Track4Map _t4m in _tracks)
            {
                _track.Add(_t4m.Track);
            }

            return _track;
        }

        //
        //
        // DB Events
        //

        private void NewActivityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Create_activity();
            FieldsParameters.database_changed = true;

        }

        private void DeleteActivityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Delete_activity();
            FieldsParameters.database_changed = true;
        }

        private void DropDown_DataBase_Save_Click(object sender, EventArgs e)
        {
            if (SC_DataBase.Any()) //prevent IndexOutOfRangeException for empty list - code snippet from code.grepper.com
            {
                Save_SC_DataBase();
                FieldsParameters.database_changed = false;
            }
        }

        private void DropDown_Activity_old_Click(object sender, EventArgs e)
        {
            FieldsParameters.active_activity = Get_oldest_Activity();
            Center_Track(GetActualTrack(Tracks4Map, FieldsParameters.active_activity));
            Update_panel1(FieldsParameters.active_activity);
            Invalidate();
        }

        private void DropDown_Activity_young_Click(object sender, EventArgs e)
        {
            FieldsParameters.active_activity = Get_youngest_Activity();
            Center_Track(GetActualTrack(Tracks4Map, FieldsParameters.active_activity));
            Update_panel1(FieldsParameters.active_activity);
            Invalidate();
        }

        private void DropDown_Activity_last_Click(object sender, EventArgs e)
        {
            FieldsParameters.active_activity = Get_last_added_Activity();
            Center_Track(GetActualTrack(Tracks4Map, FieldsParameters.active_activity));
            Update_panel1(FieldsParameters.active_activity);
            Invalidate();
        }

        //
        // --------------------------------------------------------------------------------------------------------------
        //
        // MapControl code
        //
        //
        // MapControl Eventhandler
        //
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (_renderer == null) return;

            if (_viewport == null)
            {
                if (!TryInitializeViewport(ref _viewport, this.Width, this.Height, _tileSource.Schema, 
                    FieldsParameters.Initial_Lon, FieldsParameters.Initial_Lat, FieldsParameters.Initial_Zoom)) return;
                _fetcher.ViewChanged(_viewport.Extent, _viewport.UnitsPerPixel); // start fetching when viewport is first initialized
            }

            _renderer.Render(_viewport, _tileSource, _tileCache, Tracks4Map, FieldsParameters.active_activity);

            e.Graphics.DrawImage(_buffer, 0, 0);
            e.Graphics.DrawImage(_route, 0, 0);
        }

        private void FetcherOnDataChanged(object sender, DataChangedEventArgs<Image> e)
        {
            if (!InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate ()
                {
                    FetcherOnDataChanged(sender, e);

                });
            }
            // code sniplet from https://smehrozalam.wordpress.com/2009/11/24/control-invoke-and-begininvoke-using-lamba-and-anonymous-delegates/
            else
            {
                if (e.Error == null && e.Tile != null)

                {
                    e.Tile.Image = TileToImage(e.Tile.Data);
                    _tileCache.Add(e.Tile.Info.Index, e.Tile);
                    Invalidate();
                }
            }
        }

        private static Image TileToImage(byte[] tile)
        {
            var stream = new MemoryStream(tile);
            var image = Image.FromStream(stream);
            return image;
        }

        //
        // ------------------------------------------------------------------------------------------------------------------------------
        //
        // MapControl Events (Buttons)
        //
        
        private void DropDown_Map_land_Click(object sender, EventArgs e)
        {
            SetTileSource(new HttpTileSource(new GlobalSphericalMercator(0, 19), FieldsParameters.API_key_Landscape, new[] { "a", "b", "c" }, "OSM"),
                _viewport.UnitsPerPixel, _viewport.CenterX, _viewport.CenterY);
            DropDown_Map_hyb.Checked = false;
            DropDown_Map_sat.Checked = false;
            DropDown_Map_land.Checked = true;
            DropDown_MaP_OSM.Checked = false;
        }

        private void DropDown_Map_sat_Click(object sender, EventArgs e)
        {
            SetTileSource(KnownTileSources.Create(KnownTileSource.BingAerial), _viewport.UnitsPerPixel, _viewport.CenterX, _viewport.CenterY);
            DropDown_Map_hyb.Checked = false;
            DropDown_Map_land.Checked = false;
            DropDown_Map_sat.Checked = true;
            DropDown_MaP_OSM.Checked = false;
        }

        private void DropDown_Map_hyb_Click(object sender, EventArgs e)
        {
            SetTileSource(KnownTileSources.Create(KnownTileSource.BingHybrid), _viewport.UnitsPerPixel, _viewport.CenterX, _viewport.CenterY);
            DropDown_Map_sat.Checked = false;
            DropDown_Map_land.Checked = false;
            DropDown_Map_hyb.Checked = true;
            DropDown_MaP_OSM.Checked = false;
        }

        private void DropDown_Map_OSM_Click(object sender, EventArgs e)
        {
            SetTileSource(KnownTileSources.Create(KnownTileSource.OpenStreetMap), _viewport.UnitsPerPixel, _viewport.CenterX, _viewport.CenterY);
            DropDown_Map_sat.Checked = false;
            DropDown_Map_land.Checked = false;
            DropDown_Map_hyb.Checked = false;
            DropDown_MaP_OSM.Checked = true;
        }

        private void DropDown_Track_Center_Click(object sender, EventArgs e)
        {
            Center_Track(GetActualTrack(Tracks4Map, FieldsParameters.active_activity));
            Invalidate();
        }

        private void DropDown_ALLTracks_Center_Click_1(object sender, EventArgs e)
        {
            Center_Track(GetALLTracks(Tracks4Map));
            Invalidate();
        }

        private void ToolStripComboBox_Year_SelectedIndexChanged(object sender, EventArgs e)
        {
            FieldsParameters.Filter_Year = toolStripComboBox_Year.SelectedItem.ToString();
            Invalidate();
        }

        private void ToolStripComboBox_Month_SelectedIndexChanged(object sender, EventArgs e)
        {
            FieldsParameters.Filter_Month = toolStripComboBox_Month.SelectedItem.ToString();
            Invalidate();
        }

        private void ToolStripComboBox_Cat_SelectedIndexChanged(object sender, EventArgs e)
        {
            FieldsParameters.Filter_Cat = toolStripComboBox_Cat.SelectedItem.ToString();
            Invalidate();
        }

        private void ToolStripComboBox_Loc_SelectedIndexChanged(object sender, EventArgs e)
        {
            FieldsParameters.Filter_Loc = toolStripComboBox_Loc.SelectedItem.ToString();
            Invalidate();
        }


        //
        // ----------------------------------------------------------------------------------------------------------------
        //
        // MapControl Overrides
        //
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int dummy = FieldsParameters.active_activity;
                FieldsParameters.active_activity = Get_Activity_byLocation(_viewport.ScreenToWorld(e.X, e.Y), FieldsParameters.active_activity);

                if (dummy != FieldsParameters.active_activity)
                {
                    Update_Panels();
                }
            }

            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _previousMousePosition = new Point();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _down = false;
            _previousMousePosition = new Point();
            base.OnMouseUp(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) _down = true;
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!_down)
            {
                return;
            }

            if (_previousMousePosition == new Point())
            {
                _previousMousePosition = e.Location;
                return; // It turns out that sometimes MouseMove+Pressed is called before MouseDown
            }

            var currentMousePosition = e.Location; //Needed for both MouseMove and MouseWheel event
            _viewport.Transform(currentMousePosition.X, currentMousePosition.Y, _previousMousePosition.X, _previousMousePosition.Y);
            _previousMousePosition = currentMousePosition;
            _fetcher.ViewChanged(_viewport.Extent, _viewport.UnitsPerPixel);

            Invalidate();

            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                _viewport.UnitsPerPixel = ZoomHelper.ZoomIn(_tileSource.Schema.Resolutions.Select(r => r.Value.UnitsPerPixel).ToList(), _viewport.UnitsPerPixel);
            }
            else if (e.Delta < 0)
            {
                _viewport.UnitsPerPixel = ZoomHelper.ZoomOut(_tileSource.Schema.Resolutions.Select(r => r.Value.UnitsPerPixel).ToList(), _viewport.UnitsPerPixel);
            }

            _fetcher.ViewChanged(_viewport.Extent, _viewport.UnitsPerPixel);
            //e.Handled = true; //so that the scroll event is not sent to the html page.

            Invalidate();
            base.OnMouseWheel(e);
        }

        protected override void OnResize(EventArgs e)
        {
            if (_viewport == null) return;
            _viewport.Width = Width;
            _viewport.Height = Height;
            _buffer = new Bitmap(Width, Height);
            _route = new Bitmap(Width, Height);
            _renderer = new Renderer(_buffer, _route);
            _fetcher.ViewChanged(_viewport.Extent, _viewport.UnitsPerPixel);

            Invalidate();
            //update_panel1(active_activity);

            base.OnResize(e);
        }
        //
        // --------------------------------------------------------------------------------------------------------------
        //
        // MapControl Methods
        //
        private void InitializeFilter()
        {
            foreach (activity act in SC_DataBase[FieldsParameters.active_athlet].Activities)
            {
                if (!string.IsNullOrEmpty(act.ActivityTime.ToString()))
                {
                    if (!toolStripComboBox_Year.Items.Contains(act.ActivityTime.Year.ToString())) toolStripComboBox_Year.Items.Add(act.ActivityTime.Year.ToString());
                    if (!toolStripComboBox_Month.Items.Contains(act.ActivityTime.Month.ToString("00"))) toolStripComboBox_Month.Items.Add(act.ActivityTime.Month.ToString("00"));
                }

                if (!string.IsNullOrEmpty(act.CatName))
                {
                    if (!toolStripComboBox_Cat.Items.Contains(act.CatName)) toolStripComboBox_Cat.Items.Add(act.CatName);
                }

                if (!string.IsNullOrEmpty(act.LocationName))
                {
                    if (!toolStripComboBox_Loc.Items.Contains(act.LocationName)) toolStripComboBox_Loc.Items.Add(act.LocationName);
                }
            }

            toolStripComboBox_Year.Items.Add(" All Years");
            toolStripComboBox_Year.SelectedIndex = toolStripComboBox_Year.Items.IndexOf(" All Years");
            toolStripComboBox_Month.Items.Add(" All Months");
            toolStripComboBox_Month.SelectedIndex = toolStripComboBox_Month.Items.IndexOf(" All Months");
            toolStripComboBox_Cat.Items.Add(" All Categories");
            toolStripComboBox_Cat.SelectedIndex = toolStripComboBox_Cat.Items.IndexOf(" All Categories");
            toolStripComboBox_Loc.Items.Add(" All Locations");
            toolStripComboBox_Loc.SelectedIndex = toolStripComboBox_Loc.Items.IndexOf(" All Locations");
        }

        private void InitializeMapControl()
        {
            _buffer = new Bitmap(Width, Height);
            _route = new Bitmap(Width, Height);

            _renderer = new Renderer(_buffer, _route);

            //_tileSource = KnownTileSources.Create();
            // OSM source
            //_tileSource = new HttpTileSource(new GlobalSphericalMercator(0, 19), API_key_Landscape, new[] { "a", "b", "c" }, "OSM");
            _tileSource = KnownTileSources.Create(KnownTileSource.BingAerial);
           

            _fetcher = new Fetcher<Image>(_tileSource, _tileCache);
            _fetcher.DataChanged += FetcherOnDataChanged;
        }

        private static bool TryInitializeViewport(ref Viewport viewport, double actualWidth, double actualHeight, ITileSchema schema, 
            double _Initial_Lon, double _Initial_Lat, double _Initial_Zoom)
        {
            if (double.IsNaN(actualWidth)) return false;
            if (actualWidth <= 0) return false;

            //var nearestLevel = Utilities.GetNearestLevel(schema.Resolutions, schema.Extent.Width / actualWidth);
            var nearestLevel = Utilities.GetNearestLevel(schema.Resolutions, _Initial_Zoom);
            // Manipuliere so dass start mit BW scale erfolgt... -> Zoom = 8 -> 1222.992452344f / Zoom = 9 -> 305.748113086f

            viewport = new Viewport
            {
                Width = actualWidth,
                Height = actualHeight,
                UnitsPerPixel = schema.Resolutions[nearestLevel].UnitsPerPixel,
                //Center = new Point((int)schema.Extent.CenterX, (int)schema.Extent.CenterY)
                Center = new Point((int)(_Initial_Lon * 20037508 / 180), (int)((System.Math.Log(System.Math.Tan((_Initial_Lat + 90) / 360 * System.Math.PI)) / System.Math.PI * 180) * 20037508 / 180)) // eventuell kann auch PointD benutzt werden
                // Manipuliere so dass start mit Fokus auf BW erfolgt... -> Initial_Lon = 9.1 und Initial_Lat = 48.5 (Y ist wegen Mercator-Korrektur etwas komplexer...)
            };
            return true;
        }

        public void SetTileSource(ITileSource source, double unitsPerPixel_old, double centerOldX, double centerOldY)
        {
            //_fetcher.DataChanged -= FetcherOnDataChanged;
            _fetcher.AbortFetch();

            _tileSource = source;
            _viewport.CenterX = centerOldX;
            _viewport.CenterY = centerOldY;
            _viewport.UnitsPerPixel = unitsPerPixel_old;
            _tileCache.Clear();
            _fetcher = new Fetcher<Image>(_tileSource, _tileCache);
            _fetcher.DataChanged += FetcherOnDataChanged;
            _fetcher.ViewChanged(_viewport.Extent, _viewport.UnitsPerPixel); // start fetching...
        }

        public List<List<TrackPoint>> GetTracks (List<Track4Map> _tracks)
        {
            List<List<TrackPoint>> _list = new List<List<TrackPoint>>();
            
            foreach(Track4Map t4m in _tracks)
            {
                //List<TrackPoint> dummy = new List<TrackPoint>(t4m.Track);
                _list.Add(t4m.Track);
            }
            
            return _list;
        }

        public void Center_Track(List<List<TrackPoint>> _AllTracks)
        {
            if (_AllTracks[0].Any())
            {
                _viewport.CenterX = _viewport.LongitudeToX(TrackHelpers.Get_Min_Lon((_AllTracks))
                               + (TrackHelpers.Get_Max_Lon((_AllTracks)) - TrackHelpers.Get_Min_Lon((_AllTracks))) / 2);
                _viewport.CenterY = _viewport.LatitudeToY(TrackHelpers.Get_Min_Lat((_AllTracks))
                    + (TrackHelpers.Get_Max_Lat((_AllTracks)) - TrackHelpers.Get_Min_Lat((_AllTracks))) / 2);

                double _delta_Lon = _viewport.LongitudeToX(TrackHelpers.Get_Max_Lon((_AllTracks))) - _viewport.LongitudeToX(TrackHelpers.Get_Min_Lon((_AllTracks)));
                double _delta_Lat = _viewport.LatitudeToY(TrackHelpers.Get_Max_Lat((_AllTracks))) - _viewport.LatitudeToY(TrackHelpers.Get_Min_Lat((_AllTracks)));

                double _unitsPerPixel_dummy_Lon = _delta_Lon / (Width * 0.9);
                double _unitsPerPixel_dummy_Lat = _delta_Lat / (Height * 0.9);

                // which is the larger dimension of track?
                if (_delta_Lon < _delta_Lat)
                {
                    _viewport.UnitsPerPixel = ZoomHelper.ZoomRoute(_tileSource.Schema.Resolutions.Select(r => r.Value.UnitsPerPixel).ToList(), _viewport.UnitsPerPixel, _unitsPerPixel_dummy_Lat);
                }
                else
                {
                    _viewport.UnitsPerPixel = ZoomHelper.ZoomRoute(_tileSource.Schema.Resolutions.Select(r => r.Value.UnitsPerPixel).ToList(), _viewport.UnitsPerPixel, _unitsPerPixel_dummy_Lon);
                }

                _fetcher.ViewChanged(_viewport.Extent, _viewport.UnitsPerPixel);
            }
        }

        //
        // -----------------------------------------------------------------------------------------------------------------
        //
        // Elevation chart (chart_ele) part of code 
        //
        // Fields
        //

        private Point Start = new Point();
        private Rectangle RcDraw; // = new Rectangle( x, y, width, heigth );

        private Pen Marker_Pen = new Pen(Color.Red, 1); // ... for marking lines (see marking elevation data)

        // Elevation - Eventhandler
        //
        private void Chart_ele_MouseDown(object sender, MouseEventArgs e)
        {
            // Methode for marking elevation data...
            // Determine the initial coordinates...
            Start.X = e.X;
            RcDraw.X = e.X;
            Start.Y = -1000; // rectangle will be larger than chart_ele -> getting two vertical lines
            RcDraw.Y = -1000; // rectangle will be larger than chart_ele -> getting two vertical lines
            RcDraw.Height = 2000; // rectangle will be larger than chart_ele -> getting two vertical lines
            FieldsParameters.mouseDown = true;
            Marker_Pen.Color = Color.Red;
            label_delta.Text = "";
            label_delta.Visible = true;
            label_delta.BringToFront();
        }

        private void Chart_ele_MouseMove(object sender, MouseEventArgs e)
        {
            if (FieldsParameters.mouseDown)
            {
                // Determine the width of the rectangle...
                if (e.X < Start.X)
                {
                    RcDraw.Width = Start.X - e.X;
                    RcDraw.X = e.X;
                }
                else
                {
                    RcDraw.Width = e.X - Start.X;
                }

                chart_ele.Invalidate();
            }
        }

        private void Chart_ele_MouseUp(object sender, MouseEventArgs e)
        {
            FieldsParameters.mouseDown = false;
            bool first_Hit = true;

            FieldsParameters.minX_Value = chart_ele.ChartAreas[0].AxisX.PixelPositionToValue(RcDraw.X);
            FieldsParameters.maxX_Value = chart_ele.ChartAreas[0].AxisX.PixelPositionToValue(RcDraw.X + RcDraw.Width);

            for (int i = 0; i < SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Track.Count; i++)
            {
                
                    if (FieldsParameters.minX_Value < (SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Track[i].dist / 1000) && first_Hit)
                    {
                        first_Hit = false;
                    FieldsParameters.minX_Alt = (double)SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Track[i].ele;
                    }
                    if (FieldsParameters.maxX_Value > (SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Track[i].dist / 1000))
                    {
                    FieldsParameters.maxX_Alt = (double)SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Track[i].ele;
                    }
            }

            Marker_Pen.Color = Color.Transparent;
            chart_ele.Invalidate();

            label_delta.Text = " Delta: " + (FieldsParameters.maxX_Value - FieldsParameters.minX_Value).ToString("###0.##km") + " / " + (FieldsParameters.maxX_Alt - FieldsParameters.minX_Alt).ToString("###0m")
                    + " / " + ((FieldsParameters.maxX_Alt - FieldsParameters.minX_Alt) / (FieldsParameters.maxX_Value - FieldsParameters.minX_Value) / 1000).ToString("#0%");

            Make_Graph();
        }

        private void Chart_ele_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(new Pen(Color.Red, 1), RcDraw);
            Invalidate();
        }
        //
        // Elevation chart - method
        //        
        private void Make_Graph()
        {
            this.chart_ele.Series.Clear();
            Series series = this.chart_ele.Series.Add("Elevation");
            Series series2 = this.chart_ele.Series.Add("Marked");

            // series.ChartType = SeriesChartType.Spline;
            series.ChartType = SeriesChartType.FastLine;//  Line;
            series2.ChartType = SeriesChartType.Line;
            series2.BorderWidth = 3;

            series.Color = Color.Blue;
            series2.Color = Color.Red;
            chart_ele.ChartAreas[0].AxisY.LabelStyle.Format = "{#####}m";
            chart_ele.ChartAreas[0].AxisY.LabelStyle.Font = new Font("Microsoft Sans Serif", 8, FontStyle.Regular, GraphicsUnit.Point, 0);
            chart_ele.ChartAreas[0].AxisX.LabelStyle.Format = "{####0.#}km";

            if (SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Track.Count > 1)
            {
                for (int i = 0; i < SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Track.Count; i++)
                {
                    if (FieldsParameters.maxX_Value > SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Track[i].dist / 1000 
                        && FieldsParameters.minX_Value < SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Track[i].dist / 1000)
                        {
                            series2.Points.AddXY(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Track[i].dist / 1000,
                                SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Track[i].ele);
                            if (FieldsParameters.minAlt > SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Track[i].ele)
                            {
                            FieldsParameters.minAlt = (double)SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Track[i].ele;
                            }
                            if (FieldsParameters.maxAlt < SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Track[i].ele)
                            {
                            FieldsParameters.maxAlt = (double)SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Track[i].ele;
                            }
                        }
                        series.Points.AddXY(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Track[i].dist / 1000,
                            SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Track[i].ele);
                }
                chart_ele.ChartAreas[0].AxisY.Maximum = EleHelpers.getmaxEle(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Track) 
                    * 1.02; // sets the Maximum + 2%
                chart_ele.ChartAreas[0].AxisY.Minimum = EleHelpers.getminEle(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Track) 
                    * 0.98; // sets the Minimum - 2%
                chart_ele.ChartAreas[0].RecalculateAxesScale(); // falls bei laufendem Programm Höhenpunkte dazu kommen...
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------
        // Panel1 - Control and Overview of Activity
        //
        // Methods

        private void InitializePanel1()
        {
            foreach (activity act in SC_DataBase[FieldsParameters.active_athlet].Activities)
            {
                if (!string.IsNullOrEmpty(act.LocationName))
                {
                    if (!comboBox_Place.Items.Contains(act.LocationName)) comboBox_Place.Items.Add(act.LocationName);
                }
            }

            UpdateCalendar();
        }

        private void UpdateCalendar()
        {
            monthCalendar1.RemoveAllBoldedDates();
            monthCalendar1.BoldedDates = Get_activityDates();
        }

        private void Update_Panels()
        {
            Update_panel1(FieldsParameters.active_activity);
            Invalidate();
        }

        private void Update_panel1(int _active_activity)
        {
            if (SC_DataBase != null)
            {

                dateTimePicker1.Value = SC_DataBase[FieldsParameters.active_athlet].Activities[_active_activity].ActivityTime; // Date
                dateTimePicker2.Value = SC_DataBase[FieldsParameters.active_athlet].Activities[_active_activity].ActivityTime; // Time

                comboBox_Cat.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[_active_activity].CatName;
                comboBox_Place.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[_active_activity].LocationName;

                textBox_Distance.Text = (SC_DataBase[FieldsParameters.active_athlet].Activities[_active_activity].Distance / 1000).ToString("#0.00");

                int Stunden = (int)SC_DataBase[FieldsParameters.active_athlet].Activities[_active_activity].Duration / 3600;
                int Rest = (int)SC_DataBase[FieldsParameters.active_athlet].Activities[_active_activity].Duration % 3600;
                int Minuten = Rest / 60;
                int Sekunden = Rest % 60;
                dateTimePicker3.Value = new DateTime(2000, 1, 1, Stunden, Minuten, Sekunden); // Duration

                textBox_Pause.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[_active_activity].Pause_total.ToString();

                textBox_Descent.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[_active_activity].total_Descent.ToString("0.#");
                textBox_Ascent.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[_active_activity].total_Ascent.ToString("0.#");

                textBox_V_avg.Text = (SC_DataBase[FieldsParameters.active_athlet].Activities[_active_activity].Distance /
                    SC_DataBase[FieldsParameters.active_athlet].Activities[_active_activity].Duration * 3.6).ToString("#0.0"); // km/h

                textBox_Calories.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[_active_activity].Calories_total.ToString();

                textBox_HR_avg.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[_active_activity].mean_HR.ToString("0.#");
                textBox_Cad_avg.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[_active_activity].Cadence_mean.ToString("0.#");

                comboBox_Weather.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[_active_activity].Weather_Cond;
                textBox_Temp.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[_active_activity].Weather_Temp.ToString() + "°C";

                textBox_Info.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[_active_activity].Info_Note;

                textBox_P_avg.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[_active_activity].Power_avrg.ToString("0.#");

                monthCalendar1.SelectionStart = SC_DataBase[FieldsParameters.active_athlet].Activities[_active_activity].ActivityTime;

                Make_Graph();
                RefreshPanelWeek(SC_DataBase[FieldsParameters.active_athlet].Activities[_active_activity].ActivityTime);
                PlotPanelAnalysis();

                if (FieldsParameters.autoamtic_center_track & !FieldsParameters.program_start) Center_Track(GetActualTrack(Tracks4Map, FieldsParameters.active_activity));
                FieldsParameters.program_start = false;
                // center_track should not happen at start of program (avoiding runtime error)
            }
        }

        private void Change_Activity(activity _Activity, string _issue)
        {
            _Activity.MetaModified = DateTime.Now;
            _Activity.Info_Note += ("\r\nModified: " + _issue);
            textBox_Info.Text = _Activity.Info_Note;
        }

        //
        // Panel1 - Events
        //
        private void DateTimePicker1_ValueChanged(object sender, EventArgs e) // date
        {
            SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].ActivityTime = dateTimePicker1.Value;
            //database_changed = true;

            if (!FieldsParameters.program_start) Update_Panels();
        }

        private void MonthCalendar1_DateSelected(object sender, DateRangeEventArgs e)
        {
            FieldsParameters.active_activity = Get_Activity_byDate_MonthCal(e.Start, FieldsParameters.active_activity);

            if (!FieldsParameters.program_start) Update_Panels();
        }

        private void DropDown_Activity_GetEleSTM_Click(object sender, EventArgs e)
        {
            var srtmData = new SRTM.SRTMData(FieldsParameters.SRTM_Data_Dir, new USGSSource());
            // elevation data will be overridden !!!
            // For SmartBike elevation data from SRTM will not be uesed (creating more noise)
            if (SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].CatName != "SmartBike")
            {
                foreach (TrackPoint tpt in SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Track)
                {
                    tpt.ele = (double)TrackHelpers.check4NegativeValue(srtmData.GetElevation((double)tpt.lat, (double)tpt.lon));
                }
            }

            double[] elev = EleHelpers.Calc_Elevation(EleHelpers.GetEleData(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Track));
            SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].total_Ascent = elev[0]; // up!
            SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].total_Descent = elev[1]; // down!

            Update_panel1(FieldsParameters.active_activity);
        }

        private void DropDown_Activity_GetWeather_Click(object sender, EventArgs e)
        {
            WeatherObject weather = GetWeather.GetWetterByCoordTime((double)SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Track[0].lat,
                (double)SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Track[0].lon,
                       SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].ActivityTime, FieldsParameters.API_key_Weather);
            if (weather != null)
            {
                SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Weather_Cond = weather.data[0].weather[0].main;
                SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Weather_Temp = (int)(weather.data[0].temp - 273);
                SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Weather_Note = "ID: "
                    + weather.data[0].weather[0].id.ToString() + " / Main: " + weather.data[0].weather[0].description + " / Icon: " + weather.data[0].weather[0].icon;
            }

            Update_panel1(FieldsParameters.active_activity);
        }

        private void DropDown_Activity_CalcCal_Click(object sender, EventArgs e)
        {
            switch (SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].CatName)
            {
                case "Wandern":
                    DatabaseHelpers.calc_PowerCalories_Hiking(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity]);

                    textBox_P_avg.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Power_avrg.ToString("0.#");
                    textBox_Calories.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Calories_total.ToString();

                    Change_Activity(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity], "Calc Calories, Power");
                    FieldsParameters.database_changed = true;

                    break;

                case "Laufen":
                    DatabaseHelpers.calc_PowerCalories_Hiking(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity]);

                    textBox_P_avg.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Power_avrg.ToString("0.#");
                    textBox_Calories.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Calories_total.ToString();

                    Change_Activity(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity], "Calc Calories, Power");
                    FieldsParameters.database_changed = true;

                    break;

                case "SmartBike":
                    DatabaseHelpers.calc_Calories(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity]);

                    textBox_Calories.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Calories_total.ToString();

                    Change_Activity(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity], "Calc Calories");
                    FieldsParameters.database_changed = true;

                    break;
                case "MTB":
                    DatabaseHelpers.calc_Power_MTBBike(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity]);

                    textBox_Calories.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Calories_total.ToString();
                    textBox_P_avg.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Power_avrg.ToString("0.#");

                    Change_Activity(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity], "Calc Calories, Power");
                    FieldsParameters.database_changed = true;

                    break;

                case "Rennrad":
                    DatabaseHelpers.calc_Power_RaceBike(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity]);

                    textBox_Calories.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Calories_total.ToString();
                    textBox_P_avg.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Power_avrg.ToString("0.#");

                    Change_Activity(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity], "Calc Calories, Power");
                    FieldsParameters.database_changed = true;

                    break;

                case "CycloCross":
                    DatabaseHelpers.calc_Power_CrossBike(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity]);

                    textBox_Calories.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Calories_total.ToString();
                    textBox_P_avg.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Power_avrg.ToString("0.#");

                    Change_Activity(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity], "Calc Calories, Power");
                    FieldsParameters.database_changed = true;

                    break;

                case "Ergometer":
                    DatabaseHelpers.calc_Power_CrossBike(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity]);

                    textBox_Calories.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Calories_total.ToString();
                    textBox_P_avg.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Power_avrg.ToString("0.#");

                    Change_Activity(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity], "Calc Calories, Power");
                    FieldsParameters.database_changed = true;

                    break;

                case "ErgoMTB":
                    DatabaseHelpers.calc_Power_MTBBike(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity]);

                    textBox_Calories.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Calories_total.ToString();
                    textBox_P_avg.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Power_avrg.ToString("0.#");

                    Change_Activity(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity], "Calc Calories, Power");
                    FieldsParameters.database_changed = true;

                    break;

                case "ErgoRace":
                    DatabaseHelpers.calc_Power_RaceBike(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity]);

                    textBox_Calories.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Calories_total.ToString();
                    textBox_P_avg.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Power_avrg.ToString("0.#");

                    Change_Activity(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity], "Calc Calories, Power");
                    FieldsParameters.database_changed = true;

                    break;

                case "Schwimmen":
                    DatabaseHelpers.calc_CaloriesPower_Swimming(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity]);

                    textBox_Calories.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Calories_total.ToString();
                    textBox_P_avg.Text = SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Power_avrg.ToString("0.#");

                    Change_Activity(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity], "Calc Calories, Power");
                    FieldsParameters.database_changed = true;

                    break;
            }
        }

        private void DateTimePicker3_Leave(object sender, EventArgs e) // duration
        {
            SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Duration = (dateTimePicker3.Value - new DateTime(2000, 1, 1)).TotalSeconds;
            textBox_V_avg.Text = (SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Distance /
                SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Duration * 3.6).ToString("#0.0") + " km/h"; // km/h
            Change_Activity(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity], "Duration");
        }

        private void ComboBox_Cat_SelectedIndexChanged(object sender, EventArgs e) // category
        {
            SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].CatName = comboBox_Cat.SelectedItem.ToString();
        }

        private void ComboBox_Weather_SelectedIndexChanged(object sender, EventArgs e)
        {
            SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Weather_Cond = comboBox_Weather.SelectedItem.ToString();
        }

        private void ComboBox_Place_Leave(object sender, EventArgs e)
        {
            SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].LocationName = comboBox_Place.Text;

            if (!comboBox_Place.Items.Contains(comboBox_Place.Text))
            {
                comboBox_Place.Items.Add(comboBox_Place.Text);
            }
        }

        private void TextBox_Info_Leave(object sender, EventArgs e) // info
        {
            SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Info_Note = textBox_Info.Text;
            FieldsParameters.database_changed = true;
        }

        private void TextBox_Distance_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) & !char.IsControl(e.KeyChar) & (e.KeyChar != '.');
        }

        private void TextBox_Distance_Leave(object sender, EventArgs e)
        {
            double _number;

            if (double.TryParse(textBox_Distance.Text, out _number))
                SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Distance = _number * 1000;

            textBox_Distance.Text = (SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].Distance / 1000).ToString("#0.00");
            Change_Activity(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity], "Distance");
        }

        private void DateTimePicker2_ValueChanged(object sender, EventArgs e) // starttime
        {
            SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].ActivityTime = dateTimePicker2.Value;
            //database_changed = true;
        }

        private void ShowAllTracksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (showAllTracksToolStripMenuItem.Checked)
            {
                showAllTracksToolStripMenuItem.CheckState = CheckState.Unchecked;
                FieldsParameters.showAllTracks = false;
            }
            else
            {
                showAllTracksToolStripMenuItem.CheckState = CheckState.Checked;
                FieldsParameters.showAllTracks = true;
            }
            Invalidate();
        }

        private void ShowAllStartingPointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (showAllStartingPointsToolStripMenuItem.Checked)
            {
                showAllStartingPointsToolStripMenuItem.CheckState = CheckState.Unchecked;
                FieldsParameters.showStartPoints = false;
            }
            else
            {
                showAllStartingPointsToolStripMenuItem.CheckState = CheckState.Checked;
                FieldsParameters.showStartPoints = true;
            }

            Invalidate();
        }

        private void AutomaticCenterOfActTrackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (automaticCenterOfActTrackToolStripMenuItem.Checked)
            {
                automaticCenterOfActTrackToolStripMenuItem.CheckState = CheckState.Unchecked;
                FieldsParameters.autoamtic_center_track = false;
            }
            else
            {
                automaticCenterOfActTrackToolStripMenuItem.CheckState = CheckState.Checked;
                FieldsParameters.autoamtic_center_track = true;
            }

            Invalidate();
        }

        // -------------------------------------------------------------------------------------------------------------------------
        // Panel_Week - Control and Overview of Activity
        //
        // Events
        //

        // Methods
        //
        private void RefreshPanelWeek(DateTime _date)
        {
            DateTime _date_week = AnalysisHelpers.GetFirstDayOfWeek(_date);

            // first clear all buttons in panel_week
            foreach (Control item in panel_week.Controls.OfType<Button>().ToList())
            {
                panel_week.Controls.Remove(item);
            }

            for (int i = 0; i < 7; i++)
            {
                int j = 0;

                foreach (activity activity_DB in SC_DataBase[FieldsParameters.active_athlet].Activities)
                {
                    // actual week
                    if (_date_week.ToString() == activity_DB.ActivityTime.Date.ToString()) // hier nur das datum vergleichen! Keine Uhrzeit !!
                    {
                        NewButton(activity_DB, j);
                        j++;
                    }
                }
                _date_week = _date_week.AddDays(1);
            }
        }

        private void AnalyseWeekMonth(DateTime _date)
        {
            // actual week
            DateTime _date_week = AnalysisHelpers.GetFirstDayOfWeek(_date);
            FieldsParameters.week_distance = 0;
            FieldsParameters.week_duration = 0;
            FieldsParameters.week_calories = 0;
            FieldsParameters.week_ascent = 0;
            FieldsParameters.week_descent = 0;

            // week -1
            DateTime _date_min1_week = _date_week.Subtract(TimeSpan.FromDays(7));
            FieldsParameters.week_min1_distance = 0;
            FieldsParameters.week_min1_duration = 0;
            FieldsParameters.week_min1_calories = 0;
            FieldsParameters.week_min1_ascent = 0;
            FieldsParameters.week_min1_descent = 0;

            DateTime _date_month = AnalysisHelpers.GetFirstDayOfMonth(_date); //starts with first day of month
            FieldsParameters.month_distance = 0;
            FieldsParameters.month_duration = 0;
            FieldsParameters.month_calories = 0;
            FieldsParameters.month_ascent = 0;
            FieldsParameters.month_descent = 0;

            DateTime _date_min1_month = AnalysisHelpers.GetFirstDayOfMonth(AnalysisHelpers.GetFirstDayOfMonth(_date).Subtract(new TimeSpan(1, 0, 0, 0))); //starts with first day of month
            FieldsParameters.month_min1_distance = 0;
            FieldsParameters.month_min1_duration = 0;
            FieldsParameters.month_min1_calories = 0;
            FieldsParameters.month_min1_ascent = 0;
            FieldsParameters.month_min1_descent = 0;

            foreach (DateTime day in AnalysisHelpers.EachDay(_date_month, _date_month.AddMonths(1)))
            {
                foreach (activity activity_DB in SC_DataBase[FieldsParameters.active_athlet].Activities)
                {
                    // actual month
                    if (day.ToString() == activity_DB.ActivityTime.Date.ToString()) // hier nur das datum vergleichen! Keine Uhrzeit !!
                    {
                        FieldsParameters.month_distance += activity_DB.Distance;
                        FieldsParameters.month_duration += activity_DB.Duration;
                        FieldsParameters.month_calories += activity_DB.Calories_total;
                        FieldsParameters.month_ascent += activity_DB.total_Ascent;
                        FieldsParameters.month_descent += activity_DB.total_Descent;
                    }
                }
            }
            foreach (DateTime day in AnalysisHelpers.EachDay(_date_min1_month, _date_min1_month.AddMonths(1)))
            {
                foreach (activity activity_DB in SC_DataBase[FieldsParameters.active_athlet].Activities)
                {
                    // month -1
                    if (day.ToString() == activity_DB.ActivityTime.Date.ToString()) // hier nur das datum vergleichen! Keine Uhrzeit !!
                    {
                        FieldsParameters.month_min1_distance += activity_DB.Distance;
                        FieldsParameters.month_min1_duration += activity_DB.Duration;
                        FieldsParameters.month_min1_calories += activity_DB.Calories_total;
                        FieldsParameters.month_min1_ascent += activity_DB.total_Ascent;
                        FieldsParameters.month_min1_descent += activity_DB.total_Descent;
                    }
                }
            }

            for (int i = 0; i < 7; i++)
            {
                int j = 0;

                foreach (activity activity_DB in SC_DataBase[FieldsParameters.active_athlet].Activities)
                {
                    // actual week
                    if (_date_week.ToString() == activity_DB.ActivityTime.Date.ToString()) // hier nur das datum vergleichen! Keine Uhrzeit !!
                    {
                        FieldsParameters.week_distance += activity_DB.Distance;
                        FieldsParameters.week_duration += activity_DB.Duration;
                        FieldsParameters.week_calories += activity_DB.Calories_total;
                        FieldsParameters.week_ascent += activity_DB.total_Ascent;
                        FieldsParameters.week_descent += activity_DB.total_Descent;

                        j++;
                    }
                    // week -1 
                    if (_date_min1_week.ToString() == activity_DB.ActivityTime.Date.ToString()) // hier nur das datum vergleichen! Keine Uhrzeit !!
                    {
                        FieldsParameters.week_min1_distance += activity_DB.Distance;
                        FieldsParameters.week_min1_duration += activity_DB.Duration;
                        FieldsParameters.week_min1_calories += activity_DB.Calories_total;
                        FieldsParameters.week_min1_ascent += activity_DB.total_Ascent;
                        FieldsParameters.week_min1_descent += activity_DB.total_Descent;
                    }
                }
                _date_week = _date_week.AddDays(1);
                _date_min1_week = _date_min1_week.AddDays(1);
            }
        }

        private void NewButton(activity _activity, int _j)
        {
            Button myButton = new Button
            {
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                Font = new Font(Font.FontFamily, 7),
                Text =
                //_activity.ActivityTime.Date.ToString("d", CultureInfo.GetCultureInfo("de-DE")) + " / " +
                _activity.CatName.ToString() + " / "
                //+ _activity.LocationName.ToString() //+ "\n" 
                + (_activity.Distance / 1000).ToString("#0.00") + " km / " + AnalysisHelpers.Time2DurationStrin(_activity.Duration),
                Tag = _activity
            };

            myButton.FlatAppearance.BorderSize = 0; // no button border

            switch (_activity.ActivityTime.Date.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    myButton.Location = new Point(30, 10 + (_j * 20));
                    break;
                case DayOfWeek.Tuesday:
                    myButton.Location = new Point(30, 50 + (_j * 20));
                    break;
                case DayOfWeek.Wednesday:
                    myButton.Location = new Point(30, 90 + (_j * 20));
                    break;
                case DayOfWeek.Thursday:
                    myButton.Location = new Point(30, 130 + (_j * 20));
                    break;
                case DayOfWeek.Friday:
                    myButton.Location = new Point(30, 170 + (_j * 20));
                    break;
                case DayOfWeek.Saturday:
                    myButton.Location = new Point(30, 210 + (_j * 20));
                    break;
                case DayOfWeek.Sunday:
                    myButton.Location = new Point(30, 250 + (_j * 20));
                    break;
            }

            // attach event handler for Click event 
            myButton.Click += new EventHandler(Jump2Activity);
            panel_week.Controls.Add(myButton);
        }

        private void Jump2Activity(object sender, EventArgs e)
        {
            Button _button = (Button)sender;
            activity _act = (activity)_button.Tag;

            FieldsParameters.active_activity = Get_Activity_byDate_WeekPanel(_act.ActivityTime, FieldsParameters.active_activity);
            Update_panel1(FieldsParameters.active_activity);
            Invalidate();
        }
        // -----------------------------------------------------------------------------------------------------
        // Panel Analysis
        //
        // Events
        //

        // Panael Analysis Methods
        //
        private void PlotPanelAnalysis()
        {
            AnalyseWeekMonth(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity].ActivityTime); //update data for panelweek and panel_analysis

            label_week_dist.Text = (FieldsParameters.week_distance / 1000).ToString("#0.0") + " km";
            label_week_dur.Text = AnalysisHelpers.Time2DurationStrin(FieldsParameters.week_duration);
            label_week_cal.Text = FieldsParameters.week_calories.ToString() + " kcal";
            label_week_asc.Text = Math.Round(FieldsParameters.week_ascent).ToString() + " / " + Math.Round(FieldsParameters.week_descent).ToString() + " m";

            label_week_min1_dis.Text = (FieldsParameters.week_min1_distance / 1000).ToString("#0.0") + " km";
            label_week_min1_dur.Text = AnalysisHelpers.Time2DurationStrin(FieldsParameters.week_min1_duration);
            label_week_min1_cal.Text = FieldsParameters.week_min1_calories.ToString() + " kcal";
            label_week_min1_asc.Text = Math.Round(FieldsParameters.week_min1_ascent).ToString() + " / " + Math.Round(FieldsParameters.week_min1_descent).ToString() + " m";

            label_month_dis.Text = (FieldsParameters.month_distance / 1000).ToString("#0.0") + " km";
            label_month_dur.Text = AnalysisHelpers.Time2DurationStrin(FieldsParameters.month_duration);
            label_month_cal.Text = FieldsParameters.month_calories.ToString() + " kcal";
            label_month_asc.Text = Math.Round(FieldsParameters.month_ascent).ToString() + " / " + Math.Round(FieldsParameters.month_descent).ToString() + " m";

            label_month_min1_dis.Text = (FieldsParameters.month_min1_distance / 1000).ToString("#0.0") + " km";
            label_month_min1_dur.Text = AnalysisHelpers.Time2DurationStrin(FieldsParameters.month_min1_duration);
            label_month_min1_cal.Text = FieldsParameters.month_min1_calories.ToString() + " kcal";
            label_month_min1_asc.Text = Math.Round(FieldsParameters.month_min1_ascent).ToString() + " / " + Math.Round(FieldsParameters.month_min1_descent).ToString() + " m";
        }

        // --------------------------------------------------------------------------------------------------------------
        //
        // IO code
        //

        //
        // IO Methods
        //

        private List<activity> Load_DB_from_GPX_TCXFile()
        {
            List<activity> newActivities = new List<activity>();

            using (OpenFileDialog ofDlg = new OpenFileDialog())
            {
                ofDlg.Filter = "GPX / TCX Files (*.gpx)|*.gpx; *.tcx";
                if (ofDlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Cursor = Cursors.WaitCursor; // Wait-Cursor will be shown for time of writing process

                        switch (Path.GetExtension(ofDlg.FileName))
                        {
                            case ".gpx":
                                using (GpxReader reader = new GpxReader(new FileStream(ofDlg.FileName, FileMode.Open)))
                                {
                                    Gpx2database writer = new Gpx2database();

                                    while (reader.Read())
                                    {
                                        switch (reader.ObjectType)
                                        {
                                            case GpxObjectType.Track:
                                                newActivities = writer.WriteTrackData2database(reader.Track, SC_DataBase, FieldsParameters.active_athlet);
                                                break;
                                        }
                                    }
                                }
                                break;

                            case ".tcx":
                                using (TcxReader tcxreader = new TcxReader(new FileStream(ofDlg.FileName, FileMode.Open)))
                                {
                                    Tcx2database writer = new Tcx2database();
                                    while (tcxreader.Read())
                                    {
                                        switch (tcxreader.ObjectType)
                                        {
                                            case GpxObjectType.TcxActivities:
                                                newActivities = writer.WriteLapData2database(tcxreader.TcxActivities, SC_DataBase, FieldsParameters.active_athlet);
                                                break;
                                        }
                                    }
                                }

                                break;
                        }

                        FieldsParameters.database_changed = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                }
            }

            bool newActivity = true;

            foreach (activity activity_new in newActivities)
            {
                foreach (activity activity_DB in SC_DataBase[FieldsParameters.active_athlet].Activities)
                {
                    if (activity_new.ActivityTime.ToString() == activity_DB.ActivityTime.ToString())
                    {
                        newActivity = false;
                        DialogResult result = MessageBox.Show("Aktivität bereits in LogBook vorhanden:\n\n"
                            + "LogBook: " + activity_DB.MetaModified + "\n"
                            + "Neu:    " + activity_new.MetaModified + "\n\n"
                            + "Als Kopie speichern?",
                            "Speicherbestätigung Aktivität", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                        switch (result)
                        {
                            case DialogResult.Yes:
                                FieldsParameters.database_changed = true;
                                break;
                            case DialogResult.No:
                                newActivities.Remove(activity_new);
                                FieldsParameters.database_changed = false;
                                return newActivities;
                        }
                    }
                }

                if (newActivity)
                {
                    FieldsParameters.database_changed = true;
                }
            }

            return newActivities;

        } // load data from gpx-file (e.g. from RPS program)

        private void Load_DB_from_fitlogFile()
        {
            List<athlet> newDB_elements = new List<athlet>();

            using (OpenFileDialog ofDlg = new OpenFileDialog())
            {
                ofDlg.Filter = " Files (*.fitlog)|*.fitlog|All Files (*.*)|*.*";
                if (ofDlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Cursor = Cursors.WaitCursor; // Wait-Cursor will be shown for time of writing process

                        using (FitlogReader reader = new FitlogReader(new FileStream(ofDlg.FileName, FileMode.Open)))
                        {
                            Fitlog2database writer = new Fitlog2database();

                            while (reader.Read())
                            {
                                switch (reader.ObjectType)
                                {
                                    case GpxObjectType.AthletLogData:
                                        newDB_elements.AddRange(writer.WriteAllAthletData2database(reader.AthletLogdata)); // hier ist der fehler (falsches Format)
                                        break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                    finally
                    {
                        Cursor = Cursors.Default;
                    }
                }
            }

            foreach (athlet athlet_new in newDB_elements)
            {
                bool newAthlet = true;
                bool newActivity = true;

                foreach (athlet athlet_DB in SC_DataBase)
                {
                    if (athlet_new.AthleteID == athlet_DB.AthleteID)
                    {
                        newAthlet = false;

                        foreach (activity activity_new in athlet_new.Activities)
                        {
                            for (int i = 0; i < athlet_DB.Activities.Count; i++)
                            {

                                if (activity_new.ActivityID == athlet_DB.Activities[i].ActivityID)
                                {
                                    newActivity = false;

                                    DialogResult result = MessageBox.Show("Aktivität bereits in LogBook vorhanden:\n\n"
                                        + "LogBook Aktivität vom " + athlet_DB.Activities[i].MetaModified + "\n"
                                        + "Neue Aktivität vom " + activity_new.MetaModified + "\n\n"
                                        + "Als Kopie speichern?",
                                        "Speicherbestätigung Aktivität", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                                    switch (result)
                                    {
                                        case DialogResult.Yes:
                                            newActivity = true;
                                            break;
                                        case DialogResult.No:
                                            break;
                                    }
                                }

                                if (newActivity)
                                {
                                    athlet_DB.Activities.Add(activity_new);
                                    FieldsParameters.database_changed = true;
                                    break;
                                }
                            }
                        }

                        bool newFitness = true;

                        foreach (fitness fitness_new in athlet_new.History)
                        {
                            foreach (fitness fitness_DB in athlet_DB.History)
                            {
                                if (fitness_new.Date == fitness_DB.Date)
                                {
                                    newFitness = false;

                                    DialogResult result = MessageBox.Show("FitnessEintrag bereits in LogBook vorhanden:\n\n"
                                        + "Als Kopie speichern?",
                                        "Speicherbestätigung Fitness", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                                    switch (result)
                                    {
                                        case DialogResult.Yes:
                                            athlet_DB.History.Add(fitness_new);
                                            FieldsParameters.database_changed = true;
                                            break;
                                        case DialogResult.No:
                                            break;
                                    }
                                }
                            }

                            if (newFitness)
                            {
                                athlet_DB.History.Add(fitness_new);
                                FieldsParameters.database_changed = true;
                            }
                        }
                    }
                }

                if (newAthlet)
                {
                    SC_DataBase.Add(athlet_new);
                    FieldsParameters.database_changed = true;
                }
            }
        } // Fitlog file = part of other data base = transfer of data base elements from other (sporttracks) database

        //
        // IO Events
        //

        private void DropDown_Import_GPX_Click(object sender, EventArgs e)
        {
            SC_DataBase[FieldsParameters.active_athlet].Activities.AddRange(Load_DB_from_GPX_TCXFile());
            if (FieldsParameters.database_changed)
            {
                FieldsParameters.active_activity = SC_DataBase[FieldsParameters.active_athlet].Activities.Count - 1;
                LoadAllTracks4Map(); // update track list
                Update_panel1(FieldsParameters.active_activity);
                Center_Track(GetActualTrack(Tracks4Map, FieldsParameters.active_activity));
                Invalidate();
            }
        } // Import of GPX file

        private void DropDown_Import_FitLog_Click(object sender, EventArgs e)
        {
            Load_DB_from_fitlogFile();
            if (FieldsParameters.database_changed)
            {
                FieldsParameters.active_activity = SC_DataBase[FieldsParameters.active_athlet].Activities.Count - 1;
                LoadAllTracks4Map(); // update track list
                //UpdateCalendar();
                Update_panel1(FieldsParameters.active_activity);
                Center_Track(GetActualTrack(Tracks4Map, FieldsParameters.active_activity));
                Invalidate();
            }
        } // Import of FitLog file

        private void DropDown_Import_Garmin_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor; // Wait-Cursor will be shown for time of writing process

                using (Garmin_Reader garmin_reader = new Garmin_Reader(SC_DataBase, FieldsParameters.active_athlet))
                {
                    List<activity> newActs = garmin_reader.Import_Garmin_FR305(FieldsParameters.SRTM_Data_Dir, FieldsParameters.API_key_Weather);
                    if (newActs.Any()) //prevent IndexOutOfRangeException for empty list - code snippet from code.grepper.com
                    {
                        foreach (activity _act in newActs)
                        {
                            SC_DataBase[FieldsParameters.active_athlet].Activities.Add(_act);
                        }
                    }
                    FieldsParameters.active_activity = SC_DataBase[FieldsParameters.active_athlet].Activities.Count - 1;
                    LoadAllTracks4Map(); // update track list
                    //UpdateCalendar();
                    Update_panel1(FieldsParameters.active_activity);
                    Center_Track(GetActualTrack(Tracks4Map, FieldsParameters.active_activity));
                    Invalidate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " @ Garmin Import Process");
            }

            finally
            {
                Cursor = Cursors.Default;
            }
        } // Import of Garmin FR305 data

        private void DropDown_Export_CSV_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor; // Wait-Cursor will be shown for time of writing process

                using (CSV_export csv_writer = new CSV_export(SC_DataBase, FieldsParameters.active_athlet, FieldsParameters.active_activity))
                {
                    csv_writer.CSV_Export("summary");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " @ CSV Export Process");
            }

            finally
            {
                Cursor = Cursors.Default;
            }
        } // Exporting summary of period activity.date -> today

        private void DropDown_Export_CSV_Track_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor; // Wait-Cursor will be shown for time of writing process

                using (CSV_export csv_writer = new CSV_export(SC_DataBase, FieldsParameters.active_athlet, FieldsParameters.active_activity))
                {
                    csv_writer.CSV_Export("track");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " @ CSV Export Process");
            }

            finally
            {
                Cursor = Cursors.Default;
            } // Exporting complete track data of active activity
        }

        private void FitlogExportActivityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SC_DataBase.Any()) //prevent IndexOutOfRangeException for empty list - code snippet from code.grepper.com
            {
                using (SaveFileDialog sfDlg = new SaveFileDialog())
                {
                    sfDlg.Filter = "fitlog Files (*.fitlog)|*.fitlog|All Files (*.*)|*.*";
                    if (sfDlg.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            Cursor = Cursors.WaitCursor; // Wait-Cursor will be shown for time of writing process

                            using (FitlogWriter writer = new FitlogWriter(new FileStream(sfDlg.FileName, FileMode.Create), SC_DataBase,
                                FieldsParameters.active_athlet, FieldsParameters.active_activity))
                            {
                                writer.Write_Ativity();
                            }
                        }

                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }

                        finally
                        {
                            Cursor = Cursors.Default;
                        }
                    }
                }
            }
        } // safe Activity as fitlog file

        private void GPXExportTrackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GpxWriter.SaveGPX_Track(SC_DataBase[FieldsParameters.active_athlet].Activities[FieldsParameters.active_activity]);
        }
    } // class Form1
} // namespace SC