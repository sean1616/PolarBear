using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Drawing;
using System.IO.Ports;
using PD.ViewModel;
using PD.NavigationPages;
using OxyPlot;
using System.Threading.Tasks;

namespace PD.AnalysisModel
{
    public class Analysis
    {        
        List<string> list_read_value = new List<string>();
        List<byte> list_databuffer;
        
        //List<float> list_read_value_single = new List<float>();
        string str_read_value;
        
        ComViewModel vm;
        CurveFitting CurveFunctions = new CurveFitting();        

        public Analysis(ComViewModel vm)
        {
            this.vm = vm;

            //List<List<string>> ls = ListDefault<List<string>>(3);
        }

        public void Process_Schedule(int step, int all_steps)
        {
            decimal percentage = (decimal)step / all_steps;
            vm.Int_Process_Schedule = (int)Math.Round(percentage * 100);
        }

        public int Calculate_All_Process_Steps(int process_level)  //level 1:K V3, 2:K WL , 3:K ALL
        {
            int all_calibration_step = 0;

            int count_ch = 0;
            foreach (bool ch in vm.Bool_Gauge)  //計算ch數
            {
                if (ch == true) count_ch++;
            }

            if (process_level == 1 || process_level == 3)
            {
                #region 計算K V3所需步數
                if (vm.product_type == "UFA" || vm.product_type == "UFA(H)")
                {
                    int count_one_ch_step = 0;
                    //Rough Scan
                    count_one_ch_step = (vm.int_V3_scan_end - vm.int_V3_scan_start) / (vm.int_V3_scan_gap / 6);
                    //for (int dac = vm.int_V3_scan_start; dac <= vm.int_V3_scan_end; dac = dac + vm.int_V3_scan_gap / 6)
                    //{
                    //    count_one_ch_step++;
                    //}

                    //Normal Scan
                    count_one_ch_step = count_one_ch_step + (5);

                    //Detail Scan
                    count_one_ch_step = count_one_ch_step + (5);

                    all_calibration_step += count_one_ch_step * count_ch;
                }
                #endregion
            }

            if (process_level == 2 || process_level == 3)
            {
                #region 計算K WL所需步數
                int count_one_ch_wl_step = 0;

                //Rough Scan
                for (double wl = vm.float_WL_Scan_Start; wl <= vm.float_WL_Scan_End; wl = wl + vm.float_WL_Scan_Gap)
                {
                    count_one_ch_wl_step++;
                }
                all_calibration_step += (count_one_ch_wl_step);

                //Detail Scan
                all_calibration_step += (9 * count_ch);  //9 points

                //Final Check
                all_calibration_step += (9 * count_ch);  //9 points
                #endregion
            }
            
            return all_calibration_step;
        }

        public void Read_PM_to_Gauge(double power_PM, int ch)
        {
            int Switch_Number = ch + 1;
            vm.Value_PD.Clear();
            vm.Float_PD.Clear();
            power_PM = Math.Round(power_PM, 3);
            float y = Convert.ToSingle(power_PM);
            float z = (y * 300 / -64 - 150) * -1;
            z = z != 1350 ? z : 150;

            if (z < -150)
                z = -150;

            if (Switch_Number < 9) //Switch mode  1~8
            {
                vm.Value_PD = new List<float>() { -150, -150, -150, -150, -150, -150, -150, -150 };
                vm.Float_PD = Analysis.ListDefault<double>(8);

                vm.Value_PD[Switch_Number - 1] = z;
                vm.Float_PD[Switch_Number - 1] = y;
                vm.Value_PD = new List<float>(vm.Value_PD);

                vm.Str_PD = Analysis.ListDefault<string>(8);
                vm.Str_PD[Switch_Number - 1] = power_PM.ToString();
                vm.Str_PD = new List<string>(vm.Str_PD);
            }
            else if (Switch_Number < 13 && Switch_Number >= 9)  //Switch mode  9~12
            {
                vm.Value_PD = new List<float>() { -150, -150, -150, -150, -150, -150, -150, -150 };
                vm.Float_PD = Analysis.ListDefault<double>(8);

                vm.Value_PD[Switch_Number - 9] = z;
                vm.Float_PD[Switch_Number - 9] = y;
                vm.Value_PD = new List<float>(vm.Value_PD);

                vm.Str_PD = Analysis.ListDefault<string>(8);
                vm.Str_PD[Switch_Number - 9] = power_PM.ToString();
                vm.Str_PD = new List<string>(vm.Str_PD);
            }
            else  //Normal mode
            {
                vm.Value_PD.Add(z);  //-150~150 degree, for gauge binding
                vm.Float_PD.Add(y);  //list 0~-64dBm in float type

                vm.Str_PD = new List<string>() { power_PM.ToString() };
            }
        }

        public ComViewModel _read_analysis(string cmd, byte[] dataBuffer)
        {
            switch (cmd)
            {
                case "ID?":
                    GetMessage(dataBuffer);
                    vm.Str_cmd_read = list_read_value[0];                    
                    break;
                
                case "P0?":                     
                    str_read_value="";
                    list_read_value = new List<string>();                           
                    list_databuffer = new List<byte>(dataBuffer); //Convert array to list
                    
                    foreach (byte data in list_databuffer)
                    {
                        if (data != 10 && data != 44 && data != 13) 
                        {
                            str_read_value = str_read_value + Convert.ToChar(data);
                        }                        
                        else
                        {
                            list_read_value.Add(str_read_value);
                            str_read_value = "";
                        }
                    }
                    list_read_value.RemoveAt(0);  //list dBm in string type

                    vm.Value_PD = new List<float>();
                    vm.Float_PD = new List<double>();
                    List<string> list_dB_readpower = new List<string>();
                    
                    if (vm.dB_or_dBm == true)   //dB
                    {
                        int ch = 0;  //Channel No.
                        foreach (string x in list_read_value)
                        {
                            if (x != "")
                            {
                                double y = Math.Round(Convert.ToDouble(x) - vm.float_WL_Ref[ch], 3) ;  //y is 0~-64dB in float type
                                double z = (y * 300 / -64 - 150) * -1;
                                z = z != 1350 ? z : 150;

                                list_dB_readpower.Add(y.ToString());

                                vm.Value_PD.Add(Convert.ToSingle(z));  //-150~150 degree, for gauge binding
                                vm.Float_PD.Add(y);  //list 0~-64dBm in float type
                            }
                            ch++;
                        }
                        
                    }
                    else  //dBm
                    {
                        foreach (string x in list_read_value)
                        {
                            if (x != "")
                            {
                                double y = Convert.ToDouble(x);  //y is 0~-64dBm in float type
                                double z = (y * 300 / -64 - 150) * -1;
                                z = z != 1350 ? z : 150;
                                
                                vm.Value_PD.Add(Convert.ToSingle(z));  //-150~150 degree, for gauge binding
                                vm.Float_PD.Add(y);  //list 0~-64dBm in float type
                            }
                        }
                    }
                    vm.Value_PD = new List<float>(vm.Value_PD);
                    if (vm.isDeltaILModeOn == true)  //Delta IL mode
                    {
                        int i = 0;
                        vm.Str_PD = new List<string>();
                        foreach (float s in vm.savedPower_for_deltaMode)
                        {
                            vm.Float_PD[i] = vm.Float_PD[i] - s;

                            if (vm.Float_PD[i] > vm.double_MaxIL_for_DeltaMode[i])
                            {
                                vm.double_MaxIL_for_DeltaMode[i] = vm.Float_PD[i];
                            }
                            else if (vm.Float_PD[i] < vm.double_MinIL_for_DeltaMode[i])
                            {
                                vm.double_MinIL_for_DeltaMode[i] = vm.Float_PD[i];
                            }

                            vm.double_Maxdelta[i] = Math.Round((vm.double_MaxIL_for_DeltaMode[i] - vm.double_MinIL_for_DeltaMode[i]), 4);

                            vm.Str_PD.Add(vm.double_Maxdelta[i].ToString());
                            i++;
                        }
                    }

                    if (vm.isDeltaILModeOn == false)
                    {
                        if (vm.dB_or_dBm == false)
                            //vm.Str_cmd_read = vm.Float_PD[0].ToString();
                            vm.Str_PD = list_read_value;
                        else
                            vm.Str_PD = list_dB_readpower;
                        //vm.Str_cmd_read = vm.Float_PD[0].ToString();
                    }                   
                    break;

                default: //D1?, D2?... D1 0,0...
                    GetMessage(dataBuffer);
                    vm.Str_cmd_read = list_read_value[0];

                    break;
            }            
            return vm;
        }

        public ComViewModel _PM_read_analysis(string cmd, byte[] dataBuffer, int ch)
        {
            switch (cmd)
            {
                case "ID?":
                    GetMessage(dataBuffer);
                    vm.Str_cmd_read = list_read_value[0];
                    break;

                case "P0?":
                    str_read_value = "";
                    list_read_value = new List<string>();
                    list_databuffer = new List<byte>(dataBuffer); //Convert array to list

                    foreach (byte data in list_databuffer)
                    {
                        if (data != 10 && data != 44 && data != 13)
                        {
                            str_read_value = str_read_value + Convert.ToChar(data);
                        }
                        else
                        {
                            list_read_value.Add(str_read_value);
                            str_read_value = "";
                        }
                    }
                    list_read_value.RemoveAt(0);  //list dBm in string type

                    vm.Value_PD = new List<float>();
                    vm.Float_PD = new List<double>();
                    List<string> list_dB_readpower = new List<string>();

                    if (vm.dB_or_dBm == true)   //dB
                    {
                        //int ch = 0;  //Channel No.
                        foreach (string x in list_read_value)
                        {
                            if (x != "")
                            {
                                double y = Math.Round(Convert.ToDouble(x) - vm.float_WL_Ref[ch], 3);  //y is 0~-64dB in float type
                                double z = (y * 300 / -64 - 150) * -1;
                                z = z != 1350 ? z : 150;

                                list_dB_readpower.Add(y.ToString());

                                vm.Value_PD.Add(Convert.ToSingle(z));  //-150~150 degree, for gauge binding
                                vm.Float_PD.Add(y);  //list 0~-64dBm in float type
                            }
                            ch++;
                        }

                    }
                    else  //dBm
                    {
                        foreach (string x in list_read_value)
                        {
                            if (x != "")
                            {
                                double y = Convert.ToDouble(x);  //y is 0~-64dBm in float type
                                double z = (y * 300 / -64 - 150) * -1;
                                z = z != 1350 ? z : 150;

                                vm.Value_PD.Add(Convert.ToSingle(z));  //-150~150 degree, for gauge binding
                                vm.Float_PD.Add(y);  //list 0~-64dBm in float type
                            }
                        }
                    }
                    vm.Value_PD = new List<float>(vm.Value_PD);
                    if (vm.isDeltaILModeOn == true)  //Delta IL mode
                    {
                        int i = 0;
                        vm.Str_PD = new List<string>();
                        foreach (float s in vm.savedPower_for_deltaMode)
                        {
                            vm.Float_PD[i] = vm.Float_PD[i] - s;

                            if (vm.Float_PD[i] > vm.double_MaxIL_for_DeltaMode[i])
                            {
                                vm.double_MaxIL_for_DeltaMode[i] = vm.Float_PD[i];
                            }
                            else if (vm.Float_PD[i] < vm.double_MinIL_for_DeltaMode[i])
                            {
                                vm.double_MinIL_for_DeltaMode[i] = vm.Float_PD[i];
                            }

                            vm.double_Maxdelta[i] = Math.Round((vm.double_MaxIL_for_DeltaMode[i] - vm.double_MinIL_for_DeltaMode[i]), 4);

                            vm.Str_PD.Add(vm.double_Maxdelta[i].ToString());
                            i++;
                        }
                    }

                    if (vm.isDeltaILModeOn == false)
                    {
                        if (vm.dB_or_dBm == false)
                            //vm.Str_cmd_read = vm.Float_PD[0].ToString();
                            vm.Str_PD = list_read_value;
                        else
                            vm.Str_PD = list_dB_readpower;
                        //vm.Str_cmd_read = vm.Float_PD[0].ToString();
                    }
                    break;

                default: //D1?, D2?... D1 0,0...
                    GetMessage(dataBuffer);
                    vm.Str_cmd_read = list_read_value[0];

                    break;
            }
            return vm;
        }

        public ComViewModel _K_WL_analysis(byte[] dataBuffer)
        {
            str_read_value = "";
            list_read_value = new List<string>();
            list_databuffer = new List<byte>(dataBuffer); //Convert array to list

            foreach (byte data in list_databuffer)
            {
                if (data != 10 && data != 44 && data != 13)
                {
                    str_read_value = str_read_value + Convert.ToChar(data);  //dBm in string type
                }
                else
                {
                    list_read_value.Add(str_read_value);
                    str_read_value = "";
                }
            }
            if (list_read_value.Count > 1)
                list_read_value.RemoveAt(0); //list dBm in string type            

            List<float> _list_float = new List<float>(new float[0]);
            vm.Float_PD = new List<double>();

            foreach (string x in list_read_value)
            {
                if (x != "")
                {
                    float y = Convert.ToSingle((string)x);
                    float z = (y * 300 / -64 - 150) * -1;
                    z = z != 1350 ? z : 150;
                    _list_float.Add(z);  //-150~150 degree, for gauge binding
                    vm.Float_PD.Add(y);  //list 0~-64dBm in float type 
                }
            }
            vm.Value_PD = new List<float>(_list_float);
            vm.Str_PD = list_read_value;
                        
            return vm;
        }

        public ComViewModel _K_analysis(int dac, byte[] dataBuffer)
        {            
            str_read_value = "";
            list_read_value = new List<string>();
            list_databuffer = new List<byte>(dataBuffer); //Convert array to list

            foreach (byte data in list_databuffer)
            {
                if (data != 10 && data != 44 && data != 13)
                {
                    str_read_value = str_read_value + Convert.ToChar(data);  //dBm in string type
                }
                else
                {
                    list_read_value.Add(str_read_value);
                    str_read_value = "";
                }
            }
            if (list_read_value.Count > 1)
                list_read_value.RemoveAt(0); //list dBm in string type            

            List<float> _list_float = new List<float>(new float[0]);
            vm.Float_PD = new List<double>();

            foreach (string x in list_read_value)
            {
                if (x != "")
                {
                    float y = Convert.ToSingle((string)x);
                    float z = (y * 300 / -64 - 150) * -1;
                    z = z != 1350 ? z : 150;
                    _list_float.Add(z);  //-150~150 degree, for gauge binding
                    vm.Float_PD.Add(y);  //list 0~-64dBm in float type 
                }                                
            }
            vm.Value_PD = new List<float>(_list_float);
            vm.Str_PD = list_read_value;

            vm.List_V3.Add(dac);
            for (int i = 0; i < 8; i++)
            {
                if (vm.List_PDvalue_byV3[i] == null)
                    vm.List_PDvalue_byV3[i] = new List<float>();
                
                if(i < vm.Float_PD.Count)
                    vm.List_PDvalue_byV3[i].Add(Convert.ToSingle(vm.Float_PD[i]));
            }
            
            return vm;
        }

        public ComViewModel _K12_analysis_PM(int dac, List<double> power)
        {
            List<float> _list_float = new List<float>(new float[0]);
            vm.Float_PD = new List<double>();
            vm.Str_PD = new List<string>();
            foreach (double x in power)
            {
                if (x != 99)
                {
                    float y = Convert.ToSingle(x);
                    float z = (y * 300 / -64 - 150) * -1;
                    z = z != 1350 ? z : 150;
                    _list_float.Add(z);  //-150~150 degree, for gauge binding
                    vm.Float_PD.Add(y);  //list 0~-64dBm in float type 
                }
            }
            vm.Value_PD = new List<float>(_list_float);
            vm.Str_PD=new List<string>() { Math.Round(power[0], 2).ToString()};

            vm.List_V12.Add(dac); //Save DAC
            if (vm.List_PMvalue_byV12 == null)
                vm.List_PMvalue_byV12 = new List<float>();

            if (vm.Float_PD.Count > 0)
                vm.List_PMvalue_byV12.Add(Convert.ToSingle(vm.Float_PD[0]));

            return vm;
        }

        public ComViewModel _K12_analysis(int dac, byte[] dataBuffer)
        {
            str_read_value = "";
            list_read_value = new List<string>();
            list_databuffer = new List<byte>(dataBuffer); //Convert array to list

            foreach (byte data in list_databuffer)
            {
                if (data != 10 && data != 44 && data != 13)
                {
                    str_read_value = str_read_value + Convert.ToChar(data);  //dBm in string type
                }
                else
                {
                    list_read_value.Add(str_read_value);
                    str_read_value = "";
                }
            }
            if (list_read_value.Count > 1)
                list_read_value.RemoveAt(0); //list dBm in string type            

            List<float> _list_float = new List<float>(new float[0]);
            vm.Float_PD = new List<double>();

            foreach (string x in list_read_value)
            {
                if (x != "" && x!="ERR0001" && x!=">")
                {
                    float y = Convert.ToSingle((string)x);
                    float z = (y * 300 / -64 - 150) * -1;
                    z = z != 1350 ? z : 150;
                    _list_float.Add(z);  //-150~150 degree, for gauge binding
                    vm.Float_PD.Add(y);  //list 0~-64dBm in float type 
                }
            }
            vm.Value_PD = new List<float>(_list_float);
            vm.Str_PD = list_read_value;

            vm.List_V12.Add(dac);
            for (int i = 0; i < 8; i++)
            {
                if (vm.List_PDvalue_byV12[i] == null)
                    vm.List_PDvalue_byV12[i] = new List<float>();

                if(i<vm.Float_PD.Count)
                    vm.List_PDvalue_byV12[i].Add(Convert.ToSingle(vm.Float_PD[i]));
            }

            return vm;
        }

        public string GetMessage(byte[] dataBuffer)
        {
            str_read_value = "";
            list_read_value = new List<string>();
            //Delete "0A"                    
            //byte remove = Convert.ToByte("0A".Substring(0, 2), 16);
            dataBuffer = dataBuffer.Where(val => val != 10).ToArray(); //remove "10"    
            dataBuffer = dataBuffer.Where(val => val != 13).ToArray(); //remove "13"    
            dataBuffer = dataBuffer.Where(val => val != 62).ToArray(); //remove "62"   >   
            string txt = Encoding.ASCII.GetString(dataBuffer);
            list_read_value.Add(Encoding.ASCII.GetString(dataBuffer));
            return txt;
        }

        public DataPoint CurFit(List<DataPoint> list_datapoint)
        {
            List<System.Drawing.PointF> point = new List<PointF>();
            DataPoint curfitResult = new DataPoint();
            List<double> BestCoeffs = new List<double>();
            if (list_datapoint.Count < 1)
            {
                vm.ErrorCode = 21;
            }
            else
            {
                #region initialize
                vm.List_curfit_resultDac.Clear();
                vm.List_curfit_resultWL.Clear();
                vm.Str_Status = "Curve Fitting";
                string Best_DAC = "";
                vm.Str_cmd_read = " ";
                int all_ch_count = vm.ch_count;
                #endregion
                
                for (int i = 0; i < all_ch_count; i++)
                {

                    int mid_i = (int)Math.Round((double)list_datapoint.Count / 2);
                    double mid_X = Math.Round(list_datapoint[mid_i].X, 2);

                    foreach (DataPoint dp in list_datapoint)
                        point.Add(new PointF((float)(dp.X - list_datapoint[mid_i].X), (float)dp.Y));
                                     
                    // Find a good fit.
                    int degree = 2;
                    BestCoeffs = CurveFunctions.FindPolynomialLeastSquaresFit(point, degree);

                    if (degree == 2)
                        Best_DAC = Math.Round((-1 * BestCoeffs[1] / (2 * BestCoeffs[2]))).ToString();

                    string txt = "";
                    foreach (double coeff in BestCoeffs)
                    {
                        txt += ", " + Math.Round(coeff, 10).ToString();
                    }
                    string coe = txt.Substring(1);

                    string str_curfit_result = Best_DAC;  //If Curfit result is not a number , error occurs.

                    vm.List_curfit_resultDac[i] = Convert.ToInt32(Best_DAC);

                    vm.Str_cmd_read = vm.Str_cmd_read + "," + str_curfit_result;

                    // 繪圖- CurveFit曲線
                    double max_7points_dac = list_datapoint.LastOrDefault().X;
                    double min_7points_dac = list_datapoint[0].X;
                    double dac_gap = (max_7points_dac - min_7points_dac) / 100;

                    list_datapoint.Clear();
                    for (double x = min_7points_dac; x <= max_7points_dac; x = x + dac_gap)
                    {
                        list_datapoint.Add(new DataPoint(x, (BestCoeffs[2] * Math.Pow(x, 2) + BestCoeffs[1] * x + BestCoeffs[0])));
                    }
                }

                vm.Str_cmd_read = vm.Str_cmd_read.Substring(2);

                
            }

            return curfitResult;
        }

        public async Task<bool> CurFit_All(List<List<DataPoint>> Save_All_PD_Value ,List<PointF> Points, List<double> BestCoeffs, string action)
        {
            if (Save_All_PD_Value.Count < 1)
            {
                vm.Show_Bear_Window("無細掃資訊", false, "String");
                return false;
            }
            else
            {
                if (action == "K TF" && Save_All_PD_Value[0].Count != 7)
                {
                    //vm.Show_Bear_Window("細掃資料筆數不等於7", false, "String");
                    //return false;
                }

                vm.List_curfit_resultDac.Clear();
                vm.List_curfit_resultWL.Clear();
                vm.Str_Status = "Curve Fitting";
                string Best_DAC = "";
                vm.Str_cmd_read = " ";

                int all_ch_count = vm.ch_count;
                if (vm.PD_or_PM == true && action!="K WL")  //PM mode
                    all_ch_count = 1;

                vm.List_curfit_resultDac = Analysis.ListDefault<int>(vm.ch_count);
                for (int i = 0; i < all_ch_count; i++)
                {
                    if (!vm.Bool_Gauge[i])
                    {
                        vm.List_curfit_resultWL.Add(999);
                        continue;
                    }

                    int mid_i = (int)Math.Round((double)Save_All_PD_Value[i].Count / 2);
                    double mid_X = Math.Round(Save_All_PD_Value[i][mid_i].X, 2);

                    Points = new List<PointF>();
                    if(action=="K WL")
                    {                        
                        foreach (DataPoint dp in Save_All_PD_Value[i])
                            Points.Add(new PointF((float)(dp.X - Save_All_PD_Value[i][mid_i].X), (float)dp.Y));
                    }
                    else if (action == "K V3")
                    {
                        foreach (DataPoint dp in Save_All_PD_Value[i])
                            Points.Add(new PointF((float)(dp.X - Save_All_PD_Value[i][mid_i].X), (float)dp.Y));
                    }
                    else
                    {
                        foreach (DataPoint dp in Save_All_PD_Value[i])
                            Points.Add(new PointF((float)dp.X, (float)dp.Y));
                    }                   

                    if (Points.Count == 0)
                        continue;

                    // Find a good fit.
                    int degree = 2;
                    BestCoeffs = CurveFunctions.FindPolynomialLeastSquaresFit(Points, degree);

                    if (degree == 2 && action == "K TF")
                        Best_DAC = Math.Round((-1 * BestCoeffs[1] / (2 * BestCoeffs[2]))).ToString();
                    else
                        Best_DAC = Math.Round((-1 * BestCoeffs[1] / (2 * BestCoeffs[2])), 2).ToString();    //Best WL

                    string txt = "";
                    foreach (double coeff in BestCoeffs)
                    {
                        txt += ", " + Math.Round(coeff, 10).ToString();
                    }
                    string coe = txt.Substring(1);

                    string str_curfit_result = Best_DAC;  //If Curfit result is not a number , error occurs.

                    if (action == "K TF")
                    {
                        vm.List_curfit_resultDac[i] = Convert.ToInt32(Best_DAC);

                        vm.Str_cmd_read = vm.Str_cmd_read + "," + str_curfit_result;
                    }
                    else if (action == "K WL")
                    {
                        vm.List_curfit_resultWL.Add(Convert.ToDouble(Best_DAC) + mid_X);

                        vm.Str_cmd_read = vm.Str_cmd_read + "," + str_curfit_result;
                    }
                    else if (action == "K WL")
                    {
                        vm.List_curfit_resultWL.Add(Convert.ToDouble(Best_DAC) + mid_X);

                        vm.Str_cmd_read = vm.Str_cmd_read + "," + str_curfit_result;
                    }
                    else
                    {
                        vm.Str_cmd_read = vm.Str_cmd_read + ", Error";
                    }

                    if (action == "K TF")  // 繪圖- CurveFit曲線
                    {
                        double max_7points_dac = Save_All_PD_Value[i].LastOrDefault().X;
                        double min_7points_dac = Save_All_PD_Value[i][0].X;
                        double dac_gap = (max_7points_dac - min_7points_dac) / 100;

                        Save_All_PD_Value[i].Clear();
                        for (double x = min_7points_dac; x <= max_7points_dac; x = x + dac_gap)
                        {
                            Save_All_PD_Value[i].Add(new DataPoint(x, (BestCoeffs[2] * Math.Pow(x, 2) + BestCoeffs[1] * x + BestCoeffs[0])));
                        }

                        //vm.Chart_DataPoints = new List<DataPoint>(Save_All_PD_Value[i]);  //The first lineseries
                    }
                    else  //K WL
                    {
                        double max_7points_WL = (Save_All_PD_Value[i].LastOrDefault().X) - mid_X;
                        double min_7points_WL = (Save_All_PD_Value[i][0].X) - mid_X;
                        double dac_gap = Math.Round(max_7points_WL - min_7points_WL, 2)/100 ;

                        Save_All_PD_Value[i].Clear();
                        for (double x = min_7points_WL; x <= max_7points_WL; x = x + dac_gap)
                        {
                            Save_All_PD_Value[i].Add(new DataPoint(x + mid_X, (BestCoeffs[2] * Math.Pow(x, 2) + BestCoeffs[1] * x + BestCoeffs[0])));
                        }

                        //vm.Chart_DataPoints = new List<DataPoint>(Save_All_PD_Value[i]);  //The first lineseries
                    }
                }

                vm.Str_cmd_read = vm.Str_cmd_read.Substring(2);

                return true;
            }
        }

        public void CurFit_single(List<DataPoint> List_DataPoint, List<PointF> Points, List<double> BestCoeffs, string action)
        {
            if (List_DataPoint.Count <= 3)
            {
                vm.Str_cmd_read = "點資料不足";
                return;
            }
            else
            {               
                vm.List_curfit_resultDac.Clear();
                vm.List_curfit_resultWL.Clear();
                vm.Str_Status = "Curve Fitting";
                string Best_DAC = "";
                vm.Str_cmd_read = " ";

                int all_ch_count = vm.ch_count;
                if (vm.PD_or_PM == true && action != "K WL")  //PM mode
                    all_ch_count = 1;

                vm.List_curfit_resultDac = Analysis.ListDefault<int>(vm.ch_count);
                
                int mid_i = (int)Math.Round((double)List_DataPoint.Count / 2); //中心位置點資料的index
                double mid_X = Math.Round(List_DataPoint[mid_i].X, 2);   //取出中心位置點資料的X

                #region 形成新的Point List
                Points = new List<PointF>();
                if (action == "K WL")
                {
                    foreach (DataPoint dp in List_DataPoint)
                        Points.Add(new PointF((float)(dp.X - List_DataPoint[mid_i].X), (float)dp.Y));
                }
                else if (action == "K V3")
                {
                    foreach (DataPoint dp in List_DataPoint)
                        Points.Add(new PointF((float)(dp.X - List_DataPoint[mid_i].X), (float)dp.Y));
                }
                else
                {
                    foreach (DataPoint dp in List_DataPoint)
                        Points.Add(new PointF((float)dp.X, (float)dp.Y));
                }

                if (Points.Count == 0)
                    return;
                #endregion

                // Find a good fit.
                int degree = 2;
                BestCoeffs = CurveFunctions.FindPolynomialLeastSquaresFit(Points, degree);
                                
                if (degree == 2 && action == "K TF")
                    Best_DAC = Math.Round((-1 * BestCoeffs[1] / (2 * BestCoeffs[2]))).ToString();   //K Dac
                else
                    Best_DAC = Math.Round((-1 * BestCoeffs[1] / (2 * BestCoeffs[2])), 2).ToString();    //K 波長
                               
                string txt = "";
                foreach (double coeff in BestCoeffs)
                {
                    txt += ", " + Math.Round(coeff, 10).ToString();
                }
                string coe = txt.Substring(1);

                string str_curfit_result = Best_DAC;  //If Curfit result is not a number , error occurs.

                if (action == "K TF")
                {
                    vm.List_curfit_resultDac_single = Convert.ToInt32(Best_DAC);

                    vm.Str_cmd_read = vm.Str_cmd_read + "," + str_curfit_result;
                }
                else if (action == "K WL")
                {
                    vm.List_curfit_resultWL.Add(Convert.ToDouble(Best_DAC) + mid_X);
                    vm.List_curfit_resultWL_single = Convert.ToDouble(Best_DAC) + mid_X;

                    vm.Str_cmd_read = vm.Str_cmd_read + "," + str_curfit_result;
                }
                else
                {
                    vm.Str_cmd_read = vm.Str_cmd_read + ", Error";
                }

                //if (action == "K TF")  // 繪圖- CurveFit曲線
                //{
                //    double max_7points_dac = List_DataPoint.LastOrDefault().X;
                //    double min_7points_dac = List_DataPoint[0].X;
                //    double dac_gap = (max_7points_dac - min_7points_dac) / 100;

                //    List_DataPoint.Clear();
                //    for (double x = min_7points_dac; x <= max_7points_dac; x = x + dac_gap)
                //    {
                //        List_DataPoint.Add(new DataPoint(x, (BestCoeffs[2] * Math.Pow(x, 2) + BestCoeffs[1] * x + BestCoeffs[0])));
                //    }
                //}
                //else  //K WL
                //{
                //    double max_7points_WL = (List_DataPoint.LastOrDefault().X) - mid_X;
                //    double min_7points_WL = (List_DataPoint[0].X) - mid_X;
                //    double dac_gap = Math.Round(max_7points_WL - min_7points_WL, 2) / 100;

                //    List_DataPoint.Clear();
                //    for (double x = min_7points_WL; x <= max_7points_WL; x = x + dac_gap)
                //    {
                //        List_DataPoint.Add(new DataPoint(x + mid_X, (BestCoeffs[2] * Math.Pow(x, 2) + BestCoeffs[1] * x + BestCoeffs[0])));
                //    }
                //}

                vm.Str_cmd_read = vm.Str_cmd_read.Substring(2);
            }
        }

        public async void Ask_ID()
        {
            await Port_ReOpen();
            vm.Str_comment = "ID?";
            await Cmd_Write_RecieveData(vm.Str_comment);
        }

        public async Task<bool> Port_ReOpen()
        {
            try
            {
                if (!vm.IsGoOn)
                {
                    vm.port_PD.Open();
                }
                else
                {
                    if (vm.PD_or_PM == false)
                        vm.timer2.Stop();
                    else
                        vm.timer3.Stop();
                    await AccessDelayAsync(vm.Int_Read_Delay);
                    vm.port_PD.Close();
                    await AccessDelayAsync(50);
                    vm.port_PD.Open();
                    vm.port_PD.DiscardInBuffer();       // RX
                    vm.port_PD.DiscardOutBuffer();      // TX
                }
            }
            catch { }

            return vm.IsGoOn;
        }        

        private async Task<bool> Cmd_Write_RecieveData(string cmd)
        {
            try
            {
                if (vm.port_PD.IsOpen)
                {
                    vm.port_PD.Write(vm.Str_comment + "\r");

                    await AccessDelayAsync(vm.Int_Read_Delay);

                    int size = vm.port_PD.BytesToRead;
                    byte[] dataBuffer = new byte[size];
                    int length = vm.port_PD.Read(dataBuffer, 0, size);

                    //Show read back message
                    vm = _read_analysis(vm.Str_comment, dataBuffer);

                    vm.port_PD.DiscardInBuffer();       // RX
                    vm.port_PD.DiscardOutBuffer();      // TX

                    vm.port_PD.Close();
                }
            }
            catch { }

            return vm.IsGoOn;
        }

        public async Task AccessDelayAsync(int delayTime)
        {
            await Task.Delay(delayTime);
        }

        #region 幫助宣告List初始長度 方法1
        public static List<T> ListDefault<T>(int count)
        {            
            return Repeated(default(T), count);

        }

        public static List<T> Repeated<T>(T value, int count)
        {
            List<T> ret = new List<T>(count);
            ret.AddRange(Enumerable.Repeat(value, count));
            return ret;
        }
        #endregion

        #region 宣告List初始長度 方法2
        public static List<List<T>> ListDefine<T>(List<List<T>> target_list, int count, List<T> element)
        {
            target_list = new List<List<T>>();
            for (int i = 0; i < count; i++)
            {                
                target_list.Add(new List<T>(element));
            }
            return target_list;
        }
        #endregion
    }
}
