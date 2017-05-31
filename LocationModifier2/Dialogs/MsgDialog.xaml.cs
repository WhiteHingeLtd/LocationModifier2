using System.Windows;

namespace LocationModifier2.Dialogs
{
    /// <summary>
    /// Interaction logic for MsgDialog.xaml
    /// </summary>
    public partial class MsgDialog
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
