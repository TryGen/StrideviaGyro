using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace StrideviaGyroIntake
{
    public partial class Animation : Form
    {
        private Button playButton;
        private TrackBar frameSlider;
        private bool isPlaying = false;
        private int currentFrame = 0;

     
        private double[,] toeTrail;
        private double thighLen = 0.5;
        private double shankLen = 0.5;
        private double footLen = 0.1;
        private double[,] R;

        public Animation()
        {
            InitializeComponent();

            DataIntake data = new DataIntake();

            data.getRawData();

            /*   DataIntake data = new DataIntake();
               data.getRawData();*/

            int N = DataIntake.size;
            double[] kneeAngle = new double[N];
            double[] kneeAngleDeg = new double[N];
            double[] thighAngleDeg = new double[N];
         //   double[] footToeAngleDeg = new double[N];


            double[] rawKneeDeg = new double[N];
            double[] rawThighDeg = new double[N];
         //   double[] rawFootToeDeg = new double[N];

        

            int lastUpdatedTrailFrame = -1;

            for (int i = 0; i < N; i++)
            {
                double w = DataIntake.gyroData[i].normalized.w;
                double y = DataIntake.gyroData[i].normalized.y;
                double x = DataIntake.gyroData[i].normalized.x;
                double z = DataIntake.gyroData[i].normalized.z;

                double sinp = 2 * (w * y - x * z);

                double pitch = Math.Abs(sinp) >= 1 ? Math.Sign(sinp) * Math.PI / 2 : Math.Asin(sinp);

                kneeAngle[i] = pitch;


                double dorsiflexionDeg = pitch * (180.0 / Math.PI);

                double thighDeg = 180 - dorsiflexionDeg / 2 - 200;
                double shankDeg = dorsiflexionDeg / 2 + 10;

                rawThighDeg[i] = thighDeg;
                rawKneeDeg[i] = shankDeg;


                double footAngle =dorsiflexionDeg;

               // rawFootToeDeg[i] = footAngle;
            }

            double thighStart = rawThighDeg[0];
            double kneeStart = rawKneeDeg[0];
           // double footStart = rawFootToeDeg[0];

            float scale_factor = 1f;

            double offset = 20;

            for (int i = 0; i < N; i++)
            {
                thighAngleDeg[i] = (rawThighDeg[i] - thighStart + offset + 4) * scale_factor;
                kneeAngleDeg[i] = (rawKneeDeg[i] - kneeStart + offset) * scale_factor;
             //   footToeAngleDeg[i] = (rawFootToeDeg[i] - footStart + offset) * scale_factor;
            }

          

            AngleChartsForm angleCharts = new AngleChartsForm(kneeAngleDeg, thighAngleDeg/*,footToeAngleDeg*/);

            
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(-5, -5); // You can adjust this

            this.Width -= 150;

          //  angleCharts.Width -= 90;
            
            //Need to do this here because it's dependent on the sizes of the Animation Form.
            angleCharts.StartPosition = FormStartPosition.Manual;
            angleCharts.Location = new Point(this.Location.X + this.Width - 16, this.Location.Y); 

            angleCharts.Show();


            toeTrail = new double[N, 2];

            double frameTime = 1.0 / 20.0;
            double theta = 3 * Math.PI / 2;
            R = new double[2, 2]
            {
                { Math.Cos(theta), -Math.Sin(theta) },
                { Math.Sin(theta),  Math.Cos(theta) }
            };

            Chart chart1 = new Chart { Dock = DockStyle.Fill };
           

            chart1.ChartAreas.Add(new ChartArea());

            frameSlider = new TrackBar
            {
                Dock = DockStyle.Top,
                Minimum = 0,
                Maximum = N - 1,
                TickStyle = TickStyle.None
            };

            playButton = new Button
            {
                Text = "Play",
                Dock = DockStyle.Top
            };

            playButton.Click += (s, e) =>
            {
                isPlaying = !isPlaying;
                playButton.Text = isPlaying ? "Pause" : "Play";
            };

            frameSlider.Scroll += (s, e) =>
            {
                isPlaying = false;
                playButton.Text = "Play";
                currentFrame = frameSlider.Value;
                UpdateCharts(currentFrame);
            };

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

    
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Chart

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // Slider

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));  // Play button


            chart1.Margin = new Padding(0);
            frameSlider.Margin = new Padding(0);
            playButton.Margin = new Padding(0);

            
            playButton.Dock = DockStyle.Fill;
            playButton.Font = new Font(playButton.Font.FontFamily, 10, FontStyle.Bold);

            
            layout.Controls.Add(chart1, 0, 0);
            layout.Controls.Add(frameSlider, 0, 1);
            layout.Controls.Add(playButton, 0, 2);

            this.Controls.Add(layout);


            async void UpdateCharts(int i)
            {

                double[] hip, knee, foot, toe, com;

                for (int k = lastUpdatedTrailFrame + 1; k <= i; k++)
                {
                    GetSides(out hip, out knee, out foot, out toe, out com, k,
                    DataIntake.gyroData[k].normalized.w,
                    DataIntake.gyroData[k].normalized.y,
                     DataIntake.gyroData[k].normalized.x,
                     DataIntake.gyroData[k].normalized.z);

                    toe = RotatePoint(R, toe);
                    toeTrail[k, 0] = toe[0];
                    toeTrail[k, 1] = toe[1];
                }

                lastUpdatedTrailFrame = Math.Max(lastUpdatedTrailFrame, i);

                GetSides(out hip, out knee, out foot, out toe, out com, i,
                    DataIntake.gyroData[i].normalized.w,
                    DataIntake.gyroData[i].normalized.y,
                     DataIntake.gyroData[i].normalized.x,
                     DataIntake.gyroData[i].normalized.z);



                hip = RotatePoint(R, hip);
                knee = RotatePoint(R, knee);
                foot = RotatePoint(R, foot);
                toe = RotatePoint(R, toe);
                com = RotatePoint(R, com);


                double[] length = new double[]
                {
                    Math.Pow(foot[0] - knee[0] , 2),
                    Math.Pow(foot[1] - knee[1] , 2)
                };

                Console.WriteLine(Math.Sqrt(length[0] + length[1]));

             
                chart1.Series.Clear();
                AddLineSeries(chart1, "Hip-Knee", hip, knee, Color.Blue,3);
                AddLineSeries(chart1, "Knee-Foot", knee, foot, Color.Red,3);
                AddLineSeries(chart1, "Foot-Toe", foot, toe, Color.Black,3);
                AddPointSeries(chart1, "Hip", hip, Color.Black);
                AddPointSeries(chart1, "Knee", knee, Color.Black);
                AddPointSeries(chart1, "Foot", foot, Color.Black);
                AddPointSeries(chart1, "Toe", toe, Color.Black);
                AddPointSeries(chart1, "CoM", com, Color.Magenta);

                AddGroundLine(chart1, R);
                if (i > 0) AddTrailSeries(chart1, toeTrail, i, Color.Green);

                chart1.ChartAreas[0].AxisX.Minimum = -1;
                chart1.ChartAreas[0].AxisX.Maximum = 1;
                chart1.ChartAreas[0].AxisY.Minimum = -1;
                chart1.ChartAreas[0].AxisY.Maximum = 1;

                chart1.ChartAreas[0].AxisX.Interval = 1;
                chart1.ChartAreas[0].AxisY.Interval = 1;

                chart1.Titles.Clear();


                Title title = new Title($"Frame {i + 1} / {kneeAngle.Length}", Docking.Top, new Font("Arial", 12, FontStyle.Bold), Color.Black);
                chart1.Titles.Add(title);



                angleCharts.UpdateCharts(i);
              
            }

            UpdateCharts(0);

            Task.Run(async () =>
            {
                while (true)
                {
                    if (isPlaying)
                    {
                        if (currentFrame >= N) currentFrame = 0;

                        Invoke(new MethodInvoker(() =>
                        {
                            frameSlider.Value = currentFrame;
                            UpdateCharts(currentFrame);
                        }));

                        currentFrame++;
                        await Task.Delay((int)(frameTime * 1000));
                    }
                    else
                    {
                        await Task.Delay(100);
                    }
                }
            });




            void GetSides(
             out double[] hip, out double[] knee, out double[] foot, out double[] toe, out double[] com,
             int i, double w, double x, double y, double z)
            {
                double dorsiflexion = GetDorsiflexionAngleFromQuaternion(w, x, y, z);

                double thighAngle = -kneeAngle[i] / 2 - Math.PI / 8;
                double shankAngle = kneeAngle[i] / 2 + Math.PI / 10;

                hip = new double[] { -0.1, 0 };

                knee = new double[]
                {
                   hip[0] + thighLen * Math.Cos(thighAngle),
                   hip[1] + thighLen * Math.Sin(thighAngle)
                };

                foot = new double[]
                {
                  knee[0] + shankLen * Math.Cos(thighAngle + shankAngle),
                  knee[1] + shankLen * Math.Sin(thighAngle + shankAngle)
                };

                double footAngle = thighAngle + shankAngle + (Math.PI / 2 + dorsiflexion);

                toe = new double[]
                {
                  foot[0] + footLen * Math.Cos(footAngle),
                  foot[1] + footLen * Math.Sin(footAngle)
                };

                com = new double[]
                {
                 (hip[0] + knee[0] + foot[0]) / 3.0,
                 (hip[1] + knee[1] + foot[1]) / 3.0
                };
            }


        }

        double GetDorsiflexionAngleFromQuaternion(double w, double x, double y, double z)
        {

            double sinp = 2.0 * (w * y - x * z);
            if (Math.Abs(sinp) >= 1)
                return (sinp >= 0 ? 1 : -1) * (Math.PI / 2);
            else
                return Math.Asin(sinp);
        }

       
        void QuaternionToEulerYZX(double w, double x, double y, double z, out double yaw, out double pitch, out double roll)
        {
            
            double R11 = 1 - 2 * (y * y + z * z);
            double R12 = 2 * (x * y - z * w);
            double R13 = 2 * (x * z + y * w);
            double R21 = 2 * (x * y + z * w);
            double R22 = 1 - 2 * (x * x + z * z);
            double R23 = 2 * (y * z - x * w);
            double R31 = 2 * (x * z - y * w);
            double R32 = 2 * (y * z + x * w);
            double R33 = 1 - 2 * (x * x + y * y);

           
            pitch = Math.Asin(-R31); 
            roll = Math.Atan2(R32, R33); 
            yaw = Math.Atan2(R21, R11);
        }

        

        static double[] RotatePoint(double[,] R, double[] point)
        {
            return new double[]
            {
                R[0, 0] * point[0] + R[0, 1] * point[1],
                R[1, 0] * point[0] + R[1, 1] * point[1]
            };
        }

        static void AddLineSeries(Chart chart, string name, double[] point1, double[] point2, Color color, int width)
        {
            var series = new Series(name) { ChartType = SeriesChartType.Line, Color = color, BorderWidth = width};
            series.Points.AddXY(point1[0], point1[1]);
            series.Points.AddXY(point2[0], point2[1]);
            chart.Series.Add(series);
        }

        static void AddPointSeries(Chart chart, string name, double[] point, Color color)
        {
            var series = new Series(name) { ChartType = SeriesChartType.Point, Color = color, MarkerSize = 10 };
            series.Points.AddXY(point[0], point[1]);
            chart.Series.Add(series);
        }

        static void AddGroundLine(Chart chart, double[,] R)
        {
            double[] start = RotatePoint(R, new double[] { -2, 0 });
            double[] end = RotatePoint(R, new double[] { 2, 0 });
            AddLineSeries(chart, "Ground", start, end, Color.Black , 1);
        }

        static void AddTrailSeries(Chart chart, double[,] toeTrail, int count, Color color)
        {
            var series = new Series("Toe Trail") { ChartType = SeriesChartType.Line, Color = color };
            for (int i = 0; i <= count; i++)
                series.Points.AddXY(toeTrail[i, 0], toeTrail[i, 1]);
            chart.Series.Add(series);
        }

        
    }
}
