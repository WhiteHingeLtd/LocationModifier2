using LocationModifier2.Dialogs;
using System;
using System.Diagnostics;
using System.Linq;
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
        public IssueControl(Order order, IssueData currentIssue, IssuesList dialog)
        {

            InitializeComponent();
            IssueListDialog = dialog;
            CurrentIssueData = currentIssue;
            CurrentOrder = order;
            OrderNumText.Text = order.OrderId;

            TimeText.Text = currentIssue.TimeReported.ToString("HH:mm:ss");
            OrderNumText.Text = currentIssue.Reason;
            var Badsku = FindCorrectSku(IssueListDialog.IwRef.OldMw.FullSkuCollection, currentIssue.DodgySku);
            //var Badsku2 = IssueListDialog.IwRef.OldMw.FullSkuCollection.SearchSKUS(currentIssue.DodgySku)[0];
            CurrentSku = Badsku;

            if (Badsku.Locations.Any(x => x.LocationType == SKULocation.SKULocationType.Prepack ||
                                          x.LocationType == SKULocation.SKULocationType.PrepackInstant))
                nameText.Text = "Prepackable";

            MessageText.Text = Badsku.GetLocation(SKULocation.SKULocationType.Pickable).LocationText + ": " +
                               Badsku.Title.Label;

            Pickroute = Badsku.GetLocation(SKULocation.SKULocationType.Pickable).LocationID;

        }

        private void MainIssueSourceButton_Click(object sender, RoutedEventArgs e)
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
                    ResetToPrepack(CurrentOrder);
                    break;
                case IssuesList.IssueResolution.ResetOrder:
                    ResetOrder(CurrentOrder);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
                
            
         
        }
        private void ResetToPrepack(Order order)
        {
            var loader = new GenericDataController();
            var ordex = loader.LoadOrdex(order.Filename);
            if (ordex.Status == OrderStatus._Withdrawn)
            {

            }
            else
            {
                try
                {
                    var currentIssue = ordex.issues.Single(x => x.IssueItemIndex == CurrentIssueData.IssueItemIndex);
                    ordex.AddIssue2017(OrderStatus._Prepack,currentIssue.IssueItemIndex, CurrentSku, IssueListDialog.IwRef.OldMw.AuthdEmployee,ordex.PickingType,currentIssue.Quantity);
                    ordex.SetStatus(OrderStatus._Prepack, IssueListDialog.IwRef.OldMw.AuthdEmployee);
                }
                catch (Exception e)
                {
                    WHLClasses.Reporting.ErrorReporting.ReportException(e);
                    throw;
                }
               
            }
            loader.SaveDataToFile(ordex.LinnOpenOrder.NumOrderId.ToString() + ".ordex",ordex,@"T:\AppData\Orders");
            new MsgDialog("Prepacked", "Order sent to prepack").ShowDialog();
        }

        private void ResetOrder(Order order)
        {
            var loader = new GenericDataController();
            var ordex = loader.LoadOrdex(order.Filename);
            if (ordex.Status == OrderStatus._Withdrawn)
            {

            }
            else
            {
                try
                {
                    foreach (var issue in ordex.issues)
                    {
                        if (issue.IssueItemIndex != CurrentIssueData.IssueItemIndex) continue;
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

        private WhlSKU FindCorrectSku(SkuCollection searchColl, string sku)
        {
            return searchColl.Single(x => x.SKU == sku);
        }
    }
}
