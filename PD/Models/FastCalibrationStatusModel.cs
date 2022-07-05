using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using PD.ViewModel;

namespace PD.Models
{
    public class FastCalibrationStatusModel : FastCalibrationStatus_FailureMode_Model
    {

        private string _station_name = "S01";
        public string station_name
        {
            get { return _station_name; }
            set
            {
                _station_name = value;
                OnPropertyChanged("station_name");
            }
        }

        private int _ch_count = 8;
        public int ch_count
        {
            get { return _ch_count; }
            set
            {
                _ch_count = value;
                OnPropertyChanged("ch_count");
            }
        }

        private string _station_volt_measurment_A_latest = "";
        public string station_volt_measurment_A_latest
        {
            get { return _station_volt_measurment_A_latest; }
            set
            {
                _station_volt_measurment_A_latest = value;
            }
        }

        private string _station_volt_measurment_B_latest = "";
        public string station_volt_measurment_B_latest
        {
            get { return _station_volt_measurment_B_latest; }
            set
            {
                _station_volt_measurment_B_latest = value;
            }
        }

        private List<string> _station_volt_measurment_log_path_list = new List<string>();
        public List<string> station_volt_measurment_log_path_list
        {
            get { return _station_volt_measurment_log_path_list; }
            set
            {
                _station_volt_measurment_log_path_list = value;
            }
        }

        public string station_volt_measurment_directory_path { get; set; } = "";


        private List<string> _station_fix_log = new List<string>();
        public List<string> station_fix_log
        {
            get { return _station_fix_log; }
            set
            {
                _station_fix_log = value;
                OnPropertyChanged("station_fix_log");
            }
        }

        private ObservableCollection<FastCalibrationStatus_FailureMode_Model> _FailureMode_Models =
            new ObservableCollection<FastCalibrationStatus_FailureMode_Model>() { new FastCalibrationStatus_FailureMode_Model() { ch_name = " ", ch_UFV=" " } };
        public ObservableCollection<FastCalibrationStatus_FailureMode_Model> FailureMode_Models
        {
            get { return _FailureMode_Models; }
            set
            {
                _FailureMode_Models = value;
                OnPropertyChanged("FailureMode_Models");
            }
        }
    }

    public class FastCalibrationStatus_FailureMode_Model : NotifyBase
    {
        private string _ch_name = " ";
        public string ch_name
        {
            get { return _ch_name; }
            set
            {
                _ch_name = value;
                OnPropertyChanged("ch_name");
            }
        }

        private string _ch_UFV = " ";
        public string ch_UFV
        {
            get { return _ch_UFV; }
            set
            {
                _ch_UFV = value;
                OnPropertyChanged("ch_UFV");
            }
        }

        private bool _is_PDFail = false;
        public bool is_PDFail
        {
            get { return _is_PDFail; }
            set
            {
                _is_PDFail = value;
                OnPropertyChanged("is_PDFail");
            }
        }

        private bool _is_VoltFail = false;
        public bool is_VoltFail
        {
            get { return _is_VoltFail; }
            set
            {
                _is_VoltFail = value;
                OnPropertyChanged("is_VoltFail");
            }
        }
    }
}
