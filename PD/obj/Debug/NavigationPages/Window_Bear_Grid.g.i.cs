﻿#pragma checksum "..\..\..\NavigationPages\Window_Bear_Grid.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "76DB4CE7FB1DD8F85A4CDF1571FB5018D663BAD8"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using PD.NavigationPages;
using PD.UI;
using PD.ViewModel;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace PD.NavigationPages {
    
    
    /// <summary>
    /// Window_Bear_Grid
    /// </summary>
    public partial class Window_Bear_Grid : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 50 "..\..\..\NavigationPages\Window_Bear_Grid.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid mainGrid;
        
        #line default
        #line hidden
        
        
        #line 52 "..\..\..\NavigationPages\Window_Bear_Grid.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RowDefinition window_grid_row_1;
        
        #line default
        #line hidden
        
        
        #line 59 "..\..\..\NavigationPages\Window_Bear_Grid.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border border_title;
        
        #line default
        #line hidden
        
        
        #line 66 "..\..\..\NavigationPages\Window_Bear_Grid.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_min;
        
        #line default
        #line hidden
        
        
        #line 75 "..\..\..\NavigationPages\Window_Bear_Grid.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_close;
        
        #line default
        #line hidden
        
        
        #line 87 "..\..\..\NavigationPages\Window_Bear_Grid.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid dataGrid;
        
        #line default
        #line hidden
        
        
        #line 140 "..\..\..\NavigationPages\Window_Bear_Grid.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock txt_05dB_BW;
        
        #line default
        #line hidden
        
        
        #line 144 "..\..\..\NavigationPages\Window_Bear_Grid.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock txt_3dB_BW;
        
        #line default
        #line hidden
        
        
        #line 151 "..\..\..\NavigationPages\Window_Bear_Grid.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_cal_deltaIL;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/PD-v5.5.10;component/navigationpages/window_bear_grid.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\NavigationPages\Window_Bear_Grid.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 15 "..\..\..\NavigationPages\Window_Bear_Grid.xaml"
            ((PD.NavigationPages.Window_Bear_Grid)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Window_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.mainGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 3:
            this.window_grid_row_1 = ((System.Windows.Controls.RowDefinition)(target));
            return;
            case 4:
            this.border_title = ((System.Windows.Controls.Border)(target));
            
            #line 60 "..\..\..\NavigationPages\Window_Bear_Grid.xaml"
            this.border_title.MouseMove += new System.Windows.Input.MouseEventHandler(this.border_title_MouseMove);
            
            #line default
            #line hidden
            
            #line 61 "..\..\..\NavigationPages\Window_Bear_Grid.xaml"
            this.border_title.PreviewMouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.border_title_PreviewMouseLeftButtonDown);
            
            #line default
            #line hidden
            
            #line 62 "..\..\..\NavigationPages\Window_Bear_Grid.xaml"
            this.border_title.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.border_title_MouseLeftButtonUp);
            
            #line default
            #line hidden
            return;
            case 5:
            this.btn_min = ((System.Windows.Controls.Button)(target));
            
            #line 74 "..\..\..\NavigationPages\Window_Bear_Grid.xaml"
            this.btn_min.Click += new System.Windows.RoutedEventHandler(this.btn_min_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.btn_close = ((System.Windows.Controls.Button)(target));
            
            #line 83 "..\..\..\NavigationPages\Window_Bear_Grid.xaml"
            this.btn_close.Click += new System.Windows.RoutedEventHandler(this.btn_close_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.dataGrid = ((System.Windows.Controls.DataGrid)(target));
            return;
            case 8:
            this.txt_05dB_BW = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 9:
            this.txt_3dB_BW = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 10:
            this.btn_cal_deltaIL = ((System.Windows.Controls.Button)(target));
            
            #line 151 "..\..\..\NavigationPages\Window_Bear_Grid.xaml"
            this.btn_cal_deltaIL.Click += new System.Windows.RoutedEventHandler(this.btn_cal_deltaIL_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

