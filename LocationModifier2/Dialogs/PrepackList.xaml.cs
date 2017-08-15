using LocationModifier2.Cool;
using LocationModifier2.UserControls;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using WHLClasses.Orders;

namespace LocationModifier2.Dialogs
{
    /// <summary>
    /// Interaction logic for PrepackList.xaml
    /// </summary>
    public partial class PrepackList
    {
        internal ItemWindow IwRef;
        internal ViewOrReset CurrentViewState = ViewOrReset.View;
        public PrepackList(OrderDefinition fullOrderDefinition, ItemWindow itemWindow)
        {
            InitializeComponent();
            IwRef = itemWindow;
            var tempOrddef = fullOrderDefinition.GetByStatus(OrderStatus._Prepack);

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

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewButton.IsChecked != null && ViewButton.IsChecked.Value)
            {
                CurrentViewState = ViewOrReset.View;
            }
            else
            {
                CurrentViewState = ViewOrReset.Reset;
            }
        }

        internal enum ViewOrReset
        {
            View = 0,
            Reset = 1
        }

    }
}
