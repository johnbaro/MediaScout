using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MediaScoutGUI
{
    class FileSystem
    {
        int _bufferSize = 3 * 1024 * 1024;
        public int BufferSize
        {
            get
            {
                return _bufferSize;
            }
            set
            {
                _bufferSize = value;
            }
        }

        #region 'CopyProgress' event definition code

        /// <summary>
        ///     EventArgs derived type which holds the custom event fields
        /// </summary>
        public class CopyProgressEventArgs : System.EventArgs
        {
            /// <summary>
            ///     Use this constructor to initialize the event arguments
            ///     object with the custom event fields
            /// </summary>
            public CopyProgressEventArgs(decimal percentage, long copiedBytes,
                long totalBytes, double eta, double elapsedTime)
            {
                this.percentage = percentage;
                this.copiedBytes = copiedBytes;
                this.totalBytes = totalBytes;
                this.eta = eta;
                this.elapsedTime = elapsedTime;

            }

            /// <summary>
            ///     Percentage of copied bytes
            /// </summary>
            public readonly decimal percentage;

            /// <summary>
            ///     Number of copied bytes
            /// </summary>
            public readonly long copiedBytes;

            /// <summary>
            ///     Total bytes to be copied
            /// </summary>
            public readonly long totalBytes;

            /// <summary>
            ///     Estimated time left in seconds
            /// </summary>
            public readonly double eta;

            /// <summary>
            ///     Time Elapsed
            /// </summary>
            public readonly double elapsedTime;

            private bool cancel = false;
            public bool Cancel
            {
                get { return cancel; }
                set { cancel = value; }
            }

        }

        // Private delegate linked list (explicitly defined)
        private EventHandler<CopyProgressEventArgs> CopyProgressEventHandlerDelegate;

        /// <summary>
        ///     Provide feedback for file processing progress
        /// </summary>
        public event EventHandler<CopyProgressEventArgs> CopyProgress
        {
            // Explicit event definition with accessor methods
            add
            {
                CopyProgressEventHandlerDelegate = (EventHandler<CopyProgressEventArgs>)Delegate.Combine(CopyProgressEventHandlerDelegate, value);
            }
            remove
            {
                CopyProgressEventHandlerDelegate = (EventHandler<CopyProgressEventArgs>)Delegate.Remove(CopyProgressEventHandlerDelegate, value);
            }
        }

        /// <summary>
        ///     This is the method that is responsible for notifying
        ///     receivers that the event occurred
        /// </summary>
        protected virtual void OnCopyProgress(CopyProgressEventArgs e)
        {
            if (CopyProgressEventHandlerDelegate != null)
            {
                CopyProgressEventHandlerDelegate(this, e);
            }
        }

        #endregion //('CopyProgress' event definition code)

        #region 'FileCopyCompleted' event definition code

        /// <summary>
        ///     EventArgs derived type which holds the custom event fields
        /// </summary>
        public class FileCopyCompletedEventArgs : System.EventArgs
        {
            /// <summary>
            ///     Use this constructor to initialize the event arguments
            ///     object with the custom event fields
            /// </summary>
            public FileCopyCompletedEventArgs(bool Successful)
            {
                this.Successful = Successful;
            }

            /// <summary>
            ///     TODO: Describe the purpose of this event argument here
            /// </summary>
            public readonly bool Successful;

        }

        /// <summary>
        ///     This represents the delegate method prototype that
        ///     event receivers must implement
        /// </summary>
        public delegate void FileCopyCompletedEventHandler(object sender, FileCopyCompletedEventArgs args);

        /// <summary>
        ///     TODO: Describe the purpose of this event here
        /// </summary>
        public event FileCopyCompletedEventHandler FileCopyCompleted;

        /// <summary>
        ///     This is the method that is responsible for notifying
        ///     receivers that the event occurred
        /// </summary>
        protected virtual void OnFileCopyCompleted(FileCopyCompletedEventArgs e)
        {
            if (FileCopyCompleted != null)
            {
                FileCopyCompleted(this, e);
            }
        }

        #endregion //('FileCopyCompleted' event definition code)

        /// <summary>
        ///     Copies the sourceFile to the outFile
        /// </summary>
        /// <param name="sourceFile" type="string">
        ///     <para>
        ///         Source file to be copied
        ///     </para>
        /// </param>
        /// <param name="outFile" type="string">
        ///     <para>
        ///         File copy destination
        ///     </para>
        /// </param>
        /// <returns>
        ///     A bool value that indicate a successful copy finished.
        /// </returns>
        public bool CopyFile(string sourceFile, string outFile)
        {
            FileInfo fi = new FileInfo(sourceFile);
            long totalBytes = fi.Length;
            bool success = true;

            if (totalBytes == 0)//no file data
            {
                File.Create(outFile).Close();
            }
            else
            {
                var readStream = new FileStream(sourceFile, FileMode.Open);
                var writeStream = new FileStream(outFile, FileMode.CreateNew);
                int readBytes = 1;
                DateTime startTime = DateTime.Now;
                long totalCopiedBytes = 0;
                byte[] buffer = new byte[_bufferSize];

                while (readBytes > 0)
                {
                    readBytes = readStream.Read(buffer, 0, _bufferSize);
                    totalCopiedBytes += readBytes;
                    writeStream.Write(buffer, 0, readBytes);

                    var m = DateTime.Now.Subtract(startTime).TotalMilliseconds;
                    var speed = totalCopiedBytes / m;
                    var eta = (totalBytes - totalCopiedBytes) / speed;

                    var evt = new CopyProgressEventArgs((decimal)totalCopiedBytes / totalBytes, totalCopiedBytes,
                     totalBytes, eta, m);
                    OnCopyProgress(evt);

                    if (evt.Cancel)
                    {
                        success = false;
                        break;
                    }

                }

                writeStream.Close();
                readStream.Close();
            }
            //If everthing is ok copy file attributes to the newly created file.
            if (success)
            {
                File.SetCreationTime(outFile, File.GetCreationTime(sourceFile));
                File.SetLastWriteTime(outFile, File.GetLastWriteTime(sourceFile));
                File.SetAttributes(outFile, File.GetAttributes(sourceFile));
            }
            else
            {
                if (File.Exists(outFile))
                {
                    File.Delete(outFile);
                }
            }

            OnFileCopyCompleted(new FileCopyCompletedEventArgs(success));
            return success;
        }

        public bool MoveFile(string sourceFile, string outFile)
        {
            if (CopyFile(sourceFile, outFile))
            {
                File.Delete(sourceFile);
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
