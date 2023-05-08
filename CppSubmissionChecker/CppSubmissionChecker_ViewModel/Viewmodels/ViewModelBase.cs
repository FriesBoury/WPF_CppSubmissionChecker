using CommunityToolkit.Mvvm.ComponentModel;
using CppSubmissionChecker_ViewModel.Interfaces;
using System.ComponentModel;


namespace CppSubmissionChecker_ViewModel
{
    public class ViewmodelBase: ObservableObject
    {
        public void SetMainDispatcher(IDispatcher dispatcher)
        {
            MainDispatcher = dispatcher;
        }
        protected static IDispatcher? MainDispatcher { get;  set; }

    }
}