using System;
using System.Collections.Generic;
using System.Windows.Controls;
using PD.ViewModel;

namespace PD.Functions
{
    public class Setting
    {
        ComViewModel vm;

        //List<string> list_CalibrationItems;

        public Setting(ComViewModel vm)
        {
            this.vm = vm;
        }

        public void timer_setup()
        {

        }

        public async void Set_Laser_WL(double WL)
        {
            try
            {
                vm.tls.SetWL(WL);
                await vm.AccessDelayAsync(75);
                vm.pm.SetWL(WL);
                await vm.AccessDelayAsync(75);
                vm.Double_Laser_Wavelength = vm.tls.ReadWL();
                await vm.AccessDelayAsync(120);

            }
            catch
            {
                vm.Double_Laser_Wavelength = Convert.ToDouble(WL);
                vm.Str_cmd_read = "GPIB is not available";
            }
        }

        public void combox_setting(ComboBox obj)
        {
            obj.Items.Clear();
        }

        public void Product_Setting()
        {
            double gap = 0.1, bonus_wl = 0.8;
            if (vm.selected_K_WL_Type.Equals("Human Like"))
            {
                bonus_wl = 2.8;  //0.8
                gap = 0.4;
            }
            else if (vm.selected_K_WL_Type.Equals("All Range"))
            {
                bonus_wl = 0;
                gap = 0.1;
            }

            vm.csv_product_wl_setting_path = System.IO.Path.Combine(vm.CurrentPath , "Product_WL");
            CSVFunctions.Read_Ref_CSV(vm.csv_product_wl_setting_path, "product_wl_setting", vm);

            string key = string.Format("{0}_{1}", vm.product_type, vm.selected_band);

            if (vm.Dictionary_Product_WL_Setting.ContainsKey(key))
            {
                List<string> ls = vm.Dictionary_Product_WL_Setting[key];
                double start = 1545;
                double.TryParse(vm.Dictionary_Product_WL_Setting[key][0], out start);
                double end = 1550;
                double.TryParse(vm.Dictionary_Product_WL_Setting[key][1], out end);
                double gp = 0.8;
                double.TryParse(vm.Dictionary_Product_WL_Setting[key][2], out gp);
                K_WL_setting(start, end, gp);

                switch (vm.product_type)
                {
                    case "UFA":
                        choose_product_voltage_setting(-65500, 65500, 25, 30, 200);
                        break;

                    case "UFA-T":
                        choose_product_voltage_setting(-65500, 65500, 0, 10, 200);
                        break;

                    case "UFA(H)":
                        choose_product_voltage_setting(-65500, 65500, 34, 40, 200);
                        break;                                          

                    case "MTF":
                        choose_product_setting(-65500, 65500);
                        break;

                    default:
                        choose_product_setting(0, 65500);
                        break;
                }
            }
            else
                switch (vm.product_type)
                {
                    case "UFA":
                        choose_product_voltage_setting(-65500, 65500, 25, 30, 200);
                        if (vm.selected_band.Equals("C Band"))
                            K_WL_setting(1547 - bonus_wl, 1549.5 + bonus_wl, gap * 2, 1548.51); //1548.51 gap:0.6
                        else if (vm.selected_band.Equals("L Band"))
                            K_WL_setting(1589.6 - bonus_wl, 1592 + bonus_wl, gap * 2, 1591); //1591
                        break;

                    case "UFA-T":
                        choose_product_voltage_setting(-65500, 65500, 0, 10, 200);
                        if (vm.selected_band.Equals("C Band"))
                            K_WL_setting(1547 - bonus_wl, 1549.5 + bonus_wl, gap, 1548.51); //1548.51 gap:0.6
                        else if (vm.selected_band.Equals("L Band"))
                            K_WL_setting(1589.6 - bonus_wl, 1592 + bonus_wl, gap * 3 / 2, 1591); //1591
                        break;

                    case "UFA(H)":
                        choose_product_voltage_setting(-65500, 65500, 34, 40, 200);
                        if (vm.selected_band.Equals("C Band"))
                            K_WL_setting(1547 - bonus_wl, 1549.5 + bonus_wl, gap * 2, 1548.51); //1548.51
                        else if (vm.selected_band.Equals("L Band"))
                            K_WL_setting(1589.6 - bonus_wl, 1592 + bonus_wl, gap * 2, 1591); //1591
                        break;

                    case "UTF":
                        choose_product_setting(0, 65500);
                        if (vm.selected_band.Equals("C Band"))
                            K_WL_setting(1526.5 - bonus_wl, 1529 + bonus_wl, gap * 3 / 2);  //gap:0.6
                        else if (vm.selected_band.Equals("L Band"))
                            K_WL_setting(1566.5 - bonus_wl, 1568.5 + bonus_wl, gap * 2);  //<1568
                        break;

                    case "UTF300(H)":
                        choose_product_setting(0, 65500);
                        if (vm.selected_band.Equals("C Band"))
                            K_WL_setting(1547 - bonus_wl, 1551 + bonus_wl, gap * 3 / 2); //1548.51 gap:0.6
                        else if (vm.selected_band.Equals("L Band"))
                            K_WL_setting(1589.6 - bonus_wl, 1592.5 + bonus_wl, gap * 2); //1591
                        break;

                    case "UTF400":
                        choose_product_setting(0, 65500);
                        if (vm.selected_band.Equals("C Band"))
                            K_WL_setting(1547 - bonus_wl, 1551 + bonus_wl, gap * 2); //1548.51 gap:0.6
                        else if (vm.selected_band.Equals("L Band"))
                            K_WL_setting(1589.6 - bonus_wl, 1592.5 + bonus_wl, gap * 2); //1591
                        break;

                    case "UTF450":
                        choose_product_setting(0, 65500);
                        if (vm.selected_band.Equals("C Band"))
                            K_WL_setting(1547 - bonus_wl, 1551 + bonus_wl, gap * 2); //1548.51 gap:0.6
                        else if (vm.selected_band.Equals("L Band"))
                            K_WL_setting(1589.1 - bonus_wl, 1592 + bonus_wl, gap * 2); //1590.5
                        break;

                    case "UTF500":
                        choose_product_setting(0, 65500);
                        if (vm.selected_band.Equals("C Band"))
                            K_WL_setting(1547 - bonus_wl, 1551 + bonus_wl, gap * 2); //1548.51 gap:0.6
                        else if (vm.selected_band.Equals("L Band"))
                            K_WL_setting(1589.6 - bonus_wl, 1592.5 + bonus_wl, gap * 2); //1591
                        break;

                    case "UTF550":
                        choose_product_setting(0, 65500);
                        if (vm.selected_band.Equals("C Band"))
                            K_WL_setting(1547 - bonus_wl, 1551 + bonus_wl, gap * 2); //1548.51 gap:0.6
                        else if (vm.selected_band.Equals("L Band"))
                            K_WL_setting(1589.1 - bonus_wl, 1592 + bonus_wl, gap * 2); //1590.5
                        break;

                    case "CTF":
                        choose_product_setting(0, 65500);
                        if (vm.selected_band.Equals("C Band"))
                            K_WL_setting(1526.5 - bonus_wl, 1529 + 4 * bonus_wl, gap * 2);
                        break;

                    case "MTF":
                        choose_product_setting(-65500, 65500);
                        bonus_wl = 0.3;
                        switch (vm.selected_band)
                        {
                            case "C Band":
                                K_WL_setting(1546 - bonus_wl, 1547.4 + bonus_wl, gap); //gap:0.3
                                break;
                            case "L Band":
                                K_WL_setting(1587.72 - bonus_wl, 1589.72 + bonus_wl, gap); //1588.725
                                break;
                            case "C+L Band":
                                K_WL_setting(1568.83 - bonus_wl, 1572.83 + bonus_wl, gap); //1570.83
                                break;
                            case "O Band":
                                K_WL_setting(1298 - bonus_wl, 1302 + bonus_wl, gap); //1300
                                break;
                        }

                        break;
                }
        }

        public void GetPowerType_Setting()
        {
            if (vm.PD_or_PM)
            {
                vm.GetPWSettingModel.TypeName = "PM";
                vm.GetPWSettingModel.Interface = "GPIB";
            }
            else
            {
                vm.GetPWSettingModel.TypeName = "PD";
                vm.GetPWSettingModel.Interface = "RS232";
            }
            vm.GetPWSettingModel.Comport = vm.Selected_Comport;
            vm.GetPWSettingModel.BaudRate = vm.BoudRate;
            vm.GetPWSettingModel.DelayTime = vm.Int_Read_Delay;
        }

        public void choose_product_setting(int v12_roughscan_start, int v12_roughscan_end)
        {
            vm.int_rough_scan_start = v12_roughscan_start;
            vm.int_rough_scan_stop = v12_roughscan_end;
        }

        public void choose_product_voltage_setting(int? v12_roughscan_start, int? v12_roughscan_end, int? v3_Scan_Start_voltage, int? v3_Scan_End_voltage, int? v3_Scan_gap)
        {
            
            vm.int_rough_scan_start = v12_roughscan_start==null ? vm.int_rough_scan_start : (int)v12_roughscan_start;
            vm.int_rough_scan_stop = v12_roughscan_end == null ? vm.int_rough_scan_stop : (int)v12_roughscan_end;
            vm.int_V3_scan_start = v3_Scan_Start_voltage == null ? vm.int_V3_scan_start : (int)v3_Scan_Start_voltage * 1638;
            vm.int_V3_scan_end = v3_Scan_End_voltage == null ? vm.int_V3_scan_end : (int)v3_Scan_End_voltage * 1638;
            vm.int_V3_scan_gap = v3_Scan_gap == null ? vm.int_V3_scan_gap : (int)v3_Scan_gap;

            //vm.int_rough_scan_stop = (int)v12_roughscan_end;
            //vm.int_V3_scan_start = 1638 * v3_Scan_Start_voltage;
            //vm.int_V3_scan_end = 1638 * v3_Scan_End_voltage;
            //vm.int_V3_scan_gap = v3_Scan_gap;
        }

        public void K_WL_setting(double wl_scan_start, double wl_scan_end, double wl_scan_gap)
        {
            vm.float_WL_Scan_Start = wl_scan_start;
            vm.float_WL_Scan_End = wl_scan_end;
            vm.float_WL_Scan_Gap = wl_scan_gap;
        }

        public void K_WL_setting(double wl_scan_start, double wl_scan_end, double wl_scan_gap, double wl_center)
        {
            vm.float_WL_Scan_Start = wl_scan_start;
            vm.float_WL_Scan_End = wl_scan_end;
            vm.float_WL_Scan_Gap = wl_scan_gap;
            vm.Double_Laser_Wavelength = Math.Round(wl_center, 2);
        }

        public void Calibration_value_setting(object property, string section, string key)
        {
            string str_ini_read = vm.Ini_Read(section, key);
            bool bool_key_null = string.IsNullOrEmpty(str_ini_read);

            if (!bool_key_null)
                Convert.ToInt32(property);
        }
    }
}
