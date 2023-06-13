using CommunityToolkit.Mvvm.Input;
using CppSubmissionChecker_ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppSubmissionChecker_ViewModel.Viewmodels.Submissions
{
    public class StudentSubmission_Unity : StudentSubmission
    {
        new public string OpenProjectCommandText { get; private set; } = "Open In Unity";
        new public RelayCommand? OpenProjectCommand { get; private set; }
        private List<Process> _runningProcesses = new List<Process>();
        public StudentSubmission_Unity(string name, ZipArchiveEntry entry) : base(name, entry)
        {
            OpenProjectCommand = new RelayCommand(OpenProject);
        }
        public StudentSubmission_Unity() : base()
        {

        }

        public void OpenProject()
        {
            if (FullDirPath == null) return;

            string projectDir = string.Empty;
            if (SolutionPaths.Any())
            {
                projectDir = SolutionPaths.First();
            }
            else
            {
                var assetsDir = FindSubDirectory(FullDirPath, "Assets");
                if (string.IsNullOrEmpty(assetsDir))
                {
                    return;
                }
                projectDir = assetsDir;

            }

            projectDir = projectDir.Substring(0, projectDir.LastIndexOf("\\"));
            var pStart = new ProcessStartInfo(Preferences.UnityInstallation);
            //pStart.UseShellExecute = false;
            pStart.Arguments = $" -projectPath \"{projectDir}\"";

            Process? p = Process.Start(pStart);
        }

        public override async Task Unload()
        {
            await Task.Delay(10);
            //return base.Unload();
        }


        public override async Task RunProcessAsync(Process process)
        {
            _runningProcesses.Add(process);
            process.Start();
            await process.WaitForExitAsync();

            _runningProcesses.Remove(process);
        }

        public override async Task KillRunningProcesses()
        {
            for (int i = _runningProcesses.Count - 1; i >= 0; --i)
            {
                _runningProcesses[i].Kill();
            }

            while (_runningProcesses.Count > 0)
            {
                await Task.Delay(1000);
            }
        }

    }
}
