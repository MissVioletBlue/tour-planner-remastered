using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;

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
        public string DataMode { get; }

        public TourListViewModel TourList { get; }
        public TourDetailViewModel TourDetail { get; }
        public MapViewModel MapVm { get; }

        public MainViewModel(MapViewModel mapVm, TourListViewModel listVm, TourDetailViewModel detailVm, IConfiguration cfg)
        {
            MapVm = mapVm;
            TourList = listVm;
            TourDetail = detailVm;
            DataMode = cfg.GetValue("Data:UseEf", false) ? "DB: EF/Postgres" : "DB: InMemory";

            TourList.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(TourListViewModel.SelectedTour))
                {
                    TourDetail.SelectedTour = TourList.SelectedTour;
                }
            };

            TourList.Status += msg => StatusMessage = msg;
            TourDetail.Status += msg => StatusMessage = msg;
            TourDetail.TourUpdated += t => TourList.ReplaceTour(t);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
