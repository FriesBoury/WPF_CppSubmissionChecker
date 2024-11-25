using System.Diagnostics;
using System.IO.Compression;

namespace CppSubmissionChecker_ViewModel.Viewmodels.Submissions
{
    public class StudentSubmission_Default: StudentSubmission
    {
        public StudentSubmission_Default(string longName, string studentName, ZipArchiveEntry archiveEntry, MarkedFileTracker tracker) : base(longName, studentName, archiveEntry, tracker)
        {


        }

        public override Task RunProcessAsync(Process process)
        {
            throw new NotImplementedException();
        }
    }
}
