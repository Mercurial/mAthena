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
using System.IO;

namespace GRFSharper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        GRF baseGRF = new GRF();

        ExtractProgressDialog epd;
        GRFOpenProgressDialog grfopd;
        AddFileProgressDialog afd;
        WriteFileProgressDialog sfd;

        ObservableCollection<GRFFile> _GrfFileCollection = new ObservableCollection<GRFFile>();
        Thread openGRFThread,addFileThread,writeFileThread;
        string GRFSharperSignature = "GRF Sharper v0.2";
        string FileName = "Untitled.grf";

        bool isEdited = true;

        public MainWindow()
        {
            InitializeComponent();
            UpdateWindowTitle();
            lvGRFItems.ItemsSource = _GrfFileCollection;
            InitGRFEventHandlers();
        }
        void InitGRFEventHandlers()
        {
            //Initialize GRF Event Handlers
            baseGRF.ExtractComplete += new ExtractCompleteEventHandler(baseGRF_ExtractComplete);
            baseGRF.FileReadComplete += new FileReadCompleteEventHandler(baseGRF_FileReadComplete);
            baseGRF.FileCountReadComplete += new FileCountReadCompleteEventHandler(baseGRF_FileCountReadComplete);
            baseGRF.GRFOpenComplete += new GRFOpenCompleteEventHandler(baseGRF_GRFOpenComplete);
            baseGRF.FileAddComplete += new FileAddCompleteEventHandler(baseGRF_FileAddComplete);
            baseGRF.FileBodyWriteComplete += new FileBodyWriteCompleteEventHandler(baseGRF_FileBodyWriteComplete);
            baseGRF.GRFSaveComplete += new SaveCompleteEventHandler(baseGRF_GRFSaveComplete);
        }
        void baseGRF_GRFSaveComplete(object sender)
        {
            this.Dispatcher.Invoke(new ThreadStart(() =>
            {
                UpdateWindowTitle();
                grfopd = new GRFOpenProgressDialog();
                grfopd.SetFileCount(baseGRF.FileCount);
                grfopd.Show();
            }));
        }

        void baseGRF_FileBodyWriteComplete(object sender, GRFEventArg e)
        {
            sfd.Dispatcher.Invoke(new ThreadStart(() =>
            {
                sfd.UpdateProgress(e.File.Name);
            }));
        }

        void baseGRF_FileAddComplete(object sender, GRFEventArg e)
        {
            afd.Dispatcher.Invoke(new ThreadStart(() =>
                {
                    afd.UpdateProgress(e.File.Name);
                }));
        }

        void baseGRF_FileReadComplete(object sender, GRFEventArg e)
        {
            grfopd.Dispatcher.Invoke(new ThreadStart(() => 
                {
                   grfopd.UpdateProgress(e.File.Name);
                }));
        }

        void baseGRF_GRFOpenComplete(object sender)
        {
            this.Dispatcher.Invoke(new ThreadStart(() =>
            {
                UpdateWindowTitle();
                UpdateGRFList();
                if (openGRFThread != null)
                    openGRFThread.Abort();
            }));
        }

        void baseGRF_FileCountReadComplete(object sender)
        {
            grfopd.SetFileCount(baseGRF.FileCount);
        }

        void baseGRF_ExtractComplete(object sender, GRFEventArg e)
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
                FileName = ofdGRF.SafeFileName;
                grfopd = new GRFOpenProgressDialog();
                openGRFThread = new Thread(new ThreadStart(() =>
                {
                    baseGRF.Open(ofdGRF.FileName);
                }));
                openGRFThread.Start();
                //baseGRF.Open(ofdGRF.FileName);
                grfopd.ShowDialog();
            }
        }

        private void UpdateGRFList()
        {
            _GrfFileCollection.Clear();
            foreach (GRFFile file in baseGRF.Files)
                if(file.Flags==1)
                    _GrfFileCollection.Add(file);
        }

        private void buttonExtAll_Click(object sender, RoutedEventArgs e)
        {
            if (baseGRF.IsOpen)
            {
                System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
                System.Windows.Forms.DialogResult dr = fbd.ShowDialog();
                if (dr == System.Windows.Forms.DialogResult.OK)
                {
                    epd = new ExtractProgressDialog(baseGRF.FileCount);
                    Thread et = new Thread(new ThreadStart(delegate
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

        private void buttonAddFile_Click(object sender, RoutedEventArgs e)
        {
            FileDialog fd = new OpenFileDialog();
            if ((bool)fd.ShowDialog())
            {
                GRFDirectoryPromptDialog grfdpd = new GRFDirectoryPromptDialog();
                if ((bool)grfdpd.ShowDialog())
                {
                    baseGRF.AddFile(fd.FileName, grfdpd.EnteredPath+fd.SafeFileName);
                    UpdateGRFList();
                }
            }

        }

        private void buttonAddFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog()==System.Windows.Forms.DialogResult.OK)
            {
                string[] filesToAdd = Directory.GetFiles(fbd.SelectedPath, "*", SearchOption.AllDirectories);
                afd = new AddFileProgressDialog();
                afd.SetFileCount(filesToAdd.Length);
                addFileThread = new Thread(new ThreadStart(() =>
                    {
                        foreach (string file in filesToAdd)
                        {
                            baseGRF.AddFile(file, file.Replace(fbd.SelectedPath + "\\", ""));
                        }
                        this.Dispatcher.Invoke(new ThreadStart(() =>
                            {
                                UpdateGRFList();
                            }));
                        addFileThread.Abort();
                    }));
                addFileThread.Start();
                afd.ShowDialog();
            }
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            if (!baseGRF.IsOpen)
            {
                FileDialog fd = new SaveFileDialog();
                if ((bool)fd.ShowDialog())
                {
                    sfd = new WriteFileProgressDialog();
                    sfd.SetFileCount(baseGRF.FileCount);
                    writeFileThread = new Thread(new ThreadStart(() =>
                        {
                            FileName = fd.SafeFileName;
                            baseGRF.SaveAs(fd.FileName);
                            writeFileThread.Abort();
                        }));
                    writeFileThread.Start();
                    sfd.ShowDialog();
                }
            }
            else
            {
                sfd = new WriteFileProgressDialog();
                sfd.SetFileCount(baseGRF.FileCount);
                writeFileThread = new Thread(new ThreadStart(() =>
                {
                    baseGRF.Save();
                    writeFileThread.Abort();
                }));
                writeFileThread.Start();
                sfd.ShowDialog();
            }
            UpdateGRFList();
        }

        private void buttonDeleteFile_Click(object sender, RoutedEventArgs e)
        {
          foreach(GRFFile file in lvGRFItems.SelectedItems)
          {
              baseGRF.DeleteFile(file.Name);
          }
          UpdateGRFList();
        }

        private void UpdateWindowTitle()
        {
            this.Title = string.Format("{0} - {1}", GRFSharperSignature, FileName);
        }

        private void BackstageTabItem_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            FileName = "Untitled.grf";
            baseGRF.Close();
            baseGRF = new GRF();
            InitGRFEventHandlers();
            UpdateGRFList();
            UpdateWindowTitle();
            mainRibbon.SelectedTabItem = mainTab;
        }

       

    }
}
