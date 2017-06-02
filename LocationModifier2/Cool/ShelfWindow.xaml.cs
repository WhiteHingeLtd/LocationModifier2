using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WHLClasses;

namespace LocationModifier2.Cool
{
    /// <summary>
    /// Interaction logic for ShelfWindow.xaml
    /// </summary>
    public partial class ShelfWindow 
    {
        internal Dispatcher MainDisp;
        internal ItemWindow ItWi;
        internal bool Headers = true;
        private int _locationId;

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
            _locationId = Convert.ToInt32(ScanData.Replace("qlo", ""));
            MainDisp = this.Dispatcher;

            //Get matching skus from mixdown.

            Misc.OperationDialog("Scanning for Items...", DoProcess);

        }

        internal void AddControl(WhlSKU sku, int locationId)
        {
            PacksizeHolder.Children.Add(new ShelfWindowControl(sku, this, locationId));
        }

        internal void DoProcess(object sender, DoWorkEventArgs e)
        {
            //Get skus
            var Skus = GetSkusWithLocation(_locationId, ItWi.OldMw.MixdownSkuCollection);

            foreach (WhlSKU sku in Skus)
            {

                foreach (WhlSKU kid in ItWi.OldMw.FullSkuCollection.GatherChildren(sku.ShortSku))
                {
                    if ((kid.NewItem.IsListed || kid.PackSize == 1) && kid.Locations.Any(loc => loc.LocationID == _locationId))
                    { 
                        //GOGOOGOGOOGO
                        MainDisp.Invoke(() => AddControl(kid, _locationId));
                    }
                }
            }


        }

        //Then get thir children
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

        internal void ProcessScan(string scanData)
        {
            this.Hide();
            ItWi.ProcessScan(scanData);
            Refocus();
            
        }

        private Point _scrollerMousePos;
        private double _scrollerVerticalOffset;
        private double _scrollerHorizontalOffset;
        private void NotesScroller_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NotesScroller.CaptureMouse();
            _scrollerMousePos = e.GetPosition(NotesScroller);
            _scrollerVerticalOffset = NotesScroller.VerticalOffset;
            _scrollerHorizontalOffset = NotesScroller.HorizontalOffset;
        }
        private void NotesScroller_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            NotesScroller.ReleaseMouseCapture();
            Refocus();
        }
        private void NotesScroller_MouseMove(object sender, MouseEventArgs e)
        {
            if (!NotesScroller.IsMouseCaptured) return;
            NotesScroller.ScrollToVerticalOffset(_scrollerVerticalOffset + (_scrollerMousePos.Y - e.GetPosition(NotesScroller).Y));
            NotesScroller.ScrollToHorizontalOffset(_scrollerHorizontalOffset + (_scrollerMousePos.X - e.GetPosition(NotesScroller).X));
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
