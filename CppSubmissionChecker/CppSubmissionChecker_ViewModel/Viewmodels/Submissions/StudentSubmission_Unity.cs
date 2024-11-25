using CommunityToolkit.Mvvm.Input;
using CppSubmissionChecker_ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CppSubmissionChecker_ViewModel.Viewmodels.Submissions
{
    public class StudentSubmission_Unity : StudentSubmission
    {
        [DllImport("user32.dll")] private static extern int GetWindowText(int hWnd, StringBuilder title, int size);



        private List<Process> _runningProcesses = new List<Process>();
        public StudentSubmission_Unity(string name, string shortname, ZipArchiveEntry entry, MarkedFileTracker tracker) : base(name, shortname, entry, tracker)
        {
            _submissionCommands.Add(new SubmissionCommand("Open in Unity", () => { OpenProject(); }));
            this.FinishedLoading += StudentSubmission_Unity_FinishedLoading;
        }
        public StudentSubmission_Unity() : base()
        {
            this.FinishedLoading += StudentSubmission_Unity_FinishedLoading;
        }

        private void StudentSubmission_Unity_FinishedLoading()
        {
            //remove cmake solutions
            var cmakeSolutions = SolutionPaths.Where(s => s.EndsWith("CMakeLists.txt")).ToList();
            foreach (var s in cmakeSolutions)
            {
                SolutionPaths.Remove(s);
            }
        }

        public async Task OpenProject(bool exitAfterStart = false)
        {
            if (FullDirPath == null) return;

           
            var projectDir = FindSubDirectory(FullDirPath, "Assets");
            if (string.IsNullOrEmpty(projectDir))
            {
                return;
            }
 

            projectDir = projectDir.Substring(0, projectDir.LastIndexOf("\\"));
            var pStart = new ProcessStartInfo(Preferences.UnityInstallation);
            //pStart.UseShellExecute = false;
            pStart.Arguments = $" -projectPath \"{projectDir}\"";

            Process? p = Process.Start(pStart);
            if (exitAfterStart)
            {
                await Task.Run(() => MonitorProcess(p));
                p.Kill();
            }
        }

        void MonitorProcess(Process unityProcess)
        {
            while (!unityProcess.HasExited)
            {
                try
                {
                    Process other = Process.GetProcessById(unityProcess.Id);
                    string titlestr = other.MainWindowTitle;
                    string[] loadingTitles = new string[] { "Loading", "Initializing", "Compiling", "Unity Package Manager", "Open" };
                    if (!string.IsNullOrEmpty(titlestr))
                    {
                        if (!loadingTitles.Any(lt => titlestr.StartsWith(lt)))
                        {
                            break;
                        }
                    }
                    Thread.Sleep(3000);
                }
                catch
                {
                    break;
                }
            }
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
