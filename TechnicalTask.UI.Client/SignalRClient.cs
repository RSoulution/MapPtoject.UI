using Microsoft.AspNetCore.SignalR.Client;
using TechnicalTask.Entities;

namespace TechnicalTask.UI.Client
{
    public class SignalRClient //Клієнт серверу
    {
        private HubConnection _connection;

        public event Action<string> OnJoinSuccess; //Отримати відповідь про успішне приєднання до групи
        public event Action<string> OnJoinFailed; //Отримати відповідь про невдале приєднання до групи
        public event Action<EntObject> OnReceiveObjects; //Отримати об'єкти з серверу
        string url = "http://localhost:5034"; //Адреса серверу

        public SignalRClient()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl($"{url}/main")
                .WithAutomaticReconnect()
                .Build();

            _connection.On<string>("JoinSuccess", message => OnJoinSuccess?.Invoke(message));
            _connection.On<string>("JoinFailed", message => OnJoinFailed?.Invoke(message));
            _connection.On<EntObject>("ReceiveData", objects => OnReceiveObjects?.Invoke(objects));
        }

        public async Task ConnectAsync() //Підключення до серверу
        {
            if(_connection.State != HubConnectionState.Connected)
                await _connection.StartAsync();

        }

        public async Task JoinGroup(string key) //Підключення до групи за ключем
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                await _connection.InvokeAsync("JoinGroup", key);
            }
            else
                throw new Exception("Connection Failed.");
        }
        public async Task LeaveGroup() //Покинути свою групу
        {
            await _connection.InvokeAsync("LeaveGroup");
        }
    }
}
