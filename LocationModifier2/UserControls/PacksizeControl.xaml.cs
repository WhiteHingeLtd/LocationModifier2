using LocationModifier2.Cool;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using WHLClasses;

namespace LocationModifier2.UserControls
{
    /// <summary>
    /// Interaction logic for PacksizeControl.xaml
    /// </summary>
    public partial class PacksizeControl
    {
        internal WhlSKU ActiveItem;
        internal ItemWindow MainWindowRef;
        internal List<int> LocList;
        public PacksizeControl(List<int> locationList,WhlSKU sku, ItemWindow main)
        {
            InitializeComponent();
            ActiveItem = sku;
            MainWindowRef = main;
            LocList = locationList;
            PacksizeBlock.Text = sku.PackSize.ToString();
            this.Background = new SolidColorBrush(Colors.LightGray);
            foreach (var loc in locationList)
            {
                var refControl = new ButtonControl(ActiveItem,MainWindowRef,loc);
                ExpandPanel.Children.Add(refControl);
            }
            var result = MySQL.SelectDataDictionary("SELECT ow_isprepackfinalfinal from whldata.orderwise_data where sku='" +
                                       ActiveItem.SKU + "'")[0];
            IsPrepackButton.IsChecked = bool.Parse(result["ow_isprepackfinalfinal"].ToString());
            IsPrepackButton.Content = "Pack Down: " + IsPrepackButton.IsChecked.Value.ToString();
            this.Background = IsPrepackButton.IsChecked.Value ? new SolidColorBrush(Colors.LightBlue) : new SolidColorBrush(Colors.LightGray);
        }

        private void IsPrepackButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (IsPrepackButton.IsChecked == null) return;
            MySQL.insertUpdate("UPDATE whldata.orderwise_data set ow_isprepackfinalfinal = '" + IsPrepackButton.IsChecked.Value.ToString() + "' where sku ='" + ActiveItem.SKU + "'");
            IsPrepackButton.Content = "Prepack: " + IsPrepackButton.IsChecked.Value.ToString();
            if (IsPrepackButton.IsChecked.Value)
            {
                this.Background = new SolidColorBrush(Colors.LightBlue);
            }
            else
            {
                this.Background = new SolidColorBrush(Colors.LightGray);
            }
            MainWindowRef.Refocus();
        }
    }
}
