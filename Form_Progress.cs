using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SC
{
    // Simple form only for inform user program is running and fetching data from Garmin FR305
    // If other devices used in SC program the Form.text needs to be adapted :-)

    public partial class Form_Progress : Form
    {
        public Form_Progress() 
        {
            InitializeComponent();
        }
    }
}
