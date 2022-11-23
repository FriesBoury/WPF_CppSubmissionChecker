using CppSubmissionChecker_ViewModel.DataClasses;
using CppSubmissionChecker_ViewModel.Interfaces;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppSubmissionChecker_ViewModel
{
    public class SubmissionCheckerViewModel : ViewModelBase
    {
        public event Action<Exception>? ExceptionFired;
        private MultiSubmissionZipArchive? _selectedArchive;
        public bool HasData => _selectedArchive != null;

        public void SetMainDispatcher(IDispatcher dispatcher)
        {
            MainDispatcher = dispatcher;
        }
        public MultiSubmissionZipArchive? SelectedArchive
        {
            get => _selectedArchive;
            set
            {
                _selectedArchive = value;
                OnPropertyChanged(nameof(SelectedArchive));
                OnPropertyChanged(nameof(HasData));
            }
        }

        public void LoadArchive(ZipArchive zipArchive)
        {
            if(SelectedArchive != null)
            {
                SelectedArchive.ExceptionFired -= SelectedArchive_ExceptionFired;
            }
            SelectedArchive = new MultiSubmissionZipArchive(zipArchive);
            SelectedArchive.ExceptionFired += SelectedArchive_ExceptionFired;
        }

        private void SelectedArchive_ExceptionFired(Exception obj)
        {
            ExceptionFired?.Invoke(obj);
        }
    }
}
