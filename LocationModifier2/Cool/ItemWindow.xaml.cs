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
using System.Windows.Shapes;
using LocationModifier2.Dialogs;
using LocationModifier2.UserControls;
using WHLClasses;

namespace LocationModifier2.Cool
{
    /// <summary>
    /// Interaction logic for ItemWindow.xaml
    /// </summary>
    public partial class ItemWindow : Window
    {

        internal MainWindow _OldMW = null;


        public ItemWindow(MainWindow realMainWindow)
        {
            InitializeComponent();

            _OldMW = realMainWindow;
        }

        private void ScanBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessScan(ScanBox.Text);
                Refocus();
            }
        }

        internal void Refocus()
        {
            ScanBox.Text = "";
            ScanBox.Focus();
        }


        private void ProcessScan(string ScanData)
        {
            if (ScanData.StartsWith("qzu"))
            {
                _OldMW.ProcessScanBox(ScanData);
            }else if (_OldMW.AuthdEmployee != null)
            {
                if (!ScanData.StartsWith("qlo"))
                {
                    //Googogo
                    //Clear
                    PacksizeHolder.Children.Clear();
                    LocationControlHolder.Children.Clear();

                    //Find it first
                    var Matches = _OldMW.MixdownSkuCollection.SearchSKUS(ScanData, true);
                    if (Matches.Count == 1)
                    {
                        //GOGOOGO WE HACVE MATCH
                        ItemName.Text = Matches[0].Title.Label;
                        ShortSku.Text = Matches[0].ShortSku;

                        //Get kids.
                        var kids = _OldMW.FullSkuCollection.GatherChildren(Matches[0].ShortSku);
                        //Get the list of locaitons,
                        var LocationIDs = new Dictionary<int, string>();
                        foreach (WhlSKU Kid in kids)
                        {
                            //We're gonna have to iterate and get a list of locations.
                            foreach (SKULocation loc in Kid.Locations)
                            {
                                if (!LocationIDs.Keys.Contains(loc.LocationID))
                                {
                                    LocationIDs.Add(loc.LocationID, loc.LocationText);
                                }
                            }
                        }
                        //Sort them to fix the faggy ordering
                        var orderedlocations = from entry in LocationIDs orderby entry.Key ascending select entry;
                        var newdict = new Dictionary<int, string>();
                        foreach (KeyValuePair<int, string> asd in orderedlocations)
                        {
                            newdict.Add(asd.Key, asd.Value);
                        }
                        //Now go again and make the controls.
                        foreach (WhlSKU Kid in kids)
                        {
                            PacksizeHolder.Children.Add(new PacksizeControl(newdict.Keys.ToList(), Kid, this));
                        }
                        //And now the lcoations.
                        foreach (KeyValuePair<int, string> LocID in newdict)
                        {
                            LocationControlHolder.Children.Add(new ShelfControl(LocID.Key, LocID.Value, kids, this));
                        }

                    }
                }
                else
                {
                    ProcessToShelfScreen(ScanData);
                }
            }
            else
            {
                (new MsgDialog("asd", "You must log in before scannign stuff")).ShowDialog();
            }
        }

        private void ProcessToShelfScreen(string ScanData)
        {
            
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
