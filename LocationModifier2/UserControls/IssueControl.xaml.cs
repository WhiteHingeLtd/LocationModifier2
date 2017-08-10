using LocationModifier2.Dialogs;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WHLClasses;
using WHLClasses.Orders;

namespace LocationModifier2.UserControls
{
    /// <summary>
    /// Interaction logic for IssueControl.xaml
    /// </summary>
    public partial class IssueControl : UserControl
    {
        internal IssueData CurrentIssueData;
        internal IssuesList IssueListDialog;
        internal int Pickroute = 0;
        internal Order CurrentOrder;
        internal WhlSKU CurrentSku;
        internal int Warehouse;

        public IssueControl(Order order, IssueData currentIssue, IssuesList dialog)
        {
            InitializeComponent();
            IssueListDialog = dialog;
            CurrentIssueData = currentIssue;
            CurrentOrder = order;


            TimeText.Text = currentIssue.TimeReported.ToString("HH:mm:ss");
            int correctAmount;


            var Badsku = FindCorrectSku(IssueListDialog.IwRef.OldMw.FullSkuCollection, currentIssue.DodgySku);
            try
            {
                var bundleSkus = new SkuCollection(true);
                bundleSkus.AddRange(CurrentOrder.BetterItems.Select(result => FindCorrectSku(IssueListDialog.IwRef.OldMw.FullSkuCollection, result.SKU)));
                correctAmount = bundleSkus.Any(x => x.isBundle) ? CurrentOrder.BetterItems[0].OrderQuantity : CurrentOrder.BetterItems.First(x => x.SKU != null && x.SKU == CurrentIssueData.DodgySku).OrderQuantity;
            }
            catch (Exception)
            {
                correctAmount = 1;
            }
            //var Badsku2 = IssueListDialog.IwRef.OldMw.FullSkuCollection.SearchSKUS(currentIssue.DodgySku)[0];
            OrderNumText.Text = $"{correctAmount} of {Badsku.PackSize}";
            OrderNumText.Text += " " + currentIssue.Reason;
            CurrentSku = Badsku;
            var prepackString = $"Prepackable Needs {CurrentSku.PackSize}";
            if (Badsku.Locations.Any(x => (x.LocationType == SKULocation.SKULocationType.Prepack ||
                                           x.LocationType == SKULocation.SKULocationType.PrepackInstant) &&
                                          x.LocationText.Contains("PP"))) nameText.Text = prepackString;

            MessageText.Text = Badsku.GetLocation(SKULocation.SKULocationType.Pickable).LocationText + ": " +
                               Badsku.Title.Label;

            Pickroute = Badsku.GetLocation(SKULocation.SKULocationType.Pickable).LocationID;
            Warehouse = Badsku.GetLocation(SKULocation.SKULocationType.Pickable).WarehouseID;
        }

        private async void MainIssueSourceButton_Click(object sender, RoutedEventArgs e)
        {
            switch (IssueListDialog.CurrentSelectedResolution)
            {
                case IssuesList.IssueResolution.Default:
                    throw new ArgumentOutOfRangeException();
                case IssuesList.IssueResolution.ViewLocations:

                    IssueListDialog.Close();
                    IssueListDialog.IwRef.ProcessScan(CurrentIssueData.DodgySku);
                    break;
                case IssuesList.IssueResolution.SendToPrepack:
                    var yesnoprepack = new YesNoDialog("Are you sure you want to send this order to prepack?",
                        "Are you sure");
                    yesnoprepack.ShowDialog();
                    if (yesnoprepack.Result) ResetToPrepack(CurrentOrder);
                    break;
                case IssuesList.IssueResolution.ResetOrder:
                    var yesno = new YesNoDialog("Are you sure you want to reset this order?", "Are you sure");
                    yesno.ShowDialog();
                    if (yesno.Result) ResetOrder(CurrentOrder);
                    break;
                case IssuesList.IssueResolution.OtherWarehouse:
                    var yesnother = new YesNoDialog("Are you sure you want mark this order as other Warehouse?",
                        "Are you sure");
                    yesnother.ShowDialog();
                    if (yesnother.Result)
                    {
                        await OtherWarehouse(CurrentOrder);
                        new MsgDialog("Corrected", "Labelled as Other Warehouse").ShowDialog();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ResetToPrepack(Order order)
        {
            var loader = new GenericDataController();
            var ordex = loader.LoadOrdex(order.Filename);
            if (ordex.Status == OrderStatus._Withdrawn) return;
            try
            {
                var currentIssue = ordex.issues.First(x => x.IssueItemIndex == CurrentIssueData.IssueItemIndex);
                foreach (var issue in ordex.issues)
                {
                    issue.Resolved = true;
                }
                ordex.AddIssue2017(OrderStatus._Prepack, currentIssue.IssueItemIndex, CurrentSku,
                    IssueListDialog.IwRef.OldMw.AuthdEmployee, ordex.PickingType, currentIssue.Quantity,
                    "Sent To Prepack");
                ordex.SetStatus(OrderStatus._Prepack, IssueListDialog.IwRef.OldMw.AuthdEmployee);
            }
            catch (Exception e)
            {
                WHLClasses.Reporting.ErrorReporting.ReportException(e);
                throw;
            }
            loader.SaveDataToFile(ordex.LinnOpenOrder.NumOrderId.ToString() + ".ordex", ordex, @"T:\AppData\Orders");
            new MsgDialog("Prepacked", "Order sent to prepack").ShowDialog();
        }

        private void ResetOrder(Order order)
        {
            var loader = new GenericDataController();
            var ordex = loader.LoadOrdex(order.Filename);
            if (ordex.Status != OrderStatus._Withdrawn)
            {
                try
                {
                    foreach (var issue in ordex.issues)
                    {
                        if (issue.IssueItemIndex != CurrentIssueData.IssueItemIndex && issue.DodgySku.Substring(0, 7) ==
                            CurrentIssueData.DodgySku.Substring(0, 7)) continue;
                        issue.Resolved = true;
                        ordex.PickingStrictness[issue.IssueItemIndex] = PickingStrictness.BestJudgement;
                    }
                }
                catch (Exception e)
                {
                    WHLClasses.Reporting.ErrorReporting.ReportException(e);
                    throw;
                }
                if (ordex.issues.All(x => x.Resolved))
                    ordex.SetStatus(OrderStatus._New, IssueListDialog.IwRef.OldMw.AuthdEmployee);
            }
            loader.SaveDataToFile(ordex.LinnOpenOrder.NumOrderId.ToString() + ".ordex", ordex, @"T:\AppData\Orders");
            new MsgDialog("Reset", "Order has been reset").ShowDialog();
        }

        private async Task<bool> OtherWarehouse(Order order)
        {
            var loader = new GenericDataController();
            var ordex = loader.LoadOrdex(order.Filename);
            if (ordex.Status == OrderStatus._Withdrawn) return true;
            try
            {
                var currentIssue = ordex.issues.First(x => x.DodgySku == CurrentIssueData.DodgySku);
                foreach (var issue in ordex.issues.Where(x => x.DodgySku == CurrentIssueData.DodgySku))
                {
                    issue.Resolved = true;
                }

                ordex.SetStatus(OrderStatus._New, IssueListDialog.IwRef.OldMw.AuthdEmployee);
                loader.SaveDataToFile(ordex.LinnOpenOrder.NumOrderId.ToString() + ".ordex", ordex,
                    @"T:\AppData\Orders");
                var delay = Task.Delay(1000);
                Console.WriteLine("Sleeping");
                Thread.Sleep(3000);
                ordex = loader.LoadOrdex(order.Filename);
                ordex.AddIssue2017(OrderStatus._Cantfind, currentIssue.IssueItemIndex, CurrentSku,
                    IssueListDialog.IwRef.OldMw.AuthdEmployee, ordex.PickingType, currentIssue.Quantity,
                    "Item In Other Warehouse (Storage)");
                ordex.SetStatus(OrderStatus._Cantfind, IssueListDialog.IwRef.OldMw.AuthdEmployee);
                await delay;
                loader.SaveDataToFile(ordex.LinnOpenOrder.NumOrderId.ToString() + ".ordex", ordex,
                    @"T:\AppData\Orders");
                return true;
            }
            catch (Exception e)
            {
                WHLClasses.Reporting.ErrorReporting.ReportException(e);
                return false;
            }
        }

        private WhlSKU FindCorrectSku(SkuCollection searchColl, string sku)
        {
            return searchColl.Single(x => x.SKU == sku);
        }
    }
}
