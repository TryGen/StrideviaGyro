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

            int N = data.size;
            double[] kneeAngle = new double[N];
            double[] kneeAngleDeg = new double[N];
            double[] thighAngleDeg = new double[N];

            int lastUpdatedTrailFrame = -1;

            for (int i = 0; i < N; i++)
            {
                double w = data.gyroData[i].normalized.w;
                double y = data.gyroData[i].normalized.y;
                double x = data.gyroData[i].normalized.x;
                double z = data.gyroData[i].normalized.z;

                double sinp = 2 * (w * y - x * z);
                double pitch = Math.Abs(sinp) >= 1 ? Math.Sign(sinp) * Math.PI / 2 : Math.Asin(sinp);

                kneeAngle[i] = pitch;

                /* kneeAngleDeg[i] = pitch * 180.0 / Math.PI;
                 thighAngleDeg[i] = (-kneeAngle[i] / 2 - Math.PI / 8) * 180.0 / Math.PI; */

                kneeAngleDeg[i] = pitch * (180.0 / Math.PI) + 85;
                thighAngleDeg[i] = 30 - kneeAngleDeg[i] / 2;
            }

            toeTrail = new double[N, 2];

            double frameTime = 1.0 / 20.0;
            double theta = 3 * Math.PI / 2;
            R = new double[2, 2]
            {
                { Math.Cos(theta), -Math.Sin(theta) },
                { Math.Sin(theta),  Math.Cos(theta) }
            };

            Chart chart1 = new Chart { Dock = DockStyle.Fill };
            Chart chart2 = new Chart { Dock = DockStyle.Fill };
            Chart chart3 = new Chart { Dock = DockStyle.Fill };
            chart1.ChartAreas.Add(new ChartArea());
            chart2.ChartAreas.Add(new ChartArea());
            chart3.ChartAreas.Add(new ChartArea());

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
                RowCount = 5,
                ColumnCount = 1
            };

            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 13F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 13F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            layout.Controls.Add(chart1, 0, 0);
            layout.Controls.Add(chart2, 0, 1);
            layout.Controls.Add(chart3, 0, 2);
            layout.Controls.Add(frameSlider, 0, 3);
            layout.Controls.Add(playButton, 0, 4);

            this.Controls.Add(layout);

            async void UpdateCharts(int i)
            {
            
                for (int k = lastUpdatedTrailFrame + 1; k <= i; k++)
                {
                    double thighAngle = -kneeAngle[k] / 2 - Math.PI / 8;
                    double shankAngle = kneeAngle[k] / 2 + Math.PI / 10;

                    double[] hip = new double[] { -0.1, 0 };

                    double[] knee =
                    {
                       hip[0] + thighLen * Math.Cos(thighAngle),
                       hip[1] + thighLen * Math.Sin(thighAngle)
                    };

                    double[] baseFoot =
                    {
                     knee[0] + shankLen * Math.Cos(thighAngle + shankAngle),
                     knee[1] + shankLen * Math.Sin(thighAngle + shankAngle)
                    };

                    double[] offset =
                    {
                     0.08 * Math.Sin(2 * Math.PI * k / kneeAngle.Length),
                     0.08 * Math.Cos(2 * Math.PI * k / kneeAngle.Length)
                    };

                    double[] foot =
                    {
                      baseFoot[0] + offset[0],
                      baseFoot[1] + offset[1]
                    };

                    double[] toe =
                    {
                     foot[0],
                     foot[1] + footLen
                    };

                    toe = RotatePoint(R, toe);
                    toeTrail[k, 0] = toe[0];
                    toeTrail[k, 1] = toe[1];
                }

                lastUpdatedTrailFrame = Math.Max(lastUpdatedTrailFrame, i);

                
                double thighAngleNow = -kneeAngle[i] / 2 - Math.PI / 8;
                double shankAngleNow = kneeAngle[i] / 2 + Math.PI / 10;

                double[] hipNow = new double[] { -0.1, 0 };

                double[] kneeNow =
                {
                 hipNow[0] + thighLen * Math.Cos(thighAngleNow),
                 hipNow[1] + thighLen * Math.Sin(thighAngleNow)
                };

                double[] baseFootNow =
                {
                  kneeNow[0] + shankLen * Math.Cos(thighAngleNow + shankAngleNow),
                   kneeNow[1] + shankLen * Math.Sin(thighAngleNow + shankAngleNow)
                 };

                double[] offsetNow =
                {
                   0.08 * Math.Sin(2 * Math.PI * i / kneeAngle.Length),
                   0.08 * Math.Cos(2 * Math.PI * i / kneeAngle.Length)
                };

                double[] footNow =
                {
                   baseFootNow[0] + offsetNow[0],
                   baseFootNow[1] + offsetNow[1]
                };

                double[] toeNow =
                {
                  footNow[0],
                  footNow[1] + footLen
                };

                double[] comNow =
                {
                   (hipNow[0] + kneeNow[0] + footNow[0]) / 3,
                   (hipNow[1] + kneeNow[1] + footNow[1]) / 3
                };

                hipNow = RotatePoint(R, hipNow);
                kneeNow = RotatePoint(R, kneeNow);
                footNow = RotatePoint(R, footNow);
                toeNow = RotatePoint(R, toeNow);
                comNow = RotatePoint(R, comNow);

                chart1.Series.Clear();
                AddLineSeries(chart1, "Hip-Knee", hipNow, kneeNow, Color.Blue);
                AddLineSeries(chart1, "Knee-Foot", kneeNow, footNow, Color.Red);
                AddLineSeries(chart1, "Foot-Toe", footNow, toeNow, Color.Black);
                AddPointSeries(chart1, "Hip", hipNow, Color.Black);
                AddPointSeries(chart1, "Knee", kneeNow, Color.Black);
                AddPointSeries(chart1, "Foot", footNow, Color.Black);
                AddPointSeries(chart1, "Toe", toeNow, Color.Black);
                AddPointSeries(chart1, "CoM", comNow, Color.Magenta);
                AddGroundLine(chart1, R);
                if (i > 0) AddTrailSeries(chart1, toeTrail, i, Color.Green);

                chart1.ChartAreas[0].AxisX.Minimum = -1;
                chart1.ChartAreas[0].AxisX.Maximum = 1;
                chart1.ChartAreas[0].AxisY.Minimum = -1;
                chart1.ChartAreas[0].AxisY.Maximum = 1;
                chart1.Titles.Clear();
                chart1.Titles.Add($"Frame {i + 1} / {kneeAngle.Length}");

                // Knee Angle chart
                chart2.Series.Clear();
                Series kneeSeries = new Series("Knee Angle") { ChartType = SeriesChartType.Line, Color = Color.Blue };
                for (int j = 0; j <= i; j++) kneeSeries.Points.AddXY(j, kneeAngleDeg[j]);
                chart2.Series.Add(kneeSeries);
                chart2.ChartAreas[0].AxisX.Minimum = 0;
                chart2.ChartAreas[0].AxisX.Maximum = kneeAngle.Length;
                chart2.ChartAreas[0].AxisY.Minimum = Math.Floor(kneeAngleDeg.Min() - 5);
                chart2.ChartAreas[0].AxisY.Maximum = Math.Floor(kneeAngleDeg.Max() + 5);
                chart2.Titles.Clear();
                chart2.Titles.Add("Knee Angle Over Time");

                // Thigh Angle chart
                chart3.Series.Clear();
                Series thighSeries = new Series("Thigh Angle") { ChartType = SeriesChartType.Line, Color = Color.Orange };
                for (int j = 0; j <= i; j++) thighSeries.Points.AddXY(j, thighAngleDeg[j]);
                chart3.Series.Add(thighSeries);
                chart3.ChartAreas[0].AxisX.Minimum = 0;
                chart3.ChartAreas[0].AxisX.Maximum = kneeAngle.Length;
                chart3.ChartAreas[0].AxisY.Minimum = Math.Floor(thighAngleDeg.Min() - 5);
                chart3.ChartAreas[0].AxisY.Maximum = Math.Floor(thighAngleDeg.Max() + 5);
                chart3.Titles.Clear();
                chart3.Titles.Add("Thigh Angle Over Time");

                chart1.Invalidate();
                chart2.Invalidate();
                chart3.Invalidate();
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
        }

        static double[] RotatePoint(double[,] R, double[] point)
        {
            return new double[]
            {
                R[0, 0] * point[0] + R[0, 1] * point[1],
                R[1, 0] * point[0] + R[1, 1] * point[1]
            };
        }

        static void AddLineSeries(Chart chart, string name, double[] point1, double[] point2, Color color)
        {
            var series = new Series(name) { ChartType = SeriesChartType.Line, Color = color };
            series.Points.AddXY(point1[0], point1[1]);
            series.Points.AddXY(point2[0], point2[1]);
            chart.Series.Add(series);
        }

        static void AddPointSeries(Chart chart, string name, double[] point, Color color)
        {
            var series = new Series(name) { ChartType = SeriesChartType.Point, Color = color, MarkerSize = 8 };
            series.Points.AddXY(point[0], point[1]);
            chart.Series.Add(series);
        }

        static void AddGroundLine(Chart chart, double[,] R)
        {
            double[] start = RotatePoint(R, new double[] { -2, 0 });
            double[] end = RotatePoint(R, new double[] { 2, 0 });
            AddLineSeries(chart, "Ground", start, end, Color.Black);
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
