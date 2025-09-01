using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;
using System.IO;
using log4net;
using TourPlanner.Application.Common.Exceptions;
using TourPlanner.Application.Interfaces;
using TourPlanner.Domain.Entities;
using TourPlanner.UI.Commands;

namespace TourPlanner.UI.ViewModels;

public class TourDetailViewModel : INotifyPropertyChanged
{
    private readonly ITourService _tourService;
    private readonly ITourLogService _tourLogService;
    private readonly IReportService _reportService;
    private static readonly ILog Log = LogManager.GetLogger(typeof(TourDetailViewModel));
    private Tour? _selectedTour;
    private TourLog? _selectedLog;
    private string? _name;
    private string? _description;
    private string _from = "";
    private string _to = "";
    private string _transport = "car";
    private double _distanceKm;
    private TimeSpan _estimatedTime;
    private DateTime _logDate = DateTime.Today;
    private int _logRating = 3;
    private string? _logComment;
    private int _logDifficulty = 1;
    private double _logDistance;
    private TimeSpan _logTime;
    private bool _suppressTourSave;
    private bool _suppressLogSave;

    public ObservableCollection<TourLog> Logs { get; } = new();

    public Tour? SelectedTour
    {
        get => _selectedTour;
        set
        {
            _selectedTour = value;
            OnPropertyChanged();
            _suppressTourSave = true;
            if (value is not null)
            {
                Name = value.Name;
                Description = value.Description;
                From = value.From;
                To = value.To;
                TransportType = value.TransportType;
                DistanceKm = value.DistanceKm;
                EstimatedTime = value.EstimatedTime;
                _ = LoadLogsAsync(value.Id);
            }
            else
            {
                Name = null;
                Description = null;
                From = string.Empty;
                To = string.Empty;
                TransportType = "car";
                DistanceKm = 0;
                EstimatedTime = TimeSpan.Zero;
                Logs.Clear();
            }
            _suppressTourSave = false;
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

    public string From
    {
        get => _from;
        set { _from = value; OnPropertyChanged(); _ = SaveTourAsync(); }
    }

    public string To
    {
        get => _to;
        set { _to = value; OnPropertyChanged(); _ = SaveTourAsync(); }
    }

    public string TransportType
    {
        get => _transport;
        set { _transport = value; OnPropertyChanged(); _ = SaveTourAsync(); }
    }

    public double DistanceKm
    {
        get => _distanceKm;
        private set { _distanceKm = value; OnPropertyChanged(); }
    }

    public TimeSpan EstimatedTime
    {
        get => _estimatedTime;
        private set { _estimatedTime = value; OnPropertyChanged(); }
    }

    public TourLog? SelectedLog
    {
        get => _selectedLog;
        set
        {
            _selectedLog = value;
            OnPropertyChanged();
            _suppressLogSave = true;
            if (value is not null)
            {
                LogDate = value.Date;
                LogRating = value.Rating;
                LogComment = value.Comment;
                LogDifficulty = value.Difficulty;
                LogDistance = value.TotalDistance;
                LogTime = value.TotalTime;
            }
            _suppressLogSave = false;
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

    public string? LogComment
    {
        get => _logComment;
        set { _logComment = value; OnPropertyChanged(); _ = SaveLogAsync(); }
    }

    public int LogDifficulty
    {
        get => _logDifficulty;
        set { _logDifficulty = value; OnPropertyChanged(); _ = SaveLogAsync(); }
    }

    public double LogDistance
    {
        get => _logDistance;
        set { _logDistance = value; OnPropertyChanged(); _ = SaveLogAsync(); }
    }

    public TimeSpan LogTime
    {
        get => _logTime;
        set { _logTime = value; OnPropertyChanged(); _ = SaveLogAsync(); }
    }

    public ICommand AddLogCommand { get; }
    public ICommand DeleteLogCommand { get; }
    public ICommand TourReportCommand { get; }

    public event Action<string>? Status;
    public event Action<Tour>? TourUpdated;

    public TourDetailViewModel(ITourService tourService, ITourLogService tourLogService, IReportService reportService)
    {
        _tourService = tourService;
        _tourLogService = tourLogService;
        _reportService = reportService;

        AddLogCommand = new RelayCommand(async _ => await AddLogAsync(), _ => SelectedTour is not null);
        DeleteLogCommand = new RelayCommand(async _ => await DeleteLogAsync(), _ => SelectedLog is not null);
        TourReportCommand = new RelayCommand(async _ => await GenerateReportAsync(), _ => SelectedTour is not null);
    }

    private async Task GenerateReportAsync()
    {
        if (SelectedTour is null) return;
        try
        {
            var bytes = await _reportService.BuildTourReportAsync(SelectedTour.Id);
            var dir = Path.Combine(AppContext.BaseDirectory, "reports");
            Directory.CreateDirectory(dir);
            var name = string.Join("_", SelectedTour.Name.Split(Path.GetInvalidFileNameChars()));
            var path = Path.Combine(dir, $"{name}.pdf");
            await File.WriteAllBytesAsync(path, bytes);
            Status?.Invoke($"Report saved: {path}");
            try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); } catch { }
        }
        catch (Exception ex)
        {
            Status?.Invoke($"Report failed: {ex.Message}");
            Log.Error("Report generation failed", ex);
        }
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
        var log = await _tourLogService.CreateAsync(SelectedTour.Id, DateTime.Today, "", 1, SelectedTour.DistanceKm, SelectedTour.EstimatedTime, 3);
        Logs.Add(log);
        SelectedLog = log;
        Log.Info($"Added log to tour {SelectedTour.Name}");
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
        if (_suppressTourSave || _selectedTour is null) return;
        try
        {
            var updated = _selectedTour with
            {
                Name = Name ?? "",
                Description = Description,
                From = From,
                To = To,
                TransportType = TransportType
            };
            var saved = await _tourService.UpdateAsync(updated);
            _selectedTour = saved;
            DistanceKm = saved.DistanceKm;
            EstimatedTime = saved.EstimatedTime;
            OnPropertyChanged(nameof(SelectedTour));
            TourUpdated?.Invoke(saved);
        }
        catch (Exception ex)
        {
            Status?.Invoke($"Save failed: {ex.Message}");
            Log.Error("Failed to save tour", ex);
        }
    }

    private async Task SaveLogAsync()
    {
        if (_suppressLogSave || _selectedLog is null) return;
        try
        {
            var updated = _selectedLog with
            {
                Date = LogDate,
                Comment = LogComment,
                Rating = LogRating,
                Difficulty = LogDifficulty,
                TotalDistance = LogDistance,
                TotalTime = LogTime
            };
            await _tourLogService.UpdateAsync(updated);
            var idx = Logs.IndexOf(_selectedLog);
            if (idx >= 0) Logs[idx] = updated;
            _selectedLog = updated;
            OnPropertyChanged(nameof(SelectedLog));
        }
        catch (NotFoundException)
        {
            // Log was deleted concurrently; ignore.
        }
        catch (Exception ex)
        {
            Status?.Invoke($"Log save failed: {ex.Message}");
            Log.Error("Failed to save log", ex);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

