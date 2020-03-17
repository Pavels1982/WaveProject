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
using VoiceСhanging.UserControls;
using static VoiceСhanging.Models.WaveData;

namespace VoiceСhanging.ViewModels
{

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ChartViewModel Model1 { get; set; } = new ChartViewModel();
        public ChartViewModel Model2 { get; set; } = new ChartViewModel();
        public ChartAnalyzingViewModel Model3 { get; set; } = new ChartAnalyzingViewModel();


        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {
          //  Model1.WindowPositionChanged += Model1_WindowPositionChanged;
        }

        private void Model1_WindowPositionChanged(int value)
        {
          //  Model2.BindX = value;
        }

       
    

    }
}
