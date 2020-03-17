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
using System.Windows.Forms;
using System.Windows.Input;
using static VoiceСhanging.Models.WaveData;

namespace VoiceСhanging.Models
{
    public class ChartViewModel: INotifyPropertyChanged
    {
        public PlotModel Model { get; set; } 
        public LineSeries Line { get; set; } = new LineSeries();

        public PlotModel FFTModel { get; set; } 
        public LineSeries FFTLine { get; set; } = new LineSeries();
        private string FileName { get; set; }

        private RectangleBarSeries Bar { get; set; } = new RectangleBarSeries();
        private RectangleBarItem RectangleUI { get; set; }
        private bool isSelected = false;
        public int X { get; set; }
        public int LastX { get; set; }

        private int bindX;
        public int BindX
        {
            get
            {
                return bindX;
            }

            set
            {
                bindX = value;
                isPanBar = true;
               // isSelected = true;
                Model_MouseMove(null, null);
            }
        }

        private int startX { get; set; }
        private int width = 5000;
        public List<Complex> SelectedData { get; set; } = new List<Complex>();
        private bool isPanBar = false;
        List<Complex> FFTcom;

        public WavData Processed { get; set; }

        public event WindowPositionHandler WindowPositionChanged;
        public delegate void WindowPositionHandler(int value);

        private int WindowWidth = 2048;

        public int Width
        {
            get
            {
                return this.width;
            }
            set
            {
                if (value > 20 && value < 100000)
                {
                    this.width = value;
                    Model.Axes[0].MaximumRange = value;
                    Model.Axes[0].MinimumRange = value;
                    Model.InvalidatePlot(true);
                }
            }
        }

        private int fftwidth = 1025;
        public int FFTWidth
        {
            get
            {
                return this.fftwidth;
            }
            set
            {
                if (value > 20 && value < 2048)
                {
                    this.fftwidth = value;
                    FFTModel.Axes[0].MaximumRange = value;
                    FFTModel.Axes[0].MinimumRange = value;
                    FFTModel.InvalidatePlot(true);
                }
            }
        }


        #region Command
        public ICommand OpenFileCommand
        {
            get
            {
                return new RelayCommand((o) => ReadDataWave());
            }
        }

        public ICommand WriteFileCommand
        {
            get
            {
                return new RelayCommand((o) => SaveDateWave());
            }
        }
        #endregion



        public event PropertyChangedEventHandler PropertyChanged;

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);

        public ChartViewModel()
        {
            InitialChartView();
            InitialFFTChartView();
        }

        private void InitialFFTChartView()
        {
            FFTModel = new PlotModel();

            FFTLine.Color = OxyColor.FromRgb(0, 0, 255);
           // FFTModel.Series.Add(Bar);
            FFTModel.Series.Add(FFTLine);

            Func<double, string> str = (Label =>   (Label * 22f).ToString()+ "Hz" );

            LinearAxis xAxis = new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MajorStep = 50,
                IsZoomEnabled = false,
                Maximum = this.Width,
                AbsoluteMaximum = 1025,
                AbsoluteMinimum = 0,
                LabelFormatter = str,

            };

            FFTModel.Axes.Add(xAxis);

            LinearAxis YAxis = new LinearAxis()
            {
                IsAxisVisible = true,
                Position = AxisPosition.Left,
                IsPanEnabled = true,
                MaximumPadding = 1,
                Minimum = 0,
                Maximum = 100,
                MajorStep = 5000,
                MajorGridlineStyle = LineStyle.Solid,
            };
            FFTModel.Axes.Add(YAxis);
            FFTModel.InvalidatePlot(true);
        }
        
        private void InitialChartView()
        {
          
            Model = new PlotModel();
            Model.MouseDown += Model_MouseDown;
            Model.MouseUp += Model_MouseUp;
            Model.MouseMove += Model_MouseMove;

            Line.Color = OxyColor.FromRgb(0, 0, 255);
           
            Model.Series.Add(Line);
            Model.Series.Add(Bar);


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
           

            if (isSelected )
            {
                 X = (int)(OxyPlot.Axes.Axis.InverseTransform(e.Position, Model.Axes[0], Model.Axes[1]).X);

                if (RectangleUI == null)
                {
                    startX = X;
                    RectangleUI = new RectangleBarItem(startX, Int16.MinValue, X, Int16.MaxValue);
                     RectangleUI.Color = OxyColor.FromArgb(100, 250, 0, 0); 
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

            if (isPanBar && RectangleUI != null)
            {
               
                if (e == null) { X = (int)RectangleUI.X0 + BindX; }
                else
                    X = (int)(OxyPlot.Axes.Axis.InverseTransform(e.Position, Model.Axes[0], Model.Axes[1]).X);

                WindowPositionChanged?.Invoke(X - LastX);

                //  X = (int)(OxyPlot.Axes.Axis.InverseTransform(e.Position, Model.Axes[0], Model.Axes[1]).X);

                double width = RectangleUI.X1 - RectangleUI.X0;

                RectangleUI.X0 = X - (width / 2f);
                RectangleUI.X1 = (width / 2f) + X;
                SelectedData.Clear();
                int px = -1;
                Line.Points.Where(point => point.X >= RectangleUI.X0 && point.X <= RectangleUI.X1).ToList().ForEach(p =>
                {
                    if (px == -1) px = (int)p.X;
                    SelectedData.Add(new Complex(p.Y, 0));
                });

                SelectDataChanged(SelectedData, px);
                LastX = (int)RectangleUI.X0;
                Model.InvalidatePlot(true);
            }


        }

        private void SelectDataChanged(List<Complex> selectedData, int start)
        {
            FFTLine.Points.Clear();
            FFTcom = FFTHelper.FFT(selectedData.ToArray()).ToList();
         // FFTcom = FFT2Helper.fft(FFT2Helper.fft(selectedData.ToArray())).ToList();

            FFTcom.RemoveRange(FFTcom.Count / 2, FFTcom.Count / 2);

            int x = 0;
            FFTcom.ForEach(comp =>
            {
                //FFTLine.Points.Add(new DataPoint(x++, 10 * Math.Log10(GetYPos(comp) / FFTcom.Count())));
                //   FFTLine.Points.Add(new DataPoint(x++, Math.Abs(GetYPosLog(comp))));
                FFTLine.Points.Add(new DataPoint(x++,  comp.Magnitude));
            }

            );

            //FFTModel.Axes[0].MaximumRange = FFTcom.Count;
            //FFTModel.Axes[0].MinimumRange = FFTcom.Count;
            FFTModel.InvalidatePlot(true);
           //  IFFT(start);
        }

        private void IFFT(int start)
        {
            int i = start;
            foreach (var p in FFTHelper.IFFT(FFTcom.ToArray()))
            {
                Line.Points[i] = new DataPoint(i, p);
                i++;
            }
            Model.InvalidatePlot(true);
        }

        private double GetYPos(Complex c)
        {
            return Math.Sqrt(c.Real * c.Real + c.Imaginary * c.Imaginary);
        }


        private double GetYPosLog(Complex c)
        {
            // not entirely sure whether the multiplier should be 10 or 20 in this case.
            // going with 10 from here http://stackoverflow.com/a/10636698/7532
            double intensityDB = 10 * Math.Log10(Math.Sqrt(c.Real * c.Real + c.Imaginary * c.Imaginary));
            double minDB = -90;
            if (intensityDB < minDB) intensityDB = minDB;
            double percent = intensityDB / minDB;
            // we want 0dB to be at the top (i.e. yPos = 0)
            double yPos = percent;
            return yPos;
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
                    RectangleUI.Color =OxyColor.FromArgb(100,250,0,0);
                    Bar.Items.Add(RectangleUI);
                    Bar.Items.RemoveAt(0);
                    Line.Points.Where(point => point.X > min && point.X < max).ToList().ForEach(p => 
                    {
                        SelectedData.Add(new Complex(p.Y, 0));
                    });
                    LastX = (int)min;
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


        private void ReadDataWave()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All Supported Files (*.wav;*.mp3)|*.wav;*.mp3";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }

            FileName = openFileDialog.FileName;
            WavFile file = new WavFile(FileName); //входной файл
            WavData orig = file.ReadData();
            Processed = new WavData(orig.Header, orig.Data.Length);
            SelectedData.Clear();
            FFTLine.Points.Clear();
            Line.Points.Clear();

            using (orig)
            {
                int c = orig.GetSamplesCount();

                int x = 0;

                for (int i = 0; i < c; i++)
                {
                    float y = orig.ReadNextSample(); //читаем следующий семпл
                    Line.Points.Add(new DataPoint(x++, y));
                }
                Model.InvalidatePlot(true);

            }
        }


        private void SaveDateWave()
        {

            WavFile new_file = new WavFile(FileName.Remove(FileName.Length - 4) +"-Modify.wav"); //выходной файл
            Line.Points.ForEach(p => Processed.WriteSample((float)p.Y));

            new_file.WriteData(Processed);

        }




    }
}
