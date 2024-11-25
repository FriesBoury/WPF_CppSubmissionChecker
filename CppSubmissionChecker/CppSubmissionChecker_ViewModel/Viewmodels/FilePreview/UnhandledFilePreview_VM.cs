namespace CppSubmissionChecker_ViewModel.Viewmodels.FilePreview
{
    public class UnhandledFilePreview_VM : FilePreviewBaseVM
    {
        public string ExceptionMessage { get; set; }
        public UnhandledFilePreview_VM(string path, string exception) : base(path)
        {
            ExceptionMessage = exception;
        }
    }
}
