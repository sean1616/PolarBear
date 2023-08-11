using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Controls;
using PD.ViewModel;
using PD.Models;

namespace PD.NavigationPages
{
    /// <summary>
    /// Page_Ref_Grid.xaml 的互動邏輯
    /// </summary>
    public partial class Page_Ref_Grid : UserControl
    {
        ComViewModel vm;
        //ObservableCollection<RefModel> Ref_memberDatas = new ObservableCollection<RefModel>();

        List<Dictionary<double, double>> dictionaries = new List<System.Collections.Generic.Dictionary<double, double>>();

        public Page_Ref_Grid(ComViewModel vm, string path)
        {
            InitializeComponent();

            DataContext = vm;
            this.vm = vm;

            dataGrid.DataContext = vm.Ref_memberDatas;

            for (int i = 0; i < 16; i++)
            {
                dictionaries.Add(new Dictionary<double, double>());  //Initialize dictionaries for all ref file
            }

            if (System.IO.Directory.Exists(path))
                vm.Read_Ref(path);
            else
                vm.Str_cmd_read = "Ref folder is not exist or different name";

            //Auto add DataGrid Column by vm.Ref_Dictionaries.Count
            if (vm.Ref_Dictionaries.Count > 8)
            {
                for (int i = 9; i <= vm.Ref_Dictionaries.Count; i++)
                {
                    var col = new DataGridTextColumn();
                    string channel = string.Concat("Ch_", i.ToString());
                    col.Header = channel;
                    col.Binding = new Binding(channel);
                    col.Width = new DataGridLength(80, DataGridLengthUnitType.Star);
                    col.MinWidth = 80;
                    dataGrid.Columns.Add(col);
                }
            }

            vm.float_WL_Ref = new List<double>();  //目前波長的Ref值

            //從字典找Ref值
            try
            {
                if (vm.Double_Laser_Wavelength == 0)
                {
                    vm.Save_Log("Get ref", "Laser Wavelength : 0", false);
                    return;
                }

                if (vm.Ref_Dictionaries.Count >= vm.ch_count)
                {
                    System.Threading.Tasks.Parallel.For(0, vm.ch_count, ch =>
                    {
                        vm.float_WL_Ref.Add(vm.Ref_Dictionaries[ch].ContainsKey(vm.Double_Laser_Wavelength) ? vm.Ref_Dictionaries[ch][vm.Double_Laser_Wavelength] : 0);
                    });
                }
            }
            catch { }

        }
    }
}
