using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourPlanner.Application.Interfaces;
using TourPlanner.Domain.Entities;
using TourPlanner.Application.Services;
using System.Collections.ObjectModel;
using TourPlanner.UI.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace TourPlanner.UI.ViewModels
{
     public class TourListViewModel : ViewModelBase
    {
        public TourListViewModel(ObservableCollection<Tour> tours)
        {
            Tours = tours;
        }
        public ObservableCollection<Tour> Tours { get; }


    }


        public abstract class ViewModelBase : INotifyPropertyChanged
    {
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }





}
