using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace VoiceСhanging.Models
{
    public class ChartModel: INotifyPropertyChanged
    {
        public PlotModel Model { get; set; }
        public LineSeries Line { get; set; } = new LineSeries();
        private RectangleBarSeries Bar { get; set; } = new RectangleBarSeries();
        private RectangleBarItem RectangleUI { get; set; }
        private bool isSelected = false;
        public int X { get; set; }
        private int startX { get; set; }
        private int width = 5000;
        public int Width
        {
            get
            {
                return this.width;
            }
            set
            {
                if (value > 100 && value < 50000)
                {
                    this.width = value;
                    Model.Axes[0].MaximumRange = value;
                    Model.Axes[0].MinimumRange = value;
                    Model.InvalidatePlot(true);
                }
            }
        } 

        public event PropertyChangedEventHandler PropertyChanged;

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);

        public ChartModel()
        {
            InitialChartView();
        }


        private void InitialChartView()
        {
            //  Points = new List<DataPoint>();

            Model = new PlotModel();
            Model.MouseDown += Model_MouseDown;
            Model.MouseUp += Model_MouseUp;
            Model.MouseMove += Model_MouseMove;


            Line.Color = OxyColor.FromRgb(0, 0, 255);

            Model.Series.Add(Bar);
            Model.Series.Add(Line);
            

            LinearAxis xAxis = new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MajorStep = 5000,
                IsZoomEnabled = false,
                Maximum = this.Width,
                AbsoluteMaximum = 1000000,
                AbsoluteMinimum = 0

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

        private void Model_MouseMove(object sender, OxyMouseEventArgs e)
        {
        
            if (isSelected)
            {
                X = (int)(OxyPlot.Axes.Axis.InverseTransform(e.Position, Model.Axes[0], Model.Axes[1]).X);

                if (RectangleUI == null)
                {
                    startX = X;
                    RectangleUI = new RectangleBarItem(startX, -1, X, 1);
                    Bar.Items.Add(RectangleUI);
                  }
                else
                {
                    RectangleUI.X1 = X;
                }

                Model.InvalidatePlot(true);
            }
        }

        private void Model_MouseUp(object sender, OxyMouseEventArgs e)
        {
            isSelected = false;
            //Bar.Items.Clear();
            //    RectangleUI = null;
            //   Model.InvalidatePlot(true);
        }

        private void Model_MouseDown(object sender, OxyMouseDownEventArgs e)
        {
            if ((GetAsyncKeyState(System.Windows.Forms.Keys.LButton) & 0x8000) != 0)
            {
                isSelected = true;
                if (RectangleUI != null)
                {
                    Bar.Items.Clear();
                    RectangleUI = null;
                    Model.InvalidatePlot(true);
                }
            }

           


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
