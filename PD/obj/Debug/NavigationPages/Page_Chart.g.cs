﻿#pragma checksum "..\..\..\NavigationPages\Page_Chart.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "D754295FF3A8CD08FDC7A97ED27346F63AD8527ABEBAC6F7E302FDBC16BEF565"
//------------------------------------------------------------------------------
// <auto-generated>
//     這段程式碼是由工具產生的。
//     執行階段版本:4.0.30319.42000
//
//     對這個檔案所做的變更可能會造成錯誤的行為，而且如果重新產生程式碼，
//     變更將會遺失。
// </auto-generated>
//------------------------------------------------------------------------------

using OxyPlot.Wpf;
using PD.Functions;
using PD.NavigationPages;
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
    /// Page_Chart
    /// </summary>
    public partial class Page_Chart : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector, System.Windows.Markup.IStyleConnector {
        
        
        #line 57 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_previous;
        
        #line default
        #line hidden
        
        
        #line 63 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock txt_chart_number;
        
        #line default
        #line hidden
        
        
        #line 65 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Documents.Run txt_chart_now;
        
        #line default
        #line hidden
        
        
        #line 68 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Documents.Run txt_chart_all;
        
        #line default
        #line hidden
        
        
        #line 72 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_next;
        
        #line default
        #line hidden
        
        
        #line 79 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox cbox_all;
        
        #line default
        #line hidden
        
        
        #line 87 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ScrollViewer viewer;
        
        #line default
        #line hidden
        
        
        #line 92 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ItemsControl itms_gauges;
        
        #line default
        #line hidden
        
        
        #line 175 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_Calculation;
        
        #line default
        #line hidden
        
        
        #line 181 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_Timer;
        
        #line default
        #line hidden
        
        
        #line 187 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_Save_Chart;
        
        #line default
        #line hidden
        
        
        #line 194 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OxyPlot.Wpf.Plot Plot_Chart;
        
        #line default
        #line hidden
        
        
        #line 207 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OxyPlot.Wpf.LineSeries No1;
        
        #line default
        #line hidden
        
        
        #line 212 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OxyPlot.Wpf.LineSeries No2;
        
        #line default
        #line hidden
        
        
        #line 217 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OxyPlot.Wpf.LineSeries No3;
        
        #line default
        #line hidden
        
        
        #line 222 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OxyPlot.Wpf.LineSeries No4;
        
        #line default
        #line hidden
        
        
        #line 227 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OxyPlot.Wpf.LineSeries No5;
        
        #line default
        #line hidden
        
        
        #line 232 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OxyPlot.Wpf.LineSeries No6;
        
        #line default
        #line hidden
        
        
        #line 237 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OxyPlot.Wpf.LineSeries No7;
        
        #line default
        #line hidden
        
        
        #line 242 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OxyPlot.Wpf.LineSeries No8;
        
        #line default
        #line hidden
        
        
        #line 247 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OxyPlot.Wpf.LineSeries No9;
        
        #line default
        #line hidden
        
        
        #line 252 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OxyPlot.Wpf.LineSeries No10;
        
        #line default
        #line hidden
        
        
        #line 257 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OxyPlot.Wpf.LineSeries No11;
        
        #line default
        #line hidden
        
        
        #line 262 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OxyPlot.Wpf.LineSeries No12;
        
        #line default
        #line hidden
        
        
        #line 267 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OxyPlot.Wpf.LineSeries No13;
        
        #line default
        #line hidden
        
        
        #line 272 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OxyPlot.Wpf.LineSeries No14;
        
        #line default
        #line hidden
        
        
        #line 277 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OxyPlot.Wpf.LineSeries No15;
        
        #line default
        #line hidden
        
        
        #line 282 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OxyPlot.Wpf.LineSeries No16;
        
        #line default
        #line hidden
        
        
        #line 290 "..\..\..\NavigationPages\Page_Chart.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_PopupWindow;
        
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
            System.Uri resourceLocater = new System.Uri("/PD-v5.5.8;component/navigationpages/page_chart.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\NavigationPages\Page_Chart.xaml"
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
            this.btn_previous = ((System.Windows.Controls.Button)(target));
            
            #line 59 "..\..\..\NavigationPages\Page_Chart.xaml"
            this.btn_previous.Click += new System.Windows.RoutedEventHandler(this.btn_previous_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.txt_chart_number = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this.txt_chart_now = ((System.Windows.Documents.Run)(target));
            return;
            case 4:
            this.txt_chart_all = ((System.Windows.Documents.Run)(target));
            return;
            case 5:
            this.btn_next = ((System.Windows.Controls.Button)(target));
            
            #line 74 "..\..\..\NavigationPages\Page_Chart.xaml"
            this.btn_next.Click += new System.Windows.RoutedEventHandler(this.btn_next_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.cbox_all = ((System.Windows.Controls.CheckBox)(target));
            
            #line 81 "..\..\..\NavigationPages\Page_Chart.xaml"
            this.cbox_all.Click += new System.Windows.RoutedEventHandler(this.ALL_CheckBox_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.viewer = ((System.Windows.Controls.ScrollViewer)(target));
            return;
            case 8:
            this.itms_gauges = ((System.Windows.Controls.ItemsControl)(target));
            return;
            case 10:
            this.btn_Calculation = ((System.Windows.Controls.Button)(target));
            
            #line 178 "..\..\..\NavigationPages\Page_Chart.xaml"
            this.btn_Calculation.Click += new System.Windows.RoutedEventHandler(this.btn_Calculation_Click);
            
            #line default
            #line hidden
            return;
            case 11:
            this.btn_Timer = ((System.Windows.Controls.Button)(target));
            
            #line 184 "..\..\..\NavigationPages\Page_Chart.xaml"
            this.btn_Timer.Click += new System.Windows.RoutedEventHandler(this.btn_Timer_Click);
            
            #line default
            #line hidden
            return;
            case 12:
            this.btn_Save_Chart = ((System.Windows.Controls.Button)(target));
            
            #line 190 "..\..\..\NavigationPages\Page_Chart.xaml"
            this.btn_Save_Chart.Click += new System.Windows.RoutedEventHandler(this.btn_Save_Chart_Click);
            
            #line default
            #line hidden
            return;
            case 13:
            this.Plot_Chart = ((OxyPlot.Wpf.Plot)(target));
            
            #line 199 "..\..\..\NavigationPages\Page_Chart.xaml"
            this.Plot_Chart.Loaded += new System.Windows.RoutedEventHandler(this.Plot_Chart_Loaded);
            
            #line default
            #line hidden
            return;
            case 14:
            this.No1 = ((OxyPlot.Wpf.LineSeries)(target));
            return;
            case 15:
            this.No2 = ((OxyPlot.Wpf.LineSeries)(target));
            return;
            case 16:
            this.No3 = ((OxyPlot.Wpf.LineSeries)(target));
            return;
            case 17:
            this.No4 = ((OxyPlot.Wpf.LineSeries)(target));
            return;
            case 18:
            this.No5 = ((OxyPlot.Wpf.LineSeries)(target));
            return;
            case 19:
            this.No6 = ((OxyPlot.Wpf.LineSeries)(target));
            return;
            case 20:
            this.No7 = ((OxyPlot.Wpf.LineSeries)(target));
            return;
            case 21:
            this.No8 = ((OxyPlot.Wpf.LineSeries)(target));
            return;
            case 22:
            this.No9 = ((OxyPlot.Wpf.LineSeries)(target));
            return;
            case 23:
            this.No10 = ((OxyPlot.Wpf.LineSeries)(target));
            return;
            case 24:
            this.No11 = ((OxyPlot.Wpf.LineSeries)(target));
            return;
            case 25:
            this.No12 = ((OxyPlot.Wpf.LineSeries)(target));
            return;
            case 26:
            this.No13 = ((OxyPlot.Wpf.LineSeries)(target));
            return;
            case 27:
            this.No14 = ((OxyPlot.Wpf.LineSeries)(target));
            return;
            case 28:
            this.No15 = ((OxyPlot.Wpf.LineSeries)(target));
            return;
            case 29:
            this.No16 = ((OxyPlot.Wpf.LineSeries)(target));
            return;
            case 30:
            this.btn_PopupWindow = ((System.Windows.Controls.Button)(target));
            
            #line 302 "..\..\..\NavigationPages\Page_Chart.xaml"
            this.btn_PopupWindow.Click += new System.Windows.RoutedEventHandler(this.btn_PopupWindow_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        void System.Windows.Markup.IStyleConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 9:
            
            #line 103 "..\..\..\NavigationPages\Page_Chart.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Click += new System.Windows.RoutedEventHandler(this.CheckBox_Checked);
            
            #line default
            #line hidden
            break;
            }
        }
    }
}

