using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SC._01_SRTM_Data
{
    class TrackHelpers
    {
        public static double Get_Max_Lat(List<List<TrackPoint>> _AllRoutes)
        {
            double max_Lat = Convert.ToDouble(_AllRoutes[0][0].lat);
            for (int i = 0; i < _AllRoutes.Count; i++)
            {
                for (int k = 0; k < _AllRoutes[i].Count; k++)
                {
                    if (Convert.ToDouble(_AllRoutes[i][k].lat) > max_Lat)
                        max_Lat = Convert.ToDouble(_AllRoutes[i][k].lat);
                }
            }
            return max_Lat;
        }

        public static double Get_Min_Lat(List<List<TrackPoint>> _AllRoutes)
        {
            double min_Lat = double.MaxValue;

            for (int i = 0; i < _AllRoutes.Count; i++)
            {
                for (int k = 0; k < _AllRoutes[i].Count; k++)
                {
                    if (_AllRoutes[i][k].lat != null)
                    {
                        if (Convert.ToDouble(_AllRoutes[i][k].lat) < min_Lat)
                            min_Lat = Convert.ToDouble(_AllRoutes[i][k].lat);
                    }
                }
            }
            return min_Lat;
        }

        public static double Get_Max_Lon(List<List<TrackPoint>> _AllRoutes)
        { 
            double max_Lon = Convert.ToDouble(_AllRoutes[0][0].lon);
                
            for (int i = 0; i < _AllRoutes.Count; i++)   
            {
                for (int k = 0; k < _AllRoutes[i].Count; k++)   
                {
                    if (Convert.ToDouble(_AllRoutes[i][k].lon) > max_Lon)    
                        max_Lon = Convert.ToDouble(_AllRoutes[i][k].lon);   
                }    
            }
            return max_Lon;
        }

        public static double Get_Min_Lon(List<List<TrackPoint>> _AllRoutes)
        {
            double min_Lon = double.MaxValue;
            for (int i = 0; i < _AllRoutes.Count; i++)
            {
                for (int k = 0; k < _AllRoutes[i].Count; k++)
                {
                    if (_AllRoutes[i][k].lon != null)
                    {
                        if (Convert.ToDouble(_AllRoutes[i][k].lon) < min_Lon)
                            min_Lon = Convert.ToDouble(_AllRoutes[i][k].lon);
                    }
                }
            }
            return min_Lon;
        }

        public static int? check4NegativeValue(int? _value)
        {
            if (_value > 9000)
            {
                _value = 0;
            }
            return _value;
        }
        // Method to get rid of wrong hight values (negativ elevation and "int" format are not a good combination...)
        // Important if track is near sea level

        public static bool IsPointInPolygon4(Point[] polygon, Point testPoint)
        {
            bool result = false;
            int j = polygon.Length - 1;
            for (int i = 0; i < polygon.Length; i++)
            {
                if (polygon[i].Y < testPoint.Y && polygon[j].Y >= testPoint.Y || polygon[j].Y < testPoint.Y && polygon[i].Y >= testPoint.Y)
                {
                    if (polygon[i].X + (testPoint.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < testPoint.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }

        public static bool IsPointInPolygon4(PointF[] polygon, Point testPoint)
        {
            bool result = false;
            int j = polygon.Length - 1;
            for (int i = 0; i < polygon.Length; i++)
            {
                if (polygon[i].Y < testPoint.Y && polygon[j].Y >= testPoint.Y || polygon[j].Y < testPoint.Y && polygon[i].Y >= testPoint.Y)
                {
                    if (polygon[i].X + (testPoint.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < testPoint.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }

        // Find the polygon's centroid. -> http://csharphelper.com/blog/2014/07/find-the-centroid-of-a-polygon-in-c/
        public static PointF FindCentroid(Point[] _shape_Reg)
        {
            // Add the first point at the end of the array.
            int num_points = _shape_Reg.Length;
            Point[] pts = new Point[num_points + 1];
            _shape_Reg.CopyTo(pts, 0);
            pts[num_points] = _shape_Reg[0];

            // Find the centroid.
            float X = 0;
            float Y = 0;
            float second_factor;
            for (int i = 0; i < num_points; i++)
            {
                second_factor =
                    pts[i].X * pts[i + 1].Y -
                    pts[i + 1].X * pts[i].Y;
                X += (pts[i].X + pts[i + 1].X) * second_factor;
                Y += (pts[i].Y + pts[i + 1].Y) * second_factor;
            }

            // Divide by 6 times the polygon's area.
            float polygon_area = SignedPolygonArea(_shape_Reg);
            X /= (6 * polygon_area);
            Y /= (6 * polygon_area);

            // If the values are negative, the polygon is
            // oriented counterclockwise so reverse the signs.
            if (X < 0)
            {
                X = -X;
                Y = -Y;
            }
            return new PointF(X, Y);
        }

        // Return the polygon's area in "square units."
        // The value will be negative if the polygon is
        // oriented clockwise.

        private static float SignedPolygonArea(Point[] _shape_Reg)
        {
            // Add the first point to the end.
            int num_points = _shape_Reg.Length;
            Point[] pts = new Point[num_points + 1];
            _shape_Reg.CopyTo(pts, 0);
            pts[num_points] = _shape_Reg[0];

            // Get the areas.
            float area = 0;
            for (int i = 0; i < num_points; i++)
            {
                area +=
                    (pts[i + 1].X - pts[i].X) *
                    (pts[i + 1].Y + pts[i].Y) / 2;
            }

            // Return the result.
            return area;
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
    }
}

