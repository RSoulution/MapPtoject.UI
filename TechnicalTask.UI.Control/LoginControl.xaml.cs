using System.Windows.Controls;

namespace TechnicalTask.UI.Control
{
    public partial class LoginControl : UserControl
    {
        public LoginControl(LoginViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
