using LocationModifier2.Dialogs;
using System;
using System.Windows;
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
        internal MainWindow MainRefWindow;
        public ButtonControl(WhlSKU sku, MainWindow MainRef,int locationId )
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
                    ActiveItem.AdjustStockWithAudit(LocationID, MainRefWindow.AuthdEmployee, stockCounter.FinalStockEntry);
                    MainButton.Content = stockCounter.FinalStockEntry;
                }
                catch (Exception)
                {
                }
            }

            

        }
    }
}
