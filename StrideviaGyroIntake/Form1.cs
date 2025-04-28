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
    public partial class Form1 : Form
    {
     

        public Form1()
        {
            InitializeComponent();
            InitializeChart();
            PlotFunction();
        }

        private void InitializeChart()
        {
            chart1 = new Chart();
            chart1.Dock = DockStyle.Fill; 
            this.Controls.Add(chart1);
        }

        private void PlotFunction()
        {
            ChartArea chartArea = new ChartArea();
            chart1.ChartAreas.Add(chartArea);

            chart1.Legends.Clear();

            Series series = new Series
            {
                Name = "Function",
                ChartType = SeriesChartType.Line,
                Color = System.Drawing.Color.Blue,
                BorderWidth = 2
            };

            Series series2 = new Series
            {
                Name = "Function2",
                ChartType = SeriesChartType.Line,
                Color = System.Drawing.Color.Red,
                BorderWidth = 2
            };
            chart1.Series.Add(series);
            chart1.Series.Add(series2);

            for (double x = -10; x <= 10; x += 0.1)
            {
                double y = Math.Sin(x);
                series.Points.AddXY(x, y);
            }

            for (double x = -10; x <= 10; x += 0.1)
            {
                double y = Math.Cos(x);
                series2.Points.AddXY(x, y);
            }

            chartArea.AxisX.Crossing = 0;
            chartArea.AxisY.Crossing = 0;

            
            chartArea.AxisX.Minimum = -10;
            chartArea.AxisX.Maximum = 10;
            chartArea.AxisY.Minimum = -1.5;
            chartArea.AxisY.Maximum = 1.5;

           
            chartArea.AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            chartArea.AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;

           
            chartArea.AxisX.Interval = 1; 
            chartArea.AxisY.Interval = 0.5; 

           
            chartArea.AxisX.ArrowStyle = AxisArrowStyle.Triangle;
            chartArea.AxisY.ArrowStyle = AxisArrowStyle.Triangle;
        }
    }
}