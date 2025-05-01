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
        public Animation()
        {
            DataIntake data = new DataIntake();
            data.getRawData();

            int N = data.size;
            double[] kneeAngle = new double[N];

            for (int i = 0; i < N; i++)
            {
                  double w = data.gyroData[i].normalized.w;
                  double y = data.gyroData[i].normalized.y;
                  double x = data.gyroData[i].normalized.x;
                  double z = data.gyroData[i].normalized.z;


                double sinp = 2 * (w * y - x * z);
                double pitch = Math.Abs(sinp) >= 1 ? Math.Sign(sinp) * Math.PI / 2 : Math.Asin(sinp);
                kneeAngle[i] = pitch;
            }

            Application.EnableVisualStyles();

            Chart chart = new Chart { Dock = DockStyle.Fill };
            this.Controls.Add(chart);
            chart.ChartAreas.Add(new ChartArea());

            this.Shown += async (s, e) =>
            {
                double thighLen = 0.5;
                double shankLen = 0.5;
                double footLen = 0.1;
                double frameTime = 1.0 / 20.0;

                double theta = 3 * Math.PI / 2;
                double[,] R = new double[2, 2]
                {
            { Math.Cos(theta), -Math.Sin(theta) },
            { Math.Sin(theta), Math.Cos(theta) }
                };

                double[,] toeTrail = new double[N, 2];

                for (int i = 0; i < N - 1; i++)
                {

                    double thighAngle = -kneeAngle[i] / 2; 
                    double shankAngle = +kneeAngle[i] / 2; 

                    double[] hip = { 0, 0 };

                    double[] knee =
                    {
                        hip[0] + thighLen * Math.Cos(thighAngle),
                        hip[1] + thighLen * Math.Sin(thighAngle)
                    };

                    double[] foot = 
                    {
                      knee[0] + shankLen * Math.Cos(thighAngle + shankAngle),
                      knee[1] + shankLen * Math.Sin(thighAngle + shankAngle)
                    };

                    double[] toe = { foot[0], foot[1] + footLen };


                    double[] com =
                    {
                      (hip[0] + knee[0] + foot[0]) / 3,
                      (hip[1] + knee[1] + foot[1]) / 3 
                    };

                    hip = RotatePoint(R, hip);
                    knee = RotatePoint(R, knee);
                    foot = RotatePoint(R, foot);
                    toe = RotatePoint(R, toe);
                    com = RotatePoint(R, com);

                    toeTrail[i, 0] = toe[0];
                    toeTrail[i, 1] = toe[1];

                    double hipKneeLength = Math.Sqrt(
                       Math.Pow(knee[0] - hip[0], 2) +
                       Math.Pow(knee[1] - hip[1], 2)
                    );

                    Console.WriteLine($"Hip-Knee Length: {hipKneeLength:F4}");

                    chart.Series.Clear();

                    AddLineSeries(chart, "Hip-Knee", hip, knee, System.Drawing.Color.Blue);
                    AddLineSeries(chart, "Knee-Foot", knee, foot, System.Drawing.Color.Red);
                    AddLineSeries(chart, "Foot-Toe", foot, toe, System.Drawing.Color.Black);
                    AddPointSeries(chart, "Hip", hip, System.Drawing.Color.Black);

                    AddPointSeries(chart, "Knee", knee, System.Drawing.Color.Black);
                    AddPointSeries(chart, "Foot", foot, System.Drawing.Color.Black);
                    AddPointSeries(chart, "Toe", toe, System.Drawing.Color.Black);

                    AddGroundLine(chart, R);

                    if (i > 0)
                    {
                        AddTrailSeries(chart, toeTrail, i, System.Drawing.Color.Green);
                    }
                    AddPointSeries(chart, "CoM", com, System.Drawing.Color.Magenta);

                    chart.ChartAreas[0].AxisX.Minimum = -1;
                    chart.ChartAreas[0].AxisX.Maximum = 1;
                    chart.ChartAreas[0].AxisY.Minimum = -1;
                    chart.ChartAreas[0].AxisY.Maximum = 1;
                    chart.Titles.Clear();
                    chart.Titles.Add($"Frame {i + 1} / {N}");
                    chart.Invalidate();

                    await Task.Delay((int)(frameTime * 1000));
                }
            };



            InitializeComponent();
        }

        static double[] RotatePoint(double[,] R, double[] point)
        {
             return new double[]
             {
             R[0, 0] * point[0] + R[0, 1] * point[1],
             R[1, 0] * point[0] + R[1, 1] * point[1]
             };

            
        }


        static void AddLineSeries(Chart chart, string name, double[] point1, double[] point2, System.Drawing.Color color)
        {
            var series = new Series(name) { ChartType = SeriesChartType.Line, Color = color };
            series.Points.AddXY(point1[0], point1[1]);
            series.Points.AddXY(point2[0], point2[1]);
            chart.Series.Add(series);
        }

        static void AddPointSeries(Chart chart, string name, double[] point, System.Drawing.Color color)
        {
            var series = new Series(name) { ChartType = SeriesChartType.Point, Color = color, MarkerSize = 8 };
            series.Points.AddXY(point[0], point[1]);
            chart.Series.Add(series);
        }
       

        static void AddGroundLine(Chart chart, double[,] R)
        {
            double[] start = RotatePoint(R, new double[] { -2, 0 });
            double[] end = RotatePoint(R, new double[] { 2, 0 });
            AddLineSeries(chart, "Ground", start, end, System.Drawing.Color.Black);
        }

        static void AddTrailSeries(Chart chart, double[,] toeTrail, int count, System.Drawing.Color color)
        {
            var series = new Series("Toe Trail") { ChartType = SeriesChartType.Line, Color = color };
            for (int i = 0; i <= count; i++)
            {
                series.Points.AddXY(toeTrail[i, 0], toeTrail[i, 1]);
            }
            chart.Series.Add(series);
        }



    }
}
