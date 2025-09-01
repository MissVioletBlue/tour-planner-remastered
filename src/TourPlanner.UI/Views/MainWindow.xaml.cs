using System.Windows;
using TourPlanner.UI.ViewModels;

namespace TourPlanner.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}