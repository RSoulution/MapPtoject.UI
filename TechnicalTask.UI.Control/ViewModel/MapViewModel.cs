using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using TechnicalTask.Entities;
using TechnicalTask.UI.Abstractions;
using TechnicalTask.UI.Client;

namespace TechnicalTask.UI.Control
{
    public partial class MapViewModel : ObservableObject  //ViewModel вікна MapViewControl
    {
        private GMapControl _mapControl;
        private readonly SignalRClient _signalRClient;
        private readonly IControlManager _controlManager;

        [ObservableProperty] //Оголошуємо Bindings та Команди
        private PointLatLng mapCenter = new PointLatLng(50.4501, 30.5234); // Київ

        [ObservableProperty]
        private GMapProvider mapProvider = GMapProviders.GoogleMap;

        [ObservableProperty]
        private ObjectActive selectedObject;

        private ObservableCollection<ObjectActive> _objectsList = new ObservableCollection<ObjectActive>();

        public ObservableCollection<ObjectActive> ObjectsList
        {
            get => _objectsList;
            set => SetProperty(ref _objectsList, value);
        }

        public ICommand ExitCommand { get; }

        private readonly SynchronizationContext _uiContext;

        private DispatcherTimer _cleanupTimer;

        private HashSet<int> _currentCycleIds;

        public MapViewModel(SignalRClient signalRClient, IControlManager controlManager)
        {
            _uiContext = SynchronizationContext.Current;
            _signalRClient = signalRClient;
            _controlManager = controlManager;
            ExitCommand = new RelayCommand(ExecuteExitCommand);
            _signalRClient.OnReceiveObjects += OnReceiveObjects;
            _cleanupTimer = new DispatcherTimer(); // Оголошуємо таймер для контролю втрачених об'єктів і подальшого їх видалення
            _cleanupTimer.Interval = TimeSpan.FromSeconds(10);
            _cleanupTimer.Tick += CleanupInactiveObjects;
            _currentCycleIds = new HashSet<int>(); // Запам'ятовуємо об'єкти, які ми отримали в новому циклі від серверу
        }

        private void OnReceiveObjects(EntObject entityObject) //Отримання даних про об'єкт з серверу
        {
            if (_currentCycleIds.Count == 0 && _objectsList.Count == 0)
                _cleanupTimer.Start(); //Запускаємо таймер
            _currentCycleIds.Add(entityObject.Id);
            _uiContext.Post(_ =>
            {
                if (_mapControl == null) return;
                var i = _objectsList.IndexOf(_objectsList.Where(X => X.Id == entityObject.Id).FirstOrDefault());
                if (i == -1)
                {
                    ObjectsList.Add(new ObjectActive(entityObject, true, DateTime.UtcNow));
                    _mapControl.Markers.Add(FormMarker(entityObject.Latitude, entityObject.Longitude, entityObject.Azimuth, 30, Brushes.Red, "Object " + entityObject.Id));
                }
                else
                {
                    ObjectsList[i] = new ObjectActive(entityObject, true, DateTime.UtcNow);
                    _mapControl.Markers[i].Position = new PointLatLng(entityObject.Latitude, entityObject.Longitude);
                    _mapControl.Markers[i].Shape = new AzimuthMarker(30, entityObject.Azimuth, Brushes.Red, "Object " + entityObject.Id);
                }

            }, null);
        }

        partial void OnSelectedObjectChanged(ObjectActive value) // Змінюємо центр карти на вибраний об'єкт зі списку
        {
            if (value != null)
            {
                MapCenter = new PointLatLng(value.Latitude, value.Longitude);
            }
        }

        private GMapMarker FormMarker(double lat, double lng, double azimuth, int size, Brush brush, string tooltip) // Спрощуємо формування GMapMarker
        {
            var marker = new GMapMarker(new PointLatLng(lat, lng))
            {
                Shape = new AzimuthMarker(size, azimuth, brush, tooltip)
            };
            return marker;
        }

        public void SetMapControl(GMapControl mapControl) //Отримати GMapControl оскільки він не підтримує Binding маркерів
        {
            _mapControl = mapControl;
        }

        private void CleanupInactiveObjects(object sender, EventArgs e) //Робочий цикл таймера
        {
            var now = DateTime.UtcNow;

            foreach (var mapObject in ObjectsList) //Помічаємо елемети неактивними якщо на новій ітерації даних з серверу ми їх не отримали
            {
                if (!_currentCycleIds.Contains(mapObject.Id) && mapObject.IsActive)
                {
                    var i = ObjectsList.IndexOf(ObjectsList.Where(X => X.Id == mapObject.Id).FirstOrDefault());
                    _mapControl.Markers[i].Position = new PointLatLng(mapObject.Latitude, mapObject.Longitude);
                    _mapControl.Markers[i].Shape = new AzimuthMarker(30, mapObject.Azimuth, Brushes.Gray, "Object " + mapObject.Id);
                    // Якщо об'єкт не був отриманий цього циклу, ставимо IsActive в false
                    mapObject.IsActive = false;
                }
            }
            var inactiveObjects = ObjectsList.Where(o => now - o.DateTime > TimeSpan.FromMinutes(5)).ToList(); //Видаляємо елементи які не оновлювались 5 хвилин

            foreach (var inactiveObject in inactiveObjects)
            {
                var i = ObjectsList.IndexOf(ObjectsList.Where(X => X.Id == inactiveObject.Id).FirstOrDefault());
                ObjectsList.Remove(inactiveObject);
                _mapControl.Markers.RemoveAt(i);
            }

            _currentCycleIds.Clear();
        }

        private void ExecuteExitCommand() //Кнопка EXIT
        {
            _mapControl.Markers.Clear();
            _objectsList.Clear();
            _signalRClient.LeaveGroup();
            _controlManager.Place("MainWindow", "MainRegion", "LoginControl");
        }

    }
}
