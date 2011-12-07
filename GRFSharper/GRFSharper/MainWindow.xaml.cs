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
using Fluent;
using Microsoft.Win32;
using SAIB.SharpGRF;
using System.Collections.ObjectModel;

namespace GRFSharper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        SharpGRF baseGRF = new SharpGRF();
        
        ObservableCollection<GRFFile> _GrfFileCollection = new ObservableCollection<GRFFile>();
        public MainWindow()
        {
            InitializeComponent();
            lvGRFItems.ItemsSource = _GrfFileCollection;
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

       

    }
}
