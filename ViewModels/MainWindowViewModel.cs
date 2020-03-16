using MathNet.Numerics.IntegralTransforms;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using VoiceСhanging.Models;
using VoiceСhanging.Service;
using static VoiceСhanging.Models.WaveData;

namespace VoiceСhanging.ViewModels
{

    public class MainWindowViewModel : INotifyPropertyChanged
    {
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

        public ICommand IFFTCommand
        {
            get
            {
                return new RelayCommand((o) => IFFT());
            }
        }


        public ChartModel ChartModel { get; set; } = new ChartModel();

        public ChartModel FFT { get; set; } = new ChartModel();
        List<Complex> FFTcom;
        int start=0, end=0;

        public WavData Processed { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {
            ChartModel.SelectDataChanged += ChartModel_SelectDataChanged;
        }


        private void IFFT()
        {
            int i = this.start;
            foreach (var p in FFTHelper.IFFT(FFTcom.ToArray()))
            {
                ChartModel.Line.Points[i] = new DataPoint(i, p);
                i++;
            }
            ChartModel.Model.InvalidatePlot(true);
        }



        private void ChartModel_SelectDataChanged(List<Complex> selectedData, int start, int end)
        {
            this.start = start;
            this.end = end;
            FFT.Line.Points.Clear();
            // List<Complex> FFTcom = new List<Complex>(Fourier.NaiveForward(selectedData.ToArray(), FourierOptions.Default).ToList());

            FFTcom = FFTHelper.FFT(selectedData.ToArray()).ToList();
            FFTcom.RemoveRange(FFTcom.Count / 2, FFTcom.Count / 2);
            //int j = (FFTcom.Count - 1);
            //for (int y = 0; y < 300; y++)
            //{
            //    FFTcom[y] = new Complex(0, 0);
            //    FFTcom[j - y] = new Complex(0, 0);
            //}

            int x = 0;
            FFTcom.ForEach(comp =>
            {
                // FFT.Line.Points.Add(new DataPoint(x++, 10 * Math.Log10(GetYPos(comp) / FFTcom.Count())));
                //FFT.Line.Points.Add(new DataPoint(x++, Math.Abs(GetYPosLog(comp))));
                FFT.Line.Points.Add(new DataPoint(x++,comp.Magnitude));

            }
            
            );

            FFT.Model.Axes[0].AbsoluteMaximum = ChartModel.Line.Points.Count();
            FFT.Model.InvalidatePlot(true);
           // IFFT();
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



        private void ReadDataWave()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All Supported Files (*.wav;*.mp3)|*.wav;*.mp3";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }



            WavFile file = new WavFile(openFileDialog.FileName); //входной файл
            WavData orig = file.ReadData();
            Processed = new WavData(orig.Header, orig.Data.Length);
           


            using (orig)
            {
                int c = orig.GetSamplesCount();

                int x = 0;
   
                for (int i = 0; i < c; i++)
                {
                    float y = orig.ReadNextSample(); //читаем следующий семпл
                    ChartModel.Line.Points.Add(new DataPoint(x++, y));
                }
                ChartModel.Model.Axes[0].AbsoluteMaximum = ChartModel.Line.Points.Count();
                ChartModel.Model.InvalidatePlot(true);


            }
        }

        private void SaveDateWave()
        {

            WavFile new_file = new WavFile(@"note.wav"); //выходной файл
            ChartModel.Line.Points.ForEach(p => Processed.WriteSample((float)p.Y));

            new_file.WriteData(Processed);

        }

     
    }
}
