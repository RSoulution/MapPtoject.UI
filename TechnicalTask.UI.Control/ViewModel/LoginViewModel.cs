using System.ComponentModel;
using System.Windows.Input;
using TechnicalTask.UI.Client;
using System.Diagnostics;
using TechnicalTask.UI.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TechnicalTask.UI.Control
{
    public partial class LoginViewModel : ObservableObject, IDataErrorInfo   //ViewModel LoginControl
    {
        private readonly SignalRClient _signalRClient; //Client
        private readonly IControlManager _controlManager; //ControlManager for switching between windows

        [ObservableProperty] //Declaring Bindings and Commands
        private string key = string.Empty;

        [ObservableProperty]
        private string validationText = "";

        public ICommand EnterCommand { get; }
        public ICommand ExitCommand { get; }

        public LoginViewModel(SignalRClient rClient, IControlManager controlManager)
        {
            _signalRClient = rClient;
            _controlManager = controlManager;
            _signalRClient.ConnectAsync(); //Let's connect right away.
            _signalRClient.OnJoinSuccess += OnJoinSuccess; //Subscribe to server responses
            _signalRClient.OnJoinFailed += OnJoinFailed;
            EnterCommand = new RelayCommand(ExecuteEnterCommand, CanExecuteEnter);
            ExitCommand = new RelayCommand(ExecuteExitCommand);
        }

        private void OnJoinSuccess(string message) //Client event handler for successful group join
        {
            _signalRClient.Group = key;
            _controlManager.Place("MainWindow", "MainRegion", "MapViewControl");
        }

        private void OnJoinFailed(string message) //Client event handler for failed group join
        {
            Debug.WriteLine($"JoinFailed received: {message}");
            ValidationText = message; 
        }

        private void ExecuteEnterCommand() //Pressing the ENTER button
        {
            try
            {
                using var _ = _signalRClient.ConnectAsync();
                 _signalRClient.JoinGroup(Key);
            }
            catch (Exception ex)
            {
                ValidationText = ex.Message;
            }

        }

        private bool CanExecuteEnter() //Block ENTER button if key field is empty
        {
            return !string.IsNullOrWhiteSpace(Key);
        }

        private void ExecuteExitCommand() //Executing the EXIT button
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
