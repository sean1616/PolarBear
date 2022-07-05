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
using PD.UI;
using PD.ViewModel;

namespace PD.NavigationPages
{
    /// <summary>
    /// Interaction logic for Page_Chamber_Status.xaml
    /// </summary>
    public partial class Page_Chamber_Status : UserControl
    {
        ComViewModel vm;
        public Page_Chamber_Status(ComViewModel vm)
        {
            InitializeComponent();

            this.vm = vm;
            this.DataContext = vm;
        }

        private void uc_chambr_Btn_station_name_Click(object sender, RoutedEventArgs e)
        {
            Button obj = (Button)sender;

            string name = obj.Content.ToString();

            for (int i = 0; i < vm.List_FastCalibration_Status.Count; i++)
            {
                if(vm.List_FastCalibration_Status[i].station_name == name)
                {
                    Process process = new Process();
                    process.StartInfo.FileName = vm.List_FastCalibration_Status[i].station_volt_measurment_directory_path;
                    process.Start();
                    break;
                }
            }

            
        }
    }
}
