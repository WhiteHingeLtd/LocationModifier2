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

namespace LocationModifier2.Dialogs
{
    /// <summary>
    /// Interaction logic for YesNoDialog.xaml
    /// </summary>
    public partial class YesNoDialog : Window
    {
        public bool Result;
        public YesNoDialog(string message,string title)
        {
            InitializeComponent();
            this.MessageBlock.Text = message;
            this.TitleBlock.Text = title;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            this.Close();
        }
    }
}
