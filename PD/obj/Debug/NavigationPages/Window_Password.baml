   M S B A M L   `   `   `  ����D  @PD-v4.1.9, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:PD.NavigationPagesPD.NavigationPages  5      +clr-namespace:PD.ViewModelPD.ViewModel  5      (    "PD.NavigationPages.Window_Password   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      2local clr-namespace:PD.NavigationPages     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      )vmclr-namespace:PD.ViewModel     5      -   5          � Title$  Window_Password��5	      $	��330��5      $	��490��5       � Topmost .  � WindowStyle$
 None=�5      ��5
      �� *window_bear_width+ �� Mode$ TwoWay=�.��5      � -   5        
������  � RowDefinitions 5   
   �� 5      �� 5      $��9* �6   �� 5      $��20 �6    � ColumnDefinitions 5    
   �� 5!      $
��0.5* �6   �� 5"      $
��2.5* �6   �� 5#      $��4* �6   �� 5$      $
��0.5* �6   .+�5&   
   � $��0��5'      $��0��5(      $��4��5)      .+�5*      �� $��Right=�6    �� Orientation$ 
Horizontal=�65   .+�5+      �� -   5        $��btn_min��5,      
������ $��_��50      ��5-      �� *ActualHeight+$ OneWay=� �� RelativeSource �� *Self+50      �� 51      -   5        $��	btn_close��52      
������ $��X��56      ��53      �� *ActualHeight+$ OneWay=� �� *Self+56      57      58      �� 59   
   -   5        $��0��5:      $��3��5;      $��1��5<      $��10q�5=      $!��../Resources/polar-bear.png��5A      � 5C   
   -   5        $��grid_txtblock��5D      $��1��5E      $��2��5F       5G      �� 5H      �� 5I      $��5* �6#   .+�5K      � $��1��6    5L      �� 5M      $
��0.8* �6$   �� 5N      $
��0.7* �6$   �� 5O      $��1* �6$   .+�5Q      �� -   5        $��txt_username��5Q         Setting_txtBlock#��[  60   $��20(�6Z   $��Bottom=�6h   $
��Bold"�6�   $
��User��5R      �� 5S      $��1��5T      #��[  5U      $��Password��5V      G� 5W      -   5        $��
PassworBox��5X      $��2��5Y      $	��Top=�5Z      5[      5\      5]      5^      5_      