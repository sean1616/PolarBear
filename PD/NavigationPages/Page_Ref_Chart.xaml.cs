using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public Page_Ref_Chart(ComViewModel vm, ObservableCollection<DataPoint> list_datapoint)
        {
            InitializeComponent();

            this.vm = vm;
            this.DataContext = vm;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            for (int ch = 0; ch < vm.Ref_Dictionaries.Count; ch++)
            {
                if (vm.Ref_Dictionaries[ch].Count == 0) continue;

                LinearAxis linearAxis = new LinearAxis();
                linearAxis.MajorGridlineStyle = LineStyle.Solid;
                linearAxis.MinorGridlineStyle = LineStyle.Dot;
                linearAxis.Position = OxyPlot.Axes.AxisPosition.Left;
                linearAxis.Minimum = -8.5;
                linearAxis.Maximum = 0.1;
                linearAxis.Minimum = vm.Ref_Dictionaries[ch].Values.Min() - 0.1;
                linearAxis.Maximum = vm.Ref_Dictionaries[ch].Values.Max() + 0.1;

                LinearAxis linearAxis2 = new LinearAxis();
                linearAxis2.MajorGridlineStyle = LineStyle.Solid;
                linearAxis2.MinorGridlineStyle = LineStyle.Dot;
                linearAxis2.Position = OxyPlot.Axes.AxisPosition.Bottom;
                linearAxis2.Minimum = vm.list_wl.Min();
                linearAxis2.Maximum = vm.list_wl.Max();

                

                switch (ch)
                {
                    case 0:
                        Plot_Ref1.Axes.Clear();
                        Plot_Ref1.Axes.Add(linearAxis);
                        Plot_Ref1.Axes.Add(linearAxis2);
                        break;
                    case 1:
                        Plot_Ref2.Axes.Clear();
                        Plot_Ref2.Axes.Add(linearAxis);
                        Plot_Ref2.Axes.Add(linearAxis2);
                        break;
                    case 2:
                        Plot_Ref3.Axes.Clear();
                        Plot_Ref3.Axes.Add(linearAxis);
                        Plot_Ref3.Axes.Add(linearAxis2);
                        break;
                    case 3:
                        Plot_Ref4.Axes.Clear();
                        Plot_Ref4.Axes.Add(linearAxis);
                        Plot_Ref4.Axes.Add(linearAxis2);
                        break;
                    case 4:
                        Plot_Ref5.Axes.Clear();
                        Plot_Ref5.Axes.Add(linearAxis);
                        Plot_Ref5.Axes.Add(linearAxis2);
                        break;
                    case 5:
                        Plot_Ref6.Axes.Clear();
                        Plot_Ref6.Axes.Add(linearAxis);
                        Plot_Ref6.Axes.Add(linearAxis2);
                        break;
                    case 6:
                        Plot_Ref7.Axes.Clear();
                        Plot_Ref7.Axes.Add(linearAxis);
                        Plot_Ref7.Axes.Add(linearAxis2);
                        break;
                    case 7:
                        Plot_Ref8.Axes.Clear();
                        Plot_Ref8.Axes.Add(linearAxis);
                        Plot_Ref8.Axes.Add(linearAxis2);
                        break;
                    case 8:
                        Plot_Ref9.Axes.Clear();
                        Plot_Ref9.Axes.Add(linearAxis);
                        Plot_Ref9.Axes.Add(linearAxis2);
                        break;
                    case 9:
                        Plot_Ref10.Axes.Clear();
                        Plot_Ref10.Axes.Add(linearAxis);
                        Plot_Ref10.Axes.Add(linearAxis2);
                        break;
                    case 10:
                        Plot_Ref11.Axes.Clear();
                        Plot_Ref11.Axes.Add(linearAxis);
                        Plot_Ref11.Axes.Add(linearAxis2);
                        break;
                    case 11:
                        Plot_Ref12.Axes.Clear();
                        Plot_Ref12.Axes.Add(linearAxis);
                        Plot_Ref12.Axes.Add(linearAxis2);
                        break;
                    case 12:
                        Plot_Ref13.Axes.Clear();
                        Plot_Ref13.Axes.Add(linearAxis);
                        Plot_Ref13.Axes.Add(linearAxis2);
                        break;
                    case 13:
                        Plot_Ref14.Axes.Clear();
                        Plot_Ref14.Axes.Add(linearAxis);
                        Plot_Ref14.Axes.Add(linearAxis2);
                        break;
                    case 14:
                        Plot_Ref15.Axes.Clear();
                        Plot_Ref15.Axes.Add(linearAxis);
                        Plot_Ref15.Axes.Add(linearAxis2);
                        break;
                    case 15:
                        Plot_Ref16.Axes.Clear();
                        Plot_Ref16.Axes.Add(linearAxis);
                        Plot_Ref16.Axes.Add(linearAxis2);
                        break;
                }
            }

            if (vm.Ref_Dictionaries.Count <= 4 && vm.Ref_Dictionaries.Count < 8)
            {
                grid_1.Visibility = Visibility.Visible;
                grid_2.Visibility = Visibility.Collapsed;
                grid_3.Visibility = Visibility.Collapsed;
                grid_4.Visibility = Visibility.Collapsed;
            }
            else if (vm.Ref_Dictionaries.Count <= 8 && vm.Ref_Dictionaries.Count < 12)
            {
                grid_1.Visibility = Visibility.Visible;
                grid_2.Visibility = Visibility.Visible;
                grid_3.Visibility = Visibility.Collapsed;
                grid_4.Visibility = Visibility.Collapsed;
            }              
            else if (vm.Ref_Dictionaries.Count <= 12 && vm.Ref_Dictionaries.Count < 16)
            {
                grid_1.Visibility = Visibility.Visible;
                grid_2.Visibility = Visibility.Visible;
                grid_3.Visibility = Visibility.Visible;
                grid_4.Visibility = Visibility.Collapsed;
            }              
            else if (vm.Ref_Dictionaries.Count <= 16 && vm.Ref_Dictionaries.Count < 24)
            {
                grid_1.Visibility = Visibility.Visible;
                grid_2.Visibility = Visibility.Visible;
                grid_3.Visibility = Visibility.Visible;
                grid_4.Visibility = Visibility.Visible;
            }
                
        }

        private void Plot_Chart_1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Plot obj = (Plot)sender;
            string[] obj_name = obj.Name.Split('f');
            int ch = Convert.ToInt16(obj_name[1]);
            
            vm.Chart_DataPoints_ref.Clear();
            foreach (var item in vm.Chart_All_DataPoints_ref[ch - 1])
            {
                vm.Chart_DataPoints_ref.Add(item);
            }
            //vm.Chart_DataPoints_ref = vm.Chart_All_DataPoints_ref[ch - 1];
            window = new Window_Ref(vm, ch);
            window.Show();
        }
    }
}
