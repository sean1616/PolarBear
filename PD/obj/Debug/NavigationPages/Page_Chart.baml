   M S B A M L   `   `   `  ����D  @PD-v4.1.9, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:PD.NavigationPagesPD.NavigationPages  5      R NOxyPlot.Wpf, Version=1.0.0.0, Culture=neutral, PublicKeyToken=75e952ba404cdbb0>.clr-namespace:OxyPlot.Wpf;assembly=OxyPlot.WpfOxyPlot.Wpf 5      7 clr-namespace:PD.NavigationPagesPD.NavigationPages  5	      +clr-namespace:PD.ViewModelPD.ViewModel  5
      #    PD.NavigationPages.Page_Chart   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      2local clr-namespace:PD.NavigationPages     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      >oxy.clr-namespace:OxyPlot.Wpf;assembly=OxyPlot.Wpf     5      /ui clr-namespace:PD.NavigationPages     5	      )vmclr-namespace:PD.ViewModel     5
      
�������.��5      �   � ColumnDefinitions  5   
   �� 5      $��0* �6   �� 5      $��6* �6   �� 5      $��1* �6   �� 5      $��0* �6   .+�5   
   � $��2��6   ���	#FF494949 � RowDefinitions 5      �� 5      $
��0.5* �6    �� 5      �� 5      �� 5       �� 5!      �� 5"      �� 5#      �� 5$      �� 5%      �� 5&      �� 5'      �� 5(      �� 5)      .+�5+      � $��1��6     5,      �� 5-      $
��0.2* �6'   �� 5.      �� 5/      �� 50      �� 51      $
��0.2* �6'   .+�53      �� -   5        $��btn_previous��53      $��1��54      
 � Tag$$ ../../Resources/left-arrow.png��56         style_PictureBtn#���   57      (� 58      $��2��6   .��59      �� -   5        $��txt_chart_number��59         Chart_txtBlock#���  68   �� 5:      -   5         �Name$ txt_chart_now��5:      �6/   �� *int_chart_now+ �� 5;      $	� / ��6    �� 5<      -   5        $ txt_chart_all��5<      �6/   �� *int_chart_count+5=      5>      �� 5?      -   5        $��btn_next��5?      $��3��5@      $! ../../Resources/r-arrow.png��5B      #���   5C      5D      �� 5F      -   5        $��cbox_all��5G      $��2��5H      $	��ALL��5J      $% ../../Resources/right-arrow.png��5K        style_Chart_CheckBox#���  5L      
���   �5M      �� 5N      -   5        $��3��5O      $
��Ch 1��5Q      $% ../../Resources/right-arrow.png��5R      #���  5S      
��� � � d� 	IsChecked 5U      �� *
IsCheck[0]+�� 5V      -   5        $��4��5W      $
��Ch 2��5Y      $% ../../Resources/right-arrow.png��5Z      #���  5[      
���  �� 5]      �� *
IsCheck[1]+�� 5^      -	   5        $��5��5_      $
��Ch 3��5a      $% ../../Resources/right-arrow.png��5b      #���  5c      
����  � 5e      �� *
IsCheck[2]+�� 5f      -
   5        $��6��5g      $
��Ch 4��5i      $% ../../Resources/right-arrow.png��5j      #���  5k      
��� ��� 5m      �� *
IsCheck[3]+�� 5n      -   5        $��7��5o      $
��Ch 5��5q      $% ../../Resources/right-arrow.png��5r      #���  5s      
���/��� 5u      �� *
IsCheck[4]+�� 5v      -   5        $��8��5w      $
��Ch 6��5y      $% ../../Resources/right-arrow.png��5z      #���  5{      
���� �� 5}      �� *
IsCheck[5]+�� 5~      -   5        $��9��5      $
��Ch 7��5�      $% ../../Resources/right-arrow.png��5�      #���  5�      ���	#FF858585 5�      �� *
IsCheck[6]+�� 5�      -   5        $��10��5�      $
��Ch 8��5�      $% ../../Resources/right-arrow.png��5�      #���  5�      
���i�� 5�      �� *
IsCheck[7]+� 5�      $��11��6   .+�5�      �� -   5        $��btn_deltaIL��5�      $��Delta IL��5�        style_Chart_btn#���  5�      5�      5�      5�        OxyPlot.Wpf.Plot  5�   
   -   5          Name$ 
Plot_Chart��5�         Title$ Power��5�      $��1��5�      $��5,5,15,5q�5�      $��Segoe Print)�5�         Axes 5�        OxyPlot.Wpf.LinearAxis  5�        OxyPlot.Wpf.Axis   Position$ Bottom=�6R   	   Title	 6!   �� *Chart_x_title+
 �� FallbackValue$
 1��6R     5�      $
 Left=�6R   	 6!   �� *Chart_y_title+$
 2��6R      Series 5�        OxyPlot.Wpf.LineSeries  5�      -   5        $	��No1��5�        OxyPlot.Wpf.Series   Title$	 ch1��5�         Color$ Green��5�      ��5�      �� *Chart_DataPoints+ �� Mode$ OneWay=�y�5�      �� *LineSeries_Visible[0]+5�        5�      -   5        $	��No2��5�      $	 ch2��5�      $	 Red��5�      ��5�      �� *Chart_All_DataPoints[1]+$ OneWay=�y�5�      �� *LineSeries_Visible[1]+5�        5�      -   5        $	��No3��5�      $	 ch3��5�      $
 Blue��5�      ��5�      �� *Chart_All_DataPoints[2]+$ OneWay=�y�5�      �� *LineSeries_Visible[2]+5�        5�      -   5        $	��No4��5�      $	 ch4��5�      $ Orange��5�      ��5�      �� *Chart_All_DataPoints[3]+$ OneWay=�y�5�      �� *LineSeries_Visible[3]+5�        5�      -   5        $	��No5��5�      $	 ch5��5�      $ GreenYellow��5�      ��5�      �� *Chart_All_DataPoints[4]+$ OneWay=�y�5�      �� *LineSeries_Visible[4]+5�        5�      -   5        $	��No6��5�      $	 ch6��5�      $ Purple��5�      ��5�      �� *Chart_All_DataPoints[5]+$ OneWay=�y�5�      �� *LineSeries_Visible[5]+5�        5�      -   5        $	��No7��5�      $	 ch7��5�      $
 Gray��5�      ��5�      �� *Chart_All_DataPoints[6]+$ OneWay=�y�5�      �� *LineSeries_Visible[6]+5�        5�      -   5        $	��No8��5�      $	 ch8��5�      $ 	Chocolate��5�      ��5�      �� *Chart_All_DataPoints[7]+$ OneWay=�y�5�      �� *LineSeries_Visible[7]+5�        5�      -   5        $	��No9��5�      $	 ch9��5�      $ Brown��5�      ��5�      �� *Chart_All_DataPoints[8]+$ OneWay=�y�5�      �� *LineSeries_Visible[8]+5�        5�      -   5        $
��No10��5�      $
 ch10��5�      $ Purple��5�      ��5�      �� *Chart_All_DataPoints[9]+$ OneWay=�y�5�      �� *LineSeries_Visible[9]+5�        5�      -   5        $
��No11��5�      $
 ch11��5�      $
 Aqua��5�      ��5�      �� *Chart_All_DataPoints[10]+$ OneWay=�y�5�      �� *LineSeries_Visible[10]+5�        5�      -   5        $
��No12��5�      $
 ch12��5�      $ 	CadetBlue��5�      ��5�      �� *Chart_All_DataPoints[11]+$ OneWay=�y�5�      �� *LineSeries_Visible[11]+5�      5�      5�      5�      