using System.ComponentModel;
using System.Windows.Input;
using TechnicalTask.UI.Client;
using System.Diagnostics;
using TechnicalTask.UI.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TechnicalTask.UI.Control
{
    public partial class LoginViewModel : ObservableObject, IDataErrorInfo   //ViewModel вікна LoginControl
    {
        private readonly SignalRClient _signalRClient; //Клієнт
        private readonly IControlManager _controlManager; //ControlManager для перехіду між вікнами

        [ObservableProperty] //Оголошуємо Bindings та Команди
        private string key = "";

        [ObservableProperty]
        private string validationText = "";

        public ICommand EnterCommand { get; }
        public ICommand ExitCommand { get; }

        public LoginViewModel(SignalRClient rClient, IControlManager controlManager)
        {
            _signalRClient = rClient;
            _controlManager = controlManager;
            _signalRClient.ConnectAsync(); //Одразу підключаємось
            _signalRClient.OnJoinSuccess += OnJoinSuccess; //Підписуємось на відповіді серверу
            _signalRClient.OnJoinFailed += OnJoinFailed;
            EnterCommand = new RelayCommand(ExecuteEnterCommandAsync, CanExecuteEnter);
            ExitCommand = new RelayCommand(ExecuteExitCommand);
        }

        private void OnJoinSuccess(string message) //Обробник подій клієнта про успішне приєднання до групи
        {
            _controlManager.Place("MainWindow", "MainRegion", "MapViewControl");
        }

        private void OnJoinFailed(string message) //Обробник подій клієнта про невдале приєднання до групи
        {
            Debug.WriteLine($"JoinFailed received: {message}");
            ValidationText = message; 
        }

        private async void ExecuteEnterCommandAsync() //Виконання кнопки ENTER
        {
            try
            {
                await _signalRClient.ConnectAsync();
                await _signalRClient.JoinGroup(Key);
            }
            catch (Exception ex)
            {
                ValidationText = ex.Message;
            }

        }

        private bool CanExecuteEnter() //Блокування кнопки ENTER, якщо поле ключа пусте
        {
            return !string.IsNullOrWhiteSpace(Key);
        }

        private void ExecuteExitCommand() //Виконання кнопки EXIT
        {
            Process.GetCurrentProcess().Kill();
        }

        partial void OnKeyChanged(string value)
        {
            ((RelayCommand)EnterCommand).NotifyCanExecuteChanged();
        }
        public string Error => null;

        public string this[string columnName] => columnName == nameof(Key) && string.IsNullOrWhiteSpace(Key) ? "Key cannot be empty!" : null;
    }
}
