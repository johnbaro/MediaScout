using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Diagnostics;

namespace MediaScoutGUI
{
    /// <summary>
    /// Interaction logic for EpisodeImporter.xaml
    /// </summary>
    public partial class EpisodeImporter : UserControl
    {
        private String _series;
        private String _seasonNum;
        private String _episodeNum;
        private String _sourceFile;
        private String _targetFile;
        private Boolean _fetchMetadata = false;

        public EpisodeImporter()
        {
            InitializeComponent();
        }

        public String Series
        {
            get { return _series; }
            set
            {
                _series = value;
                setHeader();
            }
        }

        public String SeasonNum
        {
            get { return _seasonNum; }
            set
            {
                _seasonNum = value;
                setHeader();
            }
        }

        public String EpisodeNum
        {
            get { return _episodeNum; }
            set
            {
                _episodeNum = value;
                setHeader();
            }
        }

        public String SourceFile
        {
            get { return _sourceFile; }
            set
            {
                _sourceFile = value;
                tbSourceFile.Text = value;
            }
        }

        public String TargetFile
        {
            get { return _targetFile; }
            set
            {
                _targetFile = value;
                tbTargetFile.Text = value;
            }
        }

        public Boolean FetchMetadata
        {
            get { return _fetchMetadata; }
            set { _fetchMetadata = value; }
        }



        public bool DoImport()
        {
            //Move the file and Fetch Metadata if we're supposed to
            if (_fetchMetadata)
            {
                if (moveEpisodeFile())
                {
                    return fetchMetadata();
                }
                else return false;
            }
            else
            {
                //othersie just move it.
                return moveEpisodeFile();
            }
        }


        private void setHeader()
        {
            lblSeriesSSEE.Content = _series + " s" + _seasonNum + "e" + _episodeNum;
        }

        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);

        private bool moveEpisodeFile()
        {
            Debug.WriteLine("epImport: Move " + _sourceFile + " to " + _targetFile);

            // See if the target filename is already taken
            if (System.IO.File.Exists(_targetFile))
            {
                Debug.WriteLine(_targetFile + " already exists!");
                return false;
            }
            // Make sure the source file exists too.
            if (!System.IO.File.Exists(_sourceFile))
            {
                Debug.WriteLine(_sourceFile + " not found!");
                return false;
            }
            else
            {
                ////Configure the ProgressBar
                //pbImporting.Minimum = 0;
                //pbImporting.Maximum = short.MaxValue;
                //pbImporting.Value = 0;
                //
                ////Create a new instance of our ProgressBar Delegate that points
                //// to the ProgressBar's SetValue method.
                //UpdateProgressBarDelegate updatePbDelegate =
                //    new UpdateProgressBarDelegate(pbImporting.SetValue);

                try
                {
                    FileSystem oFS = new FileSystem();
                    oFS.CopyProgress += new EventHandler<FileSystem.CopyProgressEventArgs>(oFS_CopyProgress);
                    bool success = oFS.MoveFile(_sourceFile, _targetFile);
                    return success;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error moving: " + ex.ToString());
                    return false;
                }
            }

        }

        void oFS_CopyProgress(object sender, FileSystem.CopyProgressEventArgs e)
        {

            //lblDropCopyStatus.Content = string.Format("Copied {0} of {1}, {2:P} done, ETA:{3}",
            //    e.copiedBytes, e.totalBytes, e.percentage,
            //    new TimeSpan(0, 0, 0, 0, (int)e.eta).ToString());
            lblBytesCopied.Content = string.Format("Copied {0} of {1}, {2:P}",
                e.copiedBytes, e.totalBytes, e.percentage);
            pbImporting.Value = (int)(e.percentage * 100);
            //Application.DoEvents();
            //if (stopCopy)
            //{
            //    e.Cancel = true;
            //}
        }

        private bool fetchMetadata()
        {
            // I don't know how I am going to fetch metadata yet.
            Debug.WriteLine("epImport: not fetching metadata, yet.");
            return false;
        }

    }
}
