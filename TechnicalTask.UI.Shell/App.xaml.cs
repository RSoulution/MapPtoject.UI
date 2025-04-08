using System.Windows;
using System.Windows.Media;
using TechnicalTask.UI.Client;
using TechnicalTask.UI.Control;

namespace TechnicalTask.UI.Shell
{
    public partial class App : Application
    {
        ControlManager controlManager;
        MainWindow mainWindow;
        LoginControl loginControl;
        MapViewControl mapControl;
        private void AppStartUp(object sender, StartupEventArgs args) //Launching 
        {
            try
            {
                SignalRClient signalRClient = new SignalRClient(Settings1.Default.SignalRConnection); //We announce everything you need
                controlManager = new ControlManager();
                mainWindow = new MainWindow();
                loginControl = new LoginControl(new LoginViewModel(signalRClient, controlManager));
                mapControl = new MapViewControl(new MapViewModel(signalRClient, controlManager, Settings1.Default.MarkersSize, (SolidColorBrush)new BrushConverter().ConvertFromString(Settings1.Default.MarkerActiveColor ?? "#FF0000"), (SolidColorBrush)new BrushConverter().ConvertFromString(Settings1.Default.MarkerLostColor ?? "#FFFFFF")));



                mainWindow.WindowState = WindowState.Normal;
                this.mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                this.mainWindow.Show();
                controlManager.Register("MainWindow", mainWindow);
                controlManager.Register("LoginControl", loginControl);
                controlManager.Register("MapViewControl", mapControl);
                controlManager.Place("MainWindow", "MainRegion", "LoginControl"); //Adding a login to the window
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        private void AppExit(object sender, ExitEventArgs args) //Closing the program
        {
            this.Shutdown();
        }
    }

}
