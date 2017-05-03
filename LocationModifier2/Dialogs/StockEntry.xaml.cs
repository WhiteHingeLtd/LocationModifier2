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
        public int FinalStockEntry;
        public bool Cancel;
        private bool isNegative;
        public StockEntry()
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

        }



        private void Keypad_Click(object sender, RoutedEventArgs e)
        {
            LoginScanBox.Text += (sender as Button).Content;
        }

        private void KeypadEnter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FinalStockEntry = Convert.ToInt32(LoginScanBox.Text);
                if (isNegative) FinalStockEntry *= -1;
                this.Close();
            }
            catch (Exception)
            {
                LoginScanBox.Text = "";
            }

        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            LoginScanBox.Text = "";
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            isNegative = true;
            RemoveButton.IsEnabled = false;
            AddButton.IsEnabled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            isNegative = false;
            AddButton.IsEnabled = false;
            RemoveButton.IsEnabled = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Cancel = true;
            this.Close();
        }
    }
}
