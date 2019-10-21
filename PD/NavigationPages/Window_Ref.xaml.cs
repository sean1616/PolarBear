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
using PD.ViewModel;
using OxyPlot.Wpf;

namespace PD.NavigationPages
{
    /// <summary>
    /// Window_Ref.xaml 的互動邏輯
    /// </summary>
    public partial class Window_Ref : Window
    {
        ComViewModel vm;
        int ch = 0;

        public Window_Ref(ComViewModel vm, int ch)
        {
            InitializeComponent();

            this.DataContext = vm;
            this.vm = vm;

            this.ch = ch;
            Plot_Ref.Title = "Ref" + ch.ToString();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            axis_left.Minimum = vm.list_WL_Ref[ch - 1].Min() - 0.1;
            axis_left.Maximum = vm.list_WL_Ref[ch - 1].Max() + 0.1;

            axis_bottom.Minimum = vm.list_wl.Min();
            axis_bottom.Maximum = vm.list_wl.Max();
        }

        private void Btn_next_Click(object sender, RoutedEventArgs e)
        {
            if (ch < 8)
                ch++;
            vm.Chart_DataPoints_ref = vm.Chart_All_DataPoints_ref[ch - 1];

            axis_left.Minimum = vm.list_WL_Ref[ch - 1].Min() - 0.1;
            axis_left.Maximum = vm.list_WL_Ref[ch - 1].Max() + 0.1;

            axis_bottom.Minimum = vm.list_wl.Min();
            axis_bottom.Maximum = vm.list_wl.Max();

            Plot_Ref.Title = "Ref" + ch.ToString();
        }

        private void Btn_previous_Click(object sender, RoutedEventArgs e)
        {
            if (ch > 1)
                ch--;
            vm.Chart_DataPoints_ref = vm.Chart_All_DataPoints_ref[ch - 1];

            axis_left.Minimum = vm.list_WL_Ref[ch - 1].Min() - 0.1;
            axis_left.Maximum = vm.list_WL_Ref[ch - 1].Max() + 0.1;

            axis_bottom.Minimum = vm.list_wl.Min();
            axis_bottom.Maximum = vm.list_wl.Max();

            Plot_Ref.Title = "Ref" + ch.ToString();
        }
    }
}
