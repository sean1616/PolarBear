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
                await vm.AccessDelayAsync(50);
                vm.pm.SetWL(WL);
                await vm.AccessDelayAsync(50);
                vm.Double_Laser_Wavelength = vm.tls.ReadWL();
                await vm.AccessDelayAsync(50);
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
            switch (vm.product_type)
            {
                case "UFA":
                    choose_product_setting(-65500, 65500, 24, 31, 3600);
                    if (vm.selected_band == "C Band")
                        K_WL_setting(1547, 1549.5, 0.2); //1548.51
                    else
                        K_WL_setting(1589.6, 1592, 0.2); //1591
                    break;

                case "UFA(H)":
                    choose_product_setting(-65500, 65500, 34, 40, 3600);
                    if (vm.selected_band == "C Band")
                        K_WL_setting(1547, 1549.5, 0.2); //1548.51
                    else
                        K_WL_setting(1589.6, 1592, 0.2); //1591
                    break;

                case "UTF":
                    choose_product_setting(0, 65500);
                    if (vm.selected_band == "C Band")
                        K_WL_setting(1526.5, 1529, 0.2);
                    else K_WL_setting(1566.5, 1568.5, 0.2);  //<1568
                    break;

                case "CTF":
                    choose_product_setting(0, 65500);
                    if (vm.selected_band == "C Band")
                        K_WL_setting(1526.5, 1529, 0.2);
                    break;

                case "MTF":
                    choose_product_setting(-65500, 65500);
                    if (vm.selected_band == "C Band")
                        K_WL_setting(1546, 1547.4, 0.1);
                    else K_WL_setting(1587.72, 1589.72, 0.1); //1588.725
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
