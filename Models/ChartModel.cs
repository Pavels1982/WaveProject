using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceСhanging.Models
{
    public class ChartModel: INotifyPropertyChanged
    {
        public PlotModel Model { get; set; }
        public LineSeries Line { get; set; } = new LineSeries();
        

        private int width = 5000;
        public int Width
        {
            get
            {
                return this.width;
            }
            set
            {
                if (value > 1000 && value < 50000)
                {
                    this.width = value;
                    Model.Axes[0].Maximum = value;
                    Model.InvalidatePlot(true);
                }
            }
        } 

        public event PropertyChangedEventHandler PropertyChanged;

        public ChartModel()
        {
            InitialChartView();
        }


        private void InitialChartView()
        {
          //  Points = new List<DataPoint>();

            Model = new PlotModel();
            Line.Color = OxyColor.FromRgb(0, 0, 255);
            Model.Series.Add(Line);
            LinearAxis xAxis = new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MajorStep = 5000,
                IsZoomEnabled = false,
                Maximum = this.Width,
                AbsoluteMaximum = 1000000
            };
            Model.Axes.Add(xAxis);

            LinearAxis YAxis = new LinearAxis()
            {
                IsAxisVisible = true,
                Position = AxisPosition.Left,
                MaximumPadding = 1,
                Minimum = 0,
                Maximum = 1,
                MajorStep = 5000,
                MajorGridlineStyle = LineStyle.Solid,
            };
            Model.Axes.Add(YAxis);
            Model.InvalidatePlot(true);
        }


        private void DefaultData(List<DataPoint> points)
        {
            for (int i = 0; i < 1000; i++)
            {
                points.Add(new DataPoint(i, i));
            }
           
        }
    }
}
