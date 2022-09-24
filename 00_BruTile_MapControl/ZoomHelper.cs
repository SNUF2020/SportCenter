using System.Collections.Generic;

namespace SC
{
    public static class ZoomHelper
    {
        public static double ZoomIn(IList<double> unitsPerPixelList, double unitsPerPixel)
        {
            if (unitsPerPixelList.Count == 0) return unitsPerPixel / 2.0;

            //smaller than smallest
            if (unitsPerPixelList[unitsPerPixelList.Count - 1] > unitsPerPixel) return unitsPerPixelList[unitsPerPixelList.Count - 1];

            for (int i = 0; i < unitsPerPixelList.Count; i++)
            {
                if (unitsPerPixelList[i] < unitsPerPixel)
                    return unitsPerPixelList[i];
            }
            return unitsPerPixelList[unitsPerPixelList.Count - 1];
        }

        public static double ZoomOut(IList<double> unitsPerPixelList, double unitsPerPixel)
        {
            if (unitsPerPixelList.Count == 0) return unitsPerPixel * 2.0;

            //bigger than biggest
            if (unitsPerPixelList[0] < unitsPerPixel) return unitsPerPixelList[0];

            for (int i = unitsPerPixelList.Count - 1; i >= 0; i--)
            {
                if (unitsPerPixelList[i] > unitsPerPixel)
                    return unitsPerPixelList[i];
            }
            return unitsPerPixelList[0];
        }

        public static double ZoomRoute(IList<double> unitsPerPixelList, double unitsPerPixel, double unitsPerPixel_Route)
        {
            if (unitsPerPixelList.Count == 0) return unitsPerPixel_Route; // Wenn Liste nicht vorhanden dann nimm die "krumme" Zahl

            if (unitsPerPixel_Route < unitsPerPixel) // program has to "zoom-in"
            {
                for (int i = 0; i < unitsPerPixelList.Count; i++)
                {
                    if (unitsPerPixelList[i] < unitsPerPixel_Route)
                        return unitsPerPixelList[i - 1];
                }
            }
            else // program has to "zoom-out"
            {
                for (int i = unitsPerPixelList.Count - 1; i > 0; i--)
                {
                    if (unitsPerPixelList[i] > unitsPerPixel_Route)
                        return unitsPerPixelList[i];
                }
            }
            return unitsPerPixelList[unitsPerPixelList.Count - 1];
        } // Method for finding the right zoom for given track(s) to scale ideal to given viewport
    }
}
