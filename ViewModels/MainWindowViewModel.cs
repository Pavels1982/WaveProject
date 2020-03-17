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
        public ChartViewModel Model1 { get; set; } = new ChartViewModel();
        public ChartViewModel Model2 { get; set; } = new ChartViewModel();

        
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {
            Model1.WindowPositionChanged += Model1_WindowPositionChanged;
           
            //ChartModel.SelectDataChanged += ChartModel_SelectDataChanged;
        }

        private void Model1_WindowPositionChanged(int value)
        {
            Model2.BindX = value;
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


    }
}
