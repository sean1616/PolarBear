using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PD.ViewModel;

namespace PD.NavigationPages
{
    /// <summary>
    /// Page_Chart.xaml 的互動邏輯
    /// </summary>
    public partial class Page_Chart : UserControl
    {
        //public PlotModel MyModel { get; private set; }
        public new string Title { get; private set; }
        ComViewModel vm;
        //public IList<DataPoint> Points { get; private set; }

        public Page_Chart(ComViewModel vm)
        {
            InitializeComponent();

            this.DataContext = vm;
            this.vm = vm;

            vm.IsCheck = new List<bool>() { true, false, false, false, false, false, false, false };           
        }

        private void Plot_Chart_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            List<bool> list_ischeck = vm.IsCheck;
            vm.IsCheck = new List<bool>(list_ischeck);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            bool j = false;  //if one channel is not checked
            foreach(bool b in vm.IsCheck)
            {
                j = b;

                if (!j)
                    break;
            }

            if (j)
            {
                vm.IsCheck = new List<bool>() { false, false, false, false, false, false, false, false };
                cbox_all.IsChecked = false;
            }                
            else
            {
                vm.IsCheck = new List<bool>() { true, true, true, true, true, true, true, true };
                cbox_all.IsChecked = true;
            }                            
        }

        private void btn_deltaIL_Click(object sender, RoutedEventArgs e)
        {            
            string str_allCH_maxDeltaIL = "";

            try
            {
                for (int i = 0; i < 8; i++)
                {
                    List<double> list_all_PD_Value = new List<double>();
                    int c = vm.Chart_All_DataPoints[i].Count();
                    for (int j = 0; j < c; j++)
                    {
                        list_all_PD_Value.Add(vm.Chart_All_DataPoints[i][j].Y);
                    }
                    string str_maxDeltaIL = (list_all_PD_Value.Max() - list_all_PD_Value.Min()).ToString();

                    if (str_allCH_maxDeltaIL != "")
                        str_allCH_maxDeltaIL = str_allCH_maxDeltaIL + Environment.NewLine + "Delta IL : " + str_maxDeltaIL;
                    else
                        str_allCH_maxDeltaIL = "Delta IL : " + str_maxDeltaIL;
                }
                MessageBox.Show(str_allCH_maxDeltaIL);
            }
            catch { MessageBox.Show("Null Data"); }
        }
                
        private void btn_previous_Click(object sender, RoutedEventArgs e)
        {
            if (vm.int_chart_now > 1)
            {
                vm.int_chart_now --;
                vm.Chart_All_DataPoints = new List<List<OxyPlot.DataPoint>>(vm.Chart_All_Datapoints_History[vm.int_chart_now - 1]);
                if (vm.Chart_All_DataPoints.Count == 0) return;
                vm.Chart_DataPoints = new List<OxyPlot.DataPoint>(vm.Chart_All_DataPoints[0]);
            }            
        }

        private void btn_next_Click(object sender, RoutedEventArgs e)
        {
            if (vm.int_chart_now >= vm.int_chart_count)
                return;
                
            vm.Chart_All_DataPoints = new List<List<OxyPlot.DataPoint>>(vm.Chart_All_Datapoints_History[vm.int_chart_now]);

            if (vm.Chart_All_DataPoints.Count != 0)
                vm.Chart_DataPoints = new List<OxyPlot.DataPoint>(vm.Chart_All_DataPoints[0]);

            vm.int_chart_now++;
        }
    }
}
