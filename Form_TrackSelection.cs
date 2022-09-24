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
    public partial class Form_TrackSelection : Form
    {
        private int numInput; // Definition of number of checkBoxes
        private CheckBox[] checkbox;
        private Label[] label1;
        private Label[] label2;
        private Label[] label3;

        private bool hand_over = false;

        private Data select = new Data();

        public Form_TrackSelection(int _numInput, Data _select)
        {
            InitializeComponent();

            this.select = _select;

            this.numInput = _numInput; // Legt Anzahl der Controls fest
            this.checkbox = new CheckBox[numInput];
            this.label1 = new Label[numInput];
            this.label2 = new Label[numInput];
            this.label3 = new Label[numInput];

            this.SuspendLayout();
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Progress_FormClosing);
            this.ResumeLayout(false);
        }
        private void Form_TrackSelection_Load(object sender, EventArgs e)
        {
            for (int i = 1; i < numInput; i++)
            {
                checkbox[i] = new CheckBox();
                checkbox[i].Height = 20;
                //checkbox[i].Width = 50;
                checkbox[i].AutoSize = true;
                checkbox[i].Location = new Point(50, 20 + i * 20);
                checkbox[i].Text = "Track #" + i.ToString() + ":";
                if (select.track[i].Check == true) checkbox[i].Checked = true;
                else checkbox[i].Checked = false;
                
                label1[i] = new Label();
                label1[i].AutoSize = true;
                label1[i].Height = 20;
                label1[i].Location = new Point(130, 20 + i * 20);
                label1[i].Text = select.track[i].ActivityTime.ToString();

                label2[i] = new Label();
                label2[i].AutoSize = true;
                label2[i].Height = 20;
                label2[i].Location = new Point(240, 20 + i * 20);
                label2[i].Text = (select.track[i].Distance / 1000).ToString("#0.00") + " km";

                label3[i] = new Label();
                label3[i].AutoSize = true;
                label3[i].Height = 20;
                label3[i].Location = new Point(300, 20 + i * 20);
                label3[i].Text = (select.track[i].Duration / 3600).ToString("0") + "h"
                    + ((select.track[i].Duration % 3600) / 60).ToString("0") + "min";

                checkbox[i].Tag = i;
                this.Controls.Add(checkbox[i]);
                this.Controls.Add(label1[i]);
                this.Controls.Add(label2[i]);
                this.Controls.Add(label3[i]);
            }
            this.Size = new Size(400, 120 + numInput * 20);
        }        

        private void Get_CheckedTracks()
        {
            for (int i = 1; i < numInput; i++)
            {
                if (checkbox[i].Checked)
                {
                    select.sel_tracks.Add(i);
                }
            }
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            Get_CheckedTracks();
            hand_over = true;
        }
        
        private void Form_Progress_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!hand_over)
            {
                DialogResult result = MessageBox.Show("Auswahl übergeben?", "Speicherbestätigung Logbuch", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

                switch (result)
                {
                    case DialogResult.Yes:
                        Get_CheckedTracks();
                        break;
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
        }

        
    }
}
