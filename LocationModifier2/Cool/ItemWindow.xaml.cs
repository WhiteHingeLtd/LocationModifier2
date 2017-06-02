﻿using System;
using System.Collections.Generic;
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
using LocationModifier2.Dialogs;
using LocationModifier2.UserControls;
using WHLClasses;

namespace LocationModifier2.Cool
{
    /// <summary>
    /// Interaction logic for ItemWindow.xaml
    /// </summary>
    public partial class ItemWindow : Window
    {

        internal MainWindow _OldMW = null;
        internal ShelfWindow ShelfWin = null;
        internal WhlSKU ActiveItem;
        internal SkuCollection ActiveCollection;
        public ItemWindow(MainWindow realMainWindow)
        {
            InitializeComponent();
            ShelfWin = new ShelfWindow(this);
            _OldMW = realMainWindow;
        }

        private void ScanBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessScan(ScanBox.Text);
                Refocus();
            }
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
            var kids = _OldMW.FullSkuCollection.GatherChildren(sku.ShortSku);
            //Get the list of locaitons,
            var LocationIDs = new Dictionary<int, SKULocation>();
            var locationMapping = new Dictionary<SKULocation.SKULocationType, int>()
            {
                {SKULocation.SKULocationType.Pickable, 0},
                {SKULocation.SKULocationType.Storage, 1},
                {SKULocation.SKULocationType.Delivery, 2},
                {SKULocation.SKULocationType.Prepack, 3},
                {SKULocation.SKULocationType.PrepackInstant, 4}
            };
            foreach (WhlSKU Kid in kids)
            {
                //Refresh thing
                Kid.RefreshLocations();
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
            var newdict = new Dictionary<int, string>();
            foreach (KeyValuePair<int, SKULocation> asd in orderedlocations)
            {
                newdict.Add(asd.Key, asd.Value.LocationText);
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
            //And now the lcoations.
            foreach (KeyValuePair<int, string> LocID in newdict)
            {
                LocationControlHolder.Children.Add(new ShelfControl(LocID.Key, LocID.Value, kids, this));
            }

        }

        internal void ProcessScan(string ScanData)
        {
            try
            {
                ShelfWin.Hide();
            }
            catch{}
            try
            {
                if (ScanData.StartsWith("qzu"))
                {
                    _OldMW.ProcessScanBox(ScanData);
                }
                else if (_OldMW.AuthdEmployee != null)
                {
                    if (!ScanData.StartsWith("qlo"))
                    {
                        //Googogo
                        //Clear
                    PacksizeHolder.Children.Clear();
                    LocationControlHolder.Children.Clear();

                    //Find it first
                    var Matches = _OldMW.MixdownSkuCollection.SearchBarcodes(ScanData);
                    if (Matches.Count == 0) Matches = _OldMW.MixdownSkuCollection.SearchSKUS(ScanData, true);
                    if (Matches.Count == 1)
                    {
                        ActiveItem = Matches[0];
                        ActiveCollection = _OldMW.FullSkuCollection.GatherChildren(ActiveItem.ShortSku);
                       LoadGrid(Matches[0]);
                    }else if (Matches.Count > 1)
                    {
                        var item = Distinguish.DistinguishSku(Matches);
                        ActiveItem = item;
                        ActiveCollection = _OldMW.FullSkuCollection.GatherChildren(ActiveItem.ShortSku);
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
