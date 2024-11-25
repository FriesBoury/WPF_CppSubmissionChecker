using CppSubmissionChecker_ViewModel.Viewmodels.FilePreview;
using System.Collections.ObjectModel;

namespace CppSubmissionChecker_ViewModel
{
    public class CodeFileViewer_VM : ViewmodelBase
    {
        public ObservableCollection<FilePreviewBaseVM> CodeFiles { get; private set; } = new ObservableCollection<FilePreviewBaseVM>();
        public FilePreviewBaseVM? SelectedCodeFile { get; set; }
        private static FilePreviewFactory _previewFactory = new FilePreviewFactory();
        public void AddCodeFile(string? path)
        {
            if (string.IsNullOrEmpty(path)) return;

            FilePreviewBaseVM? existing = CodeFiles.FirstOrDefault(x => x.Path == path);
            if (existing == null)
            {
                FilePreviewBaseVM? file = _previewFactory.BuildFilePreviewVM(path);
                if (file == null) return;

                file.Closed += CodeFile_Closed;
                CodeFiles.Add(file);
                SelectedCodeFile = file;
                OnPropertyChanged(nameof(SelectedCodeFile));
            }
            else
            {
                SelectedCodeFile = existing;
                OnPropertyChanged(nameof(SelectedCodeFile));
            }
       
        }

        public void Clear()
        {
            CodeFiles.Clear();
            SelectedCodeFile = null;
            OnPropertyChanged(nameof(SelectedCodeFile));
        }


        private void CodeFile_Closed(object? sender, EventArgs e)
        {
            if (sender is FilePreviewBaseVM codeFileVm)
            {
                CodeFiles.Remove(codeFileVm);
                if (codeFileVm == SelectedCodeFile)
                {
                    SelectedCodeFile = null;
                }
            }

        }
    }
}
