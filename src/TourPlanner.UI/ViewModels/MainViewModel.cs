using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TourPlanner.Application.Interfaces;
using TourPlanner.Domain.Entities;
using TourPlanner.UI.Commands;

namespace TourPlanner.UI.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly ITourService _tourService;

    public MainViewModel(ITourService tourService)
    {
        _tourService = tourService;
        Tours = new ObservableCollection<Tour>();
        AddTourCommand = new RelayCommand(async _ => await AddTourAsync(), _ => !string.IsNullOrWhiteSpace(NewTourName));
        _ = LoadAsync();
    }

    public string Title => "TourPlanner – MVVM Skeleton";
    public ObservableCollection<Tour> Tours { get; }

    private string? _newTourName;
    public string? NewTourName
    {
        get => _newTourName;
        set { _newTourName = value; OnPropertyChanged(); }
    }

    public ICommand AddTourCommand { get; }

    private async Task LoadAsync()
    {
        var items = await _tourService.GetAllAsync();
        foreach (var t in items) Tours.Add(t);
    }

    private async Task AddTourAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTourName)) return;
        var created = await _tourService.CreateAsync(NewTourName, null, 0);
        Tours.Add(created);
        NewTourName = string.Empty;

    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) 
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
