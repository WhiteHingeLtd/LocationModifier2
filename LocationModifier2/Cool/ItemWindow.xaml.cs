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
            if (!ScanData.StartsWith("qlo"))
            {
                //Googogo
                //Find it first
                var Matches = _OldMW.MixdownSkuCollection.SearchSKUS(ScanData, true);
                if (Matches.Count == 1)
                {
                    //GOGOOGO WE HACVE MATCH
                    ItemName.Text = Matches[0].Title.Label;
                    ShortSku.Text = Matches[0].ShortSku;

                    //Get kids.
                    var kids = _OldMW.FullSkuCollection.GatherChildren(Matches[0].ShortSku);
                }
            }
            else
            {
                ProcessToShelfScreen(ScanData);
            }
        }

        private void ProcessToShelfScreen(string ScanData)
        {
            
        }
    }
}
