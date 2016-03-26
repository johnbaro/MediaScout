using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Globalization;
using System.Diagnostics;

namespace MediaScoutGUI
{
    /// <summary>
    /// Interaction logic for NoTVResults.xaml
    /// </summary>
    public partial class NoTVResults : Window
    {
        public String Term;

        //public NoTVResults(String term, String foldername)
        //{
        //    Debug.WriteLine("Opening NoTVResults dialoge with Skip/Cancel split button");
        //    InitializeComponent();
        //    this.txtTerm.Text = term;
        //    this.txtFolderName.Text = foldername;
        //    this.btnsSkip.Visibility = Visibility.Visible;
        //    this.btnCancel.Visibility = Visibility.Hidden;
        //}

        //public NoTVResults(String term, bool CancelOnly)
        //{
        //    new NoTVResults(term, null, CancelOnly);
        //}

        public NoTVResults(String term, String foldername, bool CancelOnly)
        {
            InitializeComponent();
            this.txtTerm.Text = term;
            this.txtFolderName.Text = foldername;

            if (foldername == null)
            {
                Debug.WriteLine("Hiding the folder text box from view");
                this.txtFolderName.Visibility = Visibility.Hidden;
            }
            else
            {
                this.txtFolderName.Visibility = Visibility.Visible;
            }

            if (CancelOnly)
            {
                Debug.WriteLine("Creating NoTVResults dialoge with just a Cancel button");
                this.btnsSkip.Visibility = Visibility.Hidden;
                this.btnCancel.Visibility = Visibility.Visible;
            }
            else
            {
                Debug.WriteLine("Creating NoTVResults dialoge with Skip/Cancel split button");
                this.btnsSkip.Visibility = Visibility.Visible;
                this.btnCancel.Visibility = Visibility.Hidden;
            }

        }

        // FreQi -
        //I thought maybe I could use a Public Property to represent the decision made
        //by the user since I can't seem to get DialogResult to return null.  This would
        //also open the door to several different decisions that a user could make...
        private DecisionType _decision = DecisionType.Search;

        public enum DecisionType
        {
            Cancel = 0,
            Skip = 1,
            Ignore = 2,
            Search = 3,
        }

        public DecisionType Decision
        {
            get { return _decision; }
            set { _decision = value; }
        }

        private void btnsSkip_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Canceling");
            this._decision = DecisionType.Cancel;
            this.Close();
        }
        
        private void btnsSkip_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Skipping");
            this._decision = DecisionType.Skip;
            this.Close();
        }

        private void btnsSkip_Ignore_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Ignoring.  Creating 0byte file - " + txtFolderName.Text + @"\IGNORE");
            this._decision = DecisionType.Ignore;
            TextWriter w = new StreamWriter(txtFolderName.Text + @"\IGNORE");
            w.Close();
            this.Close();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Searching.");
            this._decision = DecisionType.Search;
            this.Term = txtTerm.Text;
            this.DialogResult = true;
            this.Close();
        }

        private void btnsSearch_StripDots_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Stripping dots from Search term.");
            this.txtTerm.Text = txtTerm.Text.Replace(".", " ");
        }

        private void btnsSearch_ScrubTerms_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Scrubbing Search Terms.");

            //This should replace characters like like ë, ä, ç, with e, a and c respectively.
            String Term = txtTerm.Text.Normalize(NormalizationForm.FormD);
            StringBuilder normOutputBilder = new StringBuilder();
            foreach (char c in Term)
            {
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    normOutputBilder.Append(c);
                }
            }
            this.txtTerm.Text = normOutputBilder.ToString();
        }

    }
}
