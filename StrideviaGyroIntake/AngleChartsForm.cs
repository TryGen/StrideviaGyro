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

            this.Width -= 100;

            this.Height += 50;

            /*      chartKnee = new Chart { Dock = DockStyle.Top, Height = this.Height / 2 };
                  chartThigh = new Chart { Dock = DockStyle.Fill };*/

              chartKnee = new Chart
              {
                Size = new Size(this.ClientSize.Width, 250),
                Location = new Point(0, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
              };

              chartThigh = new Chart
              {
                Size = new Size(this.ClientSize.Width, 250),
                Location = new Point(0, chartKnee.Bottom),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
              };


            chartKnee.ChartAreas.Add(new ChartArea());
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

            SetChart
             (
              ref chartKnee,
              0,
              kneeAngleDeg.Length,
              Math.Floor(kneeAngleDeg.Min() - 5),
              Math.Floor(kneeAngleDeg.Max() + 5),
              "Frame",
              "Knee Angle (°)",
              "Knee Angle Over Time"
              );

            chartThigh.Series.Clear();
                var thighSeries = new Series("Thigh Angle") { ChartType = SeriesChartType.Line, Color = Color.Orange };
                for (int j = 0; j <= currentFrame; j++) thighSeries.Points.AddXY(j, thighAngleDeg[j]);
                chartThigh.Series.Add(thighSeries);

               SetChart
               (
                ref chartThigh,
                0, 
                thighAngleDeg.Length,
                Math.Floor(thighAngleDeg.Min() - 5),
                Math.Floor(thighAngleDeg.Max() + 5),
                "Frame",
                "Thigh Angle (°)",
                "Thigh Angle Over Time"
                );

               

                chartKnee.Invalidate();
                chartThigh.Invalidate();
            }

           private void SetChart(ref Chart chart,double xMin, double xMax, double yMin, double yMax, string xTitle, string yTitle, string Title)
          {
            chart.ChartAreas[0].AxisX.Minimum = xMin;
            chart.ChartAreas[0].AxisX.Maximum = xMax;
            chart.ChartAreas[0].AxisY.Minimum = yMin;
            chart.ChartAreas[0].AxisY.Maximum = yMax;
            chart.ChartAreas[0].AxisX.Title = xTitle;
            chart.ChartAreas[0].AxisX.TitleFont = new Font("Arial", 12, FontStyle.Bold);
            chart.ChartAreas[0].AxisY.Title = yTitle;
            chart.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 12, FontStyle.Bold);

            //All the charts should go like this for easier data visualisation
            chart.ChartAreas[0].AxisY.Interval = 10;

            chart.Titles.Clear();
            Title title = new Title(Title, Docking.Top, new Font("Arial", 14, FontStyle.Bold), Color.Black);
            chart.Titles.Add(title);
            
        }


    }
}

