using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using PD.ViewModel;
using OxyPlot;

namespace PD.Models
{
    public class GaugeModel : ChannelModel
    {       
        public GaugeModel() { }

        public GaugeModel(GaugeModel gm)
        {
            PD_or_PM = gm.PD_or_PM;
            SN_Row = gm.SN_Row;
            boolGauge = gm.boolGauge;
            GaugeColor = gm.GaugeColor;
            GaugeEndAngle = gm.GaugeEndAngle;
            GaugeValue = gm.GaugeValue;
            GaugeUnit = gm.GaugeUnit;
            GaugeD0_Select = gm.GaugeD0_Select;
            GaugeD0_1 = gm.GaugeD0_1;
            GaugeD0_2 = gm.GaugeD0_2;
            GaugeD0_3 = gm.GaugeD0_3;
            GaugeSN = gm.GaugeSN;
            GaugeBearSay_1 = gm.GaugeBearSay_1;
            GaugeBearSay_2 = gm.GaugeBearSay_2;
            GaugeBearSay_3 = gm.GaugeBearSay_3;
            GaugeMode = gm.GaugeMode;
            DataPoints = new List<DataPoint>(gm.DataPoints);
            chModel = gm.chModel;
        }

        public ChannelModel chModel { get; set; } = new ChannelModel();

        public bool PD_or_PM { get; set; } = true;

        private int _SN_Row = 2;
        public int SN_Row
        {
            get { return _SN_Row; }
            set
            {
                _SN_Row = value;
                OnPropertyChanged_Normal("SN_Row");
            }
        }

        //private string _SNnumber;
        //public string SNnumber
        //{
        //    get { return _SNnumber; }
        //    set
        //    {
        //        _SNnumber = value;
        //        OnPropertyChanged_Normal("SNnumber");
        //    }
        //}

        private bool _boolGauge = false;
        public bool boolGauge
        {
            get { return _boolGauge; }
            set
            {
                _boolGauge = value;
                OnPropertyChanged_Normal("boolGauge");
            }
        }       

        public SolidColorBrush GaugeColor { get; set; } = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF10E2C4"));

        private double _GaugeEndAngle = -150;
        public double GaugeEndAngle
        {
            get { return _GaugeEndAngle; }
            set
            {
                _GaugeEndAngle = value;
                OnPropertyChanged_Normal("GaugeEndAngle");
            }
        }

        private string _GaugeValue = "";
        public string GaugeValue
        {
            get { return _GaugeValue; }
            set
            {
                _GaugeValue = value;
                OnPropertyChanged_Normal("GaugeValue");

                #region Transform GaugeValue to GaugeAngle
                double _IL;
                if(double.TryParse(value, out _IL))
                {
                    double y = _IL;  //y is 0~-64dBm 
                    if (!PD_or_PM)  //PD mode, y is 0~-64dBm 
                    {
                        double angle = (y * 300 / -64 - 150) * -1;
                        angle = angle != 1350 ? angle : 150;
                        GaugeEndAngle = angle;
                    }
                    else //PD mode, y is 7~-70dBm 
                    {
                        double angle = (y * 300 + 8550) / 71;
                        angle = angle >= 150 ? 150 : angle;
                        angle = angle <= -150 ? -150 : angle;
                        GaugeEndAngle = angle;
                    }
                }
                else GaugeEndAngle = -150;
                #endregion
            }
        }

        private string _GaugeUnit;
        public string GaugeUnit
        {
            get { return _GaugeUnit; }
            set
            {
                _GaugeUnit = value;
                OnPropertyChanged_Normal("GaugeUnit");
            }
        }

        public string GaugeChannel { get; set; }
        public string GaugeD0_Select { get; set; }

        private string _GaugeD0_1 ;
        public string GaugeD0_1
        {
            get
            {
                if (!string.IsNullOrEmpty(_GaugeD0_1))
                    return _GaugeD0_1;
                else
                    return "0";
            }
            set
            {
                _GaugeD0_1 = value;
                OnPropertyChanged_Normal("GaugeD0_1");
            }
        }

        private string _GaugeD0_2 ;
        public string GaugeD0_2
        {
            get
            {
                if (!string.IsNullOrEmpty(_GaugeD0_2))
                    return _GaugeD0_2;
                else
                    return "0";
            }
            set
            {
                _GaugeD0_2 = value;
                OnPropertyChanged_Normal("GaugeD0_2");
            }
        }

        private string _GaugeD0_3 ;
        public string GaugeD0_3
        {
            get
            {
                if (!string.IsNullOrEmpty(_GaugeD0_3))
                    return _GaugeD0_3;
                else
                    return "0";
            }
            set
            {
                _GaugeD0_3 = value;
                OnPropertyChanged_Normal("GaugeD0_3");
            }
        }

        private string _GaugeSN = "";
        public string GaugeSN
        {
            get { return _GaugeSN; }
            set
            {
                _GaugeSN = value;
                OnPropertyChanged_Normal("GaugeSN");
            }
        }

        private string _GaugeBearSay_1;
        public string GaugeBearSay_1
        {
            get { return _GaugeBearSay_1; }
            set
            {
                _GaugeBearSay_1 = value;
                OnPropertyChanged_Normal("GaugeBearSay_1");
            }
        }

        private string _GaugeBearSay_2;
        public string GaugeBearSay_2
        {
            get { return _GaugeBearSay_2; }
            set
            {
                _GaugeBearSay_2 = value;
                OnPropertyChanged_Normal("GaugeBearSay_2");
            }
        }

        private string _GaugeBearSay_3;
        public string GaugeBearSay_3
        {
            get { return _GaugeBearSay_3; }
            set
            {
                _GaugeBearSay_3 = value;
                OnPropertyChanged_Normal("GaugeBearSay_3");
            }
        }
       
        public bool GaugeContinueSelect { get; set; }

        private Visibility _GaugeMode = Visibility.Collapsed;
        public Visibility GaugeMode
        {
            get { return _GaugeMode; }
            set
            {
                _GaugeMode = value;
                OnPropertyChanged_Normal("GaugeMode");
            }
        }

        private List<DataPoint> _datapoints = new List<DataPoint>();
        public List<DataPoint> DataPoints
        {
            get { return _datapoints; }
            set
            {
                _datapoints = value;
                OnPropertyChanged("DataPoints");
            }
        }

    }
}
