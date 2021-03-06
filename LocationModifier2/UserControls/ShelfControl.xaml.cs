﻿using LocationModifier2.Cool;
using LocationModifier2.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using WHLClasses;
using WHLClasses.Exceptions;
using Color = System.Windows.Media.Color;

namespace LocationModifier2.UserControls
{
    /// <summary>
    /// Interaction logic for ShelfControl.xaml
    /// </summary>
    public partial class ShelfControl
    {
        internal int LocationId;
        internal SkuCollection ActiveCollection;
        internal ItemWindow IwRef;
        internal Dictionary<string,int> Additionals = new Dictionary<string, int>();
        public ShelfControl(int locId,string locText,SkuCollection skuColl, ItemWindow main, bool multiPick = false,bool isPick = false)
        {
            InitializeComponent();
            LocationId = locId;
            ActiveCollection = skuColl;
            Button1.Content = locText;
            IwRef = main;
            if (multiPick && !isPick) Button1.Background = new SolidColorBrush(Color.FromArgb(145,255,0,0));
            else if(isPick) Button1.Background = new SolidColorBrush(Color.FromArgb(145,0,255,155));
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
        }

        private void RemoveAllFromShelf()
        {
            var item = ActiveCollection[0];
            item.RefreshLocations();
            if (item.Locations.FirstOrDefault((x)=> x.LocationID==LocationId).Additional!="0")
            {
                var msg = new MsgDialog("ERROR","These shelves aren't empty!");
                msg.ShowDialog();
            }
            else
            {
                try
                {
                    if (item.GetLocationsByType(SKULocation.SKULocationType.Pickable).Count == 1 && item.GetLocationsByType(SKULocation.SKULocationType.Pickable)[0].LocationID == LocationId) throw new LocationNullReferenceException("This location has only one pickable location");
                    else
                    {
                        try
                        {
                            item.RemoveLocationWithAudit(LocationId, IwRef.OldMw.AuthdEmployee,"Location Modifier");
                        }
                        catch (LocationNullReferenceException)
                        {
                        }
                        catch (Exception ex)
                        {
                            ex.Data.Add("Sku",item.SKU);
                            throw;
                        }
                            
                    }
                    var msg = new MsgDialog("Success", "This location has been removed");
                    msg.ShowDialog();
                }
                catch (LocationNullReferenceException)
                {
                    var msg = new MsgDialog("ERROR", "You cannot remove this location as it is the last pickable location");
                    msg.ShowDialog();
                }

            }
            
            IwRef.ProcessScan(ActiveCollection[0].ShortSku);
            IwRef.Refocus();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            RemoveAllFromShelf();
        }
    }
}
