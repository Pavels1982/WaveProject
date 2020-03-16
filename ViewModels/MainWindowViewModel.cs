using OxyPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using VoiceСhanging.Models;
using VoiceСhanging.Service;
using static VoiceСhanging.Models.WaveData;

namespace VoiceСhanging.ViewModels
{

    public class MainWindowViewModel:INotifyPropertyChanged
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

        public ChartModel ChartModel { get; set; } = new ChartModel();

        public ChartModel FFT { get; set; } = new ChartModel();

        public WavData Processed { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

      
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
