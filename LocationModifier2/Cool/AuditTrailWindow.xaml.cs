using System.Collections.Generic;
using System.Windows;
using WHLClasses.SQL;

namespace LocationModifier2.Cool
{
    /// <summary>
    /// Interaction logic for AuditTrailWindow.xaml
    /// </summary>
    public partial class AuditTrailWindow : Window
    {
        private ItemWindow iwRef;
        public AuditTrailWindow(string Shortsku, string Title,ItemWindow main)
        {
            InitializeComponent();

            ItemName.Text = Title;
            iwRef = main;
            //Get teh data and iterate.
            var data =
                MySQL_New.GetData(
                    "SELECT Shortsku, CAST(SUBSTRING(Shortsku,8,4)as signed integer) as packsize, LocationID, loctext,  CAST(DateOfEvent as datetime) as DateRecorded, additional as stock, AuditUserID, AuditEvent FROM whldata.locationaudit as a Join whldata.locationreference as b on a.LocationID = b.locID WHERE Shortsku LIKE '"+Shortsku+"%' order by AuditID DESC LIMIT 100; ");
            foreach (Dictionary<string, object> row in data)
            {
                ActualAuditContainer.Children.Add(new AuditControl(row));
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            iwRef.Refocus();
        }
    }
}
