using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using WHLClasses;

namespace LocationModifier2.Cool
{
    /// <summary>
    /// Interaction logic for ShelfWindow.xaml
    /// </summary>
    public partial class ShelfWindow : Window
    {
        internal Dispatcher maindisp = null;
        internal ItemWindow ItWi = null;
        internal bool Headers = true;
        private int LocationID = 0;
        public ShelfWindow(ItemWindow IW)
        {
            InitializeComponent();

            ItWi = IW;
        }


        internal void LoadShelf(string ScanData)
        {
            Headers = false;
            ItemName.Text = "No items found";
            ShortSku.Text = ScanData.Replace("qlo", "");
            PacksizeHolder.Children.Clear();
           LocationID = Convert.ToInt32(ScanData.Replace("qlo", ""));
            maindisp = this.Dispatcher;

            //Get matching skus from mixdown.

            Misc.OperationDialog("Scanning for Items...", DoProcess);
            
        }

        internal void AddControl(WhlSKU Sku, int LocationID)
        {
            PacksizeHolder.Children.Add(new ShelfWindowControl(Sku, this, LocationID));
        }

        internal void DoProcess(object sender, DoWorkEventArgs e)
        {
            var worker = (sender as BackgroundWorker);
            //Get skus
            var Skus = GetSkusWithLocation(LocationID, ItWi._OldMW.MixdownSkuCollection);
            //Then get thir children
            foreach (WhlSKU sku in Skus)
            {
                foreach (WhlSKU kid in ItWi._OldMW.FullSkuCollection.GatherChildren(sku.ShortSku))
                {
                    if (kid.NewItem.IsListed || kid.PackSize == 1)
                    {
                        //GOGOOGOGOOGO
                        maindisp.Invoke(() => AddControl(kid, LocationID));
                    }
                }
            }
        }



        #region Steals

        internal List<WhlSKU> GetSkusWithLocation(int TargetID, SkuCollection Source)
        {

            List<WhlSKU> asd =
                (Source.Where(sku => sku.Locations.Any(location => location.LocationID == TargetID)).ToList());
            asd.Sort((sku, whlSku) => sku.SKU.CompareTo(whlSku.SKU));
            return asd;
        }

        private void ScanBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ScanBox.Background = Brushes.LightCoral;
        }

        private void ScanBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ScanBox.Background = Brushes.PaleGreen;
        }

        private void ScanBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessScan(ScanBox.Text);
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        internal void ProcessScan(string ScanData)
        {
            this.Hide();
            ItWi.ProcessScan(ScanData);
            Refocus();
            
        }

        Point ScrollerMousePos;
        double ScrollerVerticalOffset;
        double ScrollerHorizontalOffset;
        private void NotesScroller_MouseLeftButtonDown(object Sender, MouseButtonEventArgs E)
        {
            NotesScroller.CaptureMouse();
            ScrollerMousePos = E.GetPosition(NotesScroller);
            ScrollerVerticalOffset = NotesScroller.VerticalOffset;
            ScrollerHorizontalOffset = NotesScroller.HorizontalOffset;
        }
        private void NotesScroller_MouseLeftButtonUp(object Sender, MouseButtonEventArgs E)
        {
            NotesScroller.ReleaseMouseCapture();
            Refocus();
        }
        private void NotesScroller_MouseMove(object Sender, MouseEventArgs E)
        {
            if (NotesScroller.IsMouseCaptured)
            {
                NotesScroller.ScrollToVerticalOffset(ScrollerVerticalOffset + (ScrollerMousePos.Y - E.GetPosition(NotesScroller).Y));
                NotesScroller.ScrollToHorizontalOffset(ScrollerHorizontalOffset + (ScrollerMousePos.X - E.GetPosition(NotesScroller).X));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Refocus();
        }

        private void ScanBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Refocus();
        }

        internal void Refocus()
        {
            ScanBox.Text = "";
            ScanBox.Focus();
        }

        #endregion

    }
}
