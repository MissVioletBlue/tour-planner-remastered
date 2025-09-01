using Microsoft.Web.WebView2.Wpf;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TourPlanner.UI.ViewModels;

namespace TourPlanner.UI.Views;

public partial class MapView : UserControl
{
    public MapView()
    {
        InitializeComponent();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await Map.EnsureCoreWebView2Async();
        var htmlPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "map", "index.html");
        Map.Source = new Uri(htmlPath);
        if (DataContext is MapViewModel vm)
        {
            vm.DrawRouteOnMap = async coords =>
            {
                var latlngs = coords.Select(c => new[] { c.Lat, c.Lng });
                var json = JsonSerializer.Serialize(latlngs);
                await Map.CoreWebView2.ExecuteScriptAsync($"window.drawRoute({json})");
            };
        }
    }
}
