using System;
using System.Collections.Generic;
using System.Windows.Controls;
using PD.ViewModel;

namespace PD.Functions
{
    public class Setting
    {
        ComViewModel vm;

        List<string> list_CalibrationItems;

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
                await vm.AccessDelayAsync(70);
                vm.pm.SetWL(WL);
                await vm.AccessDelayAsync(70);
                vm.Double_Laser_Wavelength = vm.tls.ReadWL();
                await vm.AccessDelayAsync(100);
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
            if (vm.selected_K_WL_Type == "Human Like")
            {
                bonus_wl = 0.8;
                gap = 0.4;
            }                
            else if (vm.selected_K_WL_Type == "All Range")
            {
                bonus_wl = 0;
                gap = 0.1;
            }                

            switch (vm.product_type)
            {
                case "UFA":
                    choose_product_setting(-65500, 65500, 24, 31, 3600);
                    if (vm.selected_band == "C Band")
                        K_WL_setting(1547- bonus_wl, 1549.5+ bonus_wl, gap * 2); //1548.51 gap:0.6
                    else
                        K_WL_setting(1589.6- bonus_wl, 1592+ bonus_wl, gap * 2); //1591
                    break;

                case "UFA-T":
                    choose_product_setting(-65500, 65500, 0, 10, 3600);
                    if (vm.selected_band == "C Band")
                        K_WL_setting(1547 - bonus_wl, 1549.5 + bonus_wl, gap * 2); //1548.51 gap:0.6
                    else
                        K_WL_setting(1589.6 - bonus_wl, 1592 + bonus_wl, gap * 2); //1591
                    break;

                case "UFA(H)":
                    choose_product_setting(-65500, 65500, 34, 40, 3600);
                    if (vm.selected_band == "C Band")
                        K_WL_setting(1547- bonus_wl, 1549.5+ bonus_wl, gap * 2); //1548.51
                    else
                        K_WL_setting(1589.6- bonus_wl, 1592+ bonus_wl, gap * 2); //1591
                    break;

                case "UTF":
                    choose_product_setting(0, 65500);
                    if (vm.selected_band == "C Band")
                        K_WL_setting(1526.5- bonus_wl, 1529+ bonus_wl, gap * 2);
                    else K_WL_setting(1566.5- bonus_wl, 1568.5+ bonus_wl, gap * 2);  //<1568
                    break;

                case "UTF400":
                    choose_product_setting(0, 65500);
                    if (vm.selected_band == "C Band")
                        K_WL_setting(1547 - bonus_wl, 1549.5 + bonus_wl, gap * 2); //1548.51 gap:0.6
                    else
                        K_WL_setting(1589.6 - bonus_wl, 1592 + bonus_wl, gap * 2); //1591
                    break;

                case "UTF500":
                    choose_product_setting(0, 65500);
                    if (vm.selected_band == "C Band")
                        K_WL_setting(1547 - bonus_wl, 1549.5 + bonus_wl, gap * 2); //1548.51 gap:0.6
                    else
                        K_WL_setting(1589.6 - bonus_wl, 1592 + bonus_wl, gap * 2); //1591
                    break;

                case "CTF":
                    choose_product_setting(0, 65500);
                    if (vm.selected_band == "C Band")
                        K_WL_setting(1526.5- bonus_wl, 1529+ bonus_wl, gap * 2);
                    break;

                case "MTF":
                    choose_product_setting(-65500, 65500);
                    if (vm.selected_band == "C Band")
                        K_WL_setting(1546- bonus_wl, 1547.4+ bonus_wl, gap); //gap:0.3
                    else K_WL_setting(1587.72- bonus_wl, 1589.72+ bonus_wl, gap); //1588.725
                    break;
            }
        }

        public void choose_product_setting(int v12_roughscan_start, int v12_roughscan_end)
        {
            vm.int_rough_scan_start = v12_roughscan_start;
            vm.int_rough_scan_stop = v12_roughscan_end;
        }

        public void choose_product_setting(int v12_roughscan_start, int v12_roughscan_end, int v3_Scan_Start_voltage, int v3_Scan_End_voltage, int v3_Scan_gap) 
        {
            vm.int_rough_scan_start = v12_roughscan_start;
            vm.int_rough_scan_stop = v12_roughscan_end;
            vm.int_V3_scan_start = 1638 * v3_Scan_Start_voltage;
            vm.int_V3_scan_end = 1638 * v3_Scan_End_voltage;
            vm.int_V3_scan_gap = v3_Scan_gap;
        }

        public void K_WL_setting(double wl_scan_start, double wl_scan_end, double wl_scan_gap)
        {
            vm.float_WL_Scan_Start = wl_scan_start;
            vm.float_WL_Scan_End = wl_scan_end;
            vm.float_WL_Scan_Gap = wl_scan_gap;
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
