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
using WHLClasses;

namespace LocationModifier2.Dialogs
{
    /// <summary>
    /// Interaction logic for PacksizeSelector.xaml
    /// </summary>
    public partial class PacksizeSelector : Window
    {
        public WhlSKU SelectedSku;
        public SkuCollection skucoll;
        public PacksizeSelector(SkuCollection skus)
        {
            skucoll = skus;
            InitializeComponent();
            foreach (var sku in skus)
            {
                var refctrl = new Button();
                refctrl.Click += Refctrl_Click;
                refctrl.Uid = sku.SKU;
                refctrl.Content = sku.Title.Invoice;
                UniformPacksizeGrid.Children.Add(refctrl);
            }
        }

        private void Refctrl_Click(object sender, RoutedEventArgs e)
        {
            var ctrl = sender as Button;
            try
            {
                SelectedSku = skucoll.SearchBarcodes(ctrl.Uid)[0];
                if (SelectedSku.SKU == ctrl.Uid)
                {
                    this.Close();
                }
                else throw new NullReferenceException();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                LoginTitle.Text = "Please try again";
            }
        }
    }
}
