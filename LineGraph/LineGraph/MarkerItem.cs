using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LineGraph
{
    public class MarkerItem : INotifyPropertyChanged
    {
        public enum EMarkerColor
        {
            [Description("#5BA0AD")]
            Mark1,
            [Description("#F200F2")]
            Mark2,
            [Description("#00FF00")]
            Mark3,
            [Description("#DF0000")]
            Mark4,
        }

        private int _MarkerNum;
        public int MarkerNum { get { return _MarkerNum; } set { _MarkerNum = value; MakeMarkString(); } }
        public string _MarkerString;
        public string MarkerString { get { return _MarkerString; } set { _MarkerString = value; OnPropertyChanged("MarkerString"); } }
        private bool _IsDelta;
        public bool IsDelta { get { return _IsDelta; } set { _IsDelta = value; MakeMarkString(); } }
        private double _Freq;
        public double Freq { get { return _Freq; } set { _Freq = value; MakeMarkString(); } }
        private double _DeltaFreq;
        public double DeltaFreq { get { return _DeltaFreq; } set { _DeltaFreq = value; MakeMarkString(); } }
        private double _Value;
        public double Value { get { return _Value; } set { _Value = value; MakeMarkString(); } }
        private double _DeltaValue;
        public double DeltaValue { get { return _DeltaValue; } set { _DeltaValue = value; MakeMarkString(); } }

        public string Color { get; set; }
        
        public StackPanel MarkerControl { get; set; }
        public StackPanel DeltaMarkerControl { get; set; }

        private void MakeMarkString()
        {
            if (IsDelta == false)
                MarkerString = string.Format("M{0}:{1}MHz,{2}dBm", MarkerNum, Freq, Value);
            else MarkerString = string.Format("△{0}:{1}MHz,{2}dBc", MarkerNum, Freq - DeltaFreq, Value - DeltaValue);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null) return;
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
