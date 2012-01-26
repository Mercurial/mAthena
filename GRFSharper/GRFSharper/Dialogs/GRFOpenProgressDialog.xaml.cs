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
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace GRFSharper.Dialogs
{
    /// <summary>
    /// Interaction logic for GRFOpenProgressDialog.xaml
    /// </summary>
    public partial class GRFOpenProgressDialog : Window
    {
        private int _totalFileCount = 0;
        private int _fileExtCtr = 0;
        private string _currentFilename = string.Empty;
        DispatcherTimer dt = new DispatcherTimer();

        public GRFOpenProgressDialog()
        {
            InitializeComponent();
            this.Topmost = true;
            dt.Tick += new EventHandler(dt_Tick);
        }

        void dt_Tick(object sender, EventArgs e)
        {
            lblFileName.Content = _currentFilename;
            UpdateFileCount();
            UpdateProgressBar();
            if (_fileExtCtr == _totalFileCount)
            {
                dt.Stop();
                this.Close();
            }
        }

        public void SetFileCount(int count)
        {
            _totalFileCount = count;
            dt.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dt.Start();
        }

        private void UpdateFileCount()
        {
            lblFileCount.Content = string.Format("{0}/{1}", _fileExtCtr, _totalFileCount);
        }

        private void UpdateProgressBar()
        {
            if(_fileExtCtr!=0 && _totalFileCount !=0)
                progressBar1.Value = ((double)_fileExtCtr / (double)_totalFileCount) * 100;
        }


        public void UpdateProgress(string filename)
        {
            _fileExtCtr++;
            _currentFilename = filename;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateFileCount();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            dt.Stop();
        }
    }
}
