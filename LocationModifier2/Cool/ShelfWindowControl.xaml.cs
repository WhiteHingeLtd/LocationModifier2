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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WHLClasses;

namespace LocationModifier2.Cool
{
    /// <summary>
    /// Interaction logic for ShelfWindowControl.xaml
    /// </summary>
    public partial class ShelfWindowControl : UserControl
    {

        internal ShelfWindow _window = null;
        

        public ShelfWindowControl(WhlSKU sku, ShelfWindow window, int LocationID)
        {
            InitializeComponent();
            _window = window;
            Shortsku.Text = sku.ShortSku;
            ItemTitle.Text = sku.Title.Label;
            Packsize.Text = "Pack " + sku.PackSize.ToString();

            //Get the location. We can use it for the headers if we need to too.
            SKULocation loc = sku.Locations.First(location => location.LocationID == LocationID);
            Stocklevel.Text = loc.Additional;

            if (!window.Headers)
            {
                window.ItemName.Text = loc.LocationText;
                window.ShortSku.Text = loc.LocationID.ToString();
                window.Headers = true;

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _window.ProcessScan(Shortsku.Text);
        }
    }
}
