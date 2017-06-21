using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using LocationModifier2.Cool;
using LocationModifier2.UserControls;
using WHLClasses;
using WHLClasses.Orders;

namespace LocationModifier2.Dialogs
{
    /// <summary>
    /// Interaction logic for IssuesList.xaml
    /// </summary>
    public partial class IssuesList : Window
    {
        internal ItemWindow IwRef;
        internal SkuCollection IssueSkuColl;
        internal OrderDefinition LocalOrddef;
        internal bool SendingToPrepack;
        public IssuesList(ItemWindow window,OrderDefinition FullOrddef)
        {
            InitializeComponent();
            IwRef = window;
            SendingToPrepack = false;
            IssueSkuColl = IwRef.OldMw.FullSkuCollection;
            LocalOrddef = FullOrddef;
            var WorkingOrddef = new OrderDefinition();
            WorkingOrddef.AddRange(LocalOrddef.GetByStatus(OrderStatus._MissingItem));
            WorkingOrddef.AddRange(LocalOrddef.GetByStatus(OrderStatus._Cantfind));
            var controlList = new List<IssueControl>();
            var ordersWithIssues = WorkingOrddef.Where(x => x.issues.Any());
            foreach (var order in ordersWithIssues)
            {
                controlList.AddRange(order.issues.Select(issue => new IssueControl(order, issue, this)));
            }
            controlList.Sort((x,y) => x.Pickroute.CompareTo(y.Pickroute));
            foreach (var control in controlList)
            {
                ActualAuditContainer.Children.Add(control);
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            IwRef.Refocus();
        }


        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBox1.IsChecked != null) SendingToPrepack = CheckBox1.IsChecked.Value;
        }
    }
}
