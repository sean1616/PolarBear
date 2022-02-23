using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using PD.ViewModel;
using OxyPlot;

namespace PD.Models
{
    public class Chart_UI_Model : NotifyBase
    {
        private int _Button_Channel;
        public int Button_Channel
        {
            get { return _Button_Channel; }
            set
            {
                _Button_Channel = value;
                OnPropertyChanged_Normal("Button_Channel");
            }
        }

        private string _Button_Content = "";
        public string Button_Content
        {
            get { return _Button_Content; }
            set
            {
                _Button_Content = value;
                OnPropertyChanged_Normal("Button_Content");
            }
        }

        private string _Button_Tag = "";
        public string Button_Tag
        {
            get { return _Button_Tag; }
            set
            {
                _Button_Tag = value;
                OnPropertyChanged_Normal("Button_Tag");
            }
        }

        private bool _Button_IsChecked = false;
        public bool Button_IsChecked
        {
            get { return _Button_IsChecked; }
            set
            {
                _Button_IsChecked = value;
                OnPropertyChanged_Normal("Button_IsChecked");
            }
        }

        private Visibility _Button_IsVisible = Visibility.Visible;
        public Visibility Button_IsVisible
        {
            get { return _Button_IsVisible; }
            set
            {
                _Button_IsVisible = value;
                OnPropertyChanged_Normal("Button_IsVisible");
            }
        }

        public SolidColorBrush Button_Color { get; set; } = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF10E2C4"));
    }
}
