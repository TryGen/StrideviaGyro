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

                /* kneeAngleDeg[i] = pitch * 180.0 / Math.PI;
                 thighAngleDeg[i] = (-kneeAngle[i] / 2 - Math.PI / 8) * 180.0 / Math.PI; */

                kneeAngleDeg[i] = pitch * (180.0 / Math.PI) + 85;
                thighAngleDeg[i] = 30 - kneeAngleDeg[i] / 2;
            }

            AngleChartsForm angleCharts = new AngleChartsForm(kneeAngleDeg, thighAngleDeg);

            
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

            ///Stiling the Buttons and Animation:
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
                    GetSides(out hip, out knee,out foot,out toe,out com, k);

                    toe = RotatePoint(R, toe);
                    toeTrail[k, 0] = toe[0];
                    toeTrail[k, 1] = toe[1];
                }

                lastUpdatedTrailFrame = Math.Max(lastUpdatedTrailFrame, i);

                GetSides(out hip, out knee, out foot, out toe, out com, i);


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

            void GetSides(out double[] hip, out double[] knee, out double[] foot, out double[] toe, out double[] com, int i)
            {
                double thighAngle = -kneeAngle[i] / 2 - Math.PI / 8;
                double shankAngle = kneeAngle[i] / 2 + Math.PI / 10;

                hip = new double[] { -0.1, 0 };

                knee = new double[]
                {
                     hip[0] + thighLen * Math.Cos(thighAngle),
                     hip[1] + thighLen * Math.Sin(thighAngle)
                };

              /*   double[] baseFoot =
                 {
                     knee[0] + shankLen * Math.Cos(thighAngle + shankAngle),
                     knee[1] + shankLen * Math.Sin(thighAngle + shankAngle)
                 };

                 double[] offset =
                 {
                    0.08 * Math.Sin(2 * Math.PI * i / kneeAngle.Length),
                    0.08 * Math.Cos(2 * Math.PI * i / kneeAngle.Length)
                  };

                 foot = new double[]
                 {
                     baseFoot[0] + offset[0],
                     baseFoot[1] + offset[1]
                 };
                */
                foot = new double[]
                {
                  knee[0] + shankLen * Math.Cos(thighAngle + shankAngle),
                  knee[1] + shankLen * Math.Sin(thighAngle + shankAngle)
                };

                toe = new double[]
                {
                  foot[0],
                  foot[1] + footLen
                };

                com = new double[]
                {
                    (hip[0] + knee[0] + foot[0]) / 3,
                   (hip[1] + knee[1] + foot[1]) / 3
                };

            }

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
