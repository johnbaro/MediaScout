﻿#pragma checksum "..\..\NoTVResults.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "FD8AFB1413ED2B3F8F28C0B94F0E5D30"
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
using Wpf.Controls;


namespace MediaScoutGUI {
    
    
    /// <summary>
    /// NoTVResults
    /// </summary>
    public partial class NoTVResults : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 10 "..\..\NoTVResults.xaml"
        internal System.Windows.Controls.Button btnCancel;
        
        #line default
        #line hidden
        
        
        #line 11 "..\..\NoTVResults.xaml"
        internal Wpf.Controls.SplitButton btnsSkip;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\NoTVResults.xaml"
        internal Wpf.Controls.SplitButton btnsSearch;
        
        #line default
        #line hidden
        
        
        #line 20 "..\..\NoTVResults.xaml"
        internal System.Windows.Controls.TextBox txtTerm;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\NoTVResults.xaml"
        internal System.Windows.Controls.Label label1;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\NoTVResults.xaml"
        internal System.Windows.Controls.TextBox txtFolderName;
        
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
            System.Uri resourceLocater = new System.Uri("/MediaScoutGUI;component/notvresults.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\NoTVResults.xaml"
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
            this.btnCancel = ((System.Windows.Controls.Button)(target));
            
            #line 10 "..\..\NoTVResults.xaml"
            this.btnCancel.Click += new System.Windows.RoutedEventHandler(this.btnsSkip_Cancel_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.btnsSkip = ((Wpf.Controls.SplitButton)(target));
            
            #line 11 "..\..\NoTVResults.xaml"
            this.btnsSkip.Click += new System.Windows.RoutedEventHandler(this.btnsSkip_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 12 "..\..\NoTVResults.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.btnsSkip_Ignore_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 13 "..\..\NoTVResults.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.btnsSkip_Cancel_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.btnsSearch = ((Wpf.Controls.SplitButton)(target));
            
            #line 16 "..\..\NoTVResults.xaml"
            this.btnsSearch.Click += new System.Windows.RoutedEventHandler(this.btnSearch_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            
            #line 17 "..\..\NoTVResults.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.btnsSearch_StripDots_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            
            #line 18 "..\..\NoTVResults.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.btnsSearch_ScrubTerms_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.txtTerm = ((System.Windows.Controls.TextBox)(target));
            return;
            case 9:
            this.label1 = ((System.Windows.Controls.Label)(target));
            return;
            case 10:
            this.txtFolderName = ((System.Windows.Controls.TextBox)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
