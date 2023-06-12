namespace CppSubmissionChecker_ViewModel.Viewmodels.FilePreview
{
    public class CodeFile_VM : FilePreviewBaseVM
    {
        public CodeFile_VM(string path) : base(path)
        {
            FileContent = File.ReadAllText(path);
        }

        private bool _enableEditing;
        public string? FileContent { get; set; }

        public bool EnableEditing
        {
            get => _enableEditing; set
            {
                _enableEditing = value;
                OnPropertyChanged(nameof(EnableEditing));
            }
        }


    }
}
