using PD.Functions;
using PD.ViewModel;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Excel = Microsoft.Office.Interop.Excel;
using PD.Models;
using OxyPlot.Wpf;

namespace PD.NavigationPages
{
    /// <summary>
    /// Page_Chart.xaml 的互動邏輯
    /// </summary>
    public partial class Page_Chart : System.Windows.Controls.UserControl
    {
        //public PlotModel MyModel { get; private set; }
#pragma warning disable CS0109 // Member does not hide an inherited member; new keyword is not required
        public new string Title { get; private set; }
#pragma warning restore CS0109 // Member does not hide an inherited member; new keyword is not required
        ComViewModel vm;
        //public IList<DataPoint> Points { get; private set; }

        ControlCmd cmd;

        public Page_Chart(ComViewModel vm)
        {
            InitializeComponent();

            this.DataContext = vm;
            this.vm = vm;
            viewer.DataContext = vm;  //將DataContext指給使用者控制項，必要!

            cmd = new ControlCmd(vm);

            window_Bear_Grid = new Window_Bear_Grid(vm, "Delta IL");
        }
              

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox chbox = (System.Windows.Controls.CheckBox)sender;
            Chart_UI_Model uiModel = (Chart_UI_Model)chbox.DataContext;
        }

        private void ALL_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            bool judge = false;  //if one channel is not checked
            foreach (Chart_UI_Model chUIModel in vm.list_Chart_UI_Models)
            {
                judge = chUIModel.Button_IsChecked;

                if (!judge)
                    break;
            }

            if (judge)
            {
                cbox_all.IsChecked = false;
            }
            else
            {
                cbox_all.IsChecked = true;
            }

            for (int i = 0; i < vm.list_Chart_UI_Models.Count; i++)
            {
                vm.list_Chart_UI_Models[i].Button_IsChecked = !judge;
            }
        }



        private void btn_previous_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (vm.int_chart_now > 1)
                {
                    vm.int_chart_now--;
                    vm.Chart_All_DataPoints = new List<List<OxyPlot.DataPoint>>(vm.Chart_All_Datapoints_History[vm.int_chart_now - 1]);
                    if (vm.Chart_All_DataPoints.Count == 0) return;
                    vm.Chart_DataPoints = new List<OxyPlot.DataPoint>(vm.Chart_All_DataPoints[0]);

                    vm.ChartNowModel = vm.list_ChartModels[vm.int_chart_now - 1];

                    vm.Chart_x_title = vm.ChartNowModel.title_x;
                    vm.Chart_y_title = vm.ChartNowModel.title_y;

                    vm.msgModel.msg_3 = Math.Round(vm.ChartNowModel.TimeSpan, 1).ToString();

                    for (int i = 0; i < vm.ch_count; i++)
                    {
                        if (i < vm.list_ch_title.Count)
                            if (vm.list_ChartModels.Count > (vm.int_chart_now - 1))
                            {
                                if (i <= vm.list_ch_title.Count - 1 && i <= vm.ChartNowModel.list_delta_IL.Count)
                                    vm.list_ch_title[i] = string.Format("ch{0} ,Delta IL : {1}", i + 1, vm.ChartNowModel.list_delta_IL[i]);
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.StackTrace.ToString());
            }
        }

        private void btn_next_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (vm.int_chart_now >= vm.int_chart_count)
                    return;

                if (vm.Chart_All_Datapoints_History.Count <= vm.int_chart_now)
                {
                    vm.Chart_All_DataPoints = new List<List<OxyPlot.DataPoint>>(vm.Chart_All_Datapoints_History.Last());
                }
                else
                    vm.Chart_All_DataPoints = new List<List<OxyPlot.DataPoint>>(vm.Chart_All_Datapoints_History[vm.int_chart_now]);

                if (vm.Chart_All_DataPoints.Count != 0)
                    vm.Chart_DataPoints = new List<OxyPlot.DataPoint>(vm.Chart_All_DataPoints[0]);  //Update Chart UI

                vm.int_chart_now++;

                vm.ChartNowModel = vm.list_ChartModels[vm.int_chart_now - 1];

                vm.Chart_x_title = vm.ChartNowModel.title_x;
                vm.Chart_y_title = vm.ChartNowModel.title_y;

                vm.msgModel.msg_3 = Math.Round(vm.ChartNowModel.TimeSpan, 1).ToString();

                for (int i = 0; i < vm.ch_count; i++)
                {
                    if (i < vm.list_ch_title.Count)
                        if (vm.list_ChartModels.Count > (vm.int_chart_now - 1))
                        {
                            if (i <= vm.list_ch_title.Count - 1 && i <= vm.ChartNowModel.list_delta_IL.Count)
                                vm.list_ch_title[i] = string.Format("ch{0} ,Delta IL : {1}", i + 1, vm.ChartNowModel.list_delta_IL[i]);
                        }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.StackTrace.ToString());
            }
        }

        private void btn_Save_Chart_Click(object sender, RoutedEventArgs e)
        {
            #region Save Chart
            RenderTargetBitmap renderTargetBitmap =
               new RenderTargetBitmap((int)Plot_Chart.ActualWidth, (int)Plot_Chart.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(Plot_Chart);
            PngBitmapEncoder pngImage = new PngBitmapEncoder();
            pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Save Data";
            saveFileDialog.InitialDirectory = @"D:\PD";
            saveFileDialog.FileName = "Image001.jpg";
            saveFileDialog.Filter = "Image (*.jpg)|*.jpg|All files (*.*)|*.*";

            string img_filePath = @"D:\PD\Image001.jpg";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                img_filePath = saveFileDialog.FileName;

                using (Stream fileStream = File.Create(img_filePath))
                {
                    pngImage.Save(fileStream);
                }
            }

            string folder = Directory.GetParent(img_filePath).ToString();
            //await cmd.Save_Chart();
            #endregion

            #region Save Datapoint
            bool csv_or_excel = true;

            ////Save as excel file
            //try
            //{
            //    //設定檔案路徑
            //    string datapoint_filePath = Path.Combine(folder, Path.GetFileNameWithoutExtension(img_filePath));
            //    //string datapoint_filePath = @"D:\PD\" + Path.GetFileNameWithoutExtension(img_filePath);

            //    //應用程序
            //    Excel.Application Excel_App1 = new Excel.Application();
            //    //檔案
            //    Excel.Workbook Excel_WB1 = Excel_App1.Workbooks.Add();
            //    //工作表
            //    Excel.Worksheet Excel_WS1 = new Excel.Worksheet();
            //    Excel_WS1 = Excel_WB1.Worksheets[1];
            //    Excel_WS1.Name = "001";

            //    //儲存格
            //    for (int i = 1; i <= vm.Chart_All_DataPoints.Count; i++)
            //    {
            //        if (vm.Chart_All_DataPoints[i - 1].Count != 0)
            //            Excel_App1.Cells[i, 1] = "Ch " + i.ToString();

            //        if (vm.Chart_All_DataPoints[i - 1].Count > 0)
            //        {
            //            for (int j = 1; j <= vm.Chart_All_DataPoints[i - 1].Count; j++)
            //            {
            //                Excel_App1.Cells[j + 1, i] = vm.Chart_All_DataPoints[i - 1][j - 1].X.ToString();
            //            }
            //        }
            //    }

            //    //存檔
            //    Excel_WB1.SaveAs(datapoint_filePath);
            //    //關閉物件
            //    Excel_WS1 = null;
            //    Excel_WB1.Close();
            //    Excel_WB1 = null;
            //    Excel_App1.Quit();
            //    Excel_App1 = null;
            //}
            //catch { csv_or_excel = false; }

            csv_or_excel = false;

            //若電腦沒有安裝Excel，則存成Csv檔
            if (!csv_or_excel)
            {
                string csvpath = Path.Combine(folder, Path.GetFileNameWithoutExtension(img_filePath)) + ".csv";
                //string csvpath = @"D:\PD\" + Path.GetFileNameWithoutExtension(img_filePath) + ".csv";

                if (!File.Exists(csvpath))
                {
                    File.AppendAllText(csvpath, "");
                }

                //Clear file content and lock the file
                FileStream fileStream = File.Open(csvpath, FileMode.Open);
                fileStream.SetLength(0);
                fileStream.Close();

                StringBuilder _stringBuilder = new StringBuilder();

                _stringBuilder.Append(vm.Chart_x_title);
                _stringBuilder.Append(",");

                //資料Title
                for (int i = 0; i < vm.Chart_All_DataPoints.Count; i++)
                {
                    if (vm.Chart_All_DataPoints[i].Count != 0)
                    {
                        _stringBuilder.Append("Ch " + (i + 1).ToString());
                        _stringBuilder.Append(",");
                    }
                }

                _stringBuilder.AppendLine();

                List<int> list_data_count = new List<int>();
                foreach (List<OxyPlot.DataPoint> d in vm.Chart_All_DataPoints)
                {
                    list_data_count.Add(d.Count);
                }

                int max_dataCount = list_data_count.Max();

                //資料內容
                for (int i = 0; i < max_dataCount; i++)
                {
                    _stringBuilder.Append(vm.Chart_All_DataPoints[0][i].X);
                    _stringBuilder.Append(",");

                    for (int j = 0; j < vm.Chart_All_DataPoints.Count; j++)
                    {
                        try
                        {
                            _stringBuilder.Append(vm.Chart_All_DataPoints[j][i].Y);
                            _stringBuilder.Append(",");
                        }
                        catch { }
                    }
                    _stringBuilder.AppendLine();
                }

                File.AppendAllText(csvpath, _stringBuilder.ToString());  //Save string builder to csv

                //string csvpath_rename = Application.StartupPath + @"\" + txtBox_name.Text + ".csv";
                //File.Move(datapoint_filePath, csvpath_rename);
            }

            #endregion

            vm.Show_Bear_Window("Saved", false, "String", false);
        }

        public Window_Bear_Timer window_Bear;
        private void btn_Timer_Click(object sender, RoutedEventArgs e)
        {
            window_Bear = new Window_Bear_Timer(vm);
            window_Bear.Show();
        }

        private void btn_PopupWindow_Click(object sender, RoutedEventArgs e)
        {
            Window_Chart_Popup window = new Window_Chart_Popup(vm);
            window.Show();
        }

        public Window_Bear_Grid window_Bear_Grid;
        private void btn_Calculation_Click(object sender, RoutedEventArgs e)
        {


            //vm.List_bear_grid_data = new System.Collections.ObjectModel.ObservableCollection<string>();

            //try
            //{
            //    for (int i = 0; i < vm.ch_count; i++)
            //    {
            //        List<double> list_all_PD_Value = new List<double>();
            //        int c = vm.Chart_All_DataPoints[i].Count();

            //        for (int j = 0; j < c; j++)
            //        {
            //            list_all_PD_Value.Add(vm.Chart_All_DataPoints[i][j].Y);
            //        }
            //        string str_maxDeltaIL = Math.Round(list_all_PD_Value.Max() - list_all_PD_Value.Min(), 6).ToString();

            //        vm.List_bear_grid_data.Add(str_maxDeltaIL);
            //    }

            //    if (window_Bear_Grid.IsLoaded) window_Bear_Grid.Close();
            //    window_Bear_Grid = new Window_Bear_Grid(vm, "Cal.");
            //    window_Bear_Grid.Show();
            //}
            //catch { vm.Show_Bear_Window("No Data", false, "String", false); }
        }

        private void btn_line_x_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
