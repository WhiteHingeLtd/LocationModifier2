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
    /// Interaction logic for MsgDialog.xaml
    /// </summary>
    public partial class MsgDialog : Window
    {
        public bool Result;
        public MsgDialog(string title, string message)
        {
            InitializeComponent();
            MessageBlock.Text = message;
            TitleBlock.Text = title;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            Close();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            Close();
        }
    }
}
