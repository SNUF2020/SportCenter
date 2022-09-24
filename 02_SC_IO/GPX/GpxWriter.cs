using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SC
{
    public class GpxWriter
    {
        public static void SaveGPX_Track(activity _act)
        {
            if (_act.Track != null)
            {
                using (SaveFileDialog sFDlg = new SaveFileDialog())
                {
                    sFDlg.Filter = "Save Track as GPX-file|*.gpx";
                    sFDlg.Title = "Save Track File";

                    if (sFDlg.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            //Cursor = Cursors.WaitCursor; // Wait-Cursor will be shown for time of writing process

                            XmlWriterSettings settings = new XmlWriterSettings();
                            settings.Indent = true;
                            settings.NewLineOnAttributes = true;

                            XmlWriter writer = XmlWriter.Create(@sFDlg.FileName, settings);

                            // Start-Element/-Attribute "gpx": code snippet from https://github.com/macias/Gpx/tree/master/Gpx 
                            const string GPX_VERSION = "1.1";
                            const string GPX_CREATOR = "SportCenter";
                            const string GARMIN_EXTENSIONS_PREFIX = "gpxx";
                            const string GARMIN_WAYPOINT_EXTENSIONS_PREFIX = "gpxwpx";
                            const string GARMIN_TRACKPOINT_EXTENSIONS_V2_PREFIX = "gpxtpx";
                            const string DLG_EXTENSIONS_PREFIX = "dlg";
                            const string GPX_NAMESPACE = "http://www.topografix.com/GPX/1/1";
                            const string GARMIN_EXTENSIONS_NAMESPACE = "http://www.garmin.com/xmlschemas/GpxExtensions/v3";
                            // const string GARMIN_TRACKPOINT_EXTENSIONS_V1_NAMESPACE = "http://www.garmin.com/xmlschemas/TrackPointExtension/v1";
                            const string GARMIN_TRACKPOINT_EXTENSIONS_V2_NAMESPACE = "http://www.garmin.com/xmlschemas/TrackPointExtension/v2";
                            const string GARMIN_WAYPOINT_EXTENSIONS_NAMESPACE = "http://www.garmin.com/xmlschemas/WaypointExtension/v1";
                            const string DLG_EXTENSIONS_NAMESPACE = "http://dlg.krakow.pl/gpx/extensions/v1";

                            writer.WriteStartElement("gpx", GPX_NAMESPACE);
                            writer.WriteAttributeString("version", GPX_VERSION);
                            writer.WriteAttributeString("creator", GPX_CREATOR);
                            writer.WriteAttributeString("xmlns", GARMIN_EXTENSIONS_PREFIX, null, GARMIN_EXTENSIONS_NAMESPACE);
                            writer.WriteAttributeString("xmlns", GARMIN_WAYPOINT_EXTENSIONS_PREFIX, null, GARMIN_WAYPOINT_EXTENSIONS_NAMESPACE);
                            writer.WriteAttributeString("xmlns", GARMIN_TRACKPOINT_EXTENSIONS_V2_PREFIX, null, GARMIN_TRACKPOINT_EXTENSIONS_V2_NAMESPACE);
                            writer.WriteAttributeString("xmlns", DLG_EXTENSIONS_PREFIX, null, DLG_EXTENSIONS_NAMESPACE);

                            writer.WriteStartElement("trk");

                            writer.WriteStartElement("trkseg");


                            for (int i = 0; i < _act.Track.Count; i++)
                            {
                                
                                    writer.WriteStartElement("trkpt");
                                    writer.WriteAttributeString("lat", _act.Track[i].lat.ToString().Replace(',', '.'));
                                    writer.WriteAttributeString("lon", _act.Track[i].lon.ToString().Replace(',', '.'));

                                    writer.WriteStartElement("ele");
                                    writer.WriteString(_act.Track[i].ele.ToString().Replace(',', '.'));
                                    writer.WriteEndElement(); // ele

                                    writer.WriteStartElement("time");
                                    // track time = seconds from track beginning - convert to start time
                                    writer.WriteString(_act.ActivityTime.AddSeconds((double)_act.Track[i].tm).ToString("yyyy-MM-ddTHH':'mm':'ss.FFFZ"));
                                    writer.WriteEndElement(); // time                                    

                                    writer.WriteEndElement(); // trkpt
                                
                            }

                            writer.WriteEndElement(); // trkseg

                            writer.WriteEndElement(); // trk

                            writer.WriteEndElement(); // gpx

                            // Write the XML and close the writer.
                            writer.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            //Cursor = Cursors.Default;
                        }
                    }
                }
            }
        }
    }
}
