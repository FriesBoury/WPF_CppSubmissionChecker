using CppSubmissionChecker_ViewModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CppSubmissionChecker_View
{
    internal class ViewmodelDispatcher : IDispatcher
    {
        private readonly Dispatcher _dispatcher;

        public ViewmodelDispatcher(Dispatcher dispatcher)
        {
            this._dispatcher = dispatcher;
        }
        public void Invoke(Action action)
        {
            _dispatcher.Invoke(action);
        }
    }
}
