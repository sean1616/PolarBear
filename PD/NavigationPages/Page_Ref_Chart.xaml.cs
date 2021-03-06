using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PD.ViewModel;
using PD.NavigationPages;
using OxyPlot;
using OxyPlot.Wpf;

namespace PD.NavigationPages
{
    /// <summary>
    /// Page_Ref_Chart.xaml 的互動邏輯
    /// </summary>
    public partial class Page_Ref_Chart : UserControl
    {
        ComViewModel vm;
        Window_Ref window;

        public Page_Ref_Chart(ComViewModel vm, List<DataPoint> list_datapoint)
        {
            InitializeComponent();

            this.vm = vm;
            this.DataContext = vm;

            int count_ch = vm.list_WL_Ref.Count;
            for (int ch = 0; ch < count_ch; ch++)
            {
                if (vm.list_WL_Ref[ch].Count == 0)
                    continue;

                LinearAxis linearAxis = new LinearAxis();
                linearAxis.MajorGridlineStyle = LineStyle.Solid;
                linearAxis.MinorGridlineStyle = LineStyle.Dot;
                linearAxis.Position = OxyPlot.Axes.AxisPosition.Left;
                linearAxis.Minimum = -8.5;
                linearAxis.Maximum = 0.1;
                linearAxis.Minimum = vm.list_WL_Ref[ch].Min() - 0.1;
                linearAxis.Maximum = vm.list_WL_Ref[ch].Max() + 0.1;

                LinearAxis linearAxis2 = new LinearAxis();
                linearAxis2.MajorGridlineStyle = LineStyle.Solid;
                linearAxis2.MinorGridlineStyle = LineStyle.Dot;
                linearAxis2.Position = OxyPlot.Axes.AxisPosition.Bottom;
                linearAxis2.Minimum = vm.list_wl.Min();
                linearAxis2.Maximum = vm.list_wl.Max();
               
                switch (ch)
                {
                    case 0:
                        Plot_Ref1.Axes.Add(linearAxis);
                        Plot_Ref1.Axes.Add(linearAxis2);
                        break;
                    case 1:
                        Plot_Ref2.Axes.Add(linearAxis);
                        Plot_Ref2.Axes.Add(linearAxis2);
                        break;
                    case 2:
                        Plot_Ref3.Axes.Add(linearAxis);
                        Plot_Ref3.Axes.Add(linearAxis2);
                        break;
                    case 3:
                        Plot_Ref4.Axes.Add(linearAxis);
                        Plot_Ref4.Axes.Add(linearAxis2);
                        break;
                    case 4:
                        Plot_Ref5.Axes.Add(linearAxis);
                        Plot_Ref5.Axes.Add(linearAxis2);
                        break;
                    case 5:
                        Plot_Ref6.Axes.Add(linearAxis);
                        Plot_Ref6.Axes.Add(linearAxis2);
                        break;
                    case 6:
                        Plot_Ref7.Axes.Add(linearAxis);
                        Plot_Ref7.Axes.Add(linearAxis2);
                        break;
                    case 7:
                        Plot_Ref8.Axes.Add(linearAxis);
                        Plot_Ref8.Axes.Add(linearAxis2);
                        break;
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void Plot_Chart_1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Plot obj = (Plot)sender;
            string[] obj_name = obj.Name.Split('f');
            int ch = Convert.ToInt16(obj_name[1]);
            window = new Window_Ref(vm, ch);

            vm.Chart_DataPoints_ref = vm.Chart_All_DataPoints_ref[ch - 1];
            window.Show();
        }
    }
}
