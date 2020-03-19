﻿using OxyPlot;
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
using System.Windows.Forms;
using System.Windows.Input;
using VoiceСhanging.Models;
using static VoiceСhanging.Models.WaveData;

namespace VoiceСhanging.UserControls
{
    public class ChartAnalyzingViewModel : INotifyPropertyChanged
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
            public bool IsMagnitude { get; set; } = false;



        private int startX { get; set; }
            private int width = 5000;
            public List<Complex> SelectedData { get; set; } = new List<Complex>();
            private bool isPanBar = false;
            List<Complex> FFTcom;

            public WavData Processed { get; set; }

            public event WindowPositionHandler WindowPositionChanged;
            public delegate void WindowPositionHandler(int value);

   

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

            public ICommand MorphingCommand
        {
            get
            {
                return new RelayCommand((o) => Morphing());
            }
        }

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

            public ChartAnalyzingViewModel()
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

                Func<double, string> str = (Label => (Label * 22f).ToString());

                LinearAxis xAxis = new LinearAxis()
                {
                    Position = AxisPosition.Bottom,
                    MajorGridlineStyle = LineStyle.Solid,
                    MajorStep = 50,
                    IsZoomEnabled = false,
                    Maximum = this.Width,
                    AbsoluteMaximum = 2049,
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

            X = (int)(OxyPlot.Axes.Axis.InverseTransform(e.Position, Model.Axes[0], Model.Axes[1]).X);
            if (isSelected)
                {
                    

                    if (RectangleUI == null)
                    {
                        startX = X;
                        RectangleUI = new RectangleBarItem(startX, Int16.MinValue, X, Int16.MaxValue);
                        RectangleUI.Color = OxyColor.FromArgb(100, 0, 0, 250);
                        Bar.Items.Add(RectangleUI);
                    }
                    else
                    {
                        RectangleUI.X1 = X;
                    }

                    Model.InvalidatePlot(true);
                }

                if (isPanBar && RectangleUI != null)
                {
                //double width = RectangleUI.X1 - RectangleUI.X0;
                double width = 2048;
                int offset = X - LastX;
                //RectangleUI.X0 = X - (width / 2f);
                //RectangleUI.X1 = (width / 2f) + X;
                RectangleUI.X0 += offset;
                RectangleUI.X1 += offset;
                SelectedData.Clear();
                double[] han = FFTHelper.blackmannHarris((int)width);
                int px = -1;
                int i = 0;
                Line.Points.Where(point => point.X >= RectangleUI.X0 && point.X <= RectangleUI.X1).ToList().ForEach(p =>
                    {
                        if (px == -1) px = (int)p.X;
                       
                        double pY = IsMagnitude ? p.Y* han[i] : p.Y;
                        SelectedData.Add(new Complex(pY, 0));
                        if (i < width-1) i++;
                    });

                //  SelectDataChanged(SelectedData, px);

                LastX = X;
                Model.InvalidatePlot(true);
                }


            }

            private void SelectDataChanged(List<Complex> selectedData, int start)
            {
                FFTLine.Points.Clear();
                FFTcom = FFT2Helper.fft(selectedData.ToArray()).ToList();
                double[] han = FFTHelper.hanning(FFTcom.Count());
                int x = 0;
                FFTcom.ForEach(comp =>
                {
                 
                    FFTLine.Points.Add(new DataPoint(x, comp.Magnitude));
                    x++;
               
                }

                );
                FFTModel.InvalidatePlot(true);

            }

            private void IFFT(Complex[] source, int start)
            {
                int i = start;
                foreach (var p in FFTHelper.IFFT(source))
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
                        RectangleUI.Color = OxyColor.FromArgb(100, 0, 0, 250);
                        Bar.Items.Add(RectangleUI);
                        Bar.Items.RemoveAt(0);
                        Line.Points.Where(point => point.X > min && point.X < max).ToList().ForEach(p =>
                        {
                            SelectedData.Add(new Complex(p.Y, 0));
                        });
                        LastX = (int)min;

                    if (!IsMagnitude)
                    {
                        Process((int)RectangleUI.X0, (int)RectangleUI.X1);
                    }
                    else
                        Morphing();
                   


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
                    if (X > RectangleUI.X0 && X < RectangleUI.X1)
                    {
                        isPanBar = true;
                        LastX = X;
                    }
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


        class Val
        {
            public double Magnitude { get; set; } = 0;
            public int Rep { get; set; } = 0;
        }


        private void Process(int start, int end)
        {
            FFTLine.Points.Clear();
            List<Val> temp = new List<Val>(2048);

            double[] result = new double[2048];
            result.ToList().ForEach(r => temp.Add(new Val()));


            for (int x = 0; x < SelectedData.Count() - 2048; x += 250)
            {
                int i = 0;
                if (x + 2048 < SelectedData.Count() - 2048)
                    FFT2Helper.fft(SelectedData.GetRange(x, 2048).ToArray()).ToList().ForEach(p =>
                    {

                            temp[i].Magnitude += p.Magnitude;
                            temp[i].Rep++;
                        i++;
                    });
            }

            for (int i = 0; i < 2048; i++)
            {
                temp[i].Magnitude = (temp[i].Magnitude / 2048);
               // temp[i].Magnitude = temp[i].Rep > cons ? (temp[i].Magnitude / temp[i].Rep) : 0;
                FFTLine.Points.Add(new DataPoint(i, temp[i].Magnitude));
            }

            FFTModel.InvalidatePlot(true);

        }

        private void Morphing()
        {
            int startPos = (int)RectangleUI.X0;
            DataPoint[] res = new DataPoint[SelectedData.Count() - 2048];
            for (int x = 0; x < SelectedData.Count() - 2048; x += 256)
            {

                if (x + 2048 < SelectedData.Count() - 2048)
                {
                    Complex[] tempFFT = FFT2Helper.fft(SelectedData.GetRange(x, 2048).ToArray());

                    //   Complex[] tempIFFT = new Complex[2048];
                    int index = 100;
                    


                    for (int i1 = 0; i1 < 2048; i1++)
                    {


                        tempFFT[index] = tempFFT[20];

                        tempFFT[2047 - index] = tempFFT[index];


                        //// tempIFFT[i1] = FFTLine.Points[i1].Y;
                        //// tempIFFT[i1] =new  Complex(0, 0);
                        //if (tempFFT[i1].Magnitude < FFTLine.Points[i1].Y)
                        //{
                        //    tempFFT[i1] = new Complex(0, 0);
                        //}
                        //else
                        //{
                        //    tempFFT[i1] = FFTLine.Points[i1].Y;
                        //}

                    }
                    //FFTHelper.IFFT(tempFFT).ToList().ForEach(e => res[x]  = new DataPoint(x + (Line.Points.Count-1), e) );
                    IFFT(tempFFT, startPos + x);
                }
            }
          //  Line.Points.AddRange(res.ToList());
            Model.InvalidatePlot(true);


        }



        #region DataReadWriteBlock
        private void ReadDataWave()
            {
                System.Windows.Forms.OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "All Supported Files (*.wav)|*.wav";
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }

                FileName = openFileDialog.FileName;
                WavFile file = new WavFile(FileName); //входной файл
                WavData orig = file.ReadData();
                Processed = new WavData(orig.Header, orig.Data.Length);
                SelectedData.Clear();
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

                WavFile new_file = new WavFile(FileName.Remove(FileName.Length - 4) + "-Modify.wav"); //выходной файл
                Line.Points.ForEach(p => Processed.WriteSample((float)p.Y));

                new_file.WriteData(Processed);

            }
        #endregion


    }
}