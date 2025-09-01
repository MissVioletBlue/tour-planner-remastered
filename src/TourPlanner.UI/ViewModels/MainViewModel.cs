using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Extensions.Configuration;
using TourPlanner.UI.Commands;

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
        public ICommand OpenLogCommand { get; }

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
            OpenLogCommand = new RelayCommand(_ => OpenLog());
        }

        private void OpenLog()
        {
            try
            {
                var dir = Path.Combine(AppContext.BaseDirectory, "logs");
                var file = Directory.Exists(dir)
                    ? Directory.GetFiles(dir, "app-*.log").OrderByDescending(f => f).FirstOrDefault()
                    : null;
                if (file != null)
                    Process.Start(new ProcessStartInfo(file) { UseShellExecute = true });
                else
                    StatusMessage = "No log file found";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Open log failed: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
