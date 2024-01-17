using System.Diagnostics;
using System.IO.Compression;

namespace CppSubmissionChecker_ViewModel.Viewmodels.Submissions
{
    public class StudentSubmission_GPPExam : StudentSubmission_CSharp
    {
        static readonly string _batchRunnerContents = 
            "start GPP_EXAM_RELEASE.exe -x 30 -y 30 -s 53       \r\n\r\n" +
            "start GPP_EXAM_RELEASE.exe -x 1000 -y 30 -s 17     \r\n\r\n" +
            "start GPP_EXAM_RELEASE.exe -x 30 -y 600 -s 25      \r\n\r\n" +
            "start GPP_EXAM_RELEASE.exe -x 1000 -y 600 -s 78    \r\n" +
            "echo new ActiveXObject(\"WScript.Shell\").AppActivate(\"GPP_TEST_RELEASE.exe\"); > tmp.js\r\ncscript //nologo tmp.js & del tmp.js";

        public StudentSubmission_GPPExam(string studentName, ZipArchiveEntry archiveEntry, MarkedFileTracker tracker) : base(studentName, archiveEntry, tracker)
        {
            _submissionCommands.Clear();
            _submissionCommands.Add(new SubmissionCommand("Run 4 windows", Run4Windows));
        }

        async void Run4Windows()
        {
            if (string.IsNullOrEmpty(FullDirPath)) return;

            string? releaseDir = FindSubDirectory(FullDirPath, "_DEMO_RELEASE");
            if (string.IsNullOrEmpty(releaseDir)) return;

            SelectedOutputTabIndex = 1;
            string batchFilePath = Path.GetFullPath(Path.Combine(releaseDir, "4WindowExamRunner.bat"));

            //Overwrite batch file contents
            File.WriteAllText(batchFilePath, _batchRunnerContents);

            //Run the build
            Process runProcess = new Process();
            _runningProcesses.Add(runProcess);
            runProcess.StartInfo.FileName = batchFilePath;
            runProcess.StartInfo.WorkingDirectory = releaseDir;
            await RunAndMonitorProcess(runProcess, BuildExecutionOutput, true, null);
            _runningProcesses.Remove(runProcess);
        }
    }
}
