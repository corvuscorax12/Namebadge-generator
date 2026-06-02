using System.Windows;
using System.Windows.Threading;
namespace YawgoogIdGenerator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)

        {
            MessageBox.Show(e.Exception.ToString());
        }
    }

}
