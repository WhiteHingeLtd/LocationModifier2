using System;
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
using WHLClasses.SQL;

namespace LocationModifier2.Cool
{
    /// <summary>
    /// Interaction logic for AuditTrailWindow.xaml
    /// </summary>
    public partial class AuditTrailWindow : Window
    {
        public AuditTrailWindow(string Shortsku, string Title)
        {
            InitializeComponent();

            ItemName.Text = Title;

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
        }
    }
}
