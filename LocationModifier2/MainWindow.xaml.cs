using LocationModifier2.Dialogs;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LocationModifier2.Cool;
using WHLClasses;
using WHLClasses.Exceptions;
using WHLClasses.MySQL_Old;
using WHLClasses.SQL;

namespace LocationModifier2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public GenericDataController Loader = new GenericDataController();
        public SkuCollection FullSkuCollection;
        public SkuCollection MixdownSkuCollection;
        public EmployeeCollection EmpCol = new EmployeeCollection();
        public Employee AuthdEmployee;
        public Mode CurrentSelectedMode;
        public ScanState CurrentScanState;
        public WhlSKU ActiveItem;
        public SkuCollection ActiveChildrenCollection;
        public int InitialLocationId;
        public int NewLocationId;
        
        private DispatcherTimer _updateMode = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();
            this.Focus();
            FullSkuCollection = Loader.SmartSkuCollLoad(true);
            Misc.OperationDialog("Preparing other stuff", delegate
            {
                MixdownSkuCollection = FullSkuCollection.MakeMixdown();
            });
            _updateMode.Interval = new TimeSpan(0,0,0,500);
            _updateMode.Tick += UpdateMode_Tick;
            CurrentSelectedMode = Mode.View;
            CurrentScanState = ScanState.InitialScan;
            _updateMode.Start();
            UpdateMode_Tick(null, null);
            ScanBox.Focus();

            var realwindow = new ItemWindow(this);
            realwindow.Show();
            this.Hide();
            realwindow.WindowState = WindowState.Maximized;

        }

        private void UpdateMode_Tick(object sender, EventArgs e)
        {
            switch (CurrentSelectedMode)
            {
                case Mode.InitialValue:
                    throw new Exception("Mode initialized incorrectly");
                case Mode.Location:
                    CurrentMode.Text = "Location";
                    break;
                case Mode.View:
                    CurrentMode.Text = "View";
                    break;
                case Mode.Count:
                    CurrentMode.Text = "Count";
                    break;
                default:
                    throw new NullReferenceException("Mode initialized incorrectly");    
            }
            _updateMode.Stop();
        }
        /// <summary>
        /// 
        /// </summary>
        public enum Mode
        {
            InitialValue = 0,
            Location = 1,
            View = 2,
            Count = 3
            
        }

        public enum ScanState
        {
            InitialScan = 0,
            ScannedItem = 1,
            ScannedOriginShelf = 2,
            ScannedNewShelf = 3
        }

        public void ProcessScanBox(string scannedText)
        {
            if (scannedText.StartsWith("qzu"))
            {
                var payrollidstr = scannedText.Replace("qzu", "");
                try
                {
                    AuthdEmployee = EmpCol.FindEmployeeByID(Convert.ToInt32(payrollidstr));
                    if (AuthdEmployee != null) Instruct("Please scan a barcode");
                    else throw new NullReferenceException();
                    EmployeeBlock.Text = AuthdEmployee.FullName;
                }
                catch (Exception)
                {
                    Instruct("Please scan a valid ID card");
                }
            }
            else if (AuthdEmployee != null)
            {
                if (scannedText.Length > 0)
                {
                    switch (CurrentSelectedMode)
                    {
                        case Mode.Count:
                            ProcessCount(scannedText);
                            break;
                        case Mode.Location:
                            ProcessLocation(scannedText);
                            break;
                        case Mode.View:
                            ProcessView(scannedText);
                            break;
                        case Mode.InitialValue:
                            throw new NullReferenceException();
                        default:
                            throw new NullReferenceException();

                    }


                }
                else Instruct("Please scan a valid barcode");
            }
            else if (AuthdEmployee == null)
            {
                Instruct("Please scan your ID card");
            }
            else Instruct("Please scan a valid barcode");

        }

        private void ScanBox_KeyUp(object sender,KeyEventArgs e)
        {
            var ctrl = sender as TextBox;
            if (e.Key == Key.Return && ctrl != null)
            {
                e.Handled = true;
                try
                {
                    ProcessScanBox(ctrl.Text);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
                finally
                {
                    ctrl.Text = "";
                }
            }

        }
        /// <summary>
        /// Method to easily update the instruction Block
        /// </summary>
        /// <param name="instruction">The string to update the Block to</param>
        /// <param name="colour">The colour of the instruction block. Defaults to black</param>
        private void Instruct(string instruction, SolidColorBrush colour = null)
        {
            if (colour == null) colour = Brushes.Black;
            InstructionBox.Text = instruction;
            InstructionBox.Foreground = colour;
        }

        private void ProcessCount(string data)
        {
            switch (CurrentScanState)
            {
                case ScanState.InitialScan:
                    if (data.StartsWith("qlo"))
                    {
                        Instruct("Please scan an item to adjust the stock");
                    }
                    else if (data.Length == 7)
                    {
                        ActiveItem = null;
                        Instruct("Please select a packsize");
                        var searchColl = FullSkuCollection.GatherChildren(data);                  
                        var results = new PacksizeSelector(searchColl);
                        results.ShowDialog();
                        ActiveItem = results.SelectedSku;
                        Instruct("Please scan a location");
                        CurrentScanState = ScanState.ScannedItem;

                    }
                    else if (data.Length == 11 | data.Length == 13)
                    {
                        ActiveItem = null;
                        var searchcoll = FullSkuCollection.SearchBarcodes(data);
                        var searchcoll2 = new SkuCollection(true);
                        searchcoll2.AddRange(from sku in searchcoll where !sku.SKU.Contains("xxxx") select sku);
                        
                        if (searchcoll2.Count == 1)
                        {
                            ActiveItem = searchcoll[0];
                            Instruct("Please scan a location");
                            CurrentScanState = ScanState.ScannedItem;
                        }
                        else
                        {
                            
                            var results = new PacksizeSelector(searchcoll);
                            results.ShowDialog();
                            ActiveItem = results.SelectedSku;
                            if (ActiveItem != null)
                            {
                                Instruct("Please scan a location");
                                CurrentScanState = ScanState.ScannedItem;
                            }
                            else
                            {
                                Instruct("Please scan a valid barcode");
                            }
                        }

                    }
                    else
                    {
                        Instruct("Please scan a valid barcode");
                    }
                    break;
                case ScanState.ScannedItem:
                    if (data.StartsWith("qlo"))
                    {
                        var currentLoc = Convert.ToInt32(data.Replace("qlo", ""));
                        Instruct(LocationIdConversion(currentLoc));
                        var stockCheck = new StockEntry(true);
                        stockCheck.ShowDialog();
                        if (stockCheck.Cancel)
                        {
                            Instruct("Please select a new location");
                            CurrentScanState = ScanState.ScannedItem;
                        }
                        else
                        {
                            var amount = stockCheck.FinalStockEntry;
                            var currentstate = stockCheck.CurrentSet;
                            try
                            {
                                switch (currentstate)
                                {
                                        case StockEntry.AddRemoveSet.Add:
                                            ActiveItem.AdjustStockWithAudit(currentLoc, AuthdEmployee, amount);
                                            HistoryBlock.Text = HistoryBlock.Text.Insert(0,
                                                    "Added " + amount.ToString() + " of " + ActiveItem.SKU + " by " +
                                                    amount.ToString() + Environment.NewLine + "At " +
                                                    LocationIdConversion(currentLoc) + Environment.NewLine +
                                                    "======================" + Environment.NewLine);  
                                            Instruct("Success. Please scan a new item");
                                        break;
                                        case StockEntry.AddRemoveSet.Remove:
                                            if (amount == 0)
                                            {
                                                ActiveItem.RemoveLocationWithAudit(currentLoc, AuthdEmployee);
                                                HistoryBlock.Text = HistoryBlock.Text.Insert(0, "Removed all of " + ActiveItem.SKU + Environment.NewLine + "At " +
                                                                                                LocationIdConversion(currentLoc) + Environment.NewLine +
                                                                                                "======================" + Environment.NewLine);
                                            }
                                            else
                                            {
                                                ActiveItem.AdjustStockWithAudit(currentLoc, AuthdEmployee, amount);
                                                HistoryBlock.Text = HistoryBlock.Text.Insert(0, "Removed " + amount.ToString() + " of " + ActiveItem.SKU + Environment.NewLine + "At " +
                                                                                                LocationIdConversion(currentLoc) + Environment.NewLine +
                                                                                                "======================" + Environment.NewLine);
                                            }
                                            
                                            
                                            Instruct("Success. Please scan a new item");
                                        break;
                                        case StockEntry.AddRemoveSet.Set:
                                            ActiveItem.SetLocationStockWithAudit(currentLoc,AuthdEmployee,amount);
                                            HistoryBlock.Text = HistoryBlock.Text.Insert(0, "Set stock of " + ActiveItem.SKU + " To "+ amount.ToString() + Environment.NewLine + "At " +
                                                                     LocationIdConversion(currentLoc) + Environment.NewLine +
                                                                     "======================" + Environment.NewLine);
                                            Instruct("Success. Please scan a new item");
                                        break;
                                }

                                CurrentScanState = ScanState.InitialScan;
                            }
                            catch (NegativeStockException e)
                            {
                                Console.WriteLine(e);
                                Instruct("The stock cannot be negative. Please rescan the location");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                                Instruct("Failed to add location. Please try again");
                                CurrentScanState = ScanState.InitialScan;
                                throw;
                            }
                        }
                    }
                    else
                    {
                        Instruct("Please scan a valid shelf ID");
                    }
                    break;
                case ScanState.ScannedNewShelf:
                    break;
                case ScanState.ScannedOriginShelf:
                    break;
                default:
                    Instruct("An error occured please try again");
                    throw new NullReferenceException();
            }

           
        }

        private void ProcessLocation(string data)
        {

            switch (CurrentScanState)
            {
                case ScanState.InitialScan:
                    if (data.StartsWith("qlo"))
                    {
                        Instruct("Please scan an item");
                        ActiveItem = null;
                        NewLocationId = 0;
                        InitialLocationId = 0;
                        CurrentScanState = ScanState.InitialScan;
                    }
                    else if ((data.StartsWith("10") && data.Length > 7) | data.Length == 13)
                    {
                        ItemDetailsStackPanel.Children.Clear();
                        ActiveItem = FullSkuCollection.SearchBarcodes(data)[0];
                        Instruct("Please scan the original location");
                        var infoblock = new TextBlock
                        {
                            Text = "Active item:" + Environment.NewLine + ActiveItem.Title.Label + "  " + ActiveItem.SKU,
                            FontSize = 36.0
                        };
                        ItemDetailsStackPanel.Children.Add(infoblock);
                        InitialLocationId = 0;
                        CurrentScanState = ScanState.ScannedItem;
                    }
                    else if (data.Length == 7 && data.StartsWith("10") | data.Length == 13)
                    {
                        ActiveItem = null;
                        var searchcoll = FullSkuCollection.SearchBarcodes(data);
                        if (searchcoll.Count(x => !x.SKU.Contains("xxxx")) == 1)
                        {
                            ActiveItem = searchcoll[0];
                            Instruct("Please scan a location");
                            CurrentScanState = ScanState.ScannedItem;
                        }
                        else
                        {
                            var searchColl = FullSkuCollection.GatherChildren(data);
                            var results = new PacksizeSelector(searchColl);
                            results.ShowDialog();
                            ActiveItem = results.SelectedSku;
                            if (ActiveItem != null)
                            {
                                Instruct("Please scan a location");
                                CurrentScanState = ScanState.ScannedItem;
                            }
                            else
                            {
                                Instruct("Please scan a valid barcode");
                            }
                        }


                    }
                    else
                    {
                        Instruct("Please scan a valid barcode");
                    }
                    break;
                case ScanState.ScannedItem:
                    if (data.Contains("qlo"))
                    {
                        InitialLocationId = Convert.ToInt32(data.Replace("qlo", ""));
                        NewLocationId = 0;
                        CurrentScanState = ScanState.ScannedOriginShelf;
                        Instruct("Please scan the new location");
                    }
                    else if (data.StartsWith("10"))
                    {
                        Instruct("Please scan an item");
                        ActiveItem = null;
                        NewLocationId = 0;
                        InitialLocationId = 0;
                        CurrentScanState = ScanState.InitialScan;
                    }
                    else
                    {
                        var conv = LocationNameConversion(data);
                        if (conv != -1)
                        {
                            InitialLocationId = conv;
                            NewLocationId = 0;
                            CurrentScanState = ScanState.ScannedOriginShelf;
                            Instruct("Please input the new location");
                        }
                        else Instruct("Please scan the original location");
                    }
                    break;
                case ScanState.ScannedOriginShelf:
                    if (data.Contains("qlo"))
                    {
                        NewLocationId = Convert.ToInt32(data.Replace("qlo", ""));
                    }
                    else
                    {
                        var conv = LocationNameConversion(data);
                        NewLocationId = conv != -1 ? conv : 0;
                    }
                    
                    CurrentScanState = ScanState.ScannedNewShelf;
                    if (ActiveItem != null && NewLocationId != 0 && InitialLocationId != 0)
                    {
                        var stockCheck = new StockEntry();
                        stockCheck.ShowDialog();
                        var amount = stockCheck.FinalStockEntry;
                        try
                        {
                            ActiveItem.AdjustLocationWithAudit(InitialLocationId, AuthdEmployee, amount, NewLocationId);
                            HistoryBlock.Text = HistoryBlock.Text.Insert(0,
                                "Moved " + amount.ToString() + " of " + ActiveItem.SKU + Environment.NewLine + "From "
                                + LocationIdConversion(InitialLocationId) + " to " +
                                LocationIdConversion(NewLocationId) + Environment.NewLine
                                + "======================" + Environment.NewLine);
                            ActiveItem = null;
                            NewLocationId = 0;
                            InitialLocationId = 0;
                            CurrentScanState = ScanState.InitialScan;
                            Instruct("Item Moved. Please scan a new item");
                        }
                        catch (NegativeStockException)
                        {
                            Instruct("You cannot have negative stock. Please enter a valid value");
                            CurrentScanState = ScanState.ScannedOriginShelf;
                        }
                        catch (Exception)
                        {
                            Instruct("There was an error please try again.");
                            ActiveItem = null;
                            NewLocationId = 0;
                            InitialLocationId = 0;
                            CurrentScanState = ScanState.InitialScan;
                            throw;
                        }
                    }
                    else if (data.StartsWith("10"))
                    {
                        Instruct("Please scan an item");
                        ActiveItem = null;
                        NewLocationId = 0;
                        InitialLocationId = 0;
                        CurrentScanState = ScanState.InitialScan;
                    }
                    else
                    {
                        Instruct("There was an error please try again.");
                        ActiveItem = null;
                        NewLocationId = 0;
                        InitialLocationId = 0;
                        CurrentScanState = ScanState.InitialScan;
                    }
                    break;
                case ScanState.ScannedNewShelf:
                {
                    Instruct("There was an error please try again.");
                    ActiveItem = null;
                    NewLocationId = 0;
                    InitialLocationId = 0;
                    CurrentScanState = ScanState.InitialScan;
                }
                    break;
                default:
                    throw new NullReferenceException("The ScanState enum was invalid");
            }

        }

        private void ProcessView(string data)
        {
            if (data.StartsWith("qlo") )
            {
                Instruct(LocationIdConversion(Convert.ToInt32(data.Replace("qlo",""))));
                ItemDetailsStackPanel.Children.Clear();
                PickLocationsBlock.Text = "";
                OtherLocationsBlock.Text = "";
                var dict = MySQL_New.GetData("SELECT * FROM whldata.sku_locations WHERE LocationRefID='"+ data.Replace("qlo","") + "';");
                
                
                foreach (var result in dict)
                {
                    if (result["Sku"].ToString().Contains("xxxx")) continue;
                    int stock;
                    try
                    {
                        stock = Convert.ToInt32(result["additionalInfo"]);
                    }
                    catch (Exception)
                    {
                        stock = 0;
                    }
                    var onShelf = new TextBlock();
                    var searchColl = FullSkuCollection.SearchBarcodes(result["Sku"].ToString());
                    var item = searchColl[0];
                    onShelf.Text += item.SKU + " " + item.Title.Label + Environment.NewLine + "Stock: " +
                                    stock.ToString() + Environment.NewLine;
                    onShelf.FontSize = 36;
                    ItemDetailsStackPanel.Children.Add(onShelf);
                }
                
            }
            else if (data.StartsWith("10") && data.Length == 7)
            {
                PickLocationsBlock.Text = "";
                OtherLocationsBlock.Text = "";
                var searchcoll = FullSkuCollection.GatherChildren(data);
                ItemDetailsStackPanel.Children.Clear();
                var currentItem = searchcoll[0];
                Instruct(currentItem.Title.Label);
                foreach (var result in searchcoll)
                {
                    result.RefreshLocations();
                    var onShelf = new TextBlock();
                    onShelf.Text += result.SKU + " " + result.Title.Label;
                    onShelf.Text += Environment.NewLine;
                    foreach (var loc in result.Locations)
                    {
                        onShelf.Text += loc.LocationText + ", " + Environment.NewLine + "Stock: " + loc.Additional;

                    }
                    onShelf.Text = onShelf.Text.Trim().TrimEnd(',');
                    onShelf.Text += Environment.NewLine;
                    onShelf.FontSize = 36;
                    ItemDetailsStackPanel.Children.Add(onShelf);
                }
            }
            else if (data.StartsWith("10") | data.Length == 13)
            {
                var searchcoll = FullSkuCollection.SearchBarcodes(data);
                PickLocationsBlock.Text = "";
                OtherLocationsBlock.Text = "";
                if (searchcoll.Count == 1)
                {
                    var item = searchcoll[0];
                    ItemDetailsStackPanel.Children.Clear();
                    Instruct(item.Title.Label);
                    item.RefreshLocations();
                    Instruct(searchcoll[0].SKU + " " + searchcoll[0].Title.Label);
                    
                    foreach (var loc in item.Locations)
                    {
                        var stockInfoBlock = new TextBlock
                        {
                            FontSize = 36.0
                        };
                        if (loc.LocationType == SKULocation.SKULocationType.Pickable)
                        {
                            PickLocationsBlock.Text += loc.LocationText + ", ";
                        }
                        else
                        {
                            OtherLocationsBlock.Text += loc.LocationText + ", ";
                        }
                        stockInfoBlock.Text = loc.LocationText + " Stock: " + Convert.ToInt32(loc.Additional).ToString();
                        ItemDetailsStackPanel.Children.Add(stockInfoBlock);
                    }
                    PickLocationsBlock.Text = PickLocationsBlock.Text.Trim().TrimEnd(',');
                    OtherLocationsBlock.Text = OtherLocationsBlock.Text.Trim().TrimEnd(',');
                    

                }
                else if (searchcoll.Count > 1)
                {
                    ItemDetailsStackPanel.Children.Clear();
                    var currentItem = searchcoll[0];
                    Instruct(currentItem.Title.Label);
                    foreach (var result in searchcoll)
                    {
                        var onShelf = new TextBlock();
                        onShelf.Text += result.SKU + " " + result.Title.Label +" " + result.GetLocation(SKULocation.SKULocationType.Pickable).LocationText;
                        onShelf.FontSize = 36;
                        ItemDetailsStackPanel.Children.Add(onShelf);
                    }
                }
                else
                {
                    Instruct("We couldn't recognise that barcode. Please try again");
                }
            }
            else
            {
                Instruct("Please scan a valid barcode");
            }
        }

        private void LocationButton_Click(object sender, RoutedEventArgs e)
        {
            var currentButton = sender as Button;
            if (currentButton != null) currentButton.IsEnabled = false;
            CountButton.IsEnabled = true;
            ViewButton.IsEnabled = true;
            CurrentSelectedMode = Mode.Location;
            ResetForNewScan();

            ScanBox.Focus();
        }

        private void CountButton_Click(object sender, RoutedEventArgs e)
        {
            var currentButton = sender as Button;
            if (currentButton != null) currentButton.IsEnabled = false;
            LocationButton.IsEnabled = true;
            ViewButton.IsEnabled = true;
            CurrentSelectedMode = Mode.Count;
            ResetForNewScan();
            ScanBox.Focus();
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            var currentButton = sender as Button;
            if (currentButton != null) currentButton.IsEnabled = false;
            CountButton.IsEnabled = true;
            LocationButton.IsEnabled = true;
            CurrentSelectedMode = Mode.View;
            ResetForNewScan();
            ScanBox.Focus();
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            var msg = new MsgDialog("App Shutdown","Do you want to close this application?");
            msg.ShowDialog();
            if (msg.Result)
            {
                Application.Current.Shutdown();
            }
            else
            {
                ScanBox.Focus();
            }
            
        }
        public string LocationIdConversion(int location)
        {
            try
            {
                var locationString = "";
                var locObject = MySQL_Ext.SelectData("SELECT locText FROM whldata.locationreference WHERE locID=" + location.ToString() + " LIMIT 1;") as ArrayList;
                if (locObject != null && locObject.Count > 0)
                {
                    foreach (ArrayList locList in locObject)
                    {
                        locationString = locList[0].ToString();
                    }
                }
                else
                {
                    throw new NullReferenceException();
                }

                return locationString;
            }
            catch (Exception)
            {
                return "Could not find shelf name";
            }

        }

        public int LocationNameConversion(string location)
        {
            try
            {
                var locationID = 0;
                var locObject = MySQL_Ext.SelectData("SELECT locid FROM whldata.locationreference WHERE locText='" + location.ToUpper() + "' LIMIT 1;") as ArrayList;
                if (locObject != null && locObject.Count > 0)
                {
                    foreach (ArrayList locList in locObject)
                    {
                        int.TryParse(locList[0].ToString(), out locationID);
                    }
                }
                else
                {
                    locationID = -1;
                    return locationID;

                }

                return locationID;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private void ResetForNewScan()
        {
            CurrentScanState = ScanState.InitialScan;
            ActiveItem = null;
            UpdateMode_Tick(null, null);
            Instruct("Please scan a barcode");
            ItemDetailsStackPanel.Children.Clear();
            PickLocationsBlock.Text = "";
            OtherLocationsBlock.Text = "";
        }

        private void ScanBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ScanBox.Background = Brushes.Red;
        }

        private void ScanBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ScanBox.Background = Brushes.White;
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            ScanBox.Focus();
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ScanBox.Focus();
        }
    }
}
