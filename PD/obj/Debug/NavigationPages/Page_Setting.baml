   M S B A M L   `   `   `  ����L  HCustomized_TxtBox, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullR;clr-namespace:WpfControlLibrary1;assembly=Customized_TxtBoxWpfControlLibrary1  5      D @PD-v4.1.9, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:PD.NavigationPagesPD.NavigationPages 5      clr-namespace:PD.UIPD.UI 5	      T PMahApps.Metro, Version=2.0.0.0, Culture=neutral, PublicKeyToken=51482d6f650b2b3fV;clr-namespace:MahApps.Metro.Controls;assembly=MahApps.MetroMahApps.Metro.Controls 5
      +clr-namespace:PD.ViewModelPD.ViewModel 5      %   PD.NavigationPages.Page_Setting   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Q	cusTxtBox;clr-namespace:WpfControlLibrary1;assembly=Customized_TxtBox     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      J FWpfAnimatedGif, Version=1.4.18.0, Culture=neutral, PublicKeyToken=null,gif"http://wpfanimatedgif.codeplex.com  5      ,local clr-namespace:PD.NavigationPages  5      ucclr-namespace:PD.UI  5	      Jcontrols;clr-namespace:MahApps.Metro.Controls;assembly=MahApps.Metro  5
      Dmc;http://schemas.openxmlformats.org/markup-compatibility/2006  5      #vmclr-namespace:PD.ViewModel  5      
�������  � 	Resources  5         Setting_Border �� 
TargetName �� TargetProperty8  2System.Windows.Media.Animation.EasingColorKeyFrame �� KeyTime �� Value��%�   &	        �� 5   
   .��5      �� $ border��6+   $0 *(Panel.Background).(SolidColorBrush.Color)�6J   .g�5        $ 0��6&   $ White��62     5      $ 0:0:0.2��6&   $ 	#FF4AD87E��68   5      5      5       � DataContext 5        PD.ViewModel.ComViewModel  5   
   .��5      �  � ColumnDefinitions 5    
   �� 5!      $��0* �6   �� 5"      �� 5#      �� 5$      �� 5%      �� 5&      $��0* �6    � RowDefinitions 5(   
   �� 5)      �� 5*      �� 5+      �� 5,      �� 5-      �� 5.      �� 5/      �� 50      �� 51      �� 52      �� 53      �� 54      .+�58   
   �� $��0��59      $��12��5:      $��1��5;      
���   �$��0,0,1,0q�5=      �� 5>   
   -   5        $��ComBox_TLS_WL_Range��5?      $��0��5@      $��1��5A        style_combobox_black#��[ 5D       �� SelectedItem 5B      �� *selected_band+��5C      �� * list_combox_TLS_WL_Range_items+5E      �� 5F   
   -   5        $��ComBox_Station_Type��5F      $��1��5G      $��1��5H      #��[ 5K       5I      �� *station_type+��5J      �� *&$list_combox_Working_Table_Type_items+5L      �� 5M   
   -   5        $��ComBox_Control_Board_Type��5M      $��2��5N      $��1��5O      	 �� SelectedIndex$	 0��5R      #��[ 5T      ��5S      �� *&$list_combox_Control_Board_Type_items+5U      %   WpfControlLibrary1.UserControl1  5V   
   $��3��5W      $��1��5X      
   txtbox_content$
 WL Scan Start��5Y         txtbox_value 5Z      �� *float_WL_Scan_Start+ �� Mode$ TwoWay=�  5[   
   $��4��5\      $��1��5]      $
 WL Scan End��5^       5_      �� *float_WL_Scan_End+$ TwoWay=�  5`   
   $��5��5a      $��1��5b      $
 WL Scan Gap��5c       5d      �� *float_WL_Scan_Gap+$ TwoWay=��� 5e   
   -   5        $��ComBox_K_WL_Type��5e      $��6��5f      $��1��5g      $	 0��5i      #��[ 5l       5j      �� *selected_K_WL_Type+��5k      �� *list_combox_K_WL_Type_items+5m      �� 5n   
   -   5        $��ComBox_Laser_Selection��5n      $��7��5o      $��1��5p      $	 0��5s      #��[ 5u      ��5t      �� *list_combox_Laser_Type_items+5v      �� 5y   
   $��0��5z      $��12��5{      $��2��5|      
���   �$��0,0,1,0q�5~        5   
   $��0��5�      $��2��5�      $
 Rough-Scan Gap��5�       5�      �� *int_rough_scan_gap+$ TwoWay=�  5�   
   $��1��5�      $��2��5�      $
 Detail-Scan Gap��5�       5�      �� *int_detail_scan_gap+$ TwoWay=�  5�   
   $��2��5�      $��2��5�      $
 Rough-Scan Start��5�       5�      �� *int_rough_scan_start+$ TwoWay=�  5�   
   $��3��5�      $��2��5�      $
 Rough-Scan End��5�       5�      �� *int_rough_scan_stop+$ TwoWay=�  5�   
   $��4��5�      $��2��5�      $
 V3-Scan Gap��5�       5�      �� *int_V3_scan_gap+$ TwoWay=�  5�   
   $��5��5�      $��2��5�      $
 V3-Scan Start��5�       5�      �� *int_V3_scan_start+$ TwoWay=�  5�   
   $��6��6!   $��2��5�   !   $
 V3-Scan End��5�   !    5�   !   �� *int_V3_scan_end+$ TwoWay=�  5�   
   $��8��5�      $��2��5�      $
 Read-Cmd Delay��5�       5�      �� *Int_Read_Delay+$ TwoWay=�  5�   
   $��9��5�      $��2��5�      $
 Write-Cmd Delay��5�       5�      �� *Int_Write_Delay+$ TwoWay=�  5�   
   $��10��5�      $��2��5�      $
 Set-WL Delay��5�       5�      �� *Int_Set_WL_Delay+$ TwoWay=��� 5�   
   $��0��5�      $��12��5�      $��3��5�      
���   �$��0,0,1,0q�5�        5�   
   $��0��5�      $��3��5�      $
 TLS BoardNum��5�       5�      �� *tls_BoardNumber+$ TwoWay=�  5�   
   $��1��5�      $��3��5�      $
 TLS Address��5�       5�      �� *
tls_Addr+$ TwoWay=�  5�   
   $��2��5�      $��3��5�      $
 PM Slot��5�       5�      �� *	PM_slot+$ TwoWay=�  5�   
   $��3��5�      $��3��5�      $
 Switch Comport��5�       5�      �� *Comport_Switch+$ TwoWay=�  5�   
   $��4��6!   $��3��5�   !   $
 Chamber Comport��5�   !    5�   !   �� *Comport_chamber+$ TwoWay=�#  PD.UI.UC_Setting_ToggleButton  5�   
   $��0��6%   $��4��5�   %      txtbox_content$ PD / PM��5�   %      	Ischecked 5�   %   �� *
PD_or_PM+$ TwoWay=�  5�   
   $��1��6%   $��4��5�   %   $ Auto Connect��5�   %    5�   %   �� *Auto_Connect_TLS+$ TwoWay=�5�   %     PD.UI.UC_Setting_Button  5�   
   -   5          Name$ btn_board_comport��5�      $��2��6:   $��4��5�         txtbox_content$ Board Comport��5�        5�   
   -   5        $ btn_ini��5�      $��3��60   $ Open ini��6=   $��4��5�      6/     5�   
   $��4��6%   $��4��5�   %   $ Switch mode��5�   %    5�   %   �� *Is_switch_mode+$ TwoWay=��� 5�   
   -   5        $��txt_path��5�      $��25��5�      $��5q�5�      $��5��5�      $��4��5�      $��Stretch=�5�      $��Center=�5�      *  $MahApps.Metro.Controls.TextBoxHelper   ClearTextButton .    	Watermark$ Board table path��5�      ��5�      �� *txt_board_table_path+$ TwoWay=�5�      �� 5�   
   -	   5        $��txt_save_wl_data_path��5�      $��25��5�      $��5q�5�      $��6��5�      $��4��5�      $��Stretch=�5�      $��Center=�5�       . $ Board table path��5      ��5�      �� *txt_save_wl_data_path+$ TwoWay=�5        5  
   -
   5        $��7��5  %   $��4��5  %   $ CH / EN��5  %   5  %   �� 5  
   -   5        $��Img_gif��5     $��8��5	     $��4��5
     $��4��5     "  WpfAnimatedGif.ImageBehavior   AnimatedSource$ ../Resources/byby_1.gif��5        	AutoStart .   J� Opacity$
 0.05Q�5     5     �� 5  
   -   5        $��8��5     $��4��5     $��4��5     $y�Hidden=�5     5     5     