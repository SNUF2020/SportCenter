using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC._03_Garmin_FR305.Core.Device.Garmin.ApplicationProtocol.A001.Enumerations
{
    public enum Protocol_Data_Type_Tag
    {
        Tag_Phys_Prot_Id = 'P',  /* tag for Physical protocol ID */
        Tag_Link_Prot_Id = 'L',  /* tag for Link protocol ID */
        Tag_Appl_Prot_Id = 'A',  /* tag for Application protocol ID */
        Tag_Data_Type_Id = 'D'   /* tag for Data Type ID */
    }
}
