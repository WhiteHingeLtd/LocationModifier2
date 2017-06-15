using System;
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
    /// Interaction logic for IssuesList.xaml
    /// </summary>
    public partial class IssuesList : Window
    {
        internal ItemWindow IwRef;
        internal SkuCollection IssueSkuColl;
        internal OrderDefinition LocalOrddef;
        public IssuesList(ItemWindow window)
        {
            InitializeComponent();
            IwRef = window;
            IssueSkuColl = IwRef.OldMw.FullSkuCollection;
            try
            {
                var OrddefClient = WHLClasses.Services.OrderServer.Fucnt.ConnectChannel("net.tcp://orderserver.ad.whitehinge.com:801/OrderServer/1");
                LocalOrddef = OrddefClient.StreamOrderDefinition();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            var WorkingOrddef = new OrderDefinition();
            WorkingOrddef.AddRange(LocalOrddef.GetByStatus(OrderStatus._MissingItem));
            WorkingOrddef.AddRange(LocalOrddef.GetByStatus(OrderStatus._Cantfind));
            var controlList = new List<IssueControl>();
            foreach (var order in WorkingOrddef.Where(x => x.issues.Any()))
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
    }
}
