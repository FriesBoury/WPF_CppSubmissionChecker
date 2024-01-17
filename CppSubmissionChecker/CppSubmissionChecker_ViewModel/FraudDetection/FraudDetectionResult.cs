using CppSubmissionChecker_ViewModel.Viewmodels.Submissions;

namespace CppSubmissionChecker_ViewModel.FraudDetection
{
    public class DiffHash
    {
        public string FileName { get; set; } = string.Empty;
        public List<string> ModifiedLines { get; private set; } = new List<string>();
        public int HashCode => GetHashCode();

     
    }
  
    public class FraudDetectionResult
    {
        public enum ResultType
        {
            Match,
            NoMatch,
            NotFound,
            Error,
        }

        public ResultType DetectionResult
        {
            get; set;
        }
        public StudentSubmission Submission { get; }
        public FraudDetectionResult(StudentSubmission submission)
        {
            Submission = submission;
            _diffHashies = new List<DiffHash>();
        }

        public List<DiffHash> _diffHashies { get; private set; }


    }
}
