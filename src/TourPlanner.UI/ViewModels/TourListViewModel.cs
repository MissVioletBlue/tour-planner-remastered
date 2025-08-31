using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using TourPlanner.Domain.Entities;

namespace TourPlanner.UI.ViewModels;

public class TourListViewModel : ViewModelBase
{
    public TourListViewModel(ObservableCollection<Tour> tours)
    {
        Tours = tours;
        ToursView = CollectionViewSource.GetDefaultView(Tours);
        ToursView.Filter = Filter;
    }

    public ObservableCollection<Tour> Tours { get; }
    public ICollectionView ToursView { get; }

    private Tour? _selectedTour;
    public Tour? SelectedTour
    {
        get => _selectedTour;
        set => SetProperty(ref _selectedTour, value);
    }

    private string? _searchText;
    public string? SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                ToursView.Refresh();
        }
    }

    private bool Filter(object obj)
    {
        if (obj is not Tour t) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;
        return t.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
