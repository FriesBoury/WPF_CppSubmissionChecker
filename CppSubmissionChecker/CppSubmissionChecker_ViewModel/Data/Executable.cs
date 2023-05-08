using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppSubmissionChecker_ViewModel.Data
{
    public class Executable
    {
        public event EventHandler? Clicked;
        public string FullPath { get; private set; }
        public string FileName { get; private set; }

        public RelayCommand ClickCommand { get; private set; } 

        public Executable(string path)
        {
            FullPath = path;
            FileName = Path.GetFileName(path);

            ClickCommand = new RelayCommand(() => { Clicked?.Invoke(this, EventArgs.Empty); });
        }
    }
}
