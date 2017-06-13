using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WHLClasses;
namespace LocationModifier2.Dialogs
{
    /// <summary>
    /// Interaction logic for StockEntry2.xaml
    /// </summary>
    public partial class StockEntry2
    {
        internal WhlSKU ActiveItem;
        internal int ActiveLocation;
        internal int NewLocation;
        internal ButtonType CurrentState;
        internal int FinalStockEntry;
        internal bool Cancel;
        public StockEntry2(WhlSKU sku,int locationId,ButtonType buttonState)
        {
            InitializeComponent();
            ScanBox.Focus();
            CurrentState = buttonState;
            ActiveLocation = locationId;
            ActiveItem = sku;
            NewLocation = -1;
            switch (CurrentState)
            {
                case ButtonType.SetStock:
                    SetStock.IsEnabled = false;
                    MoveAllStock.IsEnabled = true;
                    MoveSomeStock.IsEnabled = true;
                    AddButton.IsEnabled = true;
                    Minus.IsEnabled = true;
                    this.Background = new SolidColorBrush(Color.FromRgb(14, 0, 153));
                    break;
                case ButtonType.MoveSomeStock:
                        CurrentState = ButtonType.SetStock;
                        SetStock.IsEnabled = false;
                        MoveAllStock.IsEnabled = true;
                        MoveSomeStock.IsEnabled = true;
                        AddButton.IsEnabled = true;
                        Minus.IsEnabled = true;
                        this.Background = new SolidColorBrush(Color.FromRgb(14, 0, 153));
                    break;
                case ButtonType.MoveAllStock:
                        CurrentState = ButtonType.SetStock;
                        SetStock.IsEnabled = false;
                        MoveAllStock.IsEnabled = true;
                        MoveSomeStock.IsEnabled = true;
                        AddButton.IsEnabled = true;
                        Minus.IsEnabled = true;
                        this.Background = new SolidColorBrush(Color.FromRgb(14, 0, 153));
                    break;
                case ButtonType.Add:
                    SetStock.IsEnabled = true;
                    MoveAllStock.IsEnabled = true;
                    MoveSomeStock.IsEnabled = true;
                    AddButton.IsEnabled = false;
                    Minus.IsEnabled = true;
                    this.Background = System.Windows.Media.Brushes.DarkGreen;
                    break;
                case ButtonType.Minus:
                    SetStock.IsEnabled = true;
                    MoveAllStock.IsEnabled = true;
                    MoveSomeStock.IsEnabled = true;
                    AddButton.IsEnabled = true;
                    Minus.IsEnabled = false;
                    this.Background = System.Windows.Media.Brushes.DarkRed;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public enum ButtonType
        {
            SetStock = 1,
            MoveSomeStock = 2,
            MoveAllStock = 3,
            Add = 4,
            Minus = 5
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Cancel = true;
            this.Close();
        }

        private void KeypadEnter_Click(object sender, RoutedEventArgs e)
        {
            if (ScanBox.Text.Length == 0 && CurrentState != ButtonType.MoveAllStock) return;
            if (CurrentState == ButtonType.SetStock)
            {
                FinalStockEntry = Convert.ToInt32(ScanBox.Text);
                this.Close();
            }
            else if ( CurrentState == ButtonType.MoveSomeStock && NewLocation != -1 && ScanBox.Text.Length != 0)
            {
                FinalStockEntry = Convert.ToInt32(ScanBox.Text);
                this.Close();
            }
            else if (CurrentState == ButtonType.MoveAllStock  && NewLocation != -1 )
            {
                this.Close();
            }
            else if (CurrentState == ButtonType.Add || CurrentState == ButtonType.Minus)
            {
                FinalStockEntry = Convert.ToInt32(ScanBox.Text);
                this.Close();
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ScanBox.Text = "";
            ScanBox.Focus();
        }

        private void Keypad_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null) ScanBox.Text += button.Content;
            ScanBox.Focus();
        }

        private void SetStock_Click(object sender, RoutedEventArgs e)
        {
            CurrentState = ButtonType.SetStock;
            SetStock.IsEnabled = false;
            MoveAllStock.IsEnabled = true;
            MoveSomeStock.IsEnabled = true;
            AddButton.IsEnabled = true;
            Minus.IsEnabled = true;
            ResetMovePanel();
        }

        private void MoveSomeStock_Click(object sender, RoutedEventArgs e)
        {
            CurrentState = ButtonType.MoveSomeStock;
            SetStock.IsEnabled = true;
            MoveAllStock.IsEnabled = true;
            MoveSomeStock.IsEnabled = false;
            SetupMovePanel();
        }

        private void MoveAllStock_Click(object sender, RoutedEventArgs e)
        {
            CurrentState = ButtonType.MoveAllStock;
            SetStock.IsEnabled = true;
            MoveAllStock.IsEnabled = false;
            MoveSomeStock.IsEnabled = true;
            SetupMovePanel();
        }

        private void SetupMovePanel()
        {
            ActiveItem.RefreshLocations();
            ButtonPanel.Children.Clear();
            foreach (var loc in ActiveItem.Locations.Where(x => x.LocationID != ActiveLocation))
            {
                var control = new Button
                {
                    Margin = new Thickness(2.0),
                    Content = loc.LocationText,
                    FontSize = 18.0,
                    Uid = loc.LocationID.ToString()
                   
                };
                control.Click += Control_Click;
                ButtonPanel.Children.Add(control);
            }
            var cancel = new Button
            {
                Margin = new Thickness(2.0),
                Content = "Cancel",
                FontSize = 18.0
            };
            cancel.Click += Cancel_Click;
            ButtonPanel.Children.Add(cancel);

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ResetMovePanel();
        }

        private void Control_Click(object sender, RoutedEventArgs e)
        {
            var refCont = sender as Button;
            if (refCont == null) return;
            NewLocation = Convert.ToInt32(refCont.Uid);
            switch (CurrentState)
            {
                case ButtonType.MoveSomeStock:
                    LoginTitle.Text = "Enter the stock you wish to move to " + WHLClasses.MiscFunctions.Misc.LocationIdConversion(NewLocation);
                    LoginTitle.FontSize = 20.0;
                    break;
                case ButtonType.MoveAllStock:
                    KeypadEnter_Click(null,null);
                    break;
                case ButtonType.SetStock:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ResetMovePanel()
        {
            ButtonPanel.Children.Clear();
            ButtonPanel.Children.Add(SetStock);
            ButtonPanel.Children.Add(MoveAllStock);
            ButtonPanel.Children.Add(MoveSomeStock);
            LoginTitle.FontSize = 24.0;

            CurrentState = ButtonType.SetStock;
            SetStock.IsEnabled = false;
            MoveAllStock.IsEnabled = true;
            MoveSomeStock.IsEnabled = true;
            Minus.IsEnabled = true;
            AddButton.IsEnabled = true;
            this.Background = new SolidColorBrush(Color.FromRgb(14,0,153));
        }

        private void Minus_Click(object sender, RoutedEventArgs e)
        {
            this.Background = System.Windows.Media.Brushes.DarkRed;
            CurrentState = ButtonType.Minus;
            SetStock.IsEnabled = true;
            MoveAllStock.IsEnabled = true;
            MoveSomeStock.IsEnabled = true;
            AddButton.IsEnabled = true;
            Minus.IsEnabled = false;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            this.Background = System.Windows.Media.Brushes.DarkGreen;
            CurrentState = ButtonType.Add;
            SetStock.IsEnabled = true;
            MoveAllStock.IsEnabled = true;
            MoveSomeStock.IsEnabled = true;
            AddButton.IsEnabled = false;
            Minus.IsEnabled = true;
        }
    }
}
