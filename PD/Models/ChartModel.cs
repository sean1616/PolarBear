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
}
