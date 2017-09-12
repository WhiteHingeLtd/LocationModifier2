using LocationModifier2.Dialogs;
using System;
using System.Linq;
using System.ServiceModel.Configuration;
using System.Windows;
using LocationModifier2.Cool;
using WHLClasses;
using WHLClasses.Exceptions;

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
            var stockCounter = new StockEntry2(ActiveItem,LocationID,MainRefWindow.CurrentButtonType);
            stockCounter.ShowDialog();
            if (stockCounter.FinalStockEntry > -1 && !stockCounter.Cancel)
            {
                MainRefWindow.CurrentButtonType = stockCounter.CurrentState;
                switch (stockCounter.CurrentState)
                {
                    case StockEntry2.ButtonType.SetStock:
                        try
                        {
                            ActiveItem.SetLocationStockWithAudit(LocationID, MainRefWindow.OldMw.AuthdEmployee, stockCounter.FinalStockEntry, "Location Modifier");
                            if (ActiveItem.PackSize == 1 && stockCounter.FinalStockEntry == 0)
                            {
                                var rootItem = MainRefWindow.OldMw.FullSkuCollection.GatherChildren(ActiveItem.ShortSku)
                                    .Where(x => x.PackSize == 0).ToList();
                                if (rootItem.Count == 1)
                                {
                                    rootItem[0].SetLocationStockWithAudit(LocationID, MainRefWindow.OldMw.AuthdEmployee,
                                        stockCounter.FinalStockEntry, "Location Modifier");
                                }
                            }
                            MainButton.Content = stockCounter.FinalStockEntry;
                        }
                        catch (Exception ex)
                        {
                            // ignored
                        }
                        finally
                        {
                            MainRefWindow.ProcessScan(ActiveItem.ShortSku);
                            MainRefWindow.Refocus();
                        }
                        break;
                    case StockEntry2.ButtonType.MoveSomeStock:
                        try
                        {
                            ActiveItem.MoveLocationWithAudit(LocationID, MainRefWindow.OldMw.AuthdEmployee, stockCounter.FinalStockEntry,stockCounter.NewLocation, "Location Modifier");
                            MainButton.Content = stockCounter.FinalStockEntry;
                        }
                        catch (Exception ex)
                        {
                            Console.Write(ex.Message);
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
                            try
                            {
                                if (ActiveItem.Locations.Single(x => x.LocationID == LocationID).LocationType !=
                                    SKULocation.SKULocationType.Pickable)
                                {
                                    ActiveItem.MoveAllBetweenLocations(LocationID, MainRefWindow.OldMw.AuthdEmployee,stockCounter.NewLocation, "Location Modifier");
                                }
                                else
                                {
                                    var currentLoc = ActiveItem.Locations.Single(x => x.LocationID == LocationID);
                                    ActiveItem.MoveLocationWithAudit(LocationID, MainRefWindow.OldMw.AuthdEmployee,Convert.ToInt32(currentLoc.Additional),stockCounter.NewLocation, "Location Modifier");
                                }
                                    
                            }
                            catch (NegativeStockException)
                            {
                                throw;
                            }
                            catch (Exception)
                            {
                                ActiveItem.MoveAllBetweenLocations(LocationID, MainRefWindow.OldMw.AuthdEmployee,stockCounter.NewLocation, "Location Modifier");
                            }
                            
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
                    case StockEntry2.ButtonType.Add:
                        try
                        {
                            ActiveItem.AdjustStockWithAudit(LocationID, MainRefWindow.OldMw.AuthdEmployee, stockCounter.FinalStockEntry, "Location Modifier");
                            MainButton.Content = stockCounter.FinalStockEntry;
                        }
                        catch (NegativeStockException)
                        {
                            new MsgDialog("ERROR", "You cannot have negative stock").ShowDialog();
                            // ignored
                        }
                        finally
                        {
                            MainRefWindow.ProcessScan(ActiveItem.ShortSku);
                            MainRefWindow.Refocus();
                        }
                        break;
                    case StockEntry2.ButtonType.Minus:
                        try
                        {
                            ActiveItem.AdjustStockWithAudit(LocationID, MainRefWindow.OldMw.AuthdEmployee, stockCounter.FinalStockEntry*-1, "Location Modifier");
                            MainButton.Content = stockCounter.FinalStockEntry;
                        }
                        catch (NegativeStockException)
                        {
                            new MsgDialog("ERROR", "You cannot have negative stock").ShowDialog();
                            // ignored
                        }
                        finally
                        {
                            MainRefWindow.ProcessScan(ActiveItem.ShortSku);
                            MainRefWindow.Refocus();
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            }
            else if (stockCounter.Cancel)// If I cancel, it doesn't auto-refocus. Then I scan something and it acts like I pressed a button
            {                            // The intent here is for this to fix the issue. Please verify, as I don't want to delete the SPC to debug
                MainRefWindow.Refocus(); // - Colin
            }
        }
    }
}
