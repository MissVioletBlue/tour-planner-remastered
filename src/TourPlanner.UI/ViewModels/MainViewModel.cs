using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TourPlanner.UI.Models;

namespace TourPlanner.UI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _statusMessage = "Ready";

        public string Title { get; } = "TourPlanner";
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public TourListViewModel TourList { get; }
        public TourDetailViewModel TourDetail { get; }

        public MainViewModel()
        {
            var tours = new ObservableCollection<Tour>
            {
                new Tour { Name = "Vienna City Walk", From = "Stephansplatz", To = "Belvedere", DistanceKm = 4.2 },
                new Tour { Name = "Wachau Day Trip", From = "Krems", To = "Dürnstein", DistanceKm = 12.7 },
                new Tour { Name = "Salzburg Old Town", From = "Mirabell", To = "Hohensalzburg", DistanceKm = 3.8 },
            };

            TourList = new TourListViewModel(tours, msg => StatusMessage = msg);
            TourDetail = new TourDetailViewModel();
            
            TourList.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(TourListViewModel.SelectedTour))
                {
                    TourDetail.SelectedTour = TourList.SelectedTour;
                }
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

namespace TourPlanner.UI.Models
{
    using System.Collections.Generic;
    using System.Linq;

    public class Tour : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private string _name = "";
        private string _from = "";
        private string _to = "";
        private double _distanceKm;

        public Guid Id { get; } = Guid.NewGuid();

        public string Name
        {
            get => _name;
            set { _name = value; Validate(); OnPropertyChanged(); }
        }

        public string From
        {
            get => _from;
            set { _from = value; OnPropertyChanged(); }
        }

        public string To
        {
            get => _to;
            set { _to = value; OnPropertyChanged(); }
        }

        public double DistanceKm
        {
            get => _distanceKm;
            set { _distanceKm = value; OnPropertyChanged(); }
        }

        public ObservableCollection<TourLog> Logs { get; } = new();

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion

        #region Validation (INotifyDataErrorInfo)
        private readonly Dictionary<string, List<string>> _errors = new();

        public bool HasErrors => _errors.Count > 0;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public System.Collections.IEnumerable GetErrors(string? propertyName)
        {
            if (propertyName != null && _errors.TryGetValue(propertyName, out var list))
                return list;
            return Enumerable.Empty<string>();
        }

        private void Validate()
        {
            SetErrors(nameof(Name),
                string.IsNullOrWhiteSpace(Name) || Name.Trim().Length < 3
                    ? new[] { "Name must be at least 3 characters." }
                    : Array.Empty<string>());
        }

        private void SetErrors(string propertyName, IEnumerable<string> errors)
        {
            var list = errors.ToList();
            if (list.Count == 0)
            {
                if (_errors.Remove(propertyName))
                    ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
                return;
            }

            _errors[propertyName] = list;
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
        #endregion
    }

    public class TourLog : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private DateTime _date = DateTime.Today;
        private int _rating = 3;
        private string _notes = "";

        public DateTime Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(); }
        }

        public int Rating
        {
            get => _rating;
            set { _rating = value; Validate(); OnPropertyChanged(); }
        }

        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion

        #region Validation
        private readonly Dictionary<string, List<string>> _errors = new();
        public bool HasErrors => _errors.Count > 0;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public System.Collections.IEnumerable GetErrors(string? propertyName)
        {
            if (propertyName != null && _errors.TryGetValue(propertyName, out var list))
                return list;
            return Array.Empty<string>();
        }

        private void Validate()
        {
            var errs = new List<string>();
            if (Rating < 1 || Rating > 5) errs.Add("Rating must be between 1 and 5.");
            SetErrors(nameof(Rating), errs);
        }

        private void SetErrors(string propertyName, IEnumerable<string> errors)
        {
            var list = errors.ToList();
            if (list.Count == 0)
            {
                if (_errors.Remove(propertyName))
                    ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
                return;
            }

            _errors[propertyName] = list;
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
        #endregion
    }
}
