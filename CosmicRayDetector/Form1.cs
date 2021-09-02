using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace CosmicRayDetector
{
    public partial class Form1 : Form
    {
        //Stored values that are checked for Single Event Upsets (SEU)
        byte[] seuBytes = new byte[(800 * 800) * 3]; // total is 15,360,000 bits

        byte defByte = 111;

        Thread DrawThread = null;

        Graphics mDraw = null;

        Graphics g = null;

        Bitmap btm = new Bitmap(800, 800);

        bool drawResults = true;

        List<string> log = new List<string>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
     
        }

        public void Draw()
        {
            if (drawResults)
            {
                Bitmap btm = new Bitmap(Width, Height);

                mDraw = CreateGraphics();
                g = Graphics.FromImage(btm);

                g.Clear(Color.Black);

                mDraw.DrawImage(btm, Point.Empty);
            }

            while (true)
            {
                for(int i = 0; i < seuBytes.Length; i++)
                {
                    if(seuBytes[i] != defByte)
                    {
                        // potentially add a save feature
                        string status = "Detected Bit Flip (SEU) Value: " + seuBytes[i] + " " + DateTime.Now.ToString();
                        Console.WriteLine(status);
                        log.Add(status); // keep list of detections in a log for review later

                        if (drawResults)
                        {
                            int x = i % (btm.Width * 3); //calculate x change
                            int y = i / (btm.Width * 3); //calculate y change

                            int rgb = x % 3;

                            x /= 3;

                            byte r = 0;
                            byte g = 0;
                            byte b = 0;

                            if (rgb == 0)
                            {
                                r = seuBytes[i];
                                g = seuBytes[i + 1];
                                b = seuBytes[i + 2];
                            }
                            if (rgb == 1)
                            {
                                r = seuBytes[i - 1];
                                g = seuBytes[i];
                                b = seuBytes[i + 1];
                            }
                            if (rgb == 2)
                            {
                                r = seuBytes[i - 2];
                                g = seuBytes[i - 1];
                                b = seuBytes[i];
                            }

                            if (x >= 0 && x < btm.Width && y >= 0 && y < btm.Height)
                            {
                                btm.SetPixel(x, y, Color.FromArgb(r, g, b));
                            }
                            else
                            {
                                Console.WriteLine("Calculation Error"); 
                            }
                        }
                    }                    
                }

                if (drawResults)
                {
                    mDraw.DrawImage(btm, Point.Empty);
                }

                Thread.Sleep(2500); //not necesary to be constantly checking
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StartIt(true);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StartIt(false);
        }

        public void StartIt(bool drawRes)
        {
            drawResults = drawRes;

            button1.Visible = false;
            button2.Visible = false;

            numericUpDown1.Visible = false;

            if (drawRes)
            {
                seuBytes = new byte[(Width * Height) * 3];
            }
            else
            {
                seuBytes = new byte[(int)numericUpDown1.Value];
            }

            for (int x = 0; x < seuBytes.Length; x++)
            {
                seuBytes[x] = defByte; // make sure the array is zero'd out
            }

            DrawThread = new Thread(Draw);
            DrawThread.IsBackground = true;
            DrawThread.Start();
        }
    }
}
