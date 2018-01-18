using LocationModifier2.Cool;
using LocationModifier2.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WHLClasses.Orders;

namespace LocationModifier2.Dialogs
{
    /// <summary>
    /// Interaction logic for IssuesList.xaml
    /// </summary>
    public partial class IssuesList 
    {
        internal ItemWindow IwRef;
        internal OrderDefinition LocalOrddef;
        internal IssueResolution CurrentSelectedResolution;

        public IssuesList(ItemWindow window, OrderDefinition fullOrddef)
        {
            InitializeComponent();
            
            IwRef = window;
            switch (IwRef.OldMw.Unit)
            {
                case MainWindow.CurrentUnit.Unit14:
                    Unit14Button.IsChecked = true;
                    Unit1Button.IsChecked = false;
                    AllUnitsButton.IsChecked = false;
                    break;
                case MainWindow.CurrentUnit.Unit1:
                    Unit1Button.IsChecked = true;
                    Unit14Button.IsChecked = false;
                    AllUnitsButton.IsChecked = false;
                    break;
                case MainWindow.CurrentUnit.AllUnits:
                    Unit1Button.IsChecked = false;
                    Unit14Button.IsChecked = false;
                    AllUnitsButton.IsChecked = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            CurrentSelectedResolution = IssueResolution.ViewLocations;
            ItemName.Text = IwRef.LastOrddefRefresh.ToShortTimeString();
            LocalOrddef = fullOrddef;
            var workingOrddef = new OrderDefinition();
            workingOrddef.AddRange(LocalOrddef.GetByStatus(OrderStatus._MissingItem));
            workingOrddef.AddRange(LocalOrddef.GetByStatus(OrderStatus._Cantfind));
            var controlList = new List<IssueControl>();
            var ordersWithIssues = workingOrddef.Where(x => x.issues.Any());
            foreach (var order in ordersWithIssues)
            {
                try
                {
                    if (order.State == OrderStatus._Prepack || order.State == OrderStatus._Withdrawn) continue;
                    controlList.AddRange(from issue in order.issues where !issue.Resolved && !issue.Reason.ToLower().Contains("prepack") && issue.DodgySku != null select new IssueControl(order, issue, this));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

            }
            List<IssueControl> controlListSorted;
            switch (IwRef.OldMw.Unit)
            {
                    case MainWindow.CurrentUnit.Unit14:
                    controlListSorted = controlList.Where(x => x.Warehouse == 1).ToList();
                    controlListSorted.Sort((x, y) => x.Pickroute.CompareTo(y.Pickroute));
                    break;
                case MainWindow.CurrentUnit.Unit1:
                    controlListSorted = controlList.Where(x => x.Warehouse == 2).ToList();
                    controlListSorted.Sort((x, y) => x.Pickroute.CompareTo(y.Pickroute));
                    break;
                case MainWindow.CurrentUnit.AllUnits:
                    controlListSorted = controlList;
                    controlListSorted.Sort((x, y) => x.Pickroute.CompareTo(y.Pickroute));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            foreach (var control in controlListSorted)
            {
                ActualAuditContainer.Children.Add(control);
            }
            ViewLocations.IsChecked = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
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
                controlList.AddRange(from issue in order.issues
                    where !issue.Resolved && !issue.Reason.ToLower().Contains("prepack")
                    select new IssueControl(order, issue, this));
            }
            var controlListSorted = new List<IssueControl>();
            switch (IwRef.OldMw.Unit)
            {
                case MainWindow.CurrentUnit.Unit14:
                    controlListSorted = controlList.Where(x => x.Warehouse == 1).ToList();
                    controlListSorted.Sort((x, y) => x.Pickroute.CompareTo(y.Pickroute));
                    break;
                case MainWindow.CurrentUnit.Unit1:
                    controlListSorted = controlList.Where(x => x.Warehouse == 2).ToList();
                    controlListSorted.Sort((x, y) => x.Pickroute.CompareTo(y.Pickroute));
                    break;
                case MainWindow.CurrentUnit.AllUnits:
                    controlListSorted = controlList;
                    controlListSorted.Sort((x, y) => x.Pickroute.CompareTo(y.Pickroute));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            controlList.Sort((x, y) => x.Pickroute.CompareTo(y.Pickroute));
            foreach (var control in controlListSorted)
            {
                if (control.CurrentIssueData.Reason.ToLower().Contains("prepack") |
                    control.CurrentIssueData.Reason.ToLower().Contains("gs1")) continue;
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
                var orddefClient =
                    WHLClasses.Services.OrderServer.Fucnt.ConnectChannel(
                        "net.tcp://orderserver.ad.whitehinge.com:801/OrderServer/1");
                returnOrddef = orddefClient.StreamOrderDefinition();
            }
            catch (Exception)
            {
                throw new InvalidOperationException();
            }
            if (returnOrddef != null) return returnOrddef;
            throw new InvalidOperationException();
        }

        private void Unit14Button_Click(object sender, RoutedEventArgs e)
        {
            if (Unit14Button.IsChecked != null && Unit14Button.IsChecked.Value)
            {
                IwRef.OldMw.Unit = MainWindow.CurrentUnit.Unit14;
                
            }
            else if (Unit1Button.IsChecked != null && Unit1Button.IsChecked.Value)
            {
                IwRef.OldMw.Unit = MainWindow.CurrentUnit.Unit1;
            }
            else if (AllUnitsButton.IsChecked != null && AllUnitsButton.IsChecked.Value)
            {
                IwRef.OldMw.Unit = MainWindow.CurrentUnit.AllUnits;
            }
        }
    }
}
