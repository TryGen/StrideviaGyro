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
    public partial class AngleChartsForm : Form
    {
        
            private Chart chartKnee;
            private Chart chartThigh;
            private double[] kneeAngleDeg;
            private double[] thighAngleDeg;

            public AngleChartsForm(double[] kneeAngleDeg, double[] thighAngleDeg)
            {
                InitializeComponent();
                this.kneeAngleDeg = kneeAngleDeg;
                this.thighAngleDeg = thighAngleDeg;

                chartKnee = new Chart { Dock = DockStyle.Top, Height = this.Height / 2 };
                chartKnee.ChartAreas.Add(new ChartArea());

                chartThigh = new Chart { Dock = DockStyle.Fill };
                chartThigh.ChartAreas.Add(new ChartArea());

                this.Controls.Add(chartThigh);
                this.Controls.Add(chartKnee);
            }

            public void UpdateCharts(int currentFrame)
            {
                chartKnee.Series.Clear();
                var kneeSeries = new Series("Knee Angle") { ChartType = SeriesChartType.Line, Color = Color.Blue };
                for (int j = 0; j <= currentFrame; j++) kneeSeries.Points.AddXY(j, kneeAngleDeg[j]);
                chartKnee.Series.Add(kneeSeries);
                chartKnee.ChartAreas[0].AxisX.Minimum = 0;
                chartKnee.ChartAreas[0].AxisX.Maximum = kneeAngleDeg.Length;
                chartKnee.ChartAreas[0].AxisY.Minimum = Math.Floor(kneeAngleDeg.Min() - 5);
                chartKnee.ChartAreas[0].AxisY.Maximum = Math.Floor(kneeAngleDeg.Max() + 5);
                chartKnee.Titles.Clear();
                chartKnee.Titles.Add("Knee Angle Over Time");

                chartThigh.Series.Clear();
                var thighSeries = new Series("Thigh Angle") { ChartType = SeriesChartType.Line, Color = Color.Orange };
                for (int j = 0; j <= currentFrame; j++) thighSeries.Points.AddXY(j, thighAngleDeg[j]);
                chartThigh.Series.Add(thighSeries);
                chartThigh.ChartAreas[0].AxisX.Minimum = 0;
                chartThigh.ChartAreas[0].AxisX.Maximum = thighAngleDeg.Length;
                chartThigh.ChartAreas[0].AxisY.Minimum = Math.Floor(thighAngleDeg.Min() - 5);
                chartThigh.ChartAreas[0].AxisY.Maximum = Math.Floor(thighAngleDeg.Max() + 5);
                chartThigh.Titles.Clear();
                chartThigh.Titles.Add("Thigh Angle Over Time");

                chartKnee.Invalidate();
                chartThigh.Invalidate();
            }
        }
    }

