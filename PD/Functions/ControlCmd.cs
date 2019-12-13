using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using PD.ViewModel;
using PD.AnalysisModel;
using OxyPlot;


namespace PD.Functions
{
    class ControlCmd
    {
        ComViewModel vm;
        Analysis anly;

        public ControlCmd(ComViewModel vm)
        {
            this.vm = vm;
            anly = new Analysis(vm);
        }

        public ControlCmd()
        {

        }

        public void Set_WL()
        {
            try
            {
                if (vm.station_type == "Hermetic Test")
                {
                    if (vm.selected_band == "C Band")
                    {
                        switch (vm.product_type)
                        {
                            case "UFA":
                                Set_WL(1548.5);
                                break;

                            case "UFA(H)":
                                Set_WL(1548.5);
                                break;

                            case "UTF":
                                Set_WL(1527.5);
                                break;

                            case "CTF":
                                Set_WL(1527.5);
                                break;

                            case "MTF":
                                //cmd.Set_WL(1548.5);
                                break;
                        }

                    }
                    else if (vm.selected_band == "L Band")
                    {
                        switch (vm.product_type)
                        {
                            case "UFA":
                                Set_WL(1591);
                                break;

                            case "UFA(H)":
                                Set_WL(1591);
                                break;

                            case "UTF":
                                Set_WL(1568);
                                break;

                            case "CTF":
                                //cmd.Set_WL(1527.5);
                                break;

                            case "MTF":
                                //cmd.Set_WL(1548.5);
                                break;
                        }
                    }
                }
            }
            catch { }
        }

        public async void Set_WL(double wl)
        {
            try
            {
                vm.tls.SetWL(wl);
                await vm.AccessDelayAsync(vm.Int_Set_WL_Delay);
                vm.pm.SetWL(wl);
                await vm.AccessDelayAsync(vm.Int_Set_WL_Delay);

                vm.Double_Laser_Wavelength = wl;
                //vm.Double_Laser_Wavelength = vm.tls.ReadWL();
            }
            catch { }
        }

        public async void Set_V3_Dac(string selected_comport, int dac)
        {
            //Reset COM port
            if (string.IsNullOrEmpty(selected_comport)) return;
            if (string.IsNullOrEmpty(dac.ToString())) return;

            await vm.Port_ReOpen(selected_comport);            

            //Set Dac
            try
            {
                vm.Str_comment = "D1 0,0," + (dac).ToString();  //cmd = D1 0,0,1000
                vm.port_PD.Write(vm.Str_comment + "\r");
                await vm.AccessDelayAsync(vm.Int_Write_Delay);
                vm.port_PD.Close();
            }
            catch { vm.Str_cmd_read = "Write Dac Error"; }
        }

        public async void Set_V3_Volt(string selected_comport, double volt)
        {
            //Reset COM port
            if (string.IsNullOrEmpty(selected_comport)) return;
            if (string.IsNullOrEmpty(volt.ToString())) return;

            int final_dac = 0;

            await vm.Port_ReOpen(selected_comport);
                       
            if (vm.station_type != "Hermetic Test")
            {
                return;
                //If station is not Hermetic Test, then we need a method to find out the name of this control board.
            }
            else
            {
                #region Read Board Table
                List<double> list_voltage = new List<double>();
                List<int> list_dac = new List<int>();

                int count = 0;
                foreach (string strline in vm.board_read[vm.switch_index - 1])
                {
                    string[] board_read = strline.Split(',');
                    if (board_read.Length <= 1)
                        continue;

                    double voltage = double.Parse(board_read[0]);
                    int board_dac = int.Parse(board_read[1]);

                    list_voltage.Add(voltage);
                    list_dac.Add(board_dac);

                    if (voltage >= volt && count > 0)
                    {
                        final_dac = board_dac;
                        break;
                    }

                    count++;
                }
                #endregion
            }

            //Set Dac
            try
            {
                vm.Str_comment = "D1 0,0," + (final_dac).ToString();  //cmd = D1 0,0,1000
                vm.port_PD.Write(vm.Str_comment + "\r");
                await vm.AccessDelayAsync(vm.Int_Write_Delay);
                vm.port_PD.Close();
            }
            catch { vm.Str_cmd_read = "Write Dac Error"; }
        }

        public async Task<bool> Get_PD_Value()  //PD Value save in vm.Float_PD
        {
            vm.Str_comment = "P0?";
            try
            {
                vm.port_PD.Write(vm.Str_comment + "\r");
                await AccessDelayAsync(vm.Int_Read_Delay);
            }
            catch { return false; }

            int size = vm.port_PD.BytesToRead;
            byte[] dataBuffer = new byte[size];
            int length = vm.port_PD.Read(dataBuffer, 0, size);
            
            if (dataBuffer.Length > 0)
                vm = anly._K_WL_analysis(dataBuffer);

            return true;
        }

        public async Task Get_PM_Value(int ch) //PM Value save in vm.Float_PD
        {
            vm.Float_PD[ch] = vm.pm.ReadPower();
            await vm.AccessDelayAsync(80);
        }

        public void Save_Calibration_Data(string Data_Type)
        {
            #region Delete and Save Bear say to txt file
            if (vm.List_bear_say.Count == 0) return;

            switch (Data_Type)
            {
                case "K WL":
                    vm.List_bear_say_DataLabel = new List<string>() { "K WL", "WL", "IL" };
                    break;

                case "K V3":
                    vm.List_bear_say_DataLabel = new List<string>() { "K V3", "DAC", "V3" };
                    break;
            }

            if (File.Exists(@"D:\PD\BearSay.txt")) File.Delete(@"D:\PD\BearSay.txt");

            using (System.IO.StreamWriter file =
                 new System.IO.StreamWriter(@"D:\PD\BearSay.txt", true))
            {
                string str = "";
                for (int i = 0; i < vm.List_bear_say.Count; i++)
                {
                    for (int j = 0; j < vm.List_bear_say[i].Count; j++)
                    {
                        str = str + vm.List_bear_say[i][j];
                        if (j == 0) str += ",";
                    }
                    str = str + "\r\n";
                }
                file.WriteLine(str);

                vm._write_line = new List<string>();
            }
            #endregion
        }

        public int Save_K_WL_Data(string Data_Type, string userID, string SNnumber, int ch)
        {
            #region Save Bear say to txt file
            if (vm.List_bear_say.Count == 0) return 1;  //ErrorCode:1 => BearSay is empty

            switch (Data_Type)
            {
                case "K WL":
                    vm.List_bear_say_DataLabel = new List<string>() { "K WL", "WL", "IL" };
                    break;
            }
            DateTime dt = DateTime.Now;
            string a = dt.ToString("yyyy-MM-dd HH:mm:ss");
            string filePath = string.Concat(@"D:\PD\", SNnumber, ".txt");

            if (File.Exists(filePath))
            {
                //File.Delete(filePath);

                List<string> quotelist = File.ReadAllLines(filePath).ToList();

                if (quotelist.Count > 1)
                    quotelist.RemoveAt(1);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(userID)) return 2;  //ErrorCode:2 => UserID is empty

                using (System.IO.StreamWriter file =
                 new System.IO.StreamWriter(filePath, true))
                {                    
                    file.WriteLine(userID + "\r\n");  //第一列：使用者ID

                    string str = "";

                    str += vm.List_bear_say[ch][0];  //第二列-波長
                    str += ",";
                    str += a;  //第二列-時間

                    //for (int j = 0; j < 3; j++)
                    //{
                    //    str += ",";
                    //    str += vm.List_bear_say[ch][j];
                    //}
                    //str += "\r\n";
                    file.WriteLine(str);

                    vm._write_line = new List<string>();
                }
            }

            return 0;
            #endregion
        }

        public void Save_Log_Message(string Data_Type, string content, string time)
        {
            #region Save log to txt file         

            //if (File.Exists(@"D:\PD\Log.txt")) File.Delete(@"D:\PD\Log.txt");

            using (System.IO.StreamWriter file =
                 new System.IO.StreamWriter(@"D:\PD\Log.txt", true))
            {
                string str = "";
                str = content + "  " + time + "\r\n";
                file.WriteLine(str);

                //vm._write_line = new List<string>();
            }
            #endregion
        }

        public void Clean_Chart()
        {
            vm.Save_PD_Value = new List<DataPoint>();
            vm.Save_All_PD_Value = Analysis.ListDefine<DataPoint>(vm.Save_All_PD_Value, vm.ch_count, new List<DataPoint>());
            //vm.Save_All_PD_Value = new List<List<DataPoint>>()
            //{
            //    new List<DataPoint>(),
            //    new List<DataPoint>(),
            //    new List<DataPoint>(),
            //    new List<DataPoint>(),
            //    new List<DataPoint>(),
            //    new List<DataPoint>(),
            //    new List<DataPoint>(),
            //    new List<DataPoint>()
            //};
        }

        public async Task Save_Chart()
        {
            vm.int_chart_count++;
            vm.int_chart_now = vm.int_chart_count;
            vm.Chart_All_Datapoints_History.Add(vm.Chart_All_DataPoints);
            await vm.AccessDelayAsync(50);
        }
        
        async Task AccessDelayAsync(int delayTime)
        {
            await Task.Delay(delayTime);
        }
    }
}
