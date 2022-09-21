using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PD.ViewModel;
using OxyPlot;

namespace PD.Models
{
    public class ChartModel : NotifyBase
    {
        public ChartModel(int ch_count)
        {
            list_dataPoints = new List<List<DataPoint>>();
            list_dataPoints.AddRange(Enumerable.Repeat(new List<DataPoint>(), ch_count));
        }

        public ChartModel(ChartModel cm)
        {
            Chart_No = cm.Chart_No;
            title_x = cm.title_x;
            title_y = cm.title_y;
            list_delta_IL = cm.list_delta_IL;

            list_dataPoints = cm.list_dataPoints;

            TimeSpan = cm.TimeSpan;

            SN_List = cm.SN_List;
            BearSay_List = cm.BearSay_List;

            BW_Setting_1 = cm.BW_Setting_1;
            BW_Setting_2 = cm.BW_Setting_2;
            BW_Setting_3 = cm.BW_Setting_3;
            BW_Setting_4 = cm.BW_Setting_4;
            BW_1 = cm.BW_1;
            BW_2 = cm.BW_2;
            BW_3 = cm.BW_3;
            BW_4 = cm.BW_4;
            BW_CWL_1 = cm.BW_CWL_1;
            BW_CWL_2 = cm.BW_CWL_2;
            BW_CWL_3 = cm.BW_CWL_3;
            BW_CWL_4 = cm.BW_CWL_4;
        }

        public int Chart_No { get; set; }
        public string title_x { get; set; }
        public string title_y { get; set; }
        public List<double> list_delta_IL { get; set; } = new List<double>();

        public string[] preDac { get; set; }
        public List<List<DataPoint>> list_dataPoints { get; set; }

        private double _TimeSpan = 0.0;
        public double TimeSpan
        {
            get { return _TimeSpan; }
            set
            {
                _TimeSpan = value;
                OnPropertyChanged_Normal("TimeSpan");
            }
        }

        private List<string> _SN_List = new List<string>();
        public List<string> SN_List
        {
            get { return _SN_List; }
            set
            {
                _SN_List = value;
                OnPropertyChanged_Normal("SN_List");
            }
        }

        #region Bear Say List
        private List<List<string>> _BearSay_List = new List<List<string>>();
        public List<List<string>> BearSay_List
        {
            get { return _BearSay_List; }
            set
            {
                _BearSay_List = value;
                OnPropertyChanged_Normal("BearSay_List");
            }
        }
        #endregion

        #region BW 

        private double _BW_Setting_1 = 0.5;
        public double BW_Setting_1
        {
            get { return _BW_Setting_1; }
            set
            {
                _BW_Setting_1 = value;
                OnPropertyChanged_Normal("BW_Setting_1");
            }
        }

        private double _BW_Setting_2 = 1.5;
        public double BW_Setting_2
        {
            get { return _BW_Setting_2; }
            set
            {
                _BW_Setting_2 = value;
                OnPropertyChanged_Normal("BW_Setting_2");
            }
        }

        private double _BW_Setting_3 = 3;
        public double BW_Setting_3
        {
            get { return _BW_Setting_3; }
            set
            {
                _BW_Setting_3 = value;
                OnPropertyChanged_Normal("BW_Setting_3");
            }
        }

        private double _BW_Setting_4 = 20;
        public double BW_Setting_4
        {
            get { return _BW_Setting_4; }
            set
            {
                _BW_Setting_4 = value;
                OnPropertyChanged_Normal("BW_Setting_4");
            }
        }

        private double _BW_1 = 0;
        public double BW_1
        {
            get { return _BW_1; }
            set
            {
                _BW_1 = value;
                OnPropertyChanged_Normal("BW_1");
            }
        }

        private double _BW_2 = 0;
        public double BW_2
        {
            get { return _BW_2; }
            set
            {
                _BW_2 = value;
                OnPropertyChanged_Normal("BW_2");
            }
        }

        private double _BW_3 = 0;
        public double BW_3
        {
            get { return _BW_3; }
            set
            {
                _BW_3 = value;
                OnPropertyChanged_Normal("BW_3");
            }
        }

        private double _BW_4 = 0;
        public double BW_4
        {
            get { return _BW_4; }
            set
            {
                _BW_4 = value;
                OnPropertyChanged_Normal("BW_4");
            }
        }

        private double _BW_CWL_1 = 0;
        public double BW_CWL_1
        {
            get { return _BW_CWL_1; }
            set
            {
                _BW_CWL_1 = value;
                OnPropertyChanged_Normal("BW_CWL_1");
            }
        }

        private double _BW_CWL_2 = 0;
        public double BW_CWL_2
        {
            get { return _BW_CWL_2; }
            set
            {
                _BW_CWL_2 = value;
                OnPropertyChanged_Normal("BW_CWL_2");
            }
        }

        private double _BW_CWL_3 = 0;
        public double BW_CWL_3
        {
            get { return _BW_CWL_3; }
            set
            {
                _BW_CWL_3 = value;
                OnPropertyChanged_Normal("BW_CWL_3");
            }
        }

        private double _BW_CWL_4 = 0;
        public double BW_CWL_4
        {
            get { return _BW_CWL_4; }
            set
            {
                _BW_CWL_4 = value;
                OnPropertyChanged_Normal("BW_CWL_4");
            }
        }

        #endregion
    }

    public class OpticalPropertyModel : NotifyBase
    {
        public int WL_No { get; set; } = 1;

        private string _SN = "27A0KPAGXXXX";
        public string SN
        {
            get { return _SN; }
            set
            {
                _SN = value;
                OnPropertyChanged_Normal("SN");
            }
        }

        #region WL setting
        private string _WL_Setting_1 = "1535 nm";
        public string WL_Setting_1
        {
            get { return _WL_Setting_1; }
            set
            {
                _WL_Setting_1 = value;
                OnPropertyChanged_Normal("WL_Setting_1");
            }
        }

        private string _WL_Setting_2 = "1550 nm";
        public string WL_Setting_2
        {
            get { return _WL_Setting_2; }
            set
            {
                _WL_Setting_2 = value;
                OnPropertyChanged_Normal("WL_Setting_2");
            }
        }

        private string _WL_Setting_3 = "1565 nm";
        public string WL_Setting_3
        {
            get { return _WL_Setting_3; }
            set
            {
                _WL_Setting_3 = value;
                OnPropertyChanged_Normal("WL_Setting_3");
            }
        }
        #endregion

        #region CWL
        private double _WL_1_CWL = 0;
        public double WL_1_CWL
        {
            get { return _WL_1_CWL; }
            set
            {
                _WL_1_CWL = value;
                OnPropertyChanged_Normal("WL_1_CWL");
            }
        }

        private double _WL_3_CWL = 0;
        public double WL_3_CWL
        {
            get { return _WL_3_CWL; }
            set
            {
                _WL_3_CWL = value;
                OnPropertyChanged_Normal("WL_3_CWL");
            }
        }

        private double _WL_2_CWL = 0;
        public double WL_2_CWL
        {
            get { return _WL_2_CWL; }
            set
            {
                _WL_2_CWL = value;
                OnPropertyChanged_Normal("WL_2_CWL");
            }
        }

        private double _WL_4_CWL = 0;
        public double WL_4_CWL
        {
            get { return _WL_4_CWL; }
            set
            {
                _WL_4_CWL = value;
                OnPropertyChanged_Normal("WL_4_CWL");
            }
        }
        #endregion

        #region IL
        private string _WL_1_IL = "0";
        public string WL_1_IL
        {
            get { return _WL_1_IL; }
            set
            {
                _WL_1_IL = value;
                OnPropertyChanged_Normal("WL_1_IL");
            }
        }

        private string _WL_2_IL = "0";
        public string WL_2_IL
        {
            get { return _WL_2_IL; }
            set
            {
                _WL_2_IL = value;
                OnPropertyChanged_Normal("WL_2_IL");
            }
        }

        private string _WL_3_IL = "0";
        public string WL_3_IL
        {
            get { return _WL_3_IL; }
            set
            {
                _WL_3_IL = value;
                OnPropertyChanged_Normal("WL_3_IL");
            }
        }

        private string _WL_4_IL = "0";
        public string WL_4_IL
        {
            get { return _WL_4_IL; }
            set
            {
                _WL_4_IL = value;
                OnPropertyChanged_Normal("WL_4_IL");
            }
        }
        #endregion

        #region PDL
        private bool _Is_PDL_Auto_Scan = true;
        public bool Is_PDL_Auto_Scan
        {
            get { return _Is_PDL_Auto_Scan; }
            set
            {
                _Is_PDL_Auto_Scan = value;
                OnPropertyChanged_Normal("Is_PDL_Auto_Scan");
            }
        }

        private int _PDL_Scan_Time = 15;
        public int PDL_Scan_Time
        {
            get { return _PDL_Scan_Time; }
            set
            {
                _PDL_Scan_Time = value;
                OnPropertyChanged_Normal("PDL_Scan_Time");
            }
        }

        private double _WL_1_PDL = 0;
        public double WL_1_PDL
        {
            get { return _WL_1_PDL; }
            set
            {
                _WL_1_PDL = value;
                OnPropertyChanged_Normal("WL_1_PDL");
            }
        }

        private double _WL_2_PDL = 0;
        public double WL_2_PDL
        {
            get { return _WL_2_PDL; }
            set
            {
                _WL_2_PDL = value;
                OnPropertyChanged_Normal("WL_2_PDL");
            }
        }

        private double _WL_3_PDL = 0;
        public double WL_3_PDL
        {
            get { return _WL_3_PDL; }
            set
            {
                _WL_3_PDL = value;
                OnPropertyChanged_Normal("WL_3_PDL");
            }
        }

        private double _WL_4_PDL = 0;
        public double WL_4_PDL
        {
            get { return _WL_4_PDL; }
            set
            {
                _WL_4_PDL = value;
                OnPropertyChanged_Normal("WL_4_PDL");
            }
        }
        #endregion

        #region Micrometer
        private double _Mic_Upper_Limit = 0;
        public double Mic_Upper_Limit
        {
            get { return _Mic_Upper_Limit; }
            set
            {
                _Mic_Upper_Limit = value;
                OnPropertyChanged_Normal("Mic_Upper_Limit");
            }
        }

        private double _Mic_Lower_Limit = 0;
        public double Mic_Lower_Limit
        {
            get { return _Mic_Lower_Limit; }
            set
            {
                _Mic_Lower_Limit = value;
                OnPropertyChanged_Normal("Mic_Lower_Limit");
            }
        }

        private double _WL_1_Mic = 0;
        public double WL_1_Mic
        {
            get { return _WL_1_Mic; }
            set
            {
                _WL_1_Mic = value;
                OnPropertyChanged_Normal("WL_1_Mic");
            }
        }

        private double _WL_2_Mic = 0;
        public double WL_2_Mic
        {
            get { return _WL_2_Mic; }
            set
            {
                _WL_2_Mic = value;
                OnPropertyChanged_Normal("WL_2_Mic");
            }
        }

        private double _WL_3_Mic = 0;
        public double WL_3_Mic
        {
            get { return _WL_3_Mic; }
            set
            {
                _WL_3_Mic = value;
                OnPropertyChanged_Normal("WL_3_Mic");
            }
        }

        private double _WL_4_Mic = 0;
        public double WL_4_Mic
        {
            get { return _WL_4_Mic; }
            set
            {
                _WL_4_Mic = value;
                OnPropertyChanged_Normal("WL_4_Mic");
            }
        }
        #endregion

        #region Temp.
        private double _HighPower_Temp = 0;
        public double HighPower_Temp
        {
            get { return _HighPower_Temp; }
            set
            {
                _HighPower_Temp = value;
                OnPropertyChanged_Normal("HighPower_Temp");
            }
        }
        #endregion

        #region BW

        private double _BW_Setting_1 = 0.5;
        public double BW_Setting_1
        {
            get { return _BW_Setting_1; }
            set
            {
                _BW_Setting_1 = value;
                OnPropertyChanged_Normal("BW_Setting_1");
            }
        }

        private double _BW_Setting_2 = 3;
        public double BW_Setting_2
        {
            get { return _BW_Setting_2; }
            set
            {
                _BW_Setting_2 = value;
                OnPropertyChanged_Normal("BW_Setting_2");
            }
        }

        private double _BW_Setting_3 = 20;
        public double BW_Setting_3
        {
            get { return _BW_Setting_3; }
            set
            {
                _BW_Setting_3 = value;
                OnPropertyChanged_Normal("BW_Setting_3");
            }
        }

        private double _WL_1_BW_1 = 0;
        public double WL_1_BW_1
        {
            get { return _WL_1_BW_1; }
            set
            {
                _WL_1_BW_1 = value;
                OnPropertyChanged_Normal("WL_1_BW_1");
            }
        }

        private double _WL_1_BW_2 = 0;
        public double WL_1_BW_2
        {
            get { return _WL_1_BW_2; }
            set
            {
                _WL_1_BW_2 = value;
                OnPropertyChanged_Normal("WL_1_BW_2");
            }
        }

        private double _WL_1_BW_3 = 0;
        public double WL_1_BW_3
        {
            get { return _WL_1_BW_3; }
            set
            {
                _WL_1_BW_3 = value;
                OnPropertyChanged_Normal("WL_1_BW_3");
            }
        }


        private double _WL_2_BW_1 = 0;
        public double WL_2_BW_1
        {
            get { return _WL_2_BW_1; }
            set
            {
                _WL_2_BW_1 = value;
                OnPropertyChanged_Normal("WL_2_BW_1");
            }
        }

        private double _WL_2_BW_2 = 0;
        public double WL_2_BW_2
        {
            get { return _WL_2_BW_2; }
            set
            {
                _WL_2_BW_2 = value;
                OnPropertyChanged_Normal("WL_2_BW_2");
            }
        }

        private double _WL_2_BW_3 = 0;
        public double WL_2_BW_3
        {
            get { return _WL_2_BW_3; }
            set
            {
                _WL_2_BW_3 = value;
                OnPropertyChanged_Normal("WL_2_BW_3");
            }
        }


        private double _WL_3_BW_1 = 0;
        public double WL_3_BW_1
        {
            get { return _WL_3_BW_1; }
            set
            {
                _WL_3_BW_1 = value;
                OnPropertyChanged_Normal("WL_3_BW_1");
            }
        }

        private double _WL_3_BW_2 = 0;
        public double WL_3_BW_2
        {
            get { return _WL_3_BW_2; }
            set
            {
                _WL_3_BW_2 = value;
                OnPropertyChanged_Normal("WL_3_BW_2");
            }
        }

        private double _WL_3_BW_3 = 0;
        public double WL_3_BW_3
        {
            get { return _WL_3_BW_3; }
            set
            {
                _WL_3_BW_3 = value;
                OnPropertyChanged_Normal("WL_3_BW_3");
            }
        }

        private double _WL_4_BW_1 = 0;
        public double WL_4_BW_1
        {
            get { return _WL_4_BW_1; }
            set
            {
                _WL_4_BW_1 = value;
                OnPropertyChanged_Normal("WL_4_BW_1");
            }
        }

        private double _WL_4_BW_2 = 0;
        public double WL_4_BW_2
        {
            get { return _WL_4_BW_2; }
            set
            {
                _WL_4_BW_2 = value;
                OnPropertyChanged_Normal("WL_4_BW_2");
            }
        }

        private double _WL_4_BW_3 = 0;
        public double WL_4_BW_3
        {
            get { return _WL_4_BW_3; }
            set
            {
                _WL_4_BW_3 = value;
                OnPropertyChanged_Normal("WL_4_BW_3");
            }
        }

        #endregion
    }
}
