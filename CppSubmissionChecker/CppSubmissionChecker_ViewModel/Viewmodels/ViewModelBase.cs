using CppSubmissionChecker_ViewModel.Interfaces;
using System.ComponentModel;

namespace CppSubmissionChecker_ViewModel
{
    public class ViewModelBase: INotifyPropertyChanged
    {
        protected static IDispatcher? MainDispatcher { get;  set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string? propertyChanged)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyChanged));   
        }
    }
}