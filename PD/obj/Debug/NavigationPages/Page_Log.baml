   M S B A M L   `   `   `  ����D  @PD-v4.1.9, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:PD.NavigationPagesPD.NavigationPages  5      !    PD.NavigationPages.Page_Log   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      2local clr-namespace:PD.NavigationPages     5      
�������.��5
      �   � RowDefinitions  5   
   �� 5      �� 5      $��7* �6   �� 5       � ColumnDefinitions 5   
   �� 5      $
��0.2* �6   �� 5      $
��2.5* �6   �� 5      $
��0.5* �6   �� 5      �� 5      $
��0.2* �6   .+�5   
   &   System.Windows.Controls.DataGrid  -   5          Name$ dataGrid��5      $��1��5      $��1��6!      AutoGenerateColumns .     CanUserReorderColumns .     SelectionUnit$ FullRow=�5      ��5      ��    Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   Width5  /System.Windows.Controls.DataGridLengthConverter$
 120* 6%      Header$ Status��5!   %   1  +System.Windows.Controls.DataGridBoundColumn	   Binding	 5    %   �� *Status+5!   %     5"      $
 120* 6%   $ Message��5$   %   	 5#   %   �� *	Message+5$   %     5%      $	 80* 6%   $
 Date��5'   %   	 5&   %   �� *Date+5'   %     5(      $	 80* 6%   $
 Time��5*   %   	 5)   %   �� *Time+5*   %   5,      5p      5q      