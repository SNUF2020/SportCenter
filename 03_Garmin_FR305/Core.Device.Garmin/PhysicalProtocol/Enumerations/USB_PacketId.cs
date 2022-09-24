using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.PhysicalProtocol.Enumerations
{
    public enum USB_PacketId : short
    {
        Pid_Data_Available = 2, //2
        Pid_Start_Session = 5, //5
        Pid_Session_Started = 6  //6   
    }
}
