using LocationModifier2.Dialogs;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WHLClasses;
namespace LocationModifier2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public GenericDataController Loader = new GenericDataController();
        public SkuCollection FullSkuCollection;
        public EmployeeCollection EmpCol = new EmployeeCollection();
        public Employee AuthdEmployee = new Employee();
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
            FullSkuCollection = Loader.SmartSkuCollLoad(true);
            _updateMode.Interval = new TimeSpan(0,0,0,500);
            _updateMode.Tick += UpdateMode_Tick;
            CurrentSelectedMode = Mode.View;
            CurrentScanState = ScanState.InitialScan;
            _updateMode.Start();
            UpdateMode_Tick(null, null);
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
                    Console.WriteLine("Shit's Fucked");
                    throw new NullReferenceException();    
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
            if (scannedText.StartsWith("qlo") && CurrentSelectedMode == Mode.Count)
            {
                Instruct("Please change your mode");
            }
            else if (scannedText.StartsWith("qzu"))
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
        private void Instruct(string instruction)
        {
            InstructionBox.Text = instruction;
        }

        private void ProcessCount(string data)
        {
            if (data.StartsWith("qlo") && CurrentScanState == ScanState.InitialScan)
            {
                Instruct("Please scan an item to adjust the stock");
            }
            else if (data.Length == 7)
            {
                Instruct("Please select a packsize");
            }
            else
            {

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
                        var infoblock = new TextBlock();
                        infoblock.Text = "Active item:" + Environment.NewLine + ActiveItem.Title.Label + "  " + ActiveItem.SKU;
                        infoblock.FontSize = 36.0;
                        ItemDetailsStackPanel.Children.Add(infoblock);
                        InitialLocationId = 0;
                        CurrentScanState = ScanState.ScannedItem;
                    }
                    else if (data.Length == 7 && data.StartsWith("10"))
                    {
                        
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
                        Instruct("Please scan the original Location");
                    }
                    break;
                case ScanState.ScannedOriginShelf:
                    NewLocationId = Convert.ToInt32(data.Replace("qlo", ""));
                    CurrentScanState = ScanState.ScannedNewShelf;
                    if (ActiveItem != null && NewLocationId != 0 && InitialLocationId != 0)
                    {
                        var stockCheck = new StockEntry();
                        stockCheck.ShowDialog();
                        var amount = stockCheck.FinalStockEntry;
                        //ActiveItem.AdjustLocationWithAudit(InitialLocationId, AuthdEmployee, amount, NewLocationId);
                        HistoryBlock.Text += "Moved " + amount.ToString() + " of " + ActiveItem.SKU + Environment.NewLine + "From " + LocationIdConversion(InitialLocationId) + " to " + LocationIdConversion(NewLocationId) + Environment.NewLine + "======================";
                        ActiveItem = null;
                        NewLocationId = 0;
                        InitialLocationId = 0;
                        CurrentScanState = ScanState.InitialScan;
                        Instruct("Item Moved. Please scan a new item");
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

                    break;
                default:
                    throw new NullReferenceException("The ScanState enum was invalid");
            }

        }

        private void ProcessView(string data)
        {
            if (data.StartsWith("qlo") )
            {
                ItemDetailsStackPanel.Children.Clear();
                var dict = MySQL.SelectDataDictionary("SELECT * FROM whldata.sku_locations WHERE LocationRefID='"+ data.Replace("qlo","") + "';");
                
                
                foreach (var result in dict)
                {
                    var onShelf = new TextBlock();
                    var searchColl = FullSkuCollection.SearchBarcodes(result["Sku"].ToString());
                    var item = searchColl[0];
                    onShelf.Text += item.SKU + " " + item.Title.Label;
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
                    item.RefreshLocations();
                    Instruct(searchcoll[0].SKU + " " + searchcoll[0].Title.Label);
                    foreach (var loc in item.Locations)
                    {
                        if (loc.LocationType == SKULocation.SKULocationType.Pickable)
                        {
                            PickLocationsBlock.Text += loc.LocationText + ", ";
                        }
                        else
                        {
                            OtherLocationsBlock.Text += loc.LocationText + ", ";
                        }
                        PickLocationsBlock.Text = PickLocationsBlock.Text.Trim().TrimEnd(',');
                        OtherLocationsBlock.Text = OtherLocationsBlock.Text.Trim().TrimEnd(',');
                    }

                    
                }
                else if (searchcoll.Count > 1)
                {
                    ItemDetailsStackPanel.Children.Clear();
                    var currentItem = searchcoll[0];
                    Instruct(currentItem.Title.Label);
                    foreach (var result in searchcoll)
                    {
                        var onShelf = new TextBlock();
                        onShelf.Text += result.SKU + " " + result.Title.Label;
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
            UpdateMode_Tick(null, null);
            ScanBox.Focus();
        }

        private void CountButton_Click(object sender, RoutedEventArgs e)
        {
            var currentButton = sender as Button;
            if (currentButton != null) currentButton.IsEnabled = false;
            LocationButton.IsEnabled = true;
            ViewButton.IsEnabled = true;
            CurrentSelectedMode = Mode.Count;
            UpdateMode_Tick(null, null);
            ScanBox.Focus();
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            var currentButton = sender as Button;
            if (currentButton != null) currentButton.IsEnabled = false;
            CountButton.IsEnabled = true;
            LocationButton.IsEnabled = true;
            CurrentSelectedMode = Mode.View;
            UpdateMode_Tick(null, null);
            ScanBox.Focus();
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        public string LocationIdConversion(int location)
        {
            try
            {
                var LocationString = "";
                var locObject = MySQL.SelectData("SELECT locText FROM whldata.locationreference WHERE locID=" + location.ToString() + " LIMIT 1;") as ArrayList;
                if (locObject != null && locObject.Count > 0)
                {
                    foreach (ArrayList locList in locObject)
                    {
                        LocationString = locList[0].ToString();
                    }
                }
                else
                {
                    throw new NullReferenceException();
                }

                return LocationString;
            }
            catch (Exception)
            {
                return "Could not find shelf name";
            }

        }
    }
}
