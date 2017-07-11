using System.Collections.Generic;
using System.Linq;
using System.Windows;
using LocationModifier2.Cool;
using LocationModifier2.UserControls;
using WHLClasses;
using WHLClasses.Orders;

namespace LocationModifier2.Dialogs
{
    /// <summary>
    /// Interaction logic for PrepackList.xaml
    /// </summary>
    public partial class PrepackList : Window
    {
        internal ItemWindow IwRef;
        internal SkuCollection IssueSkuColl;
        private OrderDefinition _fullOrddef;
        public PrepackList(OrderDefinition fullOrderDefinition, ItemWindow itemWindow)
        {
            InitializeComponent();
            IwRef = itemWindow;
            IssueSkuColl = IwRef.OldMw.FullSkuCollection;
            var tempOrddef = fullOrderDefinition.GetByStatus(OrderStatus._Prepack);
            _fullOrddef = fullOrderDefinition;
            var controlList = new List<PrepackControl>();
            foreach (var order in tempOrddef)
            {
                if (order.State == OrderStatus._Withdrawn) continue;
                controlList.AddRange(from issue in order.issues where !issue.Resolved select new PrepackControl(order, issue, this));
            }
            controlList.Sort((x, y) => x.Pickroute.CompareTo(y.Pickroute));
            foreach (var control in controlList)
            {
                ActualAuditContainer.Children.Add(control);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
