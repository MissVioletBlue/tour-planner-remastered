using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Serilog;
using TourPlanner.Application.Interfaces;
using TourPlanner.Domain.Entities;
using TourPlanner.UI.Commands;

namespace TourPlanner.UI.ViewModels;

public class TourListViewModel : INotifyPropertyChanged
{
    private readonly ITourService _tourService;
    private string _searchText = "";
    private Tour? _selectedTour;
    private bool _isBusy;

    public ObservableCollection<Tour> Tours { get; } = new();
    public ICollectionView ToursView { get; }

    public event Action<string>? Status;

    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; OnPropertyChanged(); ToursView.Refresh(); }
    }

    public Tour? SelectedTour
    {
        get => _selectedTour;
        set { _selectedTour = value; OnPropertyChanged(); CommandManager.InvalidateRequerySuggested(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set { _isBusy = value; OnPropertyChanged(); CommandManager.InvalidateRequerySuggested(); }
    }

    public ICommand AddCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand FocusSearchCommand { get; }

    public TourListViewModel(ITourService tourService)
    {
        _tourService = tourService;

        ToursView = CollectionViewSource.GetDefaultView(Tours);
        ToursView.Filter = FilterBySearch;

        AddCommand = new RelayCommand(async _ => await AddAsync(), _ => !IsBusy);
        DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => !IsBusy && SelectedTour is not null);
        FocusSearchCommand = new RelayCommand(_ => Status?.Invoke("Focus search (Ctrl+F)"));

        _ = LoadToursAsync();
    }

    public async Task LoadToursAsync()
    {
        try
        {
            IsBusy = true;
            Tours.Clear();
            var tours = await _tourService.GetAllAsync();
            foreach (var t in tours) Tours.Add(t);
        }
        finally { IsBusy = false; }
    }

    private bool FilterBySearch(object obj)
    {
        if (obj is not Tour t) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;
        return t.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
    }

    private async Task AddAsync()
    {
        try
        {
            IsBusy = true;
            var tour = await _tourService.CreateAsync("New Tour", null, 1.0);
            Tours.Add(tour);
            SelectedTour = tour;
            Status?.Invoke($"Added: {tour.Name}");
            Log.Information("Added tour {TourName}", tour.Name);
        }
        finally { IsBusy = false; }
    }

    private async Task DeleteAsync()
    {
        if (SelectedTour is null) return;
        try
        {
            IsBusy = true;
            var name = SelectedTour.Name;
            await _tourService.DeleteAsync(SelectedTour.Id);
            Tours.Remove(SelectedTour);
            SelectedTour = null;
            Status?.Invoke($"Deleted: {name}");
            Log.Information("Deleted tour {TourName}", name);
        }
        finally { IsBusy = false; }
    }

    public void ReplaceTour(Tour tour)
    {
        var idx = Tours.ToList().FindIndex(t => t.Id == tour.Id);
        if (idx >= 0) Tours[idx] = tour;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

