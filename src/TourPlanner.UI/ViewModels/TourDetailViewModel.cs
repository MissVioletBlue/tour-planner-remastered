using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Serilog;
using TourPlanner.Application.Interfaces;
using TourPlanner.Domain.Entities;
using TourPlanner.UI.Commands;

namespace TourPlanner.UI.ViewModels;

public class TourDetailViewModel : INotifyPropertyChanged
{
    private readonly ITourService _tourService;
    private readonly ITourLogService _tourLogService;
    private Tour? _selectedTour;
    private TourLog? _selectedLog;
    private string? _name;
    private string? _description;
    private double _distanceKm;
    private DateTime _logDate = DateTime.Today;
    private int _logRating = 3;
    private string? _logNotes;

    public ObservableCollection<TourLog> Logs { get; } = new();

    public Tour? SelectedTour
    {
        get => _selectedTour;
        set
        {
            _selectedTour = value;
            OnPropertyChanged();
            if (value is not null)
            {
                Name = value.Name;
                Description = value.Description;
                DistanceKm = value.DistanceKm;
                _ = LoadLogsAsync(value.Id);
            }
            else
            {
                Name = null;
                Description = null;
                DistanceKm = 0;
                Logs.Clear();
            }
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string? Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); _ = SaveTourAsync(); }
    }

    public string? Description
    {
        get => _description;
        set { _description = value; OnPropertyChanged(); _ = SaveTourAsync(); }
    }

    public double DistanceKm
    {
        get => _distanceKm;
        set { _distanceKm = value; OnPropertyChanged(); _ = SaveTourAsync(); }
    }

    public TourLog? SelectedLog
    {
        get => _selectedLog;
        set
        {
            _selectedLog = value;
            OnPropertyChanged();
            if (value is not null)
            {
                LogDate = value.Date;
                LogRating = value.Rating;
                LogNotes = value.Notes;
            }
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public DateTime LogDate
    {
        get => _logDate;
        set { _logDate = value; OnPropertyChanged(); _ = SaveLogAsync(); }
    }

    public int LogRating
    {
        get => _logRating;
        set { _logRating = value; OnPropertyChanged(); _ = SaveLogAsync(); }
    }

    public string? LogNotes
    {
        get => _logNotes;
        set { _logNotes = value; OnPropertyChanged(); _ = SaveLogAsync(); }
    }

    public ICommand AddLogCommand { get; }
    public ICommand DeleteLogCommand { get; }

    public event Action<Tour>? TourUpdated;

    public TourDetailViewModel(ITourService tourService, ITourLogService tourLogService)
    {
        _tourService = tourService;
        _tourLogService = tourLogService;

        AddLogCommand = new RelayCommand(async _ => await AddLogAsync(), _ => SelectedTour is not null);
        DeleteLogCommand = new RelayCommand(async _ => await DeleteLogAsync(), _ => SelectedLog is not null);
    }

    private async Task LoadLogsAsync(Guid tourId)
    {
        Logs.Clear();
        var logs = await _tourLogService.GetByTourAsync(tourId);
        foreach (var l in logs) Logs.Add(l);
    }

    private async Task AddLogAsync()
    {
        if (SelectedTour is null) return;
        var log = await _tourLogService.CreateAsync(SelectedTour.Id, DateTime.Today, "", 3);
        Logs.Add(log);
        Log.Information("Added log to tour {TourName}", SelectedTour.Name);
    }

    private async Task DeleteLogAsync()
    {
        if (SelectedLog is null) return;
        await _tourLogService.DeleteAsync(SelectedLog.Id);
        Logs.Remove(SelectedLog);
        SelectedLog = null;
    }

    private async Task SaveTourAsync()
    {
        if (SelectedTour is null) return;
        var updated = SelectedTour with { Name = Name ?? "", Description = Description, DistanceKm = DistanceKm };
        await _tourService.UpdateAsync(updated);
        SelectedTour = updated;
        TourUpdated?.Invoke(updated);
    }

    private async Task SaveLogAsync()
    {
        if (SelectedLog is null) return;
        var updated = SelectedLog with { Date = LogDate, Notes = LogNotes, Rating = LogRating };
        await _tourLogService.UpdateAsync(updated);
        var idx = Logs.IndexOf(SelectedLog);
        if (idx >= 0) Logs[idx] = updated;
        SelectedLog = updated;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

