using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppSubmissionChecker_ViewModel.Viewmodels.FilePreview
{
    public abstract class FilePreviewBaseVM : ViewmodelBase
    {
        public RelayCommand CloseCommand { get; private set; }
        public event EventHandler? Closed;
        public string? FileName { get; set; }
        public string Path { get; private set; }
        public FilePreviewBaseVM(string path)
        {
            Path = path;
            FileName = System.IO.Path.GetFileName(path);
            CloseCommand = new RelayCommand(Close);           
        }

        public void Close()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }
    }
}
