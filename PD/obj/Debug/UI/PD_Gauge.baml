   M S B A M L   `   `   `  ����D  @PD-v4.1.9, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:PD.UIPD.UI  5          PD.UI.PD_Gauge   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      c _Microsoft.Expression.Drawing, Version=4.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35=ed4http://schemas.microsoft.com/expression/2010/drawing  5      localclr-namespace:PD.UI  5      Dmc;http://schemas.openxmlformats.org/markup-compatibility/2006  5      .��5      � .+�5   
   � $��18q�6     � RowDefinitions  5      �� 5      $��0.15* �6    �� 5       � ColumnDefinitions 5      �� 5      �� 5      $��0.15* �6#   .+�5      %  Microsoft.Expression.Shapes.Arc  $	��185��6   $��2��5      $��2��5      $	��185��5      $��Center=�5      $��Center=�5         ArcThicknessUnit$ Pixel=�5         EndAngle$	 150Q�5       ���#dbdcdc   
StartAngle$
 -150Q�5"       �� Stretch$
 None=�5#      
������    ArcThickness 5      �� *ArcThickness+ �� ElementName$ Process_Arc_1��5$        5&      -   5          Name$ Process_Arc_1��5&      $��2��5'      $��2��5(      $	��185��5)      $	��185��5*      $��Center=�5+      $��Center=�5,      $ 10Q�5-      $ Pixel=�5.      $
 -150Q�51      $
 None=�52      
������  5/      �� *	Arc_Color+	 �� RelativeSource	 �� *FindAncestor+
 �� AncestorType   PD.UI.PD_Gauge"
  ��50      �� *Arc_EndAngle+	 �� *FindAncestor+"
  53      � 54      $��2��6   $��2��55        56      �� 57      $
��2.4* �6$   �� 58      $
��0.5* �6$   .+�5:      �� $��0��6   $��5��5;      $��Center=�5<      $��Center=�5=       �� Orientation$ 
Horizontal=�5>      .+�5?      �� -   5         ��Name$ txtBlock_PD_Value��5?       $��Center=�5@       $��Bottom=�5A       $��24(�5B       $��Segoe Print)�5C        �� TextAlignment$ Center=�5D       $
��Wrap=�5E       ��5F       �� *str_PD_value+	 �� *FindAncestor+"
  �� 5G      $��Center=�6    $��Bottom=�5H       $��19(�5I       $��Segoe Print)�5J       
���   �$ Center=�5M       $
��Wrap=�5N       ��5L       �� *
str_Unit+ �� FallbackValue$	 dBm��	 �� *FindAncestor+"
  5N       5O      �� 5P      $��3��6   $��Center=�5Q      $	��Top=�5R      $ 
Horizontal=�5S      .+�5T      �� -   5        $ CompleteValueName��5T       $��Center=�5U       $��Center=�5V       $��18(�5W       $��Segoe Print)�5X       
���   �$	��ch ��5Z       $
��Wrap=�5[       �� 5\      -   5        $ txtBlock_Channel��5\       $��Center=�5]       $��Center=�5^       $��18(�5_       $��Segoe Print)�5`       $
��Wrap=�5a       ��5b       �� *str_channel+	 �� *FindAncestor+"
  5c      5d      5m      5n      5o      