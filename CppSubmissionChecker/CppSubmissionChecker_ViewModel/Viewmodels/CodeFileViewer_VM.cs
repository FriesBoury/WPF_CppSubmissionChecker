using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppSubmissionChecker_ViewModel
{
    public class CodeFile_VM : ViewModelBase
    {
        private bool _enableEditing;

        public event EventHandler? Closed;
        public string? Path { get; set; }
        public string? FileName { get; set; }
        public string? FileContent { get; set; }

        public bool EnableEditing
        {
            get => _enableEditing; set
            {
                _enableEditing = value;
                OnPropertyChanged(nameof(EnableEditing));
            }
        }

        public void Close()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }
    }
    public class CodeFileViewer_VM : ViewModelBase
    {
        public ObservableCollection<CodeFile_VM> CodeFiles { get; private set; } = new ObservableCollection<CodeFile_VM>();
        public CodeFile_VM? SelectedCodeFile { get; set; }
        public void AddCodeFile(string? path)
        {
            if (!IsTextFile(path)) return;

            CodeFile_VM? existing = CodeFiles.FirstOrDefault(x => x.Path == path);
            if (existing == null)
            {
                CodeFile_VM codeFile = new CodeFile_VM()
                {
                    Path = path,
                    FileName = Path.GetFileName(path),
                    FileContent = File.ReadAllText(path!)
                };
                codeFile.Closed += CodeFile_Closed;
                CodeFiles.Add(codeFile);
                SelectedCodeFile = codeFile;
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

        bool IsTextFile(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            if (path.EndsWith(".cpp"))
            {
                return true;
            }
            if (path.EndsWith(".h"))
            {
                return true;
            }
            return false;
        }
        private void CodeFile_Closed(object? sender, EventArgs e)
        {
            if (sender is CodeFile_VM codeFileVm)
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
