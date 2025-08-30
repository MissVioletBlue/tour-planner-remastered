using System.Windows;
using TourPlanner.Application.Services;
using TourPlanner.Infrastructure.Repositories;
using TourPlanner.UI.ViewModels;

namespace TourPlanner.UI.Views;
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var repo = new InMemoryTourRepository();
        var service = new TourService(repo);
        DataContext = new MainViewModel(service);
    }
     

    private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {

    }
}
