using Pimax.FactoryTool.KingdeePrinter.Models;
using Serilog;
using System.Windows;
using System.Windows.Threading;

namespace Pimax.FactoryTool.KingdeePrinter
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            InitializeLogger();
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppConfig.Initialize();
            base.OnStartup(e);
        }

        private void InitializeLogger()
        {
            Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .WriteTo.File(@"logs\.txt", rollingInterval: RollingInterval.Day)
                        .CreateLogger();
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Logger.Error("UnhandledException: " + e.Exception.Message);
            Log.Logger.Information(e.Exception.StackTrace);

            MessageBox.Show(e.Exception.Message, "程序错误", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            AppConfig.KNGenerator.Save();
            base.OnExit(e);
        }
    }
}
