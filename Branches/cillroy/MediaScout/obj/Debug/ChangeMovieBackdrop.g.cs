﻿#pragma checksum "..\..\ChangeMovieBackdrop.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "A20450CD79B159A85E8AF6BC8C283A50"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.4927
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

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


namespace MediaScoutGUI {
    
    
    /// <summary>
    /// ChangeMovieBackdrop
    /// </summary>
    public partial class ChangeMovieBackdrop : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 25 "..\..\ChangeMovieBackdrop.xaml"
        internal System.Windows.Controls.Grid LayoutRoot;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\ChangeMovieBackdrop.xaml"
        internal System.Windows.Controls.ListBox lbPosters;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\ChangeMovieBackdrop.xaml"
        internal System.Windows.Controls.Button btnSelectPoster;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\ChangeMovieBackdrop.xaml"
        internal System.Windows.Controls.Button btnCancel;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\ChangeMovieBackdrop.xaml"
        internal System.Windows.Controls.Label lblNoPosters;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/MediaScoutGUI;component/changemoviebackdrop.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\ChangeMovieBackdrop.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.LayoutRoot = ((System.Windows.Controls.Grid)(target));
            return;
            case 2:
            this.lbPosters = ((System.Windows.Controls.ListBox)(target));
            return;
            case 3:
            this.btnSelectPoster = ((System.Windows.Controls.Button)(target));
            
            #line 27 "..\..\ChangeMovieBackdrop.xaml"
            this.btnSelectPoster.Click += new System.Windows.RoutedEventHandler(this.btnSelectPoster_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.btnCancel = ((System.Windows.Controls.Button)(target));
            
            #line 29 "..\..\ChangeMovieBackdrop.xaml"
            this.btnCancel.Click += new System.Windows.RoutedEventHandler(this.btnCancel_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.lblNoPosters = ((System.Windows.Controls.Label)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
