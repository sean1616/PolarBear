   M S B A M L   `   `   `  ����D  @PD-v4.1.9, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:PD.UIPD.UI  5      +clr-namespace:PD.ViewModelPD.ViewModel  5	      G CCircle_Gauge, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullA0clr-namespace:Circle_Gauge;assembly=Circle_GaugeCircle_Gauge 5
          PD.UI.UC_Gauge   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      %localclr-namespace:PD.UI     5      c _Microsoft.Expression.Drawing, Version=4.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35=ed4http://schemas.microsoft.com/expression/2010/drawing  5      #vmclr-namespace:PD.ViewModel  5	      <gauge0clr-namespace:Circle_Gauge;assembly=Circle_Gauge  5
      -   5          � DataContext  5         PD.ViewModel.ComViewModel  5   
   .��5      � .+�5   
   d� -   5         d�Name$ UC_Gauge_Tbn��5      $��0��5      $��0��5      $��7q�5      |�.     style_Gauge_Background#��[  5       d� 	IsChecked 5      �� *
Bool_Gauge+ �� RelativeSource �� *FindAncestor+ �� AncestorType   PD.UI.UC_Gauge"  .��5      (� .��5       � .+�5"      � $	��330��6    � RowDefinitions 5#      �� 5$      $��15 �6,   �� 5%      $	��200 �6,   �� 5&      $��60 �6,   �� 5'      $��60 �6,   .+�5*      %  Microsoft.Expression.Shapes.Arc  $	��185��6!   $��1��5+   !   $��2��5,   !   $��1��5-   !   $	��185��5.   !   $��Center=�5/   !   $��Center=�50   !      ArcThicknessUnit$ Pixel=�52   !      EndAngle$	 150Q�53   !   ���#dbdcdc   
StartAngle$
 -150Q�55   !   	 �� Stretch$
	 None=�56   !   
������ 
   ArcThickness
 51   !   �� *ArcThickness+ �� ElementName$ Process_Arc_1��57   !     59      -   5          Name$ Process_Arc_1��59   !   $��1��5:   !   $��2��5;   !   $��1��5<   !   $	��185��5=   !   $	��185��5>   !   $��Center=�5?   !   $��Center=�5@   !   $
 10Q�5A   !   $ Pixel=�5B   !   $
 -150Q�5E   !   $
	 None=�5F   !   
������  5C   !   �� *Gauge_EndAngle+ �� *FindAncestor+"  ��5D   !   �� *Gauge_Color+ �� *FindAncestor+"  5G   !   � 5H      $��2��6   $��1��5I      $��1��5J       5K      �� 5L   "   $
��2.4* �60   �� 5M   "   $
��0.5* �60   .+�5O      �� $��0��6)   $��5��5P   )   $��Center=�5Q   )   $��Center=�5R   )    �� Orientation$ 
Horizontal=�5S   )   .+�5T   "   �� -   5         ��Name$ txtBlock_PD_Value��5T   ,   $��Center=�5U   ,   $��Bottom=�5V   ,   $��24(�5W   ,   $��Segoe Print)�5X   ,    �� TextAlignment$ Center=�5Y   ,   $
��Wrap=�5Z   ,   ��5[   ,   �� *Gauge_value+ �� *FindAncestor+"  �� 5\   "   $��Center=�6,   $��Bottom=�5]   ,   $��19(�5^   ,   $��Segoe Print)�5_   ,   
���   �$ Center=�5b   ,   $
��Wrap=�5c   ,   ��5a   ,   �� *
Gauge_Unit+ �� FallbackValue$	 dBm�� �� *FindAncestor+"  5c   ,   5d      �� 5e      $��3��6)   $��Center=�5f   )   $	��Top=�5g   )   $ 
Horizontal=�5h   )   .+�5i   "   �� -   5        $ CompleteValueName��5i   ,   $��Center=�5j   ,   $��Center=�5k   ,   $��18(�5l   ,   $��Segoe Print)�5m   ,   
���   �$	��ch ��5o   ,   $
��Wrap=�5p   ,   �� 5q   "   -   5        $ txtBlock_Channel��5q   ,   $��Center=�5r   ,   $��Center=�5s   ,   $��18(�5t   ,   $��Segoe Print)�5u   ,   $
��Wrap=�5v   ,   ��5w   ,   �� *Gauge_channel+ �� *FindAncestor+"  5x      5y      5z      � 5}       5~      �� 5      $��6* �6,   �� 5�      $��1* �6,   �� 5�      ��6,   �� *GaugeTxtSize_Height+�� 5�      $
��0.5* �6,   .+�5�      � -   5        $��grid_dac_txt��5�      $��1��5�      $��3��5�       � ColumnDefinitions 5�      �� 5�   "   $
��2.5* �63   �� 5�   "   $��5* �63   �� 5�   "   $��5* �63   �� 5�   "   $��5* �63   �� 5�   "   $
��2.5* �63   .+�5�      �� -   5         ��Name$ _1��5�   &   $��1��5�   &   $��Center=�5�   &     style_Gauge_txtbox#���  5�   &   ��5�   &   �� *Int_Fontsize+��5�   &   �� *
D0_value_1+ �� *FindAncestor+"  �� 5�      -	   5        $ _2��5�   &   $��2��5�   &   #���  5�   &   ��5�   &   �� *
FontSize+$ _1����5�   &   �� *
D0_value_2+ �� *FindAncestor+"  �� 5�      -
   5        $ _3��5�   &   $��3��5�   &   #���  5�   &   ��5�   &   �� *
FontSize+$ _1����5�   &   �� *
D0_value_3+ �� *FindAncestor+"  �� 5�      -   5        $ _txt_1��5�   &   $��1��5�   &   $��1��5�   &   $��3��5�   &   $��2,2,2,2q�5�   &     style_SN_txtbox#��[ 5�   &   ��5�   &   �� *GaugeTabOrder[0]+��5�   &   �� *Int_Fontsize+y�5�   &   �� *GaugeText_visible_Reverse+��5�   &   �� *ActualWidth+$ Gauge_1�� �� Mode$ OneWay=���5�   &   �� *	SN_number+ �� *FindAncestor+"  5�   &   5�      � 5�      $��2��6    5�      �� 5�   "   ��63   �� *GaugeTxtSize_Column[0]+�� 5�   "   ��63   �� *GaugeTxtSize_Column[1]+�� 5�   "   ��63   �� *GaugeTxtSize_Column[2]+�� 5�   "   ��63   �� *GaugeTxtSize_Column[3]+�� 5�   "   ��63   �� *GaugeTxtSize_Column[4]+.+�5�      �� -   5         ��Name$ btn_1��5�   %   
 � Tag$	 1_9��5�   %   $��1��5�   %     style_BearBtn#���  5�   %   ��5�   %   �� *ActualHeight+$ _txt_1��$ OneWay=���5�   %   �� *List_bear_say_1+ �� *FindAncestor+"   � ContextMenu 5�   "   �� 5�   &   -   5        $��cm��5�   2    �� 	StaysOpen . .@�5�   *   m� -   5        $��
btn_update��5�   3   $��Update��6E   6U   5�   '   5�      �� 5�      -   5        $	 1_9��5�   %   $��2��5�   %   #���  5�   %   ��5�   %   �� *ActualHeight+$ _txt_1��$ OneWay=���5�   %   �� *List_bear_say_2+ �� *FindAncestor+"  5�   %   �� 5�      -   5        $	 1_9��5�   %   $��3��5�   %   #���  5�   %   ��5�   %   �� *ActualHeight+$ _txt_1��$ OneWay=���5�   %   �� *List_bear_say_3+ �� *FindAncestor+"  5�   %   5�      5�      5�      5�      5�      5�      5�      