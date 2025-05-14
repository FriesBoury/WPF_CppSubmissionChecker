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

        public string SyntaxHighlighting
        {
            get
            {
                string extension = FileName?.Substring(FileName.LastIndexOf('.') + 1) ?? string.Empty;
                switch (extension)
                {
                    default:
                    case "cs":
                        return "c#";
                    case "xaml":
                        return "xml";
                    case "json":
                        return "json";
                }
            }
        }


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
