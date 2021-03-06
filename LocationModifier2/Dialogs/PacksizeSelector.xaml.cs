﻿using System;
using System.Windows;
using System.Windows.Controls;
using WHLClasses;

namespace LocationModifier2.Dialogs
{
    /// <summary>
    /// Interaction logic for PacksizeSelector.xaml
    /// </summary>
    public partial class PacksizeSelector
    {
        public WhlSKU SelectedSku;
        public SkuCollection skucoll;
        public PacksizeSelector(SkuCollection skus)
        {
            skucoll = skus;
            InitializeComponent();
            foreach (var sku in skus)
            {
                if (sku.SKU.Contains("xxxx")) continue;
                var refctrl = new Button();
                refctrl.Click += Refctrl_Click;
                refctrl.Padding = new Thickness(1.0);
                refctrl.Uid = sku.SKU;
                refctrl.Content = "(Pack of " + sku.PackSize.ToString() +") " + sku.Title.Label;
                refctrl.FontSize = 36.0;
                UniformPacksizeGrid.Children.Add(refctrl);
            }
        }

        private void Refctrl_Click(object sender, RoutedEventArgs e)
        {
            var ctrl = sender as Button;
            try
            {
                if (ctrl == null) return;
                SelectedSku = skucoll.SearchBarcodes(ctrl.Uid)[0];
                if (SelectedSku.SKU == ctrl.Uid)
                {
                    this.Close();
                }
                else throw new NullReferenceException();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                LoginTitle.Text = "Please try again";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
