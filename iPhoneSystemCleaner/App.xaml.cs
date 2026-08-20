using System.Windows;

namespace iPhoneSystemCleaner
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var inner = e.Exception.InnerException;
            string msg = inner != null ? inner.Message + "\n" + inner.StackTrace : e.Exception.Message + "\n" + e.Exception.StackTrace;
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            System.IO.File.WriteAllText(System.IO.Path.Combine(desktop, "crash.txt"), msg);
            MessageBox.Show($"WPF Error: {msg}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            string msg = $"App Error: {ex?.Message}\n\n{ex?.StackTrace}";
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            System.IO.File.WriteAllText(System.IO.Path.Combine(desktop, "crash_domain.txt"), msg);
            MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
