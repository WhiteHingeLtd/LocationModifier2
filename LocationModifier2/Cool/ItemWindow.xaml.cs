using LocationModifier2.Dialogs;
using LocationModifier2.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WHLClasses;

namespace LocationModifier2.Cool
{
    /// <summary>
    /// Interaction logic for ItemWindow.xaml
    /// </summary>
    public partial class ItemWindow
    {

        internal MainWindow OldMw;
        internal ShelfWindow ShelfWin;
        internal WhlSKU ActiveItem;
        internal SkuCollection ActiveCollection;
        internal StockEntry2.ButtonType CurrentButtonType;
        public ItemWindow(MainWindow realMainWindow)
        {
            InitializeComponent();
            ShelfWin = new ShelfWindow(this);
            OldMw = realMainWindow;
            Refocus();
            CurrentButtonType = StockEntry2.ButtonType.SetStock;
        }

        private void ScanBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            ProcessScan(ScanBox.Text);
            Refocus();
        }

        internal void Refocus()
        {
            ScanBox.Text = "";
            ScanBox.Focus();
        }

        private void LoadGrid(WhlSKU sku)
        {
            //GOGOOGO WE HACVE MATCH
            ItemName.Text = sku.Title.Label;
            ShortSku.Text = sku.ShortSku;

            //Get kids.
            var kids = OldMw.FullSkuCollection.GatherChildren(sku.ShortSku);
            //Get the list of locaitons,
            var LocationIDs = new Dictionary<int, SKULocation>();
            var HasMultiplePicking = false;
            var locationMapping = new Dictionary<SKULocation.SKULocationType, int>()
            {
                {SKULocation.SKULocationType.Pickable, 0},
                {SKULocation.SKULocationType.Storage, 1},
                {SKULocation.SKULocationType.Delivery, 2},
                {SKULocation.SKULocationType.Prepack, 3},
                {SKULocation.SKULocationType.PrepackInstant, 4},
                {SKULocation.SKULocationType.Unused, 5}
            };
            foreach (WhlSKU Kid in kids)
            {
                //Refresh thing
                Kid.RefreshLocations();
                if (Kid.GetLocationsByType(SKULocation.SKULocationType.Pickable).Count != 1)
                {
                    HasMultiplePicking = true;
                }
                //We're gonna have to iterate and get a list of locations.
                foreach (SKULocation loc in Kid.Locations)
                {
                    if (!LocationIDs.Keys.Contains(loc.LocationID))
                    {
                        LocationIDs.Add(loc.LocationID, loc);
                    }
                }
            }
            //Sort them to fix the faggy ordering
            var orderedlocations = LocationIDs.OrderBy(x => locationMapping[x.Value.LocationType]);
            var newdict = new Dictionary<int, SKULocation>();
            foreach (KeyValuePair<int, SKULocation> asd in orderedlocations)
            {
                newdict.Add(asd.Key, asd.Value);
            }
            //Now go again and make the controls.
            foreach (WhlSKU Kid in kids)
            {
                if (Kid.NewItem.IsListed || Kid.PackSize == 1)
                {
                    var packsizecontrol = new PacksizeControl(newdict.Keys.ToList(), Kid, this);
                    packsizecontrol.MouseLeftButtonUp += NotesScroller_MouseLeftButtonUp;
                    packsizecontrol.MouseLeftButtonDown += NotesScroller_MouseLeftButtonDown;
                    packsizecontrol.MouseMove += NotesScroller_MouseMove;
                    PacksizeHolder.Children.Add(packsizecontrol);
                }

            }
            //And now the locations.
            foreach (KeyValuePair<int, SKULocation> LocID in newdict)
            {
                if (HasMultiplePicking && LocID.Value.LocationType == SKULocation.SKULocationType.Pickable)
                {
                    LocationControlHolder.Children.Add(new ShelfControl(LocID.Key, LocID.Value.LocationText, kids, this,true));
                }
                else if(LocID.Value.LocationType == SKULocation.SKULocationType.Pickable)
                {
                    LocationControlHolder.Children.Add(new ShelfControl(LocID.Key, LocID.Value.LocationText, kids, this, false,true));
                }
                else
                {
                    LocationControlHolder.Children.Add(new ShelfControl(LocID.Key, LocID.Value.LocationText, kids, this));
                }
            }

        }

        internal void ProcessScan(string ScanData)
        {
            try
            {
                ShelfWin.Hide();
            }
            catch (Exception)
            {
                //It's not visible
            }
            try
            {
                if (ScanData.StartsWith("qzu"))
                {
                    OldMw.ProcessScanBox(ScanData);
                }
                else if (OldMw.AuthdEmployee != null)
                {
                    if (!ScanData.StartsWith("qlo"))
                    {
                        //Googogo
                        //Clear
                        PacksizeHolder.Children.Clear();
                        LocationControlHolder.Children.Clear();

                        //Find it first
                        var Matches = OldMw.FullSkuCollection.SearchBarcodes(ScanData);
                        if (Matches.Count == 0) Matches = OldMw.MixdownSkuCollection.SearchSKUS(ScanData, true);
                        if (Matches.Count == 1)
                        {
                            ActiveItem = Matches[0];
                            ActiveCollection = OldMw.FullSkuCollection.GatherChildren(ActiveItem.ShortSku);
                           LoadGrid(Matches[0]);
                        }
                        else if (Matches.Count > 1)
                        {
                            var item = Distinguish.DistinguishSku(Matches);
                            ActiveItem = item;
                            ActiveCollection = OldMw.FullSkuCollection.GatherChildren(ActiveItem.ShortSku);
                            LoadGrid(item);
                        }
                        else
                        {
                            new MsgDialog("ERROR", "Unable to find any items which matched the search!").ShowDialog();
                        }
                    }
                    else
                    {
                        ProcessToShelfScreen(ScanData);
                    }
                }
                else
                {
                    (new MsgDialog("ERROR", "You must log in before scanning stuff")).ShowDialog();
                }
            }
            catch (Exception ex)
            {
                WHLClasses.Reporting.ErrorReporting.ReportException(ex, false);
                (new MsgDialog("Scan Error", "An unknown error occurred while processing you scan.")).ShowDialog();
                Refocus();
            }

            
        }

        private void ProcessToShelfScreen(string ScanData)
        {
            ShelfWin.Show();
            ShelfWin.LoadShelf(ScanData);

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
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
            if (!NotesScroller.IsMouseCaptured) return;
            NotesScroller.ScrollToVerticalOffset(ScrollerVerticalOffset + (ScrollerMousePos.Y - E.GetPosition(NotesScroller).Y));
            NotesScroller.ScrollToHorizontalOffset(ScrollerHorizontalOffset + (ScrollerMousePos.X - E.GetPosition(NotesScroller).X));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Activate();
            Refocus();
        }

        private void ScanBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Refocus();
        }

        private void ScanBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ScanBox.Background = Brushes.LightCoral;
        }

        private void ScanBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ScanBox.Background = Brushes.PaleGreen;
        }

        private void AddShelfButton_Click(object sender, RoutedEventArgs e)
        {
            new AddShelf(this, ActiveItem, ActiveCollection).ShowDialog();
            ProcessScan(ActiveItem.ShortSku);
            Refocus();
        }

        private void AuditButton_Click(object sender, RoutedEventArgs e)
        {
            (new AuditTrailWindow(ShortSku.Text, ItemName.Text,this)).ShowDialog();
        }
    }
}
