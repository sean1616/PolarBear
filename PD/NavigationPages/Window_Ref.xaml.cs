using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using PD.ViewModel;
using OxyPlot;

namespace PD.NavigationPages
{
    public class Collection_Ref
    {
        public Collection_Ref(ObservableCollection<DataPoint> dataPoints)
        {
            this.Title = "Ref";
                       
            Ref_Data = new List<DataPoint>();
            foreach (var item in dataPoints)
                Ref_Data.Add(item);           
        }

        public string Title { get; private set; }
        public IList<DataPoint> Ref_Data { get; private set; }       
    }

    /// <summary>
    /// Window_Ref.xaml 的互動邏輯
    /// </summary>
    /// 
    public partial class Window_Ref : Window
    {
        ComViewModel vm;
        int ch = 0;
        Collection_Ref _Collection_Ref;

        public Window_Ref(ComViewModel vm, int ch)
        {
            InitializeComponent();

            this.vm = vm;
            
            //this.DataContext = vm;
                        
            this.ch = ch;
            Plot_Ref.Title = "Ref" + ch.ToString();

            _Collection_Ref = new Collection_Ref(vm.Chart_DataPoints_ref);            
                                              
            this.DataContext = _Collection_Ref;
        }

        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            axis_left.Minimum = vm.Ref_Dictionaries[ch - 1].Values.Min() - 0.1;
            axis_left.Maximum = vm.Ref_Dictionaries[ch - 1].Values.Max() + 0.1;

            axis_bottom.Minimum = vm.list_wl.Min();
            axis_bottom.Maximum = vm.list_wl.Max();
        }
                
        private void Btn_next_Click(object sender, RoutedEventArgs e)
        {
            if (ch < vm.ch_count) ch++;
            else ch = 1;

            _Collection_Ref.Ref_Data.Clear();
            foreach (var item in vm.Chart_All_DataPoints_ref[ch - 1])
            {
                _Collection_Ref.Ref_Data.Add(item);
            }

            if (vm.Ref_Dictionaries[ch -1].Count > 0)
            {
                axis_left.Minimum = vm.Ref_Dictionaries[ch - 1].Values.Min() - 0.1;
                axis_left.Maximum = vm.Ref_Dictionaries[ch - 1].Values.Max() + 0.1;
            }

            axis_bottom.Minimum = vm.list_wl.Min();
            axis_bottom.Maximum = vm.list_wl.Max();

            Plot_Ref.Title = "Ref" + ch.ToString();
        }

        private void Btn_previous_Click(object sender, RoutedEventArgs e)
        {
            if (ch > 1) ch--;
            else ch = vm.ch_count;

            _Collection_Ref.Ref_Data.Clear();
            foreach (var item in vm.Chart_All_DataPoints_ref[ch - 1])
            {
                _Collection_Ref.Ref_Data.Add(item);
            }

            if (vm.Ref_Dictionaries[ch - 1].Count > 0)
            {
                axis_left.Minimum = vm.Ref_Dictionaries[ch - 1].Values.Min() - 0.1;
                axis_left.Maximum = vm.Ref_Dictionaries[ch - 1].Values.Max() + 0.1;
            }                

            axis_bottom.Minimum = vm.list_wl.Min();
            axis_bottom.Maximum = vm.list_wl.Max();

            Plot_Ref.Title = "Ref" + ch.ToString();
        }
    }
}
