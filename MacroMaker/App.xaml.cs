using System.Windows;

namespace MacroMaker
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Set up global exception handling
            DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show($"An error occurred: {args.Exception.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Cleanup will be handled by MainWindow
            base.OnExit(e);
        }
    }
}
