using System.Windows.Threading;

namespace LocationModifier2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            WHLClasses.Reporting.ErrorReporting.ReportException(e.Exception);
           
        }
    }
}
