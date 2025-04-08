using Microsoft.AspNetCore.SignalR.Client;
using System.Text.RegularExpressions;
using TechnicalTask.Entities;

namespace TechnicalTask.UI.Client
{
    public class SignalRClient //Server client
    {
        private HubConnection _connection;

        public event Action<string> OnJoinSuccess; //Receive a response about successfully joining the group
        public event Action<string> OnJoinFailed; //Get a response about a failed group join
        public event Action<EntObject> OnReceiveObjects; //Get objects from the server
        string url = "http://localhost:5034"; //Server address
        public string Group  {
            get;
            set; } = string.Empty;

        public SignalRClient(string connectionString)
        {
            url = connectionString;
            _connection = new HubConnectionBuilder()
                .WithUrl($"{url}/main")
                .WithAutomaticReconnect()
                .Build();

            _connection.On<string>("JoinSuccess", message => OnJoinSuccess?.Invoke(message));
            _connection.On<string>("JoinFailed", message => OnJoinFailed?.Invoke(message));
            _connection.On<EntObject>("ReceiveData", objects => OnReceiveObjects?.Invoke(objects));
        }

        public async Task ConnectAsync() //Connecting to the server
        {
            if(_connection.State != HubConnectionState.Connected)
                await _connection.StartAsync();

        }

        public async Task JoinGroup(string key) //Connecting to a group by key
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                await _connection.InvokeAsync("JoinGroup", key);
            }
            else
                throw new Exception("Connection Failed.");
        }
        public async Task LeaveGroup() //Leave your group
        {
            await _connection.InvokeAsync("LeaveGroup", Group);
        }
    }
}
