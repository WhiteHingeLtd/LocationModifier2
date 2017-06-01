using LocationModifier2.Cool;
using LocationModifier2.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using WHLClasses;
using WHLClasses.Exceptions;

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
        internal Dictionary<string,int> Additionals = new Dictionary<string, int>();
        public ShelfControl(int locId,string locText,SkuCollection skuColl, ItemWindow main)
        {
            InitializeComponent();
            LocationId = locId;
            LocationText = locText;
            ActiveCollection = skuColl;
            Button1.Content = locText;
            MWRef = main;
            
            UpdateDictionary();
        }

        private void ShelfControlUC_TouchUp(object sender, TouchEventArgs e)
        {
            RemoveAllFromShelf();
        }

        private void ShelfControlUC_MouseUp(object sender, MouseButtonEventArgs e)
        {
            RemoveAllFromShelf();
        }

        private void UpdateDictionary()
        {
            Additionals.Clear();
            foreach (var sku in ActiveCollection)
            {
                int info = -1;
                try
                {
                    info = (from loc in sku.Locations
                            where (loc.LocationID == LocationId)
                            select Convert.ToInt32(loc.Additional))
                        .Single();
                }
                catch (InvalidOperationException)
                {
                    continue;
                }
                if (info != -1) Additionals.Add(sku.SKU, info);
            }
        }

        private void RemoveAllFromShelf()
        {
            UpdateDictionary();
            if (Additionals.Any(add => add.Value != 0))
            {
                var msg = new MsgDialog("ERROR","These shelves aren't empty!");
                msg.ShowDialog();
            }
            else
            {
                foreach (var item in ActiveCollection)
                {
                    if(item.GetLocationsByType(SKULocation.SKULocationType.Pickable).Count == 1) throw new LocationNullReferenceException("This location has only one pickable location");
                    else item.RemoveLocationWithAudit(LocationId,MWRef._OldMW.AuthdEmployee);
                }
                var msg = new MsgDialog("Success", "This location has been removed");
                msg.ShowDialog();
            }
            MWRef.ProcessScan(ActiveCollection[0].ShortSku);
            MWRef.Refocus();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            RemoveAllFromShelf();
        }
    }
}
