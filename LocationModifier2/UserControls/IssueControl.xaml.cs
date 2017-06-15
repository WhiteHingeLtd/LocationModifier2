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
        public IssueControl(Order order, IssueData issue, IssuesList dialog)
        {
            InitializeComponent();
            IssueListDialog = dialog;
            Issue = issue;
            OrderNumText.Text = order.OrderId;
            TimeText.Text = issue.TimeReported.ToString("HH:mm:ss");
            OrderNumText.Text = issue.Reason;
            var Badsku = IssueListDialog.IwRef.OldMw.FullSkuCollection.SearchSKUS(issue.DodgySku)[0];
            MessageText.Text = Badsku.GetLocation(SKULocation.SKULocationType.Pickable).LocationText + ": " +
                               Badsku.Title.Label;
            Pickroute = Badsku.GetLocation(SKULocation.SKULocationType.Pickable).LocationID;
        }

        private void MainIssueSourceButton_Click(object sender, RoutedEventArgs e)
        {
            IssueListDialog.Close();
            IssueListDialog.IwRef.ProcessScan(Issue.DodgySku);           
        }
    }
}
