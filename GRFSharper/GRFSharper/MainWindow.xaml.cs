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
using System.IO;
using Fluent;
using Microsoft.Win32;
using SAIB.SharpGRF;
using System.Collections.ObjectModel;
using System.Media;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Documents;

namespace GRFSharper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        SharpGRF baseGRF = new SharpGRF();
        private GridViewColumnHeader _CurSortCol = null;
        private SortAdorner _CurAdorner = null;

        ObservableCollection<GRFFile> _GrfFileCollection = new ObservableCollection<GRFFile>();
        public MainWindow()
        {
            InitializeComponent();
            lvGRFItems.ItemsSource = _GrfFileCollection;
        }

        private void Ribbon_IsMinimizedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
                mainGrid.RowDefinitions[0].Height = new GridLength(140);
            else
                mainGrid.RowDefinitions[0].Height = new GridLength(49);
        }

        private void BackstageTabItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofdGRF = new OpenFileDialog();
            ofdGRF.Filter = "GRF Files (*.grf)|*.grf|GPF Files (*.gpf)|*.gpf";
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

        private void lvGRFItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (lvGRFItems.SelectedItem == null) return;
                GRFFile file = (GRFFile)lvGRFItems.SelectedItem;
                previewImage.Visibility = Visibility.Hidden;
                previewText.Visibility = Visibility.Hidden;

                BitmapImage bi = new BitmapImage();
                switch (file.Extension.ToLower())
                {

                    case ".txt":
                    case ".xml":
                        previewText.Text = System.Text.Encoding.Default.GetString(file.Data);
                        previewText.Visibility = Visibility.Visible;
                        break;

                    case ".wav":
                        SoundPlayer player = new SoundPlayer(new MemoryStream(file.Data));
                        player.Play();
                        break;

                    // Images
                    case ".bmp":
                        bi = new BitmapImage();
                        bi.BeginInit();
                        bi.StreamSource = new MemoryStream(file.Data);
                        bi.EndInit();
                        previewImage.Stretch = Stretch.None;
                        previewImage.Source = bi;
                        previewImage.Visibility = Visibility.Visible;
                        break;

                    case ".pal":
                        GRFSharperAddons.PAL pal = new GRFSharperAddons.PAL();
                        pal.Load(file.Data);
                        bi = new BitmapImage();
                        bi.BeginInit();
                        bi.StreamSource = pal.GetPalette();
                        bi.EndInit();
                        previewImage.Stretch = Stretch.None;
                        previewImage.Source = bi;
                        previewImage.Visibility = Visibility.Visible;
                        break;

                    case ".tga":
                        Paloma.TargaImage tga = new Paloma.TargaImage(file.Data);
                        bi = new BitmapImage();
                        bi.BeginInit();
                        bi.StreamSource = tga.ImageStream;
                        bi.EndInit();
                        previewImage.Stretch = Stretch.None;
                        previewImage.Source = bi;
                        previewImage.Visibility = Visibility.Visible;
                        break;

                    case ".spr":
                        GRFSharperAddons.SPR spr = new GRFSharperAddons.SPR();
                        spr.Load(file.Data);
                        bi = new BitmapImage();
                        bi.BeginInit();
                        bi.StreamSource = spr.GetPaletteImageStream(0);
                        bi.EndInit();
                        previewImage.Stretch = Stretch.None;
                        previewImage.Source = bi;
                        previewImage.Visibility = Visibility.Visible;
                        break;
                }


            }
            catch (Exception ex)
            {

            }
        }

        private void SortClick(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = sender as GridViewColumnHeader;
            String field = column.Tag as String;

            if (_CurSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(_CurSortCol).Remove(_CurAdorner);
                lvGRFItems.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (_CurSortCol == column && _CurAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            _CurSortCol = column;
            _CurAdorner = new SortAdorner(_CurSortCol, newDir);
            AdornerLayer.GetAdornerLayer(_CurSortCol).Add(_CurAdorner);
            lvGRFItems.Items.SortDescriptions.Add(new SortDescription(field, newDir));
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textBox1.Text.Length > 3)
            {
                lvGRFItems.ItemsSource = _GrfFileCollection.Where(x => x.Name.Contains(textBox1.Text));
            }
            else
            {
                lvGRFItems.ItemsSource = _GrfFileCollection;
            }
        }

    }
}
