using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Serilog;
using TourPlanner.Application.Contracts;
using TourPlanner.Application.Interfaces;
using TourPlanner.Domain.Entities;
using TourPlanner.UI.Commands;

namespace TourPlanner.UI.ViewModels;

public class TourListViewModel : INotifyPropertyChanged
{
    private readonly ITourService _tourService;
    private readonly Dictionary<Guid, TourSummaryDto> _stats = new();
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
            _stats.Clear();
            var tours = await _tourService.GetAllAsync();
            var summaries = await _tourService.GetSummariesAsync();
            foreach (var s in summaries) _stats[s.Id] = s;
            foreach (var t in tours) Tours.Add(t);
        }
        finally { IsBusy = false; }
    }

    private bool FilterBySearch(object obj)
    {
        if (obj is not Tour t) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;
        var text = SearchText;
        if (t.Name.Contains(text, StringComparison.OrdinalIgnoreCase) ||
            (t.Description?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false) ||
            t.From.Contains(text, StringComparison.OrdinalIgnoreCase) ||
            t.To.Contains(text, StringComparison.OrdinalIgnoreCase) ||
            t.TransportType.Contains(text, StringComparison.OrdinalIgnoreCase))
            return true;
        if (_stats.TryGetValue(t.Id, out var stat))
        {
            if (stat.Popularity.ToString().Contains(text, StringComparison.OrdinalIgnoreCase)) return true;
            if ((stat.ChildFriendliness?.ToString("F1").Contains(text, StringComparison.OrdinalIgnoreCase) ?? false)) return true;
        }
        return false;
    }

    private async Task AddAsync()
    {
        try
        {
            IsBusy = true;
            var tour = await _tourService.CreateAsync("New Tour", null, "Start", "End", "car");
            Tours.Add(tour);
            _stats[tour.Id] = new TourSummaryDto(tour.Id, tour.Name, tour.DistanceKm, 0, null, null);
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
            _stats.Remove(SelectedTour.Id);
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

