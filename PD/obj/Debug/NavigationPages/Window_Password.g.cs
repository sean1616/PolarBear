﻿#pragma checksum "..\..\..\NavigationPages\Window_Password.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "27F38EF64AA1F54B5D6B886D5E2704F24711AB2459F08481949C72307C3F980E"
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
    /// Window_Password
    /// </summary>
    public partial class Window_Password : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 38 "..\..\..\NavigationPages\Window_Password.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border border_title;
        
        #line default
        #line hidden
        
        
        #line 45 "..\..\..\NavigationPages\Window_Password.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_min;
        
        #line default
        #line hidden
        
        
        #line 54 "..\..\..\NavigationPages\Window_Password.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_close;
        
        #line default
        #line hidden
        
        
        #line 94 "..\..\..\NavigationPages\Window_Password.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid grid_txtblock;
        
        #line default
        #line hidden
        
        
        #line 107 "..\..\..\NavigationPages\Window_Password.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock txt_username;
        
        #line default
        #line hidden
        
        
        #line 114 "..\..\..\NavigationPages\Window_Password.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.PasswordBox PassworBox;
        
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
            System.Uri resourceLocater = new System.Uri("/PD-v5.5.15;component/navigationpages/window_password.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\NavigationPages\Window_Password.xaml"
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
            
            #line 13 "..\..\..\NavigationPages\Window_Password.xaml"
            ((PD.NavigationPages.Window_Password)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Window_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.border_title = ((System.Windows.Controls.Border)(target));
            
            #line 39 "..\..\..\NavigationPages\Window_Password.xaml"
            this.border_title.MouseMove += new System.Windows.Input.MouseEventHandler(this.TitleBar_MouseMove);
            
            #line default
            #line hidden
            
            #line 40 "..\..\..\NavigationPages\Window_Password.xaml"
            this.border_title.PreviewMouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.TitleBar_MouseDown);
            
            #line default
            #line hidden
            
            #line 41 "..\..\..\NavigationPages\Window_Password.xaml"
            this.border_title.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.TitleBar_MouseUp);
            
            #line default
            #line hidden
            return;
            case 3:
            this.btn_min = ((System.Windows.Controls.Button)(target));
            
            #line 53 "..\..\..\NavigationPages\Window_Password.xaml"
            this.btn_min.Click += new System.Windows.RoutedEventHandler(this.btn_min_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.btn_close = ((System.Windows.Controls.Button)(target));
            
            #line 62 "..\..\..\NavigationPages\Window_Password.xaml"
            this.btn_close.Click += new System.Windows.RoutedEventHandler(this.btn_close_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 89 "..\..\..\NavigationPages\Window_Password.xaml"
            ((System.Windows.Controls.Image)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Image_Loaded);
            
            #line default
            #line hidden
            
            #line 90 "..\..\..\NavigationPages\Window_Password.xaml"
            ((System.Windows.Controls.Image)(target)).PreviewMouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.Image_PreviewMouseLeftButtonDown);
            
            #line default
            #line hidden
            return;
            case 6:
            this.grid_txtblock = ((System.Windows.Controls.Grid)(target));
            return;
            case 7:
            this.txt_username = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 8:
            this.PassworBox = ((System.Windows.Controls.PasswordBox)(target));
            
            #line 117 "..\..\..\NavigationPages\Window_Password.xaml"
            this.PassworBox.KeyDown += new System.Windows.Input.KeyEventHandler(this.PasswordBox_KeyDown);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

