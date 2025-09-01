using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Serilog;
using TourPlanner.UI.Commands;
using TourPlanner.UI.Models;

namespace TourPlanner.UI.ViewModels
{
    public class TourDetailViewModel : INotifyPropertyChanged
    {
        private Tour? _selectedTour;
        private TourLog? _selectedLog;

        public Tour? SelectedTour
        {
            get => _selectedTour;
            set
            {
                _selectedTour = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Logs));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ObservableCollection<TourLog>? Logs => SelectedTour?.Logs;

        public TourLog? SelectedLog
        {
            get => _selectedLog;
            set
            {
                _selectedLog = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand AddLogCommand { get; }
        public ICommand DeleteLogCommand { get; }

        public TourDetailViewModel()
        {
            AddLogCommand = new RelayCommand(_ => AddLog(), _ => SelectedTour is not null);
            DeleteLogCommand = new RelayCommand(_ => DeleteLog(), _ => SelectedLog is not null);
        }

        private void AddLog()
        {
            if (SelectedTour is null) return;
            var log = new TourLog
            {
                Date = DateTime.Today,
                Rating = 3,
                Notes = "New log entry"
            };
            SelectedTour.Logs.Add(log);
            Log.Information("Added log to tour {TourName}", SelectedTour.Name);
            OnPropertyChanged(nameof(Logs));
            CommandManager.InvalidateRequerySuggested();
        }

        private void DeleteLog()
        {
            if (SelectedTour is null || SelectedLog is null) return;
            SelectedTour.Logs.Remove(SelectedLog);
            Log.Information("Deleted log from tour {TourName}", SelectedTour.Name);
            SelectedLog = null;
            OnPropertyChanged(nameof(Logs));
            CommandManager.InvalidateRequerySuggested();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
