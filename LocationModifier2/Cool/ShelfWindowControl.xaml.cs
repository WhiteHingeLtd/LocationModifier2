using System.Linq;
using System.Windows;
using WHLClasses;

namespace LocationModifier2.Cool
{
    /// <summary>
    /// Interaction logic for ShelfWindowControl.xaml
    /// </summary>
    public partial class ShelfWindowControl 
    {

        internal ShelfWindow SwRef;
        

        public ShelfWindowControl(WhlSKU sku, ShelfWindow window, int LocationID)
        {
            InitializeComponent();
            SwRef = window;
            Shortsku.Text = sku.ShortSku;
            ItemTitle.Text = sku.Title.Label;
            Packsize.Text = "Pack " + sku.PackSize.ToString();

            //Get the location. We can use it for the headers if we need to too.
            var loc = sku.Locations.First(location => location.LocationID == LocationID);
            Stocklevel.Text = loc.Additional;

            if (window.Headers) return;
            window.ItemName.Text = loc.LocationText;
            window.ShortSku.Text = loc.LocationID.ToString();
            window.Headers = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SwRef.ProcessScan(Shortsku.Text);
        }
    }
}
