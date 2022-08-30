using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PD.ViewModel;
using OxyPlot;

namespace PD.Models
{
    public class ChannelModel : NotifyBase
    {
        public ChannelModel()
        {

        }

        public ChannelModel(ChannelModel cm)
        {
            channel = cm.channel;
            BautRate = cm.BautRate;
            Board_ID = cm.Board_ID;
            Board_Port = cm.Board_Port;
            Board_Type = cm.Board_Type;
            Board_V1_Ratio = cm.Board_V1_Ratio;
            Board_V2_Ratio = cm.Board_V2_Ratio;

            PM_Type = cm.PM_Type;

            PM_BautRate = cm.PM_BautRate;
            PM_Board_ID = cm.PM_Board_ID;
            PM_Board_Port = cm.PM_Board_Port;

            PM_GPIB_BoardNum = cm.PM_GPIB_BoardNum;
            PM_Address = cm.PM_Address;
            PM_Slot = cm.PM_Slot;
            PM_AveTime = cm.PM_AveTime;
        }

        public string channel { get; set; } = "";

        #region Power Supply Setting
        private int _BoutRate;
        public int BautRate
        {
            get { return _BoutRate; }
            set
            {
                _BoutRate = value;
                OnPropertyChanged("BautRate");
            }
        }

        private string _Board_ID = "ID";
        public string Board_ID
        {
            get { return _Board_ID; }
            set
            {
                _Board_ID = value;
                OnPropertyChanged("Board_ID");
            }
        }

        private string _Board_Port;
        public string Board_Port
        {
            get { return _Board_Port; }
            set
            {
                _Board_Port = value;
                OnPropertyChanged("Board_Port");
            }
        }

        private string _Board_Type = "UFV";
        public string Board_Type
        {
            get { return _Board_Type; }
            set
            {
                _Board_Type = value;
                OnPropertyChanged("Board_Type");
            }
        }

        private double _Board_V1_Ratio = 0;
        public double Board_V1_Ratio
        {
            get { return _Board_V1_Ratio; }
            set
            {
                _Board_V1_Ratio = value;
                OnPropertyChanged("Board_V1_Ratio");
            }
        }

        private double _Board_V2_Ratio = 0;
        public double Board_V2_Ratio
        {
            get { return _Board_V2_Ratio; }
            set
            {
                _Board_V2_Ratio = value;
                OnPropertyChanged("Board_V2_Ratio");
            }
        }

        //private List<string> _list_combox_Control_Board_Type_items =
        //    new List<string>() { "UFV", "V" };
        //public List<string> list_combox_Control_Board_Type_items
        //{
        //    get { return _list_combox_Control_Board_Type_items; }
        //    set
        //    {
        //        _list_combox_Control_Board_Type_items = value;
        //        OnPropertyChanged("list_combox_Control_Board_Type_items");
        //    }
        //}
        #endregion

        #region Power Meter Setting


        private System.Windows.Visibility _typeGPIBVis = System.Windows.Visibility.Visible;
        public System.Windows.Visibility typeGPIBVis
        {
            get { return _typeGPIBVis; }
            set
            {
                _typeGPIBVis = value;
                OnPropertyChanged_Normal("typeGPIBVis");
            }
        }

        private List<string> _list_combox_PowerMeterType_items =
            new List<string>() { "GPIB", "RS232" };
        public List<string> list_combox_PowerMeterType_items
        {
            get { return _list_combox_PowerMeterType_items; }
            set
            {
                _list_combox_PowerMeterType_items = value;
                OnPropertyChanged("list_combox_PowerMeterType_items");
            }
        }

        private string _PM_Type { get; set; } = "GPIB";
        public string PM_Type
        {
            get { return _PM_Type; }
            set
            {
                _PM_Type = value;
                OnPropertyChanged_Normal("PM_Type");

                if (_PM_Type.Equals("RS232"))
                {
                    typeGPIBVis = System.Windows.Visibility.Collapsed;
                    rs_panel_z_index = 5;
                }
                else if (_PM_Type.Equals("GPIB"))
                {
                    typeGPIBVis = System.Windows.Visibility.Visible;
                    rs_panel_z_index = 0;
                }
            }
        }

        private int _rs_panel_z_index = 0;
        public int rs_panel_z_index
        {
            get { return _rs_panel_z_index; }
            set
            {
                _rs_panel_z_index = value;
                OnPropertyChanged("rs_panel_z_index");
            }
        }

        //RS232
        private int _PM_BautRate { get; set; } = 115200;
        public int PM_BautRate
        {
            get { return _PM_BautRate; }
            set
            {
                _PM_BautRate = value;
                OnPropertyChanged("PM_BautRate");
            }
        }

        private string _PM_Board_ID { get; set; } = "";
        public string PM_Board_ID
        {
            get { return _PM_Board_ID; }
            set
            {
                _PM_Board_ID = value;
                OnPropertyChanged("PM_Board_ID");
            }
        }

        private string _PM_Board_Port = "COM1";
        public string PM_Board_Port
        {
            get { return _PM_Board_Port; }
            set
            {
                _PM_Board_Port = value;
                OnPropertyChanged("PM_Board_Port");
            }
        }

        private string _PM_GetPower_CMD { get; set; } = "P0?";
        public string PM_GetPower_CMD
        {
            get { return _PM_GetPower_CMD; }
            set
            {
                _PM_GetPower_CMD = value;
                OnPropertyChanged("PM_GetPower_CMD");
            }
        }
        //public int panel_z_index { get; set; } = 0;

        //GPIB
        private int _PM_GPIB_BoardNum { get; set; } = 0;
        public int PM_GPIB_BoardNum
        {
            get { return _PM_GPIB_BoardNum; }
            set
            {
                _PM_GPIB_BoardNum = value;
                OnPropertyChanged("PM_GPIB_BoardNum");
            }
        }

        private int _PM_Address { get; set; } = 24;
        public int PM_Address
        {
            get { return _PM_Address; }
            set
            {
                _PM_Address = value;
                OnPropertyChanged("PM_Address");
            }
        }

        private int _PM_Slot { get; set; } = 1;
        public int PM_Slot
        {
            get { return _PM_Slot; }
            set
            {
                _PM_Slot = value;
                OnPropertyChanged("PM_Slot");
            }
        }

        private int _PM_AveTime { get; set; } = 20;
        public int PM_AveTime
        {
            get { return _PM_AveTime; }
            set
            {
                _PM_AveTime = value;
                OnPropertyChanged("PM_AveTime");
            }
        }
        #endregion
    }
}
