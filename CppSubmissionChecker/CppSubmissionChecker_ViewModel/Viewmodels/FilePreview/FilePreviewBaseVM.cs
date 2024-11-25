using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CppSubmissionChecker_ViewModel.Viewmodels.FilePreview
{
    public abstract class FilePreviewBaseVM : ViewmodelBase
    {
        public RelayCommand CloseCommand { get; private set; }
        public RelayCommand ShowInExplorerCommand { get; private set; }
        public event EventHandler? Closed;
        public string? FileName { get; set; }
        public string Path { get; private set; }
        public FilePreviewBaseVM(string path)
        {
            Path = path;
            FileName = System.IO.Path.GetFileName(path);
            CloseCommand = new RelayCommand(Close);         
            ShowInExplorerCommand = new RelayCommand(ShowInExplorer);
        }

        void ShowInExplorer()
        {
            string? path = Path;
            if (string.IsNullOrEmpty(path))
                return;
            Process process = new Process();
            process.StartInfo.FileName = "explorer";
            process.StartInfo.Arguments = "/select,  " + $"\"{path}\"";
            process.Start();
        }

        public void Close()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }
    }
}
