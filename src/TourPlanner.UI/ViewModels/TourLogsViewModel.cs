using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TourPlanner.Domain.Entities;
using TourPlanner.UI.Commands;

namespace TourPlanner.UI.ViewModels;

public sealed class TourLogsViewModel : INotifyPropertyChanged
{
    public ObservableCollection<TourLog> Logs { get; } = new();

    private Tour? _selectedTour;
    public Tour? SelectedTour
    {
        get => _selectedTour;
        set
        {
            if (_selectedTour != value)
            {
                _selectedTour = value;
                Load();
                OnPropertyChanged();
                OnPropertyChanged(nameof(Header));
            }
        }
    }

    public DateTime NewDate { get; set; } = DateTime.Today;
    public string? NewNotes { get; set; }
    public int NewRating { get; set; } = 3;

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
        OnPropertyChanged(nameof(NewNotes));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
