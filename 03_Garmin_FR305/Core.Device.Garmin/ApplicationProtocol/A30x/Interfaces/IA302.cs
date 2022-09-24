using System;
using System.Collections.Generic;


namespace SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A30x.Interfaces
{
    public interface IA302<T, U>
       where T : ITrkHdrType
       where U : ITrkPointType
    {
        T Header { get; set; }
        IList<U> TrackPoints { get; set; }
    }
}
