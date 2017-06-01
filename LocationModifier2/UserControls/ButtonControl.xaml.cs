using LocationModifier2.Dialogs;
using System;
using System.Windows;
using LocationModifier2.Cool;
using WHLClasses;
namespace LocationModifier2.UserControls
{

    /// <summary>
    /// Interaction logic for ButtonControl.xaml
    /// </summary>
    public partial class ButtonControl
    {
        internal WhlSKU ActiveItem;
        internal int LocationID;
        internal ItemWindow MainRefWindow;
        public ButtonControl(WhlSKU sku, ItemWindow MainRef,int locationId )
        {
            InitializeComponent();
            MainRefWindow = MainRef;
            LocationID = locationId;
            ActiveItem = sku;

        }


        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            var stockCounter = new StockEntry();
            stockCounter.ShowDialog();
            if (stockCounter.FinalStockEntry > 0)
            {
                try
                {
                    ActiveItem.AdjustStockWithAudit(LocationID, MainRefWindow._OldMW.AuthdEmployee, stockCounter.FinalStockEntry);
                    MainButton.Content = stockCounter.FinalStockEntry;
                }
                catch (Exception)
                {
                }
            }

            

        }
    }
}
