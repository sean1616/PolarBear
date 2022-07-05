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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using PD.Models;

namespace PD.UI
{
    /// <summary>
    /// Interaction logic for UC_Chamber_Status.xaml
    /// </summary>
    public partial class UC_Chamber_Status : UserControl
    {
        public UC_Chamber_Status()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty station_name_Property =
                  DependencyProperty.Register("txt_station_name", typeof(string), typeof(UC_Chamber_Status),
                  new UIPropertyMetadata(null));

        public string txt_station_name //提供內部binding之相依屬性
        {
            get { return (string)GetValue(station_name_Property); }
            set { SetValue(station_name_Property, value); }
        }

        public event RoutedEventHandler Btn_station_name_Click = delegate { };
        private void btn_station_name_Click(object sender, RoutedEventArgs e)
        {
            Btn_station_name_Click(sender, e);
        }
              

        //public static readonly DependencyProperty FastCalibrationStatusModel_Property =
        //         DependencyProperty.Register("fastCalibrationStatusModel", typeof(FastCalibrationStatusModel), typeof(UC_Chamber_Status),
        //         new UIPropertyMetadata(null));

        //public FastCalibrationStatusModel fastCalibrationStatusModel //提供內部binding之相依屬性
        //{
        //    get { return (FastCalibrationStatusModel)GetValue(FastCalibrationStatusModel_Property); }
        //    set { SetValue(FastCalibrationStatusModel_Property, value); }
        //}
    }
}
