﻿#pragma checksum "..\..\..\NavigationPages\Window_Ref.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "0573E223F2C9262EBDD605A315740ACE2AD06D86"
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
    /// Window_Ref
    /// </summary>
    public partial class Window_Ref : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 33 "..\..\..\NavigationPages\Window_Ref.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OxyPlot.Wpf.Plot Plot_Ref;
        
        #line default
        #line hidden
        
        
        #line 43 "..\..\..\NavigationPages\Window_Ref.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OxyPlot.Wpf.LinearAxis axis_left;
        
        #line default
        #line hidden
        
        
        #line 48 "..\..\..\NavigationPages\Window_Ref.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OxyPlot.Wpf.LinearAxis axis_bottom;
        
        #line default
        #line hidden
        
        
        #line 55 "..\..\..\NavigationPages\Window_Ref.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OxyPlot.Wpf.LineSeries oxyPlot_LineSeries;
        
        #line default
        #line hidden
        
        
        #line 64 "..\..\..\NavigationPages\Window_Ref.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_previous;
        
        #line default
        #line hidden
        
        
        #line 71 "..\..\..\NavigationPages\Window_Ref.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_next;
        
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
            System.Uri resourceLocater = new System.Uri("/PD-v5.6.4;component/navigationpages/window_ref.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\NavigationPages\Window_Ref.xaml"
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
            this.Plot_Ref = ((OxyPlot.Wpf.Plot)(target));
            
            #line 40 "..\..\..\NavigationPages\Window_Ref.xaml"
            this.Plot_Ref.Loaded += new System.Windows.RoutedEventHandler(this.Window_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.axis_left = ((OxyPlot.Wpf.LinearAxis)(target));
            return;
            case 3:
            this.axis_bottom = ((OxyPlot.Wpf.LinearAxis)(target));
            return;
            case 4:
            this.oxyPlot_LineSeries = ((OxyPlot.Wpf.LineSeries)(target));
            return;
            case 5:
            this.btn_previous = ((System.Windows.Controls.Button)(target));
            
            #line 69 "..\..\..\NavigationPages\Window_Ref.xaml"
            this.btn_previous.Click += new System.Windows.RoutedEventHandler(this.Btn_previous_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.btn_next = ((System.Windows.Controls.Button)(target));
            
            #line 76 "..\..\..\NavigationPages\Window_Ref.xaml"
            this.btn_next.Click += new System.Windows.RoutedEventHandler(this.Btn_next_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

