using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using TourPlanner.UI.Commands;
using TourPlanner.UI.Models;

namespace TourPlanner.UI.ViewModels
{
    public class TourListViewModel : INotifyPropertyChanged
    {
        private readonly Action<string> _status;
        private string _searchText = "";
        private Tour? _selectedTour;
        private bool _isBusy;

        public ObservableCollection<Tour> Tours { get; }
        public ICollectionView ToursView { get; }

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

        public TourListViewModel(ObservableCollection<Tour> tours, Action<string> statusReporter)
        {
            Tours = tours;
            _status = statusReporter;

            ToursView = CollectionViewSource.GetDefaultView(Tours);
            ToursView.Filter = FilterBySearch;

            AddCommand = new RelayCommand(_ => Add(), _ => !IsBusy);
            DeleteCommand = new RelayCommand(_ => Delete(), _ => !IsBusy && SelectedTour is not null);
            FocusSearchCommand = new RelayCommand(_ => _status("Focus search (Ctrl+F)"));
        }

        private bool FilterBySearch(object obj)
        {
            if (obj is not Tour t) return false;
            if (string.IsNullOrWhiteSpace(SearchText)) return true;
            return t.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }

        private void Add()
        {
            try
            {
                IsBusy = true;
                var nameBase = "New Tour";
                var name = nameBase;
                int i = 1;
                while (Tours.Any(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)))
                    name = $"{nameBase} ({i++})";

                var tour = new Tour
                {
                    Name = name,
                    From = "Start",
                    To = "Finish",
                    DistanceKm = 1.0
                };
                Tours.Add(tour);
                SelectedTour = tour;
                _status($"Added: {tour.Name}");
            }
            finally { IsBusy = false; }
        }

        private void Delete()
        {
            if (SelectedTour is null) return;
            try
            {
                IsBusy = true;
                var name = SelectedTour.Name;
                Tours.Remove(SelectedTour);
                SelectedTour = null;
                _status($"Deleted: {name}");
            }
            finally { IsBusy = false; }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
