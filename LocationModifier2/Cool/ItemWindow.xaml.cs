using LocationModifier2.Dialogs;
using LocationModifier2.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WHLClasses;
using WHLClasses.Orders;

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
        internal OrderDefinition OrderDefintions;
        internal DispatcherTimer OrddefReloadTimer;
        internal DateTime LastOrddefRefresh;
        public ItemWindow(MainWindow realMainWindow)
        {
            InitializeComponent();
            ShelfWin = new ShelfWindow(this);
            OldMw = realMainWindow;
            Refocus();
            OrddefReloadTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1),
                IsEnabled = true
            };
            OrddefReloadTimer.Tick += OrddefReloadTimer_Tick;
            OrddefReloadTimer.Start();
            OrddefReloadTimer_Tick(null, null);
            CurrentButtonType = StockEntry2.ButtonType.SetStock;

        }

        internal void OrddefReloadTimer_Tick(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                try
                {
                    var orddefClient =
                        WHLClasses.Services.OrderServer.Fucnt.ConnectChannel(
                            "net.tcp://orderserver.ad.whitehinge.com:801/OrderServer/1");
                    OrderDefintions = orddefClient.StreamOrderDefinition();
                    LastOrddefRefresh = DateTime.Now;
                }
                catch (Exception)
                {
                    //
                }
                
            }).Start();

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
            var locationIDs = new Dictionary<int, SKULocation>();
            var hasMultiplePicking = false;
            var locationMapping = new Dictionary<SKULocation.SKULocationType, int>()
            {
                {SKULocation.SKULocationType.Pickable, 0},
                {SKULocation.SKULocationType.Storage, 1},
                {SKULocation.SKULocationType.Delivery, 2},
                {SKULocation.SKULocationType.Prepack, 3},
                {SKULocation.SKULocationType.PrepackInstant, 4},
                {SKULocation.SKULocationType.Unused, 5}
            };
            foreach (var kid in kids)
            {
                //Refresh thing
                kid.RefreshLocations();
                if (kid.GetLocationsByType(SKULocation.SKULocationType.Pickable).Count != 1)
                {
                    hasMultiplePicking = true;
                }
                //We're gonna have to iterate and get a list of locations.
                foreach (var loc in kid.Locations)
                {
                    if (!locationIDs.Keys.Contains(loc.LocationID))
                    {
                        locationIDs.Add(loc.LocationID, loc);
                    }
                }
            }
            //Sort them to fix the faggy ordering
            var orderedlocations = locationIDs.OrderBy(x => locationMapping[x.Value.LocationType]);
            var newdict = new Dictionary<int, SKULocation>();
            foreach (var asd in orderedlocations)
            {
                newdict.Add(asd.Key, asd.Value);
            }
            //Now go again and make the controls.
            foreach (var kid in kids)
            {
                if (kid.NewItem.IsListed || kid.PackSize == 1)
                {
                    var packsizecontrol = new PacksizeControl(newdict.Keys.ToList(), kid, this);
                    packsizecontrol.MouseLeftButtonUp += NotesScroller_MouseLeftButtonUp;
                    packsizecontrol.MouseLeftButtonDown += NotesScroller_MouseLeftButtonDown;
                    packsizecontrol.MouseMove += NotesScroller_MouseMove;
                    PacksizeHolder.Children.Add(packsizecontrol);
                }
            }
            //And now the locations.
            foreach (var locId in newdict)
            {
                if (hasMultiplePicking && locId.Value.LocationType == SKULocation.SKULocationType.Pickable)
                {
                    LocationControlHolder.Children.Add(
                        new ShelfControl(locId.Key, locId.Value.LocationText, kids, this, true));
                }
                else if (locId.Value.LocationType == SKULocation.SKULocationType.Pickable)
                {
                    LocationControlHolder.Children.Add(
                        new ShelfControl(locId.Key, locId.Value.LocationText, kids, this, false, true));
                }
                else
                {
                    LocationControlHolder.Children.Add(
                        new ShelfControl(locId.Key, locId.Value.LocationText, kids, this));
                }
            }
        }

        internal void ProcessScan(string scanData)
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
                if (scanData.StartsWith("qzu"))
                {
                    OldMw.ProcessScanBox(scanData);
                    
                }
                else if (scanData == "")
                {
                    PacksizeHolder.Children.Clear();
                    LocationControlHolder.Children.Clear();
                }
                else if (OldMw.AuthdEmployee != null)
                {
                    if (scanData.StartsWith("qwz") || scanData.StartsWith("qzw"))
                    {
                        new IssuesList(this, OrderDefintions).ShowDialog();
                    }
                    else if (scanData.StartsWith("ppl"))
                    {
                        new PrepackList(OrderDefintions, this).ShowDialog();
                    }
                    else if (!scanData.StartsWith("qlo"))
                    {
                        //Googogo
                        //Clear
                        PacksizeHolder.Children.Clear();
                        LocationControlHolder.Children.Clear();

                        //Find it first
                        var matches = OldMw.FullSkuCollection.SearchBarcodes(scanData);
                        if (matches.Count == 0) matches = OldMw.MixdownSkuCollection.SearchSKUS(scanData, false);
                        if (matches.Count == 1)
                        {
                            ActiveItem = matches[0];
                            ActiveCollection = OldMw.FullSkuCollection.GatherChildren(ActiveItem.ShortSku);
                           LoadGrid(matches[0]);
                        }
                        else if (matches.Count > 1)
                        {
                            var item = Distinguish.DistinguishSku(matches);
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
                        ProcessToShelfScreen(scanData);
                    }
                }
                else
                {
                    new MsgDialog("ERROR", "You must log in before scanning stuff").ShowDialog();
                }
            }
            catch (Exception ex)
            {
                WHLClasses.Reporting.ErrorReporting.ReportException(ex, false);
                new MsgDialog("Scan Error", "An unknown error occurred while processing your scan.").ShowDialog();
                Refocus();
            }

            
        }

        private void ProcessToShelfScreen(string scanData)
        {
            ShelfWin.Show();
            ShelfWin.LoadShelf(scanData);

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
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
            if (ActiveItem == null) return;
            new AddShelf(this, ActiveItem, ActiveCollection).ShowDialog();
            ProcessScan(ActiveItem.SKU);
            Refocus();
        }

        private void AuditButton_Click(object sender, RoutedEventArgs e)
        {
            new AuditTrailWindow(ShortSku.Text, ItemName.Text,this).ShowDialog();
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(OldMw.AuthdEmployee != null) Logout();
        }

        private void Grid_TouchUp(object sender, TouchEventArgs e)
        {
            if (OldMw.AuthdEmployee != null) Logout();
        }

        private void Logout()
        {
            ProcessScan("");
            ItemName.Text = "_";
            ShortSku.Text = "_";
            OldMw.AuthdEmployee = null;
            ActiveItem = null;
            new MsgDialog("Logged out", "You have logged out sucessfully").ShowDialog();
            
        }
    }
}
