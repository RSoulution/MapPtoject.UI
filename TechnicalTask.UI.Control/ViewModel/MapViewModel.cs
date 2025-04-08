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
    public partial class MapViewModel : ObservableObject  //ViewModel MapViewControl
    {
        private GMapControl _mapControl;
        private readonly SignalRClient _signalRClient;
        private readonly IControlManager _controlManager;
        private readonly int _markerSize;
        private readonly Brush _markerActiveColor;
        private readonly Brush _markerLostColor;
        [ObservableProperty] //Declare Bindings and Commands
        private PointLatLng mapCenter = new PointLatLng(50.4501, 30.5234); // Kyiv

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

        public MapViewModel(SignalRClient signalRClient, IControlManager controlManager, int markerSize, Brush markerActiveColor, Brush markerLostColor)
        {
            _uiContext = SynchronizationContext.Current;
            _signalRClient = signalRClient;
            _controlManager = controlManager;
            _markerSize = markerSize;
            _markerActiveColor = markerActiveColor;
            _markerLostColor = markerLostColor;
            ExitCommand = new RelayCommand(ExecuteExitCommand);
            _signalRClient.OnReceiveObjects += OnReceiveObjects;
            _cleanupTimer = new DispatcherTimer(); // We announce a timer for monitoring lost objects and their subsequent removal
            _cleanupTimer.Interval = TimeSpan.FromSeconds(10);
            _cleanupTimer.Tick += CleanupInactiveObjects;
            _currentCycleIds = new HashSet<int>(); // We remember the objects that we received in the new cycle from the server
        }

        private void OnReceiveObjects(EntObject entityObject) //Retrieving object data from the server
        {
            if (_currentCycleIds.Count == 0 && _objectsList.Count == 0)
                _cleanupTimer.Start(); //Starting the timer
            _currentCycleIds.Add(entityObject.Id);
            _uiContext.Post(_ =>
            {
                if (_mapControl == null) return;
                var i = _objectsList.IndexOf(_objectsList.Where(X => X.Id == entityObject.Id).FirstOrDefault());
                if (i == -1)
                {
                    ObjectsList.Add(new ObjectActive(entityObject, true, DateTime.UtcNow));
                    _mapControl.Markers.Add(FormMarker(entityObject.Latitude, entityObject.Longitude, entityObject.Azimuth, _markerSize, _markerActiveColor, "Object " + entityObject.Id));
                }
                else
                {
                    ObjectsList[i] = new ObjectActive(entityObject, true, DateTime.UtcNow);
                    _mapControl.Markers[i].Position = new PointLatLng(entityObject.Latitude, entityObject.Longitude);
                    _mapControl.Markers[i].Shape = new AzimuthMarker(_markerSize, entityObject.Azimuth, _markerActiveColor, "Object " + entityObject.Id);
                }

            }, null);
        }

        partial void OnSelectedObjectChanged(ObjectActive value) // Change the center of the map to the selected object from the list
        {
            if (value != null)
            {
                MapCenter = new PointLatLng(value.Latitude, value.Longitude);
            }
        }

        private GMapMarker FormMarker(double lat, double lng, double azimuth, int size, Brush brush, string tooltip) // Simplifying GMapMarker formation
        {
            var marker = new GMapMarker(new PointLatLng(lat, lng))
            {
                Shape = new AzimuthMarker(size, azimuth, brush, tooltip)
            };
            return marker;
        }

        public void SetMapControl(GMapControl mapControl) //Get GMapControl because it does not support marker binding
        {
            _mapControl = mapControl;
        }

        private void CleanupInactiveObjects(object sender, EventArgs e) //Timer duty cycle
        {
            var now = DateTime.UtcNow;

            foreach (var mapObject in ObjectsList) //We mark elements as inactive if we did not receive them from the server in the new iteration of data.
            {
                if (!_currentCycleIds.Contains(mapObject.Id) && mapObject.IsActive)
                {
                    var i = ObjectsList.IndexOf(ObjectsList.Where(X => X.Id == mapObject.Id).FirstOrDefault());
                    _mapControl.Markers[i].Position = new PointLatLng(mapObject.Latitude, mapObject.Longitude);
                    _mapControl.Markers[i].Shape = new AzimuthMarker(_markerSize, mapObject.Azimuth, _markerLostColor, "Object " + mapObject.Id);
                    // If the object was not received in this cycle, set IsActive to false
                    mapObject.IsActive = false;
                }
            }
            var inactiveObjects = ObjectsList.Where(o => now - o.DateTime > TimeSpan.FromMinutes(5)).ToList(); //We delete items that have not been updated for 5 minutes

            foreach (var inactiveObject in inactiveObjects)
            {
                var i = ObjectsList.IndexOf(ObjectsList.Where(X => X.Id == inactiveObject.Id).FirstOrDefault());
                ObjectsList.Remove(inactiveObject);
                _mapControl.Markers.RemoveAt(i);
            }

            _currentCycleIds.Clear();
        }

        private void ExecuteExitCommand() //EXIT button
        {
            _mapControl.Markers.Clear();
            _objectsList.Clear();
            _signalRClient.LeaveGroup();
            _controlManager.Place("MainWindow", "MainRegion", "LoginControl");
        }

    }
}
