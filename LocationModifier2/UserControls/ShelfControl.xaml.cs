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
using LocationModifier2.Cool;
using LocationModifier2.Dialogs;
using WHLClasses;

namespace LocationModifier2.UserControls
{
    /// <summary>
    /// Interaction logic for ShelfControl.xaml
    /// </summary>
    public partial class ShelfControl : UserControl
    {
        internal int LocationId;
        internal string LocationText;
        internal SkuCollection ActiveCollection = new SkuCollection(true);
        internal ItemWindow MWRef;
        internal Dictionary<string,int> Additionals;
        public ShelfControl(int locId,string locText,SkuCollection skuColl, ItemWindow main)
        {
            InitializeComponent();
            LocationId = locId;
            LocationText = locText;
            ActiveCollection = skuColl;
            MWRef = main;
            Additionals.Clear();
            foreach (var sku in skuColl)
            {
                var info = (from loc in sku.Locations
                            where (loc.LocationID == locId)
                    select Convert.ToInt32(loc.Additional))
                    .Single();
                Additionals.Add(sku.SKU,info);
            }
        }

        private void ShelfControlUC_TouchUp(object sender, TouchEventArgs e)
        {
            RemoveAllFromShelf();
        }

        private void ShelfControlUC_MouseUp(object sender, MouseButtonEventArgs e)
        {
            RemoveAllFromShelf();
        }

        private void RemoveAllFromShelf()
        {
            if (Additionals.Any(add => add.Value != 0))
            {
                var msg = new MsgDialog("ERROR","These shelves aren't empty!");
                msg.ShowDialog();
            }
            else
            {
                foreach (var item in ActiveCollection)
                {
                    item.RemoveLocationWithAudit(LocationId,MWRef._OldMW.AuthdEmployee);
                }
            }
        }
    }
}
