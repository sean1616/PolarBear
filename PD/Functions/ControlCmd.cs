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
