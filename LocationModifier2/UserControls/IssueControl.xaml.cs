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
using LocationModifier2.Dialogs;
using WHLClasses;
using WHLClasses.Orders;

namespace LocationModifier2.UserControls
{
    /// <summary>
    /// Interaction logic for IssueControl.xaml
    /// </summary>
    public partial class IssueControl : UserControl
    {
        internal IssueData Issue;
        internal IssuesList IssueListDialog;
        internal int Pickroute = 0;
        internal Order CurrentOrder;
        internal WhlSKU CurrentSku;
        public IssueControl(Order order, IssueData issue, IssuesList dialog)
        {
            InitializeComponent();
            IssueListDialog = dialog;
            Issue = issue;
            CurrentOrder = order;
            OrderNumText.Text = order.OrderId;
            TimeText.Text = issue.TimeReported.ToString("HH:mm:ss");
            OrderNumText.Text = issue.Reason;
            var Badsku = IssueListDialog.IwRef.OldMw.FullSkuCollection.SearchSKUS(issue.DodgySku)[0];
            CurrentSku = Badsku;
            MessageText.Text = Badsku.GetLocation(SKULocation.SKULocationType.Pickable).LocationText + ": " +
                               Badsku.Title.Label;
            Pickroute = Badsku.GetLocation(SKULocation.SKULocationType.Pickable).LocationID;
        }

        private void MainIssueSourceButton_Click(object sender, RoutedEventArgs e)
        {
            if (IssueListDialog.SendingToPrepack)
            {
                SendToPrepack(CurrentOrder);
            }
            else
            {
                IssueListDialog.Close();
                IssueListDialog.IwRef.ProcessScan(Issue.DodgySku);
            }
         
        }
        private void SendToPrepack(Order order)
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
                    var currentIssue = ordex.issues.Single(x => x.IssueItemIndex == Issue.IssueItemIndex);
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
    }
}
