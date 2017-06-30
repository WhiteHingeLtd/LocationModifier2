using System.Windows;
using WHLClasses.Orders;

namespace LocationModifier2.Dialogs
{
    /// <summary>
    /// Interaction logic for PrepackList.xaml
    /// </summary>
    public partial class PrepackList : Window
    {
        internal OrderDefinition FullOrddef;
        public PrepackList(OrderDefinition fullOrderDefinition)
        {
            InitializeComponent();
            var tempOrddef = new OrderDefinition();
            FullOrddef = fullOrderDefinition;
            tempOrddef.AddRange(fullOrderDefinition.GetByStatus(OrderStatus._Prepack));

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
