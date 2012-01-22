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
using System.Collections.ObjectModel;
using Fluent;
using Microsoft.Win32;
using GRFSharp;
using GRFSharper.Dialogs;
using System.Threading;

namespace GRFSharper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        GRF baseGRF = new GRF();
        ExtractProgressDialog epd;
        ObservableCollection<GRFFile> _GrfFileCollection = new ObservableCollection<GRFFile>();

        public MainWindow()
        {
            InitializeComponent();
            lvGRFItems.ItemsSource = _GrfFileCollection;

            //Initialize GRF Event Handlers
            baseGRF.ExtractComplete += new ExtractCompleteEventHandler(baseGRF_ExtractComplete);
        }

        void baseGRF_ExtractComplete(object sender, GRFFileExtractEventArg e)
        {
            epd.Dispatcher.Invoke(new ThreadStart(() => epd.UpdateProgress(e.File.Name)));
        }

        private void Ribbon_IsMinimizedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(!(bool)e.NewValue)
                mainGrid.RowDefinitions[0].Height = new GridLength(140);
            else
                mainGrid.RowDefinitions[0].Height = new GridLength(49);
        }

        private void BackstageTabItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofdGRF = new OpenFileDialog();
            ofdGRF.Filter = "GRF Files (*.grf)|*.grf";
            ofdGRF.RestoreDirectory = true;
            if ((bool)ofdGRF.ShowDialog())
            {
                mainRibbon.SelectedTabItem = mainTab;
                if (baseGRF.IsOpen)
                    baseGRF.Close();
                baseGRF.Open(ofdGRF.FileName);
                UpdateGRFList();
            }
        }

        private void UpdateGRFList()
        {
            _GrfFileCollection.Clear();
            foreach (GRFFile file in baseGRF.Files)
                _GrfFileCollection.Add(file);
        }

        private void buttonExtAll_Click(object sender, RoutedEventArgs e)
        {   
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult dr = fbd.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                epd = new ExtractProgressDialog(baseGRF.FileCount);
                Thread et = new Thread(new ParameterizedThreadStart(delegate
                {
                    foreach (GRFFile file in baseGRF.Files)
                    {
                        baseGRF.ExtractFileToPath(file, fbd.SelectedPath + "/");
                    }
                }));
                et.Start();
                epd.ShowDialog();
                et.Abort();
            }
        }

        private void buttonExt_Click(object sender, RoutedEventArgs e)
        {
            if (lvGRFItems.SelectedItems.Count > 0)
            {
                System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
                System.Windows.Forms.DialogResult dr = fbd.ShowDialog();
                if (dr == System.Windows.Forms.DialogResult.OK)
                {
                    epd = new ExtractProgressDialog(lvGRFItems.SelectedItems.Count);
                    List<GRFFile> grfExtractList = new List<GRFFile>();
                    this.Dispatcher.Invoke(new ThreadStart(() =>
                    {
                        foreach (GRFFile file in lvGRFItems.SelectedItems)
                        {
                            grfExtractList.Add(file);
                        }
                    }));
                    Thread et = new Thread(new ParameterizedThreadStart(delegate
                    {
                        foreach (GRFFile file in grfExtractList)
                        {
                            baseGRF.ExtractFileToPath(file, fbd.SelectedPath + "/");
                        }
                    }));
                    et.Start();
                    epd.ShowDialog();
                    et.Abort();
                }
            }
        }

       

    }
}
