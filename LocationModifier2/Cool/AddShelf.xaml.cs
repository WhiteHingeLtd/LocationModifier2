using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WHLClasses;

namespace LocationModifier2.Cool
{
    /// <summary>
    /// Interaction logic for AddShelf.xaml
    /// </summary>
    public partial class AddShelf : Window
    {
        internal ItemWindow IwRef;
        internal WhlSKU ActiveItem;
        internal SkuCollection ActiveCollection;
        public AddShelf(ItemWindow Ref, WhlSKU sku,SkuCollection resultColl)
        {
            InitializeComponent();
            ActiveItem = sku;
            ActiveCollection = resultColl;
            IwRef = Ref;
            Instruct("Scan a new shelf location");
            Refocus();
        }

        private void ScanBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ScanBox.Background = Brushes.PaleGreen;
        }

        private void ScanBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ScanBox.Background = Brushes.LightCoral;
        }

        private void ProcessScan(string data)
        {
            if (data.StartsWith("qlo"))
            {
                var newdata = Convert.ToInt32(data.Replace("qlo", ""));
                var shelfname = IwRef._OldMW.LocationIdConversion(newdata);
                Instruct("Adding to " + shelfname);
                foreach (var sku in ActiveCollection)
                {
                    sku.AddLocationWithAudit(newdata,IwRef._OldMW.AuthdEmployee,0);
                }
                this.Close();
                IwRef.ProcessScan(ActiveItem.ShortSku);
            }
            else
            {
                Instruct("Please scan a valid shelf location");
            }
        }
        private void ScanBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessScan(ScanBox.Text);
                Refocus();
            }
        }
        #region StepThroughs
        [DebuggerStepThrough]
        private void Instruct(string text)
        {
            InstructBox.Text = text;
        }
        [DebuggerStepThrough]
        private void Refocus()
        {
            ScanBox.Text = "";
            ScanBox.Focus();
        }
        #endregion


    }
}
