﻿using System.Collections.Generic;
using System.Windows.Controls;
using WHLClasses;

namespace LocationModifier2.UserControls
{
    /// <summary>
    /// Interaction logic for PacksizeControl.xaml
    /// </summary>
    public partial class PacksizeControl : UserControl
    {
        internal WhlSKU ActiveItem;
        internal MainWindow MainWindowRef;
        internal List<int> LocList;
        public PacksizeControl(List<int> locationList,WhlSKU sku,MainWindow main)
        {
            InitializeComponent();
            ActiveItem = sku;
            MainWindowRef = main;
            LocList = locationList;
            PacksizeBlock.Text = sku.PackSize.ToString();
            foreach (var loc in locationList)
            {
                var refControl = new ButtonControl(ActiveItem,MainWindowRef,loc);
                ExpandPanel.Children.Add(refControl);
            }
        }
    }
}
