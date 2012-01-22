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

namespace GRFSharper.Dialogs
{
    /// <summary>
    /// Interaction logic for ExtractProgressDialog.xaml
    /// </summary>
    public partial class ExtractProgressDialog : Window
    {
        private int _totalFileCount = 0;
        private int _fileExtCtr = 0;

        public ExtractProgressDialog(int fileCount)
        {
            _totalFileCount = fileCount;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateFileCount();
        }

        private void UpdateFileCount()
        {
            lblFileCount.Content = string.Format("{0}/{1}", _fileExtCtr, _totalFileCount);
        }

        private void UpdateProgressBar()
        {
            progressBar1.Value = ((double)_fileExtCtr / (double)_totalFileCount) * 100;
        }


        public void UpdateProgress(string filename)
        {
            _fileExtCtr++;
            lblFileName.Content = filename;
            UpdateFileCount();
            UpdateProgressBar();
            if (_fileExtCtr == _totalFileCount)
                this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
