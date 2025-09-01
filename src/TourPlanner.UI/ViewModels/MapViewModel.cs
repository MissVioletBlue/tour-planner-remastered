using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TourPlanner.Application.Interfaces;
using TourPlanner.UI.Commands;

namespace TourPlanner.UI.ViewModels;

public sealed class MapViewModel : INotifyPropertyChanged
{
    private readonly IMapService _mapService;
    private string? _from;
    private string? _to;

    public Func<IEnumerable<(double Lat, double Lng)>, Task>? DrawRouteOnMap { get; set; }

    public string? From
    {
        get => _from;
        set { _from = value; OnPropertyChanged(); }
    }

    public string? To
    {
        get => _to;
        set { _to = value; OnPropertyChanged(); }
    }

    public ICommand ShowRouteCommand { get; }

    public MapViewModel(IMapService mapService)
    {
        _mapService = mapService;
        ShowRouteCommand = new RelayCommand(async _ => await ShowAsync(), _ =>
            !string.IsNullOrWhiteSpace(From) && !string.IsNullOrWhiteSpace(To));
    }

    private async Task ShowAsync()
    {
        try
        {
            var result = await _mapService.GetRouteAsync(From!, To!);
            if (result.Path.Count > 0)
            {
                if (DrawRouteOnMap != null)
                    await DrawRouteOnMap(result.Path);
            }
            else
            {
                MessageBox.Show("No route found.", "Route", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Route error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
