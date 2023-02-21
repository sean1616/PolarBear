using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using PD.ViewModel;
using PD.Utility;
using Wpf_Control_Library;

namespace PD.Models
{
    public class SettingUnitModel : NotifyBase
    {
        //public enum UC_Types
        //{
        //    Text_Textbox,
        //    Text_Button,
        //    Text_ToggleButton,
        //    ComboBox,
        //    Text_Only
        //}

        private UserControl_Mix.UC_Types _UC_Type;
        public UserControl_Mix.UC_Types UC_Type
        {
            get { return _UC_Type; }
            set
            {
                _UC_Type = value;
                OnPropertyChanged("UC_Type");
            }
        }

        private int _No;
        public int No
        {
            get { return _No; }
            set
            {
                _No = value;
                OnPropertyChanged("No");
            }
        }

        private string _Name = "";
        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                OnPropertyChanged("Name");
            }
        }

        private string _TextBox_Value;
        /// <summary>
        /// The value of Textbox
        /// </summary>
        public string TextBox_Value
        {
            get { return _TextBox_Value; }
            set
            {
                _TextBox_Value = value;
                OnPropertyChanged("TextBox_Value");
            }
        }

        private string _Tag;
        public string Tag
        {
            get { return _Tag; }
            set
            {
                _Tag = value;
                OnPropertyChanged("Tag");
            }
        }

        private bool _Toggle_IsChecked = false;
        public bool Toggle_IsChecked
        {
            get { return _Toggle_IsChecked; }
            set
            {
                _Toggle_IsChecked = value;
                OnPropertyChanged("Toggle_IsChecked");
            }
        }

        private int _Combox_SelectedIndex = 0;
        public int Combox_SelectedIndex
        {
            get { return _Combox_SelectedIndex; }
            set
            {
                _Combox_SelectedIndex = value;
                OnPropertyChanged("Combox_SelectedIndex");
            }
        }

        private string _Combox_SelectedItem = "";
        public string Combox_SelectedItem
        {
            get { return _Combox_SelectedItem; }
            set
            {
                _Combox_SelectedItem = value;
                OnPropertyChanged("Combox_SelectedItem");
            }
        }

        private ObservableCollection<string> _Combox_ItemSource = new ObservableCollection<string>();
        public ObservableCollection<string> Combox_ItemSource
        {
            get { return _Combox_ItemSource; }
            set
            {
                _Combox_ItemSource = value;
                OnPropertyChanged("Combox_ItemSource");
            }
        }
    }
}
