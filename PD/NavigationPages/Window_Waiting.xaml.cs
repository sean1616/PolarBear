using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace PD.NavigationPages
{
    /// <summary>
    /// Window_Waiting.xaml 的互動邏輯
    /// </summary>
    public partial class Window_Waiting : Window , INotifyPropertyChanged
    {
        DispatcherTimer timer_Circle_Opacity_UI;

        private ObservableCollection<double> _list_opa = new ObservableCollection<double>() { 0.99, 0.88, 0.77, 0.66, 0.55, 0.44, 0.33, 0.22, 0.11 };
        public ObservableCollection<double> list_opa
        {
            get { return _list_opa; }
            set
            {
                _list_opa = value;
                OnPropertyChanged("list_opa");
            }
        }

        private string _center_msg = "Loading";
        public string Center_MSG
        {
            get { return _center_msg; }
            set
            {
                _center_msg = value;
                OnPropertyChanged("Center_MSG");
            }
        }

        public Window_Waiting()
        {
            InitializeComponent();

            this.Left = System.Windows.Forms.Screen.AllScreens.FirstOrDefault().WorkingArea.Left;
            this.Top = System.Windows.Forms.Screen.AllScreens.FirstOrDefault().WorkingArea.Top;

            this.DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer_Circle_Opacity_UI = new DispatcherTimer();
            timer_Circle_Opacity_UI.Interval = TimeSpan.FromMilliseconds(100);
            timer_Circle_Opacity_UI.Tick += _timer_Circle_Opacity_UI;
            timer_Circle_Opacity_UI.Start();
        }

        void _timer_Circle_Opacity_UI(object sender, EventArgs e)
        {
            for (int i = 0; i < list_opa.Count; i++)
            {
                if (list_opa[i] >= 0.22)
                    list_opa[i] -= 0.11;
                else
                    list_opa[i] = 0.99;
            }
            list_opa = new ObservableCollection<double>(list_opa);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
