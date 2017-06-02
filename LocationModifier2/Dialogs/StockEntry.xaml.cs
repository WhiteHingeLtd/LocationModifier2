using System;
using System.Windows;
using System.Windows.Controls;

namespace LocationModifier2.Dialogs
{
    /// <summary>
    /// Interaction logic for StockEntry.xaml
    /// </summary>
    public partial class StockEntry
    {
        public enum AddRemoveSet
        {
            Add = 0,
            Remove = 1,
            Set = 2

        }
        public AddRemoveSet CurrentSet = AddRemoveSet.Add;
        public int FinalStockEntry;
        public bool Cancel;
        private bool _isNegative;
        public StockEntry(bool disableremove = false)
        {
            InitializeComponent();
            AddButton.IsEnabled = false;
            Keypad1.Click += Keypad_Click;
            Keypad2.Click += Keypad_Click;
            Keypad3.Click += Keypad_Click;
            Keypad4.Click += Keypad_Click;
            Keypad5.Click += Keypad_Click;
            Keypad6.Click += Keypad_Click;
            Keypad7.Click += Keypad_Click;
            Keypad8.Click += Keypad_Click;
            Keypad9.Click += Keypad_Click;
            Keypad0.Click += Keypad_Click;
            RemoveButton.IsEnabled = disableremove;
            SetButton.IsEnabled = disableremove;
        }



        private void Keypad_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null) LoginScanBox.Text += button.Content;
        }

        private void KeypadEnter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FinalStockEntry = Convert.ToInt32(LoginScanBox.Text);
                if (_isNegative) FinalStockEntry *= -1;
                this.Close();
            }
            catch (Exception)
            {
                LoginScanBox.Text = "";
                LoginTitle.Text = "Please try again";
            }

        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            LoginScanBox.Text = "";
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentSet = AddRemoveSet.Remove;
            _isNegative = true;
            RemoveButton.IsEnabled = false;
            AddButton.IsEnabled = true;
            SetButton.IsEnabled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CurrentSet = AddRemoveSet.Add;
            _isNegative = false;
            AddButton.IsEnabled = false;
            RemoveButton.IsEnabled = true;
            SetButton.IsEnabled = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Cancel = true;
            this.Close();
        }

        private void SetButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentSet = AddRemoveSet.Set;
            _isNegative = false;
            AddButton.IsEnabled = true;
            RemoveButton.IsEnabled = true;
            SetButton.IsEnabled = false;
        }
    }
}
