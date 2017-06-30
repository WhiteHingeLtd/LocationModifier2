using System;
using LocationModifier2.Cool;
using LocationModifier2.UserControls;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
        internal IssueResolution CurrentSelectedResolution;
        public IssuesList(ItemWindow window,OrderDefinition fullOrddef)
        {
            InitializeComponent();
            IwRef = window;
            CurrentSelectedResolution = IssueResolution.ViewLocations;
            IssueSkuColl = IwRef.OldMw.FullSkuCollection;
            ItemName.Text = IwRef.LastOrddefRefresh.ToShortTimeString();
            LocalOrddef = fullOrddef;
            var workingOrddef = new OrderDefinition();
            workingOrddef.AddRange(LocalOrddef.GetByStatus(OrderStatus._MissingItem));
            workingOrddef.AddRange(LocalOrddef.GetByStatus(OrderStatus._Cantfind));
            var controlList = new List<IssueControl>();
            var ordersWithIssues = workingOrddef.Where(x => x.issues.Any());
            foreach (var order in ordersWithIssues)
            {
                controlList.AddRange(order.issues.Select(issue => new IssueControl(order, issue, this)));
            }
            controlList.Sort((x,y) => x.Pickroute.CompareTo(y.Pickroute));
            foreach (var control in controlList)
            {
                ActualAuditContainer.Children.Add(control);
            }
            ViewLocations.IsChecked = true;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            IwRef.Refocus();
        }


        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (SendToPrepack.IsChecked != null && SendToPrepack.IsChecked.Value)
            {
                CurrentSelectedResolution = IssueResolution.SendToPrepack;
            }
            else if (ViewLocations.IsChecked != null && ViewLocations.IsChecked.Value)
            {
                CurrentSelectedResolution = IssueResolution.ViewLocations;
            }
            else if (ResetOrder.IsChecked != null && ResetOrder.IsChecked.Value)
            {
                CurrentSelectedResolution = IssueResolution.ResetOrder;
            }
            else if (OtherWarehouse.IsChecked != null && OtherWarehouse.IsChecked.Value)
            {
                CurrentSelectedResolution = IssueResolution.OtherWarehouse;
                
            }
        }

        private void RefreshList(OrderDefinition fullOrddef)
        {
            ActualAuditContainer.Children.Clear();
            var tempOrddef = new OrderDefinition();
            tempOrddef.AddRange(fullOrddef.GetByStatus(OrderStatus._MissingItem));
            tempOrddef.AddRange(fullOrddef.GetByStatus(OrderStatus._Cantfind));
            var controlList = new List<IssueControl>();
            var ordersWithIssues = tempOrddef.Where(x => x.issues.Any());
            foreach (var order in ordersWithIssues)
            {
                if (order.State == OrderStatus._Prepack || order.State == OrderStatus._Withdrawn) continue;
                controlList.AddRange(order.issues.Select(issue => new IssueControl(order, issue, this)));
            }
            controlList.Sort((x, y) => x.Pickroute.CompareTo(y.Pickroute));
            foreach (var control in controlList)
            {
                if (control.CurrentIssueData.Reason.ToLower().Contains("prepack") | control.CurrentIssueData.Reason.ToLower().Contains("gs1")) continue;
                ActualAuditContainer.Children.Add(control);
            }
        }

        public enum IssueResolution
        {
            Default = 0,
            ViewLocations = 1,
            SendToPrepack = 3,
            ResetOrder = 2,
            OtherWarehouse = 4
        }

        private async void ItemName_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                LocalOrddef = await Task.Run(() => RefreshLocalOrddef());
                RefreshList(LocalOrddef);
            }
            catch (Exception)
            {
                //Orddef failed to load
            }

        }

        private void ItemName_TouchUp(object sender, System.Windows.Input.TouchEventArgs e)
        {
            IwRef.OrddefReloadTimer_Tick(null, null);
            LocalOrddef = IwRef.OrderDefintions;
            RefreshList(LocalOrddef);
            ItemName.Text = IwRef.LastOrddefRefresh.ToShortTimeString();
        }

        private OrderDefinition RefreshLocalOrddef()
        {
            OrderDefinition returnOrddef;
            try
            {
                var orddefClient = WHLClasses.Services.OrderServer.Fucnt.ConnectChannel("net.tcp://orderserver.ad.whitehinge.com:801/OrderServer/1");
                returnOrddef = orddefClient.StreamOrderDefinition();
            }
            catch (Exception)
            {
                throw new InvalidOperationException();
            }
            if (returnOrddef != null) return returnOrddef;
            throw new InvalidOperationException();
        }
    }
}
