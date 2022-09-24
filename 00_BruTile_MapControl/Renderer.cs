using BruTile;
using BruTile.Cache;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace SC
{
    class Renderer
    {
        private readonly Bitmap _canvas;
        private readonly Bitmap _canvas2; // Routing layer
        //private double _minX_Value;
        //private double _maxX_Value;

        public Renderer(Bitmap canvas, Bitmap canvas2)
        {
            _canvas = canvas;
            _canvas2 = canvas2;
        }

        public void Render(Viewport viewport, ITileSource tileSource, ITileCache<Tile<Image>> tileCache, List<Track4Map> _AllTracks, int _act_Activity)
        {
            var level = Utilities.GetNearestLevel(tileSource.Schema.Resolutions, viewport.UnitsPerPixel);
            var tileInfos = tileSource.Schema.GetTileInfos(viewport.Extent, level);

            // ploting map
            using (var g = Graphics.FromImage(_canvas))
            {
                foreach (var tileInfo in tileInfos)
                {
                    var extent = viewport.WorldToScreen(tileInfo.Extent.MinX, tileInfo.Extent.MinY,
                                                            tileInfo.Extent.MaxX, tileInfo.Extent.MaxY);
                    var tile = tileCache.Find(tileInfo.Index);
                    if (tile != null)
                    {
                        DrawTile(tileSource.Schema, g, (Bitmap)tile.Image, extent, tileInfo.Index.Level);
                    }
                }
            }

            // Plot track(s) or starting point(s)
            using (var h = Graphics.FromImage(_canvas2))
            {
                h.Clear(Color.Transparent);

                if (_AllTracks != null)
                {
                    Pen Linie = new Pen(Color.Blue);
                    Point Anfang = new Point();
                    Point Ende = new Point();
                    bool firstTP = true;
                    int arrow = 0;
                    Linie.Width = 2;
                    TrackPoint _tp_old = new TrackPoint();

                    if (!FieldsParameters.showAllTracks)
                    {
                        foreach (TrackPoint _tp in _AllTracks[_act_Activity].Track)
                        {
                            PointF Punkt;

                            if (_tp.lat != null || _tp.lon != null) // checked for invalide coordinates
                            {
                                if (firstTP)
                                {
                                    firstTP = false;
                                    Punkt = viewport.WorldToScreen(viewport.LongitudeToX(Convert.ToDouble(_tp.lon)), viewport.LatitudeToY(Convert.ToDouble(_tp.lat)));

                                    // Zeichne Track in pictureBox1.Imaqge ein
                                    Anfang.X = (int)Punkt.X;
                                    Anfang.Y = (int)Punkt.Y;
                                    _tp_old = _tp;
                                }
                                else
                                {
                                    arrow++;

                                    Punkt = viewport.WorldToScreen(viewport.LongitudeToX(Convert.ToDouble(_tp.lon)), viewport.LatitudeToY(Convert.ToDouble(_tp.lat)));

                                    Ende.X = (int)Punkt.X;
                                    Ende.Y = (int)Punkt.Y;

                                    if (FieldsParameters.maxX_Value > Convert.ToDouble(_tp.dist) / 1000 && FieldsParameters.minX_Value < Convert.ToDouble(_tp.dist) / 1000)
                                    {
                                        Linie.Color = Color.Red;
                                        Linie.Width = 3;
                                    }
                                    else
                                    {
                                        Linie.Color = Color.Blue;
                                        Linie.Width = 2;
                                    }

                                    if (((double)_tp.dist - (double)_tp_old.dist) > 700)
                                        {
                                        Linie.Color = Color.Transparent;
                                    }

                                    if (arrow > 20)
                                    {
                                        arrow = 0;
                                        Linie.Width = 5;
                                        Linie.EndCap = LineCap.ArrowAnchor;
                                        h.DrawLine(Linie, Anfang, Ende);
                                        Linie.EndCap = LineCap.Round;
                                        Linie.Width = 2;
                                    }
                                    else
                                    {
                                        h.DrawLine(Linie, Anfang, Ende);
                                    }

                                    Anfang = Ende;
                                    _tp_old = _tp;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (Track4Map _Track in _AllTracks)
                        {
                            Linie.Width = 2;
                            bool firstTP_all = true;
                            PointF Punkt;

                            if (_Track.Track.Any()) // check for empty list
                            {
                                // Filter for shown tracks
                                if (FieldsParameters.Filter_Year == " All Years" || _Track.ActivityTime.Year.ToString() == FieldsParameters.Filter_Year)
                                {
                                    if (FieldsParameters.Filter_Month == " All Months" || _Track.ActivityTime.Month.ToString("00") == FieldsParameters.Filter_Month)
                                    {
                                        if (FieldsParameters.Filter_Loc == " All Locations" || _Track.LocName == FieldsParameters.Filter_Loc)
                                        {
                                            if (FieldsParameters.Filter_Cat == " All Categories" || _Track.CatName == FieldsParameters.Filter_Cat)
                                            {
                                                foreach (TrackPoint _tp in _Track.Track)
                                                {
                                                    if (_tp.lat != null || _tp.lon != null) // checked for invalide coordinates
                                                    {
                                                        if (firstTP_all)
                                                        {
                                                            firstTP_all = false;
                                                            Punkt = viewport.WorldToScreen(viewport.LongitudeToX(Convert.ToDouble(_tp.lon)), viewport.LatitudeToY(Convert.ToDouble(_tp.lat)));

                                                            // Zeichne Track in pictureBox1.Imaqge ein
                                                            Anfang.X = (int)Punkt.X;
                                                            Anfang.Y = (int)Punkt.Y;
                                                            _tp_old = _tp;
                                                        }
                                                        else
                                                        {
                                                            Punkt = viewport.WorldToScreen(viewport.LongitudeToX(Convert.ToDouble(_tp.lon)), viewport.LatitudeToY(Convert.ToDouble(_tp.lat)));

                                                            Ende.X = (int)Punkt.X;
                                                            Ende.Y = (int)Punkt.Y;
                                                            if (_AllTracks.Count == 1)
                                                            {
                                                                if (FieldsParameters.maxX_Value > Convert.ToDouble(_tp.dist) / 1000 && FieldsParameters.minX_Value < Convert.ToDouble(_tp.dist) / 1000)
                                                                {
                                                                    Linie.Color = Color.Red;
                                                                    Linie.Width = 3;
                                                                }
                                                                else
                                                                {
                                                                    Linie.Color = Color.Blue;
                                                                    Linie.Width = 2;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                Linie.Width = 2;

                                                                switch (_Track.CatName)
                                                                {
                                                                    case "Rennrad":
                                                                        Linie.Color = Color.Red;
                                                                        break;
                                                                    case "CycloCross":
                                                                        Linie.Color = Color.OrangeRed;
                                                                        break;
                                                                    case "MTB":
                                                                        Linie.Color = Color.Orange;
                                                                        break;
                                                                    case "SmartBike":
                                                                        Linie.Color = Color.DarkRed;
                                                                        break;
                                                                    case "Wandern":
                                                                        Linie.Color = Color.Blue;
                                                                        break;
                                                                    case "Laufen":
                                                                        Linie.Color = Color.LightBlue;
                                                                        break;
                                                                }
                                                            }

                                                            if (((double)_tp.dist - (double)_tp_old.dist) > 700)
                                                            {
                                                                Linie.Color = Color.Transparent;
                                                            }

                                                            h.DrawLine(Linie, Anfang, Ende);

                                                            Anfang = Ende;
                                                            _tp_old = _tp;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (FieldsParameters.showStartPoints)
                    {
                        foreach (Track4Map _Track in _AllTracks)
                        {
                            Color col = Color.Blue;

                            if (_Track.Track.Any()) // check for empty list
                            {
                                // Filter for shown tracks
                                if (FieldsParameters.Filter_Year == " All Years" || _Track.ActivityTime.Year.ToString() == FieldsParameters.Filter_Year)
                                {
                                    //MessageBox.Show(FieldsParameters.Filter_Year);
                                    if (FieldsParameters.Filter_Month == " All Months" || _Track.ActivityTime.Month.ToString("00") == FieldsParameters.Filter_Month)
                                    {
                                        //MessageBox.Show(FieldsParameters.Filter_Month);
                                        if (FieldsParameters.Filter_Loc == " All Locations" || _Track.LocName == FieldsParameters.Filter_Loc)
                                        {
                                            //MessageBox.Show(FieldsParameters.Filter_Loc);
                                            if (FieldsParameters.Filter_Cat == " All Categories" || _Track.CatName == FieldsParameters.Filter_Cat)
                                            {
                                                switch (_Track.CatName)
                                                {
                                                    case "Rennrad":
                                                        col = Color.Red;
                                                        break;
                                                    case "CycloCross":
                                                        col = Color.OrangeRed;
                                                        break;
                                                    case "MTB":
                                                        col = Color.Orange;
                                                        break;
                                                    case "SmartBike":
                                                        col = Color.DarkRed;
                                                        break;
                                                    case "Wandern":
                                                        col = Color.Blue;
                                                        break;
                                                    case "Laufen":
                                                        col = Color.LightBlue;
                                                        break;
                                                }

                                                if (_Track.Track.Any())
                                                {
                                                    float radius = (float)(level * 0.4);
                                                    PointF Punkt = PointF.Empty;

                                                    int i = 0;
                                                    int j = 0;

                                                    while (j < 3)
                                                    {
                                                        if (_Track.Track[j].lat == null || _Track.Track[i].lon == null)
                                                        {
                                                            j++;
                                                            i++;
                                                        }
                                                        else
                                                        {
                                                            j = 3;
                                                            Punkt = viewport.WorldToScreen(viewport.LongitudeToX(Convert.ToDouble(_Track.Track[i].lon)),
                                                                viewport.LatitudeToY(Convert.ToDouble(_Track.Track[i].lat)));
                                                        }
                                                    }

                                                    if (Punkt != PointF.Empty)
                                                    {
                                                        h.FillEllipse(new SolidBrush(col), Punkt.X - radius, Punkt.Y - radius, radius + radius, radius + radius);
                                                        h.DrawEllipse(new Pen(Color.Black, 1), Punkt.X - radius, Punkt.Y - radius, radius + radius, radius + radius);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }                
                        }
                    }

                    
                }
            }
        }

        private static void DrawTile(ITileSchema schema, Graphics graphics, Bitmap bitmap, RectangleF extent, int level)
        {
            // For drawing on WinForms there are two things to take into account
            // to prevent seams between tiles.
            // 1) The WrapMode should be set to TileFlipXY. This is related
            //    to how pixels are rounded by GDI+
            var imageAttributes = new ImageAttributes();
            imageAttributes.SetWrapMode(WrapMode.TileFlipXY);
            // 2) The rectangle should be rounded to actual pixels.
            var roundedExtent = RoundToPixel(extent);
            graphics.DrawImage(bitmap, roundedExtent, 0, 0, schema.GetTileWidth(level), schema.GetTileHeight(level), GraphicsUnit.Pixel, imageAttributes);
        }

        private static Rectangle RoundToPixel(RectangleF dest)
        {
            // To get seamless aligning you need to round the 
            // corner coordinates to pixel.
            return new Rectangle(
                (int)Math.Round(dest.Left),
                (int)Math.Round(dest.Top),
                (int)(Math.Round(dest.Right) - Math.Round(dest.Left)),
                (int)(Math.Round(dest.Bottom) - Math.Round(dest.Top)));
        }
    }
}
