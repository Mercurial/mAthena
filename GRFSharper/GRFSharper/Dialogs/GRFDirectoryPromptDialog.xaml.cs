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
    /// Interaction logic for GRFDirectoryPromptDialog.xaml
    /// </summary>
    public partial class GRFDirectoryPromptDialog : Window
    {
        private bool hasClickedAdd = false;

        public string EnteredPath = string.Empty;

        public GRFDirectoryPromptDialog()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            this.EnteredPath = txtPath.Text;
            hasClickedAdd = true;
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.DialogResult = hasClickedAdd;
        }
    }
}
