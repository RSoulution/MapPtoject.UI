using System.Windows;
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
        private void AppStartUp(object sender, StartupEventArgs args) //Запуск 
        {
            try
            {
                SignalRClient signalRClient = new SignalRClient(); //Оголошуємо все необхідне
                controlManager = new ControlManager();
                mainWindow = new MainWindow();
                loginControl = new LoginControl(new LoginViewModel(signalRClient, controlManager));
                mapControl = new MapViewControl(new MapViewModel(signalRClient, controlManager));



                mainWindow.WindowState = WindowState.Normal;
                this.mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                this.mainWindow.Show();
                controlManager.Register("MainWindow", mainWindow);
                controlManager.Register("LoginControl", loginControl);
                controlManager.Register("MapViewControl", mapControl);
                controlManager.Place("MainWindow", "MainRegion", "LoginControl"); //Додаємо логін на вікно
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        private void AppExit(object sender, ExitEventArgs args) //Закриття програми
        {
            this.Shutdown();
        }
    }

}
