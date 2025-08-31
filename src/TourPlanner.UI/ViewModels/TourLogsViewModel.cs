using System.Collections.ObjectModel;
using System.Windows.Input;
using TourPlanner.Domain.Entities;
using TourPlanner.UI.Commands;

namespace TourPlanner.UI.ViewModels;

public sealed class TourLogsViewModel : ViewModelBase
{
    public ObservableCollection<TourLog> Logs { get; } = new();

    private Tour? _selectedTour;
    public Tour? SelectedTour
    {
        get => _selectedTour;
        set
        {
            if (SetProperty(ref _selectedTour, value))
            {
                Load();
                OnPropertyChanged(nameof(Header));
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    private DateTime _newDate = DateTime.Today;
    public DateTime NewDate
    {
        get => _newDate;
        set => SetProperty(ref _newDate, value);
    }

    private string? _newNotes;
    public string? NewNotes
    {
        get => _newNotes;
        set
        {
            if (SetProperty(ref _newNotes, value))
                CommandManager.InvalidateRequerySuggested();
        }
    }

    private int _newRating = 3;
    public int NewRating
    {
        get => _newRating;
        set => SetProperty(ref _newRating, value);
    }

    public ICommand AddLogCommand { get; }
    public string Header => SelectedTour is null ? "Logs" : $"Logs for {SelectedTour.Name}";

    public TourLogsViewModel()
    {
        AddLogCommand = new RelayCommand(_ => Add(), _ => SelectedTour is not null && !string.IsNullOrWhiteSpace(NewNotes));
    }

    private void Load()
    {
        Logs.Clear();
        // fake: leer – später via ITourLogService füllen
    }

    private void Add()
    {
        if (SelectedTour is null) return;
        Logs.Add(new TourLog(Guid.NewGuid(), SelectedTour.Id, NewDate, NewNotes, NewRating));
        NewNotes = string.Empty;
    }
}
