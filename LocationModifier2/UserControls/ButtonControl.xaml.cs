using LocationModifier2.Dialogs;
using System;
using System.Linq;
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
            try
            {
                var select =
                    (from loc in sku.Locations where loc.LocationID == locationId select loc.Additional).Single();
                MainButton.Content = select;
            }
            catch (InvalidOperationException)
            {
                MainButton.Content = 0;
            }

            
        }


        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            var stockCounter = new StockEntry2(ActiveItem,LocationID);
            stockCounter.ShowDialog();
            if (stockCounter.FinalStockEntry > -1 && !stockCounter.Cancel)
            {
                switch (stockCounter.CurrentState)
                {
                    case StockEntry2.ButtonType.SetStock:
                        try
                        {
                            ActiveItem.SetLocationStockWithAudit(LocationID, MainRefWindow.OldMw.AuthdEmployee, stockCounter.FinalStockEntry);
                            MainButton.Content = stockCounter.FinalStockEntry;
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        finally
                        {
                            MainRefWindow.Refocus();
                        }
                        break;
                    case StockEntry2.ButtonType.MoveSomeStock:
                        try
                        {
                            ActiveItem.AdjustLocationWithAudit(LocationID, MainRefWindow.OldMw.AuthdEmployee, stockCounter.FinalStockEntry,stockCounter.NewLocation);
                            MainButton.Content = stockCounter.FinalStockEntry;
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        finally
                        {
                            MainRefWindow.ProcessScan(ActiveItem.ShortSku);
                            MainRefWindow.Refocus();
                        }
                        break;
                    case StockEntry2.ButtonType.MoveAllStock:
                        try
                        { 
                            ActiveItem.MoveAllBetweenLocations(LocationID, MainRefWindow.OldMw.AuthdEmployee, stockCounter.NewLocation);
                            MainButton.Content = stockCounter.FinalStockEntry;
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        finally
                        {
                            MainRefWindow.ProcessScan(ActiveItem.SKU);
                            MainRefWindow.Refocus();
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
            }
        }
    }
}
