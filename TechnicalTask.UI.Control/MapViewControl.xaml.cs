using System.Windows;
using System.Windows.Controls;

namespace TechnicalTask.UI.Control
{
    public partial class MapViewControl : UserControl
    {
        private MapViewModel _viewModel;
        public MapViewControl(MapViewModel mapViewModel)
        {
            InitializeComponent();
            _viewModel = mapViewModel;
        }

        private void MapLoaded(object sender, RoutedEventArgs e) 
        {
            DataContext = _viewModel;
            _viewModel.SetMapControl(this.gmap);
        }
    }
}
