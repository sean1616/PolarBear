using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Drawing;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using PD.ViewModel;
using PD.Models;
using PD.NavigationPages;
using OxyPlot;
using OxyPlot.Annotations;
using System.Threading.Tasks;

namespace PD.AnalysisModel
{
    public class Analysis
    {
        List<string> list_read_value = new List<string>();
        List<byte> list_databuffer;
        SN_Member SNMember = new SN_Member();

        //List<float> list_read_value_single = new List<float>();
        StringBuilder str_read_value;

        ComViewModel vm;
        CurveFitting CurveFunctions = new CurveFitting();

        public Analysis(ComViewModel vm)
        {
            this.vm = vm;
        }

        public T Generic_GetINISetting<T>(T input, string region, string variable) where T : new ()
        {
            if (input is int)
            {
                int value_int;
                if (int.TryParse(vm.Ini_Read(region, variable), out value_int))
                    return (T)(object)value_int;
                else
                    return input;
            }
            else if (input is double)
            {
                double value_int;
                if (double.TryParse(vm.Ini_Read(region, variable), out value_int))
                    return (T)(object)value_int;
                else
                    return input;
            }
            else if (input is float)
            {
                float value_int;
                if (float.TryParse(vm.Ini_Read(region, variable), out value_int))
                    return (T)(object)value_int;
                else
                    return input;
            }
            else if (input is long)
            {
                long value_int;
                if (long.TryParse(vm.Ini_Read(region, variable), out value_int))
                    return (T)(object)value_int;
                else
                    return input;
            }
            else if (input is byte)
            {
                byte value_int;
                if (byte.TryParse(vm.Ini_Read(region, variable), out value_int))
                    return (T)(object)value_int;
                else
                    return input;
            }
            else if (input is bool)
            {
                bool value_bool;
                if (bool.TryParse(vm.Ini_Read(region, variable), out value_bool))
                    return (T)(object)value_bool;
                else
                    return input;
            }

            return new T();
        }


        public SN_Member Product_Info_Anly(string SN)
        {
            if (string.IsNullOrWhiteSpace(SN)) return new SN_Member();

            if (SN.Count() != 12) return new SN_Member();  //If SN number length is less than 12, return.

            string _chipcode = "", _productcode = "";

            SNMember = new SN_Member();
            SNMember.SNnumber = SN;

            //List<string> prod_info = new List<string>();
            //prod_info.Add(SN);  //SN number

            if (SN.Substring(1, 1) == "9")  //U-Series Product Type 
            {
                _productcode = SN.Substring(5, 2);
                _chipcode = SN.Substring(2, 2);
                //prod_info.Add(Product_Analyze_Table(_chipcode, _productcode));
                SNMember.ProductType = Product_Analyze_Table(_chipcode, _productcode);
            }
            else if (SN.Substring(1, 1) == "7")  //MTF Product Type 
            {
                //prod_info.Add("MTF");
                SNMember.ProductType = "MTF";
            }

            if (SNMember.ProductType != "MTF")    //if production type = U-Series
            {
                string s = SN.Substring(6, 1);
                switch (s)  //Band width
                {
                    case "1":
                        //prod_info.Add("100");
                        SNMember.BandWidth = "100";
                        break;
                    case "2":
                        //prod_info.Add("250");
                        //prod_info[1] = "UFA-T";  //因目前無UTF250H的版本，若有，則需更新邏輯 add by Sean Wu 20200102

                        if (SNMember.ProductType.Equals("UFA"))
                            SNMember.ProductType = "UFA-T";
                        else if (SNMember.ProductType.Equals("UTF"))
                            SNMember.ProductType = "UTF";

                        SNMember.BandWidth = "250";
                        break;
                    case "3":
                        //prod_info.Add("300");
                        SNMember.BandWidth = "300";
                        break;
                    case "4":
                        //prod_info.Add("400");
                        SNMember.BandWidth = "400";
                        break;
                    case "5":
                        //prod_info.Add("500");
                        SNMember.BandWidth = "500";
                        break;
                }

                if (SN.Substring(3, 1) == "0")  //Laser band
                    SNMember.LaserBand = "C Band";
                else if (SN.Substring(3, 1) == "1")
                    SNMember.LaserBand = "L Band";
                else if (SN.Substring(3, 1) == "2")
                    SNMember.LaserBand = "C Band";
                else if (SN.Substring(3, 1) == "3")
                    SNMember.LaserBand = "L Band";
                else if (SN.Substring(3, 1) == "4")
                    SNMember.LaserBand = "C Band";
                else if (SN.Substring(3, 1) == "5")
                    SNMember.LaserBand = "L Band";

                SNMember.ChipType = SN.Substring(2, 1);
                SNMember.Year = SN.Substring(4, 1);

                //prod_info.Add(SN.Substring(2, 1));  //Chip Type
                //prod_info.Add(SN.Substring(4, 1));  //Year
            }
            else  //if product type = MTF
            {
                string s = SN.Substring(5, 1);
                switch (s)  //Band width
                {
                    case "1":
                        SNMember.BandWidth = "100";
                        SNMember.LaserBand = "O Band";
                        break;
                    case "2":
                        SNMember.BandWidth = "100";
                        SNMember.LaserBand = "C Band";
                        break;
                    case "3":
                        SNMember.BandWidth = "100";
                        SNMember.LaserBand = "L Band";
                        break;
                    case "4":
                        SNMember.BandWidth = "50";
                        SNMember.LaserBand = "C Band";
                        break;
                    case "5":
                        SNMember.BandWidth = "50";
                        SNMember.LaserBand = "L Band";
                        break;
                    case "6":
                        SNMember.BandWidth = "100";
                        SNMember.LaserBand = "C+L Band";
                        break;
                    case "7":
                        SNMember.BandWidth = "100";
                        SNMember.LaserBand = "E Band";
                        break;
                    case "8":
                        SNMember.BandWidth = "100";
                        SNMember.LaserBand = "S Band";
                        break;
                    case "9":
                        SNMember.BandWidth = "100";
                        SNMember.LaserBand = "U Band";
                        break;
                    default:
                        SNMember.BandWidth = "100";
                        SNMember.LaserBand = "C Band";
                        break;
                }

                SNMember.ChipType = "";
                SNMember.Year = SN.Substring(4, 1);
            }

            //SNMember = new SN_Member() { SNnumber = prod_info[0], ProductType = prod_info[1], BandWidth = prod_info[2], LaserBand = prod_info[3], ChipType = prod_info[4], Year = prod_info[5] };

            return SNMember;
        }

        private string Product_Analyze_Table(string Chip_Code, string Product_Code)  //產品表
        {
            string Product_type = "";

            if (Chip_Code.Equals("A0", StringComparison.CurrentCultureIgnoreCase) && Product_Code.Equals("A3", StringComparison.CurrentCultureIgnoreCase))  //12067-02
                Product_type = "UFA";  //C band
            else if (Chip_Code.Equals("A1", StringComparison.CurrentCultureIgnoreCase) && Product_Code.Equals("A3", StringComparison.CurrentCultureIgnoreCase))  //12067-01
                Product_type = "UFA";  //L band
            else if (Chip_Code.Equals("C0", StringComparison.CurrentCultureIgnoreCase) && Product_Code.Equals("A3", StringComparison.CurrentCultureIgnoreCase))  //UFA320-C
                Product_type = "UFA(H)";  //C band
            else if (Chip_Code.Equals("A0", StringComparison.CurrentCultureIgnoreCase) && Product_Code.Equals("A2", StringComparison.CurrentCultureIgnoreCase))  //12110-02
                Product_type = "UFA-T";
            else if (Chip_Code.Equals("B0", StringComparison.CurrentCultureIgnoreCase) && Product_Code.Equals("A3", StringComparison.CurrentCultureIgnoreCase))  //12067-12
                Product_type = "UFA(H)";  //C band
            else if (Chip_Code.Equals("B1", StringComparison.CurrentCultureIgnoreCase) && Product_Code.Equals("A3", StringComparison.CurrentCultureIgnoreCase))  //12067-11
                Product_type = "UFA(H)";  //L band
            else if (Chip_Code.Equals("A0", StringComparison.CurrentCultureIgnoreCase) && Product_Code.Equals("U3", StringComparison.CurrentCultureIgnoreCase))  //12711
                Product_type = "UTF";  //C band
            else if (Chip_Code.Equals("A1", StringComparison.CurrentCultureIgnoreCase) && Product_Code.Equals("U3", StringComparison.CurrentCultureIgnoreCase))  //12712
                Product_type = "UTF";  //L band
            else if (Chip_Code.Equals("A2", StringComparison.CurrentCultureIgnoreCase) && Product_Code.Equals("U3", StringComparison.CurrentCultureIgnoreCase))  //12711 (H)
                Product_type = "UTF";  //C band
            else if (Chip_Code.Equals("A2", StringComparison.CurrentCultureIgnoreCase) && Product_Code.Equals("U2", StringComparison.CurrentCultureIgnoreCase))  //13256
                Product_type = "UTF";  //C band
            else if (Chip_Code.Equals("A3", StringComparison.CurrentCultureIgnoreCase) && Product_Code.Equals("U3", StringComparison.CurrentCultureIgnoreCase))  //12712 (H)
                Product_type = "UTF";  //L band
            else if (Chip_Code.Equals("B0", StringComparison.CurrentCultureIgnoreCase) && Product_Code.Equals("U3", StringComparison.CurrentCultureIgnoreCase))  //13164
                Product_type = "UTF300(H)";
            else if (Chip_Code.Equals("B0", StringComparison.CurrentCultureIgnoreCase) && Product_Code.Equals("U4", StringComparison.CurrentCultureIgnoreCase))  //13076
                Product_type = "UTF400";
            else if (Chip_Code.Equals("D0", StringComparison.CurrentCultureIgnoreCase) && Product_Code.Equals("U4", StringComparison.CurrentCultureIgnoreCase))  //13076
                Product_type = "UTF450";
            else if (Chip_Code.Equals("B0", StringComparison.CurrentCultureIgnoreCase) && Product_Code.Equals("U5", StringComparison.CurrentCultureIgnoreCase))  //13075
                Product_type = "UTF500";
            else if (Chip_Code.Equals("D0", StringComparison.CurrentCultureIgnoreCase) && Product_Code.Equals("U5", StringComparison.CurrentCultureIgnoreCase))  //13075
                Product_type = "UTF550";
            else if (Chip_Code.Equals("A0", StringComparison.CurrentCultureIgnoreCase) && Product_Code.Equals("C3", StringComparison.CurrentCultureIgnoreCase))  //12355,12685
                Product_type = "CTF";

            return Product_type;
        }

       static public string WL_Range_Analyze(double wl)
        {
            //if (wl >= 1523 && wl <= 1620)
            //    return "C+L Band";
            if (wl >= 1625 && wl <= 1675)
                return "U Band";
            else if (wl >= 1560 && wl <= 1625)
                return "L Band";
            else if (wl >= 1520 && wl <= 1573)
                return "C Band";
            else if (wl >= 1460 && wl <= 1520)
                return "S Band";
            else if (wl >= 1360 && wl <= 1460)
                return "E Band";
            else if (wl >= 1260 && wl <= 1360)
                return "O Band";

            else return "";  //Out of range
        }

        public bool CheckDirectoryExist(string dir_path)
        {
            if (Directory.Exists(dir_path))
                return true;
            else
            {
                vm.Save_Log(new LogMember() { isShowMSG = true, Message = "Folder is not exist", Result = "Timeout" });
                return false;
            }
        }

        public bool CreateDirectory(string dir_path)
        {
            Directory.CreateDirectory(dir_path);  //Creat folder on 192 server

            if (Directory.Exists(dir_path))
                return true;
            else
                return false;
        }

        public List<double> list_SMR_slope = new List<double>();
        public List<double> list_SMR_Mountain_IL = new List<double>();
        public List<DataPoint> list_SMR_Mountain = new List<DataPoint>();

        public void BandWidth_Calculation()
        {
            if (vm.Chart_DataPoints.Count != 0)
            {
                var orderedSeries = vm.Chart_DataPoints.OrderBy(o => o.Y).ToList();

                int index_max = vm.Chart_DataPoints.IndexOf(orderedSeries.Last());
                OxyPlot.DataPoint dp_IL_Max = orderedSeries.Last();
                double IL_Max = dp_IL_Max.Y;

                double bwSetting = 0;
                double bw = 0;
                double bw_cwl = 0;

                for (int b = 1; b <= 4; b++)
                {
                    switch (b)
                    {
                        case 1:
                            bwSetting = vm.ChartNowModel.BW_Setting_1;
                            break;

                        case 2:
                            bwSetting = vm.ChartNowModel.BW_Setting_2;
                            break;

                        case 3:
                            bwSetting = vm.ChartNowModel.BW_Setting_3;
                            break;

                        case 4:
                            bwSetting = vm.ChartNowModel.BW_Setting_4;
                            break;
                    }

                    #region Cal. BW_1 dB Bandwidth

                    //Find over-threshold point from center to right side
                    int index_R = 0; double wl_R = 0;
                    for (int i = index_max; i < vm.Chart_DataPoints.Count; i++)
                    {
                        if (Math.Abs(IL_Max - vm.Chart_DataPoints[i].Y) >= bwSetting)
                        {
                            index_R = i;

                            if (i > 0)
                            {
                                wl_R = ((IL_Max - bwSetting) - vm.Chart_DataPoints[i - 1].Y) * (vm.Chart_DataPoints[i].X - vm.Chart_DataPoints[i - 1].X)
                                    / (vm.Chart_DataPoints[i].Y - vm.Chart_DataPoints[i - 1].Y) + vm.Chart_DataPoints[i - 1].X;
                            }
                            else
                                wl_R = vm.Chart_DataPoints[i].X;

                            break;
                        }
                    }

                    //Find over-threshold point from center to left side
                    int index_L = 0; double wl_L = 0;
                    for (int i = index_max; i >= 0; i--)
                    {
                        if (Math.Abs(IL_Max - vm.Chart_DataPoints[i].Y) >= bwSetting)
                        {
                            index_L = i;

                            if (i > 0)
                            {
                                wl_L = ((IL_Max - bwSetting) - vm.Chart_DataPoints[i].Y) * (vm.Chart_DataPoints[i - 1].X - vm.Chart_DataPoints[i].X)
                                    / (vm.Chart_DataPoints[i - 1].Y - vm.Chart_DataPoints[i].Y) + vm.Chart_DataPoints[i].X;
                            }
                            else
                                wl_L = vm.Chart_DataPoints[i].X;

                            break;
                        }
                    }

                    //Get bandwidth (cwl) by wl_L and wl_R
                    if (wl_R == 0 || wl_L == 0)
                    {
                        bw = 0;
                        bw_cwl = 0;
                    }
                    else
                    {
                        double BW = Math.Abs(wl_R - wl_L);
                        bw = Math.Round(BW, 2);
                        bw_cwl = Math.Abs(Math.Round((wl_R + wl_L) / 2, 2));
                    }

                    //if (bw != 0)
                    //{
                    //    vm.opModel_1.WL_1_BW_1 = bw;
                    //    vm.opModel_1.WL_1_CWL = bw_cwl;
                    //}
                    #endregion

                    switch (b)
                    {
                        case 1:
                            vm.ChartNowModel.BW_1 = bw;
                            vm.ChartNowModel.BW_CWL_1 = bw_cwl;
                            break;

                        case 2:
                            vm.ChartNowModel.BW_2 = bw;
                            vm.ChartNowModel.BW_CWL_2 = bw_cwl;
                            break;

                        case 3:
                            vm.ChartNowModel.BW_3 = bw;
                            vm.ChartNowModel.BW_CWL_3 = bw_cwl;
                            break;

                        case 4:
                            vm.ChartNowModel.BW_4 = bw;
                            vm.ChartNowModel.BW_CWL_4 = bw_cwl;
                            break;
                    }
                }

                if (vm.station_type != ComViewModel.StationTypes.UTF600)
                    return;

                #region Cal. FOM

                double FOM = 0;
                if (vm.ChartNowModel.BW_4 != 0)
                {
                    FOM = vm.ChartNowModel.BW_3 / vm.ChartNowModel.BW_4;

                    vm.ChartNowModel.FOM = vm.ChartNowModel.FOM != FOM ? Math.Round(FOM, 3) : 0;
                }
                else
                    vm.ChartNowModel.FOM = 0;

                #endregion


                #region Cal. SMR

                double SMR = 0;

                if (vm.ChartNowModel.list_dataPoints[0].Count < 2) return;

                list_SMR_Mountain_IL.Clear();
                list_SMR_Mountain.Clear();

                int Mountain_LeftCount = 0;
                int Mountain_TopCount = 0;
                int Mountain_RightCount = 0;
                double NoiseThreshold = 0.02;

                for (int i = 1; i < vm.ChartNowModel.list_dataPoints[0].Count; i++)
                {
                    DataPoint dp_2 = vm.ChartNowModel.list_dataPoints[0][i];
                    DataPoint dp_1 = vm.ChartNowModel.list_dataPoints[0][i - 1];

                    //Moutain not found yet
                    if (Mountain_LeftCount < 3)
                    {
                        if ((dp_2.Y - dp_1.Y) > NoiseThreshold)
                            Mountain_LeftCount++;
                    }
                    else if (Mountain_LeftCount >= 3)
                    {
                        //On the top of the mountain
                        if (Math.Abs(dp_2.Y - dp_1.Y) <= NoiseThreshold)  //stable region
                        {
                            Mountain_TopCount++;
                            Mountain_RightCount = 0;
                            continue;
                        }
                        else if ((dp_1.Y - dp_2.Y) > NoiseThreshold)  //down the hill
                        {
                            Mountain_RightCount++;
                            //Mountain_TopCount = 0;
                        }
                        else if ((dp_2.Y - dp_1.Y) > NoiseThreshold)  //up the hill
                        {
                            Mountain_TopCount = 0;
                            continue;
                        }
                    }

                    //Found a mountain
                    if (Mountain_LeftCount >= 3 && Mountain_RightCount >= 3)
                    {
                        int allCount = Mountain_LeftCount + Mountain_TopCount + Mountain_RightCount;

                        int startIndex = (i - (allCount)) < 0 ? 0 : i - (allCount);
                        List<DataPoint> list_mountainRange = vm.ChartNowModel.list_dataPoints[0].GetRange
                            (startIndex, allCount);

                        DataPoint TopMountain = list_mountainRange.OrderBy(p => p.Y).Last();

                        list_SMR_Mountain_IL.Add(TopMountain.Y);
                        list_SMR_Mountain.Add(TopMountain);

                        Mountain_TopCount = 0;
                        Mountain_LeftCount = 0;
                        Mountain_RightCount = 0;
                    }

#if false
                DataPoint dp2 = vm.ChartNowModel.list_dataPoints[0].Last();
                DataPoint dp1 = vm.ChartNowModel.list_dataPoints[0][vm.ChartNowModel.list_dataPoints[0].Count - 2];
                double slopNow = (dp2.Y - dp1.Y) / (dp2.X - dp1.X);
                list_SMR_slope.Add(Math.Round(slopNow, 3));

                int isMoutainCount = 0;
                int toleranceCount = 0;
                list_SMR_Mountain_IL.Clear();

                for (int i = 1; i < list_SMR_slope.Count; i++)
                {
                    //點數太少不予判斷
                    if (list_SMR_slope.Count < 7)
                        break;

                    if (list_SMR_slope[i] <= list_SMR_slope[i - 1])
                    {
                        if (isMoutainCount < 4)
                        {
                            if (list_SMR_slope[i] >= 0)
                                isMoutainCount++;
                        }
                        else
                        {
                            if (list_SMR_slope[i] <= 0)
                                isMoutainCount++;
                        }

                        if (isMoutainCount >= 8)
                        {
                            list_SMR_Mountain_IL.Add(vm.ChartNowModel.list_dataPoints[0][i - 3].Y);  //往回推3個點為山頂
                            isMoutainCount = 0;
                        }
                    }
                    else
                    {
                        if (isMoutainCount < 4 && list_SMR_slope[i] > 0)
                        {
                            continue;
                        }

                        else if (isMoutainCount >= 4)
                        {
                            if (list_SMR_slope[i] >= 0 && list_SMR_slope[i - 1] >= 0)
                                continue;
                        }

                        toleranceCount++;
                        if (toleranceCount > 4)  //IL跳動容許次數{
                        {
                            isMoutainCount = 0;
                            toleranceCount = 0;
                        }
                    }
                } 
#endif
                }

                //取最高的兩個IL
                if (list_SMR_Mountain.Count > 1)
                {
                    List<DataPoint> list_SMR_inorder = list_SMR_Mountain.OrderBy(x => x.Y).ToList();
                    SMR = Math.Abs(list_SMR_inorder.Last().Y - list_SMR_inorder[list_SMR_inorder.Count - 2].Y);
                    vm.ChartNowModel.SMRR = Math.Round(SMR, 2);

                    if (vm.isSMRR_Annotation)
                    {
                        vm.PointAnnotation_1.StrokeThickness = 8;
                        vm.PointAnnotation_2.StrokeThickness = 8;
                    }

                    vm.PointAnnotation_1.X = list_SMR_Mountain.Last().X;
                    vm.PointAnnotation_1.Y = list_SMR_Mountain.Last().Y;

                    vm.PointAnnotation_2.X = list_SMR_inorder[list_SMR_inorder.Count - 2].X;
                    vm.PointAnnotation_2.Y = list_SMR_inorder[list_SMR_inorder.Count - 2].Y;
                }
                else if (list_SMR_Mountain.Count == 1)
                {
                    vm.ChartNowModel.SMRR = Math.Round(list_SMR_Mountain.Last().Y, 2);

                    if(vm.PointAnnotation_1.StrokeThickness != 8)
                    {
                        if(vm.isSMRR_Annotation)
                            vm.PointAnnotation_1.StrokeThickness = 8;

                        vm.PointAnnotation_1.X = list_SMR_Mountain.Last().X;
                        vm.PointAnnotation_1.Y = list_SMR_Mountain.Last().Y;
                    }
                }


                //Add 30dB line in chart

                double maxIL = vm.ChartNowModel.list_dataPoints[0].Max(x => x.Y);
                double minIL = vm.ChartNowModel.list_dataPoints[0].Min(x => x.Y);
                double SpecLinePosition = maxIL - 30;

                if (SpecLinePosition > minIL && vm.LineAnnotation_Y.Y != SpecLinePosition)
                {
                    vm.LineAnnotation_Y.StrokeThickness = 2.5;
                    vm.LineAnnotation_Y.Y = SpecLinePosition;
                    vm.PlotViewModel.InvalidatePlot(true);
                }

                #endregion

            }
        }

        public double[,] BandWidth_Calculation(int WL_No)
        {
            double bwSetting = 0;
            double bw = 0;
            double bw_cwl = 0;

            double[,] List_BW_WL_Pos = new double[3,2];

            if (vm.Chart_DataPoints.Count != 0)
            {
                var orderedSeries = vm.Chart_DataPoints.OrderBy(o => o.Y).ToList();

                int index_max = vm.Chart_DataPoints.IndexOf(orderedSeries.Last());
                OxyPlot.DataPoint dp_IL_Max = orderedSeries.Last();
                double IL_Max = dp_IL_Max.Y;

                //Cal. 3 types of BW Setting 
                for (int b = 1; b <= 3; b++)
                {
                    foreach (var item in vm.props_opModel)
                    {
                        if (item.Name == $"BW_Setting_{b}")
                        {
                            bwSetting = (double)item.GetValue(vm.opModel_1, null);
                        }
                    }

                    #region Cal. Bandwidth

                    //Find over-threshold point from max IL to right side
                    int index_R = 0; double wl_R = 0;
                    for (int i = index_max; i < vm.Chart_DataPoints.Count; i++)
                    {
                        if (Math.Abs(IL_Max - vm.Chart_DataPoints[i].Y) >= bwSetting)
                        {
                            index_R = i;

                            if (i > 0)
                            {
                                wl_R = ((IL_Max - bwSetting) - vm.Chart_DataPoints[i - 1].Y) * (vm.Chart_DataPoints[i].X - vm.Chart_DataPoints[i - 1].X)
                                    / (vm.Chart_DataPoints[i].Y - vm.Chart_DataPoints[i - 1].Y) + vm.Chart_DataPoints[i - 1].X;
                            }
                            else
                                wl_R = vm.Chart_DataPoints[i].X;

                            break;
                        }
                    }

                    //Find over-threshold point from center to left side
                    int index_L = 0; double wl_L = 0;
                    for (int i = index_max; i >= 0; i--)
                    {
                        if (Math.Abs(IL_Max - vm.Chart_DataPoints[i].Y) >= bwSetting)
                        {
                            index_L = i;

                            if (i > 0)
                            {
                                wl_L = ((IL_Max - bwSetting) - vm.Chart_DataPoints[i].Y) * (vm.Chart_DataPoints[i - 1].X - vm.Chart_DataPoints[i].X)
                                    / (vm.Chart_DataPoints[i - 1].Y - vm.Chart_DataPoints[i].Y) + vm.Chart_DataPoints[i].X;
                            }
                            else
                                wl_L = vm.Chart_DataPoints[i].X;

                            break;
                        }
                    }

                    //Get bandwidth (cwl) by wl_L and wl_R
                    if (wl_R == 0 || wl_L == 0)
                    {
                        bw = 0;
                        bw_cwl = 0;
                    }
                    else
                    {
                        double BW = Math.Abs(wl_R - wl_L);
                        bw = Math.Round(BW, 2);

                        List_BW_WL_Pos[b - 1, 0] = Math.Round(wl_L,2);
                        List_BW_WL_Pos[b - 1, 1] = Math.Round(wl_R, 2);

                        if (b == 2)
                            bw_cwl = Math.Abs(Math.Round((wl_R + wl_L) / 2, 2));                   
                    }

                    #endregion

                    //Only 3dB bw will update cwl
                    foreach (var item in vm.props_opModel)
                    {
                        if (item.Name == ($"WL_{WL_No}_BW_{b}"))
                            item.SetValue(vm.opModel_1, bw);

                        if (b == 2 && item.Name == $"WL_{WL_No}_CWL")
                            item.SetValue(vm.opModel_1, bw_cwl);
                    }
                }
            }

            return List_BW_WL_Pos;
        }

        public void JudgeAllBoolGauge()
        {
            vm.BoolAllGauge = vm.list_GaugeModels.Where(x => x.boolGauge).ToList().Count == 0 ? true : false;
        }

        public VariableModel JudgeVariable(string input, int rtnType)
        {
            VariableModel varM = new VariableModel();
            int value;
            char c = input.ToCharArray()[0];
            switch (c)
            {
                case '*':
                    if (int.TryParse(input.Remove(0, 1), out value))
                    {
                        varM.VariableContent = vm.list_VariableModels[value].VariableContent;
                        varM.VariableIndex = value;
                    }
                    break;

                case '@':
                    if (int.TryParse(input.Remove(0, 1), out value))
                    {
                        varM.VariableBool = vm.list_VariableModels[value].VariableBool;
                        varM.VariableIndex = value;
                    }
                    break;

                default:
                    value = int.Parse(input);
                    switch (rtnType)
                    {
                        case 0:
                            varM.VariableContent = vm.list_VariableModels[value].VariableContent;
                            break;
                        case 1:
                            varM.VariableBool = vm.list_VarBoolModels[value].VariableBool;
                            break;
                    }
                    varM.VariableIndex = value;
                    break;
            }

            return varM;
        }

        public async Task<string[]> Analyze_PreDAC(string comport, string ch)
        {
            string[] result = new string[] { "", "", "" };

            //Ask DAC? and analyze
            try
            {
                if (!vm.port_PD.IsOpen)
                    await vm.Port_ReOpen(comport);

                if (vm.port_PD.IsOpen)
                {
                    string cmd = string.Format("D{0}?", ch);
                    vm.port_PD.Write(cmd + "\r");

                    await Task.Delay(vm.Int_Read_Delay);

                    int size = vm.port_PD.BytesToRead;
                    byte[] dataBuffer = new byte[size];
                    if (size > 0)
                    {
                        dataBuffer = new byte[size];
                        int length = vm.port_PD.Read(dataBuffer, 0, size);
                    }

                    //資料分析並顯示於狀態列
                    string msg = Read_analysis(cmd, dataBuffer);

                    #region Analyze Dx? and show data                                        
                    string[] words = msg.Split(',');  //V1,V2,V3 

                    if (words.Length >= 2)
                    {
                        result = words;
                        int v1, v2;
                        if (int.TryParse(words[0], out v1) && int.TryParse(words[1], out v2))
                        {
                            //If V1(or V2) is not equal to zero, keep it as usual
                            if (v1 == 0) result[0] = "0";
                            else result[1] = "0";
                        }
                    }
                    #endregion
                }
            }
            catch { }

            return result;
        }

        public float Read_PM_to_Gauge(double power_PM, int ch)
        {
            int Switch_Number = ch + 1;
            vm.Value_PD.Clear();
            vm.Double_Powers.Clear();
            power_PM = Math.Round(power_PM, 3);
            float y = Convert.ToSingle(power_PM);
            float z = (y * 300 / -64 - 150) * -1;
            z = z != 1350 ? z : 150;

            if (z < -150)
                z = -150;

            if (Switch_Number < 9) //Switch mode  1~8
            {
                vm.Value_PD = new List<float>() { -150, -150, -150, -150, -150, -150, -150, -150 };
                vm.Double_Powers = Analysis.ListDefault<double>(8);

                vm.Value_PD[Switch_Number - 1] = z;
                vm.Double_Powers[Switch_Number - 1] = y;
                vm.Value_PD = new List<float>(vm.Value_PD);

                vm.Str_PD = Analysis.ListDefault<string>(8);
                vm.Str_PD[Switch_Number - 1] = power_PM.ToString();
                vm.Str_PD = new List<string>(vm.Str_PD);
            }
            else if (Switch_Number < 13 && Switch_Number >= 9)  //Switch mode  9~12
            {
                vm.Value_PD = new List<float>() { -150, -150, -150, -150, -150, -150, -150, -150 };
                vm.Double_Powers = Analysis.ListDefault<double>(8);

                vm.Value_PD[Switch_Number - 9] = z;
                vm.Double_Powers[Switch_Number - 9] = y;
                vm.Value_PD = new List<float>(vm.Value_PD);

                vm.Str_PD = Analysis.ListDefault<string>(8);
                vm.Str_PD[Switch_Number - 9] = power_PM.ToString();
                vm.Str_PD = new List<string>(vm.Str_PD);
            }
            else  //Normal mode
            {
                vm.Value_PD.Add(z);  //-150~150 degree, for gauge binding
                vm.Double_Powers.Add(y);  //list 0~-64dBm in float type

                vm.Str_PD = new List<string>() { power_PM.ToString() };
            }

            return z;
        }
                
        Random rdm = new Random();
        public string Read_analysis(string cmd, byte[] dataBuffer)
        {
            string msg = "";

            cmd = cmd == "P0_Read" ? "P0?" : cmd;
            cmd = cmd == "PD_Read" ? "PD?" : cmd;
            cmd = cmd == "ID_Read" ? "ID?" : cmd;

            StringBuilder sb = new StringBuilder();

            switch (cmd)
            {
                case "ID?":
                    msg = GetMessage(dataBuffer);
                    vm.Str_cmd_read = list_read_value[0];
                    break;

                case "P0?":
                    str_read_value = new StringBuilder();
                    list_read_value = new List<string>();
                    list_databuffer = new List<byte>(dataBuffer); //Convert array to list

                    char c = ' ';

                    foreach (byte data in list_databuffer)
                    {
                        c = Convert.ToChar(data);

                        sb.Append(c);

                        if (data != 10 && data != 44 && data != 13)
                            str_read_value.Append(c);
                        else
                        {
                            list_read_value.Add(str_read_value.ToString());
                            str_read_value.Clear();
                        }
                    }
                                
                    if(list_read_value.Count <=0) return sb.ToString();

                    list_read_value.RemoveAt(0);  //list dBm in string type

                    vm.Double_Powers = new List<double>();
                    List<string> list_dB_readpower = new List<string>();

                    if (vm.dB_or_dBm)   //dB
                    {
                        int ch = 0;  //Channel No.
                        foreach (string x in list_read_value)
                        {
                            if (x != "")
                            {
                                double y = Math.Round(Convert.ToDouble(x) - vm.float_WL_Ref[ch], vm.decimal_place);  //y is 0~-64dB in double type
                                list_dB_readpower.Add(y.ToString());
                                vm.Double_Powers.Add(y);  //list 0~-64dBm in float type
                            }
                            ch++;
                        }
                    }
                    else  //dBm
                    {
                        foreach (string x in list_read_value)
                        {
                            if (!string.IsNullOrEmpty(x))
                            {
                                double y = Math.Round(Convert.ToDouble(x), vm.decimal_place);  //y is 0~-64dBm in float type       
                                vm.Double_Powers.Add(y);  //list 0~-64dBm in float type
                            }
                        }
                    }

                    if (vm.isDeltaILModeOn)  //Delta IL mode
                    {
                        int i = 0;
                        vm.Str_PD = new List<string>();
                        foreach (float s in vm.savedPower_for_deltaMode)
                        {
                            vm.Double_Powers[i] = vm.Double_Powers[i] - s;

                            if (vm.Double_Powers[i] > vm.double_MaxIL_for_DeltaMode[i])
                                vm.double_MaxIL_for_DeltaMode[i] = vm.Double_Powers[i];
                            else if (vm.Double_Powers[i] < vm.double_MinIL_for_DeltaMode[i])
                                vm.double_MinIL_for_DeltaMode[i] = vm.Double_Powers[i];

                            vm.double_Maxdelta[i] = Math.Round((vm.double_MaxIL_for_DeltaMode[i] - vm.double_MinIL_for_DeltaMode[i]), vm.decimal_place);

                            vm.Str_PD.Add(vm.double_Maxdelta[i].ToString());
                            i++;
                        }
                    }

                    msg = sb.ToString();

                    break;

                case "PD?":
                    str_read_value = new StringBuilder();
                    list_read_value = new List<string>();
                    list_databuffer = new List<byte>(dataBuffer); //Convert array to list

                    foreach (byte data in list_databuffer)
                    {
                        if (data != 10 && data != 44 && data != 13)
                            str_read_value.Append(Convert.ToChar(data));
                        else
                        {
                            list_read_value.Add(str_read_value.ToString());
                            str_read_value.Clear();
                        }
                    }

                    list_read_value.RemoveAt(0);  //list dBm in string type

                    vm.Double_Powers = new List<double>();
                    list_dB_readpower = new List<string>();

                    foreach (string x in list_read_value)
                    {
                        if (!string.IsNullOrEmpty(x))
                        {
                            double y = Math.Round(Convert.ToDouble(x), vm.decimal_place);  //y is 0~-64dBm in float type       
                            vm.Double_Powers.Add(y);  //list 0~-64dBm in float type
                        }
                    }

                    if (vm.isDeltaILModeOn)  //Delta IL mode
                    {
                        int i = 0;
                        vm.Str_PD = new List<string>();
                        foreach (float s in vm.savedPower_for_deltaMode)
                        {
                            vm.Double_Powers[i] = vm.Double_Powers[i] - s;

                            if (vm.Double_Powers[i] > vm.double_MaxIL_for_DeltaMode[i])
                            {
                                vm.double_MaxIL_for_DeltaMode[i] = vm.Double_Powers[i];
                            }
                            else if (vm.Double_Powers[i] < vm.double_MinIL_for_DeltaMode[i])
                            {
                                vm.double_MinIL_for_DeltaMode[i] = vm.Double_Powers[i];
                            }

                            vm.double_Maxdelta[i] = Math.Round((vm.double_MaxIL_for_DeltaMode[i] - vm.double_MinIL_for_DeltaMode[i]), vm.decimal_place);

                            vm.Str_PD.Add(vm.double_Maxdelta[i].ToString());
                            i++;
                        }
                    }
                    break;

                default: //D1?, D2?... D1 0,0...
                    msg = GetMessage(dataBuffer);
                    vm.Str_cmd_read = list_read_value[0];

                    break;
            }
            return msg;
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
                    str_read_value = new StringBuilder();
                    list_read_value = new List<string>();
                    list_databuffer = new List<byte>(dataBuffer); //Convert array to list

                    foreach (byte data in list_databuffer)
                    {
                        if (data != 10 && data != 44 && data != 13)
                        {
                            str_read_value.Append(Convert.ToChar(data));
                        }
                        else
                        {
                            list_read_value.Add(str_read_value.ToString());
                            str_read_value.Clear();
                        }
                    }
                    list_read_value.RemoveAt(0);  //list dBm in string type

                    vm.Value_PD = new List<float>();
                    vm.Double_Powers = new List<double>();
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
                                vm.Double_Powers.Add(y);  //list 0~-64dBm in float type
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
                                vm.Double_Powers.Add(y);  //list 0~-64dBm in float type
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
                            vm.Double_Powers[i] = vm.Double_Powers[i] - s;

                            if (vm.Double_Powers[i] > vm.double_MaxIL_for_DeltaMode[i])
                            {
                                vm.double_MaxIL_for_DeltaMode[i] = vm.Double_Powers[i];
                            }
                            else if (vm.Double_Powers[i] < vm.double_MinIL_for_DeltaMode[i])
                            {
                                vm.double_MinIL_for_DeltaMode[i] = vm.Double_Powers[i];
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
            str_read_value = new StringBuilder();
            list_read_value = new List<string>();
            list_databuffer = new List<byte>(dataBuffer); //Convert array to list

            foreach (byte data in list_databuffer)
            {
                if (data != 10 && data != 44 && data != 13)
                {
                    str_read_value.Append(Convert.ToChar(data));  //dBm in string type
                }
                else
                {
                    list_read_value.Add(str_read_value.ToString());
                    str_read_value.Clear();
                }
            }
            if (list_read_value.Count > 1)
                list_read_value.RemoveAt(0); //list dBm in string type            

            List<float> _list_float = new List<float>(new float[0]);
            vm.Double_Powers = new List<double>();

            foreach (string x in list_read_value)
            {
                if (x != "")
                {
                    float y = Convert.ToSingle((string)x);
                    float z = (y * 300 / -64 - 150) * -1;
                    z = z != 1350 ? z : 150;
                    _list_float.Add(z);  //-150~150 degree, for gauge binding
                    vm.Double_Powers.Add(y);  //list 0~-64dBm in float type 
                }
            }
            vm.Value_PD = new List<float>(_list_float);
            vm.Str_PD = list_read_value;

            return vm;
        }

        public ComViewModel _K_analysis(int dac, byte[] dataBuffer)
        {
            str_read_value = new StringBuilder();
            list_read_value = new List<string>();
            list_databuffer = new List<byte>(dataBuffer); //Convert array to list

            foreach (byte data in list_databuffer)
            {
                if (data != 10 && data != 44 && data != 13)
                {
                    str_read_value.Append(Convert.ToChar(data)); //dBm in string type
                }
                else
                {
                    list_read_value.Add(str_read_value.ToString());
                    str_read_value.Clear();
                }
            }
            if (list_read_value.Count > 1)
                list_read_value.RemoveAt(0); //list dBm in string type            

            List<float> _list_float = new List<float>(new float[0]);
            vm.Double_Powers = new List<double>();

            foreach (string x in list_read_value)
            {
                if (x != "")
                {
                    float y = Convert.ToSingle((string)x);
                    float z = (y * 300 / -64 - 150) * -1;
                    z = z != 1350 ? z : 150;
                    _list_float.Add(z);  //-150~150 degree, for gauge binding
                    vm.Double_Powers.Add(y);  //list 0~-64dBm in float type 
                }
            }
            vm.Value_PD = new List<float>(_list_float);
            vm.Str_PD = list_read_value;

            vm.List_V3.Add(dac);
            for (int i = 0; i < 8; i++)
            {
                if (vm.List_PDvalue_byV3[i] == null)
                    vm.List_PDvalue_byV3[i] = new List<float>();

                if (i < vm.Double_Powers.Count)
                    vm.List_PDvalue_byV3[i].Add(Convert.ToSingle(vm.Double_Powers[i]));
            }

            return vm;
        }

        public ComViewModel _K12_analysis_PM(int dac, List<double> power)
        {
            List<float> _list_float = new List<float>(new float[0]);
            vm.Double_Powers = new List<double>();
            vm.Str_PD = new List<string>();
            foreach (double x in power)
            {
                if (x != 99)
                {
                    float y = Convert.ToSingle(x);
                    float z = (y * 300 / -64 - 150) * -1;
                    z = z != 1350 ? z : 150;
                    _list_float.Add(z);  //-150~150 degree, for gauge binding
                    vm.Double_Powers.Add(y);  //list 0~-64dBm in float type 
                }
            }
            vm.Value_PD = new List<float>(_list_float);
            vm.Str_PD = new List<string>() { Math.Round(power[0], 2).ToString() };

            vm.List_V12.Add(dac); //Save DAC
            if (vm.List_PMvalue_byV12 == null)
                vm.List_PMvalue_byV12 = new List<float>();

            if (vm.Double_Powers.Count > 0)
                vm.List_PMvalue_byV12.Add(Convert.ToSingle(vm.Double_Powers[0]));

            return vm;
        }

        public ComViewModel _K12_analysis(int dac, byte[] dataBuffer)
        {
            str_read_value = new StringBuilder();
            list_read_value = new List<string>();
            list_databuffer = new List<byte>(dataBuffer); //Convert array to list

            foreach (byte data in list_databuffer)
            {
                if (data != 10 && data != 44 && data != 13)
                {
                    str_read_value.Append(Convert.ToChar(data)); //dBm in string type
                }
                else
                {
                    list_read_value.Add(str_read_value.ToString());
                    str_read_value.Clear();
                }
            }
            if (list_read_value.Count > 1)
                list_read_value.RemoveAt(0); //list dBm in string type            

            List<float> _list_float = new List<float>(new float[0]);
            vm.Double_Powers = new List<double>();

            foreach (string x in list_read_value)
            {
                if (x != "" && x != "ERR0001" && x != ">")
                {
                    float y = Convert.ToSingle((string)x);
                    float z = (y * 300 / -64 - 150) * -1;
                    z = z != 1350 ? z : 150;
                    _list_float.Add(z);  //-150~150 degree, for gauge binding
                    vm.Double_Powers.Add(y);  //list 0~-64dBm in float type 
                }
            }
            vm.Value_PD = new List<float>(_list_float);
            vm.Str_PD = list_read_value;

            vm.List_V12.Add(dac);
            for (int i = 0; i < 8; i++)
            {
                if (vm.List_PDvalue_byV12[i] == null)
                    vm.List_PDvalue_byV12[i] = new List<float>();

                if (i < vm.Double_Powers.Count)
                    vm.List_PDvalue_byV12[i].Add(Convert.ToSingle(vm.Double_Powers[i]));
            }

            return vm;
        }

        public string GetMessage(byte[] dataBuffer)
        {
            str_read_value = new StringBuilder();
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

        public CurveFittingResultModel CurFitting(List<DataPoint> list_datapoint)
        {
            CurveFittingResultModel curveFittingResultModel = new CurveFittingResultModel(list_datapoint);

            List<System.Drawing.PointF> point = new List<PointF>();

            List<double> BestCoeffs = new List<double>();

            if (list_datapoint.Count < 1)
            {
                vm.ErrorCode = 21;
                curveFittingResultModel.isCurfitSuccess = false;
            }
            else
            {
                #region initialize
                string Best_DAC = "";
                #endregion

                int mid_i = (int)Math.Round((double)list_datapoint.Count / 2);
                double mid_X = Math.Round(list_datapoint[mid_i].X, 2);

                //foreach (DataPoint dp in list_datapoint)
                //    point.Add(new PointF((float)(dp.X - list_datapoint[mid_i].X), (float)dp.Y));

                foreach (DataPoint dp in list_datapoint)
                    point.Add(new PointF((float)(dp.X), (float)dp.Y));

                // Find a good fit.
                int degree = 2;
                BestCoeffs = CurveFunctions.FindPolynomialLeastSquaresFit(point, degree);

                if (degree == 2)
                {
                    curveFittingResultModel.Best_X = Math.Round((-1 * BestCoeffs[1] / (2 * BestCoeffs[2])));
                    Best_DAC = curveFittingResultModel.Best_X.ToString();
                }
                else
                {
                    curveFittingResultModel.isCurfitSuccess = false;
                    return curveFittingResultModel;
                }

                curveFittingResultModel.BestCoeffs = BestCoeffs;


                //string txt = "";
                //foreach (double coeff in BestCoeffs)
                //{
                //    txt += ", " + Math.Round(coeff, 10).ToString();
                //}
                //string coe = txt.Substring(1);

                //string str_curfit_result = Best_DAC;  //If Curfit result is not a number , error occurs.
            }

            return curveFittingResultModel;
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

        public async Task<bool> CurFit_All(List<List<DataPoint>> Save_All_PD_Value, List<PointF> Points, List<double> BestCoeffs, string action)
        {
            await Task.Delay(1);
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
                if (vm.PD_or_PM == true && action != "K WL")  //PM mode
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
                    if (action == "K WL")
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
                        double dac_gap = Math.Round(max_7points_WL - min_7points_WL, 2) / 100;

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
            vm.Str_Command = "ID?";
            await Cmd_Write_RecieveData(vm.Str_Command, false);
        }

        public async Task<bool> Port_ReOpen()
        {
            try
            {
                if (!vm.IsGoOn)
                {
                    if (!vm.port_PD.IsOpen)
                        vm.port_PD.Open();
                }
                else
                {
                    if (vm.PD_or_PM == false)
                        vm.timer_PD_GO.Stop();
                    else
                        vm.timer_PM_GO.Stop();
                    await Task.Delay(vm.Int_Read_Delay);
                    vm.port_PD.Close();
                    await Task.Delay(50);
                    vm.port_PD.Open();
                    vm.port_PD.DiscardInBuffer();       // RX
                    vm.port_PD.DiscardOutBuffer();      // TX
                }
            }
            catch { }

            return vm.IsGoOn;
        }

        public async Task<string> Cmd_Write_RecieveData(string cmd, bool _is_port_close_after_CmdWrite)
        {
            string msg = "";
            try
            {
                if (vm.port_PD.IsOpen)
                {
                    vm.port_PD.Write(cmd + "\r");

                    await Task.Delay(vm.Int_Read_Delay);

                    int size = vm.port_PD.BytesToRead;
                    byte[] dataBuffer = new byte[size];
                    int length = vm.port_PD.Read(dataBuffer, 0, size);

                    //Show read back message
                    msg = Read_analysis(cmd, dataBuffer);
                    vm.Str_cmd_read = msg;

                    if (_is_port_close_after_CmdWrite)
                    {
                        vm.port_PD.DiscardInBuffer();       // RX
                        vm.port_PD.DiscardOutBuffer();      // TX

                        vm.port_PD.Close();
                    }
                }
            }
            catch { }

            return msg;
        }

        public async Task<bool> Cmd_Write_RecieveData(string cmd, bool _is_port_close_after_CmdWrite, int ch)
        {
            try
            {
                if (vm.port_PD.IsOpen)
                {
                    vm.port_PD.Write(vm.Str_Command + "\r");

                    await Task.Delay(vm.Int_Read_Delay);

                    int size = vm.port_PD.BytesToRead;
                    byte[] dataBuffer = new byte[size];
                    int length = vm.port_PD.Read(dataBuffer, 0, size);

                    //Show read back message
                    vm = _PM_read_analysis(vm.Str_Command, dataBuffer, ch);

                    if (vm.Str_Command != "ID?")
                    {
                        #region Analyze Dx?
                        if (vm.Str_Command.ToCharArray(0, 1)[0] == 'D' && vm.Str_Command.ToCharArray(2, 1)[0] == '?') //D1?, D2?...
                        {
                            ObservableCollection<string> list_words = new ObservableCollection<string>();  //one channel list[v1.v2.v3]
                            string[] words = vm.Str_cmd_read.Split(',');  //V1,V2,V3 
                            foreach (string s in words) list_words.Add(s);  //Convert array to list
                            vm.list_D_All.Add(list_words);  //Add one channel list to All channel list
                            vm.List_D0_value = new ObservableCollection<ObservableCollection<string>>(vm.list_D_All); //Make propertychanged event happen                        
                        }
                        #endregion
                    }

                    if (_is_port_close_after_CmdWrite)
                    {
                        vm.port_PD.DiscardInBuffer();       // RX
                        vm.port_PD.DiscardOutBuffer();      // TX
                        vm.port_PD.Close();
                    }
                }
            }
            catch { }

            return vm.IsGoOn;
        }

        public double GaussianCurve(int ch, double x0, double x1, double step, double a, double b, double c)
        {
            double y = 0;
            double B;
            double C = 2 * c * c;

            for (double X = x0; X < x1; X = X + step)
            {
                B = -Math.Pow((X - b), 2);
                y = a * Math.Exp(B / C);

                //vm.Save_All_PD_Value[ch].Add(new DataPoint(Math.Round(X, 2), y));

                if (vm.Chart_All_DataPoints.Count > ch)
                {
                    vm.Chart_All_DataPoints[ch].Add(new DataPoint(Math.Round(X, 2), y));
                    vm.Chart_DataPoints = new List<DataPoint>(vm.Chart_All_DataPoints[0]);  //A lineseries

                }
            }

            return y;
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
