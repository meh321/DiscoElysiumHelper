using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiscoElysiumHelper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void UpdateApplications()
        {
            App.RemoveAll();

            int redType = 0;
            if (this.radioButton1.Checked)
                redType = 0;
            else if (this.radioButton2.Checked)
                redType = 1;
            else if (this.radioButton3.Checked)
                redType = -1;

            int whiteType = 0;
            if (this.radioButton6.Checked)
                whiteType = 0;
            else if (this.radioButton5.Checked)
                whiteType = 1;
            else if (this.radioButton4.Checked)
                whiteType = -1;

            int passType = 0;
            if (this.radioButton9.Checked)
                passType = 0;
            else if (this.radioButton8.Checked)
                passType = 1;
            else if (this.radioButton7.Checked)
                passType = -1;

            int fakeType = 0;
            if (this.radioButton12.Checked)
                fakeType = 0;
            else if (this.radioButton11.Checked)
                fakeType = 1;
            else if (this.radioButton10.Checked)
                fakeType = -1;

            int bubbleType = 0;
            if (this.radioButton15.Checked)
                bubbleType = 0;
            else if (this.radioButton14.Checked)
                bubbleType = 1;
            else if (this.radioButton13.Checked)
                bubbleType = -1;

            bool noLock = this.checkBox1.Checked;
            bool infMorale = this.checkBox2.Checked;

            switch(redType)
            {
                case 0: break;
                case 1: new App(0x130A908, "C6 40 50 01 EB 0C", "80 78 50 00 75 0C").Apply(); break;
                case -1: new App(0x130A908, "C6 40 50 00 90 90", "80 78 50 00 75 0C").Apply(); break;
            }

            switch(whiteType)
            {
                case 0: break;
                case 1: new App(0x11DB2C2, "C6 46 50 01 EB 47", "80 7E 50 00 75 47").Apply(); break;
                case -1: new App(0x11DB2C2, "C6 46 50 00 90 90", "80 7E 50 00 75 47").Apply(); break;
            }

            switch (passType)
            {
                case 0: break;
                case 1: new App(0x139507A, "8D 4B 1F", "8D 4B 06").Apply(); break;
                case -1: new App(0x139507A, "31 C9 90", "8D 4B 06").Apply(); break;
            }

            switch(fakeType)
            {
                case 0: break;
                case 1: new App(0x127EF17, "C6 40 50 01 EB 0C", "80 78 50 00 75 0C").Apply(); break;
                case -1: new App(0x127EF17, "C6 40 50 00 90 90", "80 78 50 00 75 0C").Apply(); break;
            }

            switch (bubbleType)
            {
                case 0: break;
                case 1:
                    new App(0x12A0AF8, "8D 4B 1F", "8D 4B 06").Apply(); // bubble IsAvailable
                    new App(0x129F70D, "8D 4B 1F", "8D 4B 06").Apply(); // bubble IsPassed
                    new App(0x12BFE30, "8D 4B 1F", "8D 4B 06").Apply(); // world object is clickable?
                    new App(0x1321E5D, "83 C0 1F", "83 C0 07").Apply(); // something to do with containers
                    break;
                case -1:
                    new App(0x12A0AF8, "31 C9 90", "8D 4B 06").Apply();
                    new App(0x129F70D, "31 C9 90", "8D 4B 06").Apply();
                    new App(0x12BFE30, "31 C9 90", "8D 4B 06").Apply();
                    new App(0x1321E5D, "31 C0 90", "83 C0 07").Apply();
                    break;
            }

            if (noLock)
                new App(0x127CCB6, "EB 28", "7F 28").Apply();

            if (infMorale)
                new App(0x3EDC9B, "90 E9 15 01 00 00", "0F 84 15 01 00 00").Apply();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.UpdateApplications();
        }
    }
}
