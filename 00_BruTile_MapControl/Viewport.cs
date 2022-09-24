using BruTile;
using System.Drawing;

namespace SC
{
    class Viewport
    {
        private double _unitsPerPixel;
        private double _centerX;
        private double _centerY;
        private double _width;
        private double _height;
        Extent _extent;

        public double UnitsPerPixel
        {
            get { return _unitsPerPixel; }
            set
            {
                _unitsPerPixel = value;
                UpdateExtent();
            }
        }

        public Point Center
        {
            set
            {
                _centerX = value.X;
                _centerY = value.Y;
                UpdateExtent();
            }
        }

        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                UpdateExtent();
            }
        }

        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                UpdateExtent();
            }
        }

        public double CenterX
        {
            get { return _centerX; }
            set
            {
                _centerX = value;
                UpdateExtent();
            }
        }

        public double CenterY
        {
            get { return _centerY; }
            set
            {
                _centerY = value;
                UpdateExtent();
            }
        }

        public Extent Extent
        {
            get { return (_extent == default(Extent)) ? (_extent = new Extent(0, 0, 0, 0)) : _extent; }
        }

        public PointF WorldToScreen(Point worldPosition)
        {
            return WorldToScreen(worldPosition.X, worldPosition.Y);
        }

        public RectangleF WorldToScreen(double x1, double y1, double x2, double y2)
        {
            var point1 = WorldToScreen(x1, y1);
            var point2 = WorldToScreen(x2, y2);
            return new RectangleF(point1.X, point2.Y, point2.X - point1.X, point1.Y - point2.Y);
        }

        public PointF ScreenToWorld(Point screenPosition)
        {
            return ScreenToWorld(screenPosition.X, screenPosition.Y);
        }

        public PointF WorldToScreen(double worldX, double worldY)
        {
            return new PointD((worldX - _extent.MinX) / _unitsPerPixel, (_extent.MaxY - worldY) / _unitsPerPixel);
        }

        public PointD ScreenToWorld(double screenX, double screenY)
        {
            return new PointD((_extent.MinX + screenX * _unitsPerPixel), (_extent.MaxY - (screenY * _unitsPerPixel)));
        }

        //
        // Mercator Projection for Latitude -> Y - from https://gist.github.com/nagasudhirpulla/9b5a192ccaca3c5992e5d4af0d1e6dc4
        // 
        public double LatitudeToY(double latitude)
        {
            return (System.Math.Log(System.Math.Tan((latitude + 90) / 360 * System.Math.PI)) / System.Math.PI * 180) * 20037508 / 180;
        }// Höhere Genauigkeit? In StaticMap-Code wird 20037508.342789 verwendet

        public double LongitudeToX(double longitude)
        {
            return longitude * 20037508.342789 / 180;
        }// Höhere Genauigkeit? In StaticMap-Code wird 20037508.342789 verwendet

        //
        // Mercator Projection for Y -> Latitude - from https://wiki.openstreetmap.org/wiki/Mercator
        //
        public double YToLatitude(double y)
        {
            return System.Math.Atan(System.Math.Exp((y / 20037508 * 180) / 180 * System.Math.PI)) / System.Math.PI * 360 - 90;
        }// Höhere Genauigkeit? In StaticMap-Code wird 20037508.342789 verwendet

        public double XToLongituide(double x)
        {
            return x / 20037508 * 180;
        }// Höhere Genauigkeit? In StaticMap-Code wird 20037508.342789 verwendet -> Im Moment kein Hinweis für höhere Genauigkeit

        //
        // -----------------------------------------------------------------------------------------------------------------
        //

        public void Transform(double screenX, double screenY, double previousScreenX, double previousScreenY, double deltaScale = 1)
        {
            var previous = ScreenToWorld(previousScreenX, previousScreenY);
            var current = ScreenToWorld(screenX, screenY);

            var newX = CenterX + previous.X - current.X;
            var newY = CenterY + previous.Y - current.Y;

            // When you pinch zoom outside the center of the map 
            // this will also affect the new center. 
            var scaleCorrectionX = (1 - deltaScale) * (current.X - CenterX);
            var scaleCorrectionY = (1 - deltaScale) * (current.Y - CenterY);

            UnitsPerPixel = UnitsPerPixel / deltaScale;
            CenterX = newX - scaleCorrectionX;
            CenterY = newY - scaleCorrectionY;
        }

        private void UpdateExtent()
        {
            double spanX = _width * _unitsPerPixel;
            double spanY = _height * _unitsPerPixel;
            _extent = new Extent(
                CenterX - spanX * 0.5, CenterY - spanY * 0.5,
                CenterX + spanX * 0.5, CenterY + spanY * 0.5);
        }

        public struct PointD
        {
            private bool _isSet;
            private double _x;

            public double X
            {
                get { return _x; }
                set
                {
                    _x = value;
                    _isSet = true;
                }
            }

            private double _y;

            public double Y
            {
                get { return _y; }
                set
                {
                    _y = value;
                    _isSet = true;
                }
            }

            public PointD(double x, double y)
            {
                _x = x;
                _y = y;
                _isSet = false;
            }

            public bool IsEmpty { get { return !_isSet; } }

            public static implicit operator PointD(PointF pd)
            {
                return new PointD(pd.X, pd.Y);
            }

            public static implicit operator PointF(PointD pd)
            {
                return new PointF((float)pd.X, (float)pd.Y);
            }
        }
    }
}
