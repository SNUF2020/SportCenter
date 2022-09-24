using SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A30x.Interfaces;
using System;
using System.Collections.Generic;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A30x.Types
{
    public class A302<T, U> : IA302<T, U>
        where T : ITrkHdrType
        where U : ITrkPointType
    {
        private T hder;
        private IList<U> trkPts = new List<U>();

        public A302(T header, IList<U> trackPoints)
        {
            hder = header;
            trkPts = trackPoints;
        }

        public T Header
        {
            get { return hder; }
            set { hder = value; }
        }

        public IList<U> TrackPoints
        {
            get { return trkPts; }
            set { trkPts = value; }
        }
    }
}
