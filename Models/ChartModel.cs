using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
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
        public List<Complex> SelectedData { get; set; } = new List<Complex>();
        private bool isPanBar = false;

        public event SelectDataHandler SelectDataChanged;
        public delegate void SelectDataHandler(List<Complex> selectedData);

        private int WindowWidth = 4096;

        public int Width
        {
            get
            {
                return this.width;
            }
            set
            {
                if (value > 20 && value < 50000)
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
                IsPanEnabled = true,
                MaximumPadding = 1,
                Minimum = -1,
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
                    RectangleUI = new RectangleBarItem(startX, Int16.MinValue, X, Int16.MaxValue);
                    RectangleUI.Color = OxyColors.LightGray;
                    Bar.Items.Add(RectangleUI);
                }
                else
                {
                 
                    double min = RectangleUI.X0 < RectangleUI.X1 ? RectangleUI.X0 : RectangleUI.X1;
                    double max = RectangleUI.X0 > RectangleUI.X1 ? RectangleUI.X0 : RectangleUI.X1;
                    // Debug.WriteLine(((int)max - (int)min));
                    if (((int)max - (int)min) >= WindowWidth)
                    {
                        RectangleUI.X1 = RectangleUI.X0 < RectangleUI.X1 ? startX + (WindowWidth + 1) : startX - (WindowWidth + 1);
                    }
                    else
                    {
                        RectangleUI.X1 = X;
                    }
                       
                }

                Model.InvalidatePlot(true);
            }

            if (isPanBar)
            {
                X = (int)(OxyPlot.Axes.Axis.InverseTransform(e.Position, Model.Axes[0], Model.Axes[1]).X);

                double width = RectangleUI.X1 - RectangleUI.X0;

                RectangleUI.X0 = X - (width / 2f);
                RectangleUI.X1 = (width / 2f) + X;
                SelectedData.Clear();

                Line.Points.Where(point => point.X > RectangleUI.X0 && point.X < RectangleUI.X1).ToList().ForEach(p =>
                {
                    SelectedData.Add(new Complex(p.Y, 0));
                });

                SelectDataChanged(SelectedData);
                Model.InvalidatePlot(true);
            }


        }

        private void Model_MouseUp(object sender, OxyMouseEventArgs e)
        {
            isPanBar = false;

            if (RectangleUI != null && isSelected)
            {
                if (Math.Abs(RectangleUI.X1 - RectangleUI.X0) < 10)
                {
                    ClearBar();
                    SelectedData.Clear();
                }
                else
                {
                    double min = RectangleUI.X0 < RectangleUI.X1 ? RectangleUI.X0 : RectangleUI.X1;
                    double max = RectangleUI.X0 > RectangleUI.X1 ? RectangleUI.X0 : RectangleUI.X1;
                    RectangleUI = new RectangleBarItem(min, Int16.MinValue, max, Int16.MaxValue);
                    RectangleUI.Color = OxyColors.LightGray;
                    Bar.Items.Add(RectangleUI);
                    Bar.Items.RemoveAt(0);

                   

                    Line.Points.Where(point => point.X > min && point.X < max).ToList().ForEach(p => 
                    {
                        SelectedData.Add(new Complex(p.Y, 0));
                    });
                    Debug.WriteLine(SelectedData.Count());


                }
            }
            isSelected = false;

        }

        private void Model_MouseDown(object sender, OxyMouseDownEventArgs e)
        {
            if (IsLeftMousePressed())
            {

                X = (int)(OxyPlot.Axes.Axis.InverseTransform(e.Position, Model.Axes[0], Model.Axes[1]).X);

                if (RectangleUI != null)
                {
                        if (X > RectangleUI.X0 && X < RectangleUI.X1) isPanBar = true;
                }

                if (!isPanBar)
                {
                    isSelected = true;
                    SelectedData.Clear();
                    if (RectangleUI != null) ClearBar();
                }
            }

        }

        private void ClearBar()
        {
            Bar.Items.Clear();
            RectangleUI = null;
            Model.InvalidatePlot(true);
        }


        private void DefaultData(List<DataPoint> points)
        {
            for (int i = 0; i < 1000; i++)
            {
                points.Add(new DataPoint(i, i));
            }
           
        }


        private bool IsLeftMousePressed()
        {
            return ((GetAsyncKeyState(System.Windows.Forms.Keys.LButton) & 0x8000) != 0) ? true : false;
        }

    }
}
