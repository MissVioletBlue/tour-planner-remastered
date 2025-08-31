using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TourPlanner.Application.Interfaces;
using TourPlanner.Domain.Entities;

namespace TourPlanner.UI.ViewModels;

public sealed class MainViewModel
{
    private readonly ITourService _tourService;

    public MainViewModel(ITourService tourService)
    {
        _tourService = tourService;
        Tours = new ObservableCollection<Tour>();
        TourListVm = new TourListViewModel(Tours);
        TourLogsVm = new TourLogsViewModel();

        TourListVm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TourListViewModel.SelectedTour))
                TourLogsVm.SelectedTour = TourListVm.SelectedTour;
        };

        _ = LoadAsync();
    }

    public ObservableCollection<Tour> Tours { get; }
    public TourListViewModel TourListVm { get; }
    public TourLogsViewModel TourLogsVm { get; }

    private async Task LoadAsync()
    {
        var items = await _tourService.GetAllAsync();
        foreach (var t in items)
            Tours.Add(t);
    }
}
