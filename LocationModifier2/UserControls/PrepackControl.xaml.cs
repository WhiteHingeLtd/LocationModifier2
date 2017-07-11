using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LocationModifier2.Dialogs;
using WHLClasses;
using WHLClasses.Orders;

namespace LocationModifier2.UserControls
{
    /// <summary>
    /// Interaction logic for PrepackControl.xaml
    /// </summary>
    public partial class PrepackControl : UserControl
    {
        internal IssueData CurrentIssueData;
        internal PrepackList IssueListDialog;
        internal int Pickroute = 0;
        internal Order CurrentOrder;
        internal WhlSKU CurrentSku;
        internal int Warehouse;
        public PrepackControl(Order order, IssueData currentIssue, PrepackList dialog)
        {
            InitializeComponent();
            IssueListDialog = dialog;
            CurrentIssueData = currentIssue;
            CurrentOrder = order;


            TimeText.Text = currentIssue.TimeReported.ToString("HH:mm:ss");


            var Badsku = FindCorrectSku(IssueListDialog.IwRef.OldMw.FullSkuCollection, currentIssue.DodgySku);
            //var Badsku2 = IssueListDialog.IwRef.OldMw.FullSkuCollection.SearchSKUS(currentIssue.DodgySku)[0];
            OrderNumText.Text = "Pack :" + Badsku.PackSize.ToString();
            OrderNumText.Text += " " + currentIssue.Reason;
            OrderNumText.Text = $"{CurrentOrder.BetterItems.Single(x => x.SKU.Contains(Badsku.SKU)).OrderQuantity} x {Badsku.PackSize} Pack | {CurrentOrder.OrderId}";
            CurrentSku = Badsku;
            nameText.Text = dialog.IwRef.OldMw.EmpCol.FindEmployeeByID(CurrentIssueData.ReportingUser).FullName
                .Split(' ')[0];

            MessageText.Text = Badsku.GetLocation(SKULocation.SKULocationType.Pickable).LocationText + ": " +
                               Badsku.Title.Label;

            Pickroute = Badsku.GetLocation(SKULocation.SKULocationType.Pickable).LocationID;
            Warehouse = Badsku.GetLocation(SKULocation.SKULocationType.Pickable).WarehouseID;
        }

        private void MainIssueSourceButton_Click(object sender, RoutedEventArgs e)
        {
            IssueListDialog.Close();
            IssueListDialog.IwRef.ProcessScan(CurrentIssueData.DodgySku);
        }
        private WhlSKU FindCorrectSku(SkuCollection searchColl, string sku)
        {
            return searchColl.Single(x => x.SKU == sku);
        }
    }
}
