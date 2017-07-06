using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WHLClasses;
using WHLClasses.MySQL_Old;

namespace LocationModifier2.Cool
{
    /// <summary>
    /// Interaction logic for AddShelf.xaml
    /// </summary>
    public partial class AddShelf
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
                var shelfname = IwRef.OldMw.LocationIdConversion(newdata);
                Instruct("Adding to " + shelfname);
                foreach (var sku in ActiveCollection)
                {
                    if ((from loc in sku.Locations where loc.LocationID == newdata select loc).Any()) continue;
                    sku.AddLocationWithAudit(newdata,IwRef.OldMw.AuthdEmployee,0, "Location Modifier");
                }
                this.Close();
                IwRef.ProcessScan(ActiveItem.ShortSku);
            }
            else
            {
                try
                {
                    var newdata = LocationNameConversion(data);
                    if (newdata == -1) throw new Exception();
                    var shelfname = IwRef.OldMw.LocationIdConversion(newdata);
                    Instruct("Adding to " + shelfname);
                    foreach (var sku in ActiveCollection)
                    {
                        if ((from loc in sku.Locations where loc.LocationID == newdata select loc).Any()) continue;
                        sku.AddLocationWithAudit(newdata, IwRef.OldMw.AuthdEmployee, 0, "Location Modifier");
                    }
                    this.Close();
                    IwRef.ProcessScan(ActiveItem.ShortSku);
                }
                catch (Exception)
                {
                    Instruct("Please scan a valid shelf location");
                }

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
        /// <summary>
        /// Cancel Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            IwRef.ProcessScan(ActiveItem.ShortSku);
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
    }
}
