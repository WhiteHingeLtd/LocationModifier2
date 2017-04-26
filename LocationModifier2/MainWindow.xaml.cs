using System;
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
        private DispatcherTimer _updateMode = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();
            FullSkuCollection = Loader.SmartSkuCollLoad(true);
            _updateMode.Interval = new TimeSpan(0,0,0,500);
            _updateMode.Tick += UpdateMode_Tick;
            CurrentSelectedMode = Mode.View;
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

        public enum Mode
        {
            InitialValue = 0,
            Location = 1,
            View = 2,
            Count = 3,
            
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

        private void ScanBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                e.Handled = true;
                try
                {
                    ProcessScanBox((sender as TextBox).Text);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
                finally
                {
                    (sender as TextBox).Text = "";
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
            
        }

        private void ProcessLocation(string data)
        {
            
        }

        private void ProcessView(string data)
        {
            if (data.StartsWith("qlo"))
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
            else if (data.StartsWith("10"))
            {
                var searchcoll = FullSkuCollection.SearchBarcodes(data);
                PickLocationsBlock.Text = "";
                OtherLocationsBlock.Text = "";
                if (searchcoll.Count == 1)
                {
                    var item = searchcoll[0];
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
                    var infoBlock = new TextBlock();
                    
                }
                else if (searchcoll.Count > 1)
                {
                    
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
        }

        private void CountButton_Click(object sender, RoutedEventArgs e)
        {
            var currentButton = sender as Button;
            if (currentButton != null) currentButton.IsEnabled = false;
            LocationButton.IsEnabled = true;
            ViewButton.IsEnabled = true;
            CurrentSelectedMode = Mode.Count;
            UpdateMode_Tick(null, null);
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            var currentButton = sender as Button;
            if (currentButton != null) currentButton.IsEnabled = false;
            CountButton.IsEnabled = true;
            LocationButton.IsEnabled = true;
            CurrentSelectedMode = Mode.View;
            UpdateMode_Tick(null, null);
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
