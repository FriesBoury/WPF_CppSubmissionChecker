using CommunityToolkit.Mvvm.Input;
using CppSubmissionChecker_ViewModel.Viewmodels.TextLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppSubmissionChecker_ViewModel.Viewmodels.Submissions
{
    public class StudentSubmission_CSharp : StudentSubmission
    {
        private int _selectedOutputTab;

        public TextLog_VM BuildOutput
        {
            get; private set;
        } = new TextLog_VM();

        public TextLog_VM BuildExecutionOutput
        {
            get; private set;
        } = new TextLog_VM();

        public int SelectedOutputTabIndex
        {
            get => _selectedOutputTab;
            set
            {
                _selectedOutputTab = value;
                OnPropertyChanged(nameof(SelectedOutputTabIndex));
            }
        }

        private List<Process> _runningProcesses = new List<Process>();
        public StudentSubmission_CSharp(string studentName, ZipArchiveEntry archiveEntry, MarkedFileTracker tracker) : base(studentName, archiveEntry, tracker)
        {

            _submissionCommands.Add(new SubmissionCommand("Build And Run [Debug]", () => { BuildAndRun(false); }));
            _submissionCommands.Add(new SubmissionCommand("Build And Run [Release]", () => { BuildAndRun(true); }));

        }

        private async void BuildAndRun(bool release)
        {
            if (IsCMakeProject())
            {
                await BuildAndRun_CMake(release);
            }
            else
            {
                await BuildAndRun_MsBuild(release);
            }
        }


        private async Task BuildAndRun_MsBuild(bool release)
        {
            SelectedOutputTabIndex = 0;
            try
            {
                if (!string.IsNullOrEmpty(SelectedSolutionPath))
                {
                    string? executablePath = null;
                    Process process = new Process();
                    process.StartInfo.FileName = Preferences.MSBuildPath;
                    process.StartInfo.Arguments = $"\"{SelectedSolutionPath}\"";
                    if (!string.IsNullOrWhiteSpace(Preferences.BuildParams))
                    {
                        process.StartInfo.Arguments += " " + Preferences.BuildParams;
                    }

                    if (!process.StartInfo.Arguments.Contains("Configuration"))
                    {
                        process.StartInfo.Arguments += " /property:Configuration=" + (release ? "Release" : "Debug");
                    }

                    _runningProcesses.Add(process);
                    await RunAndMonitorProcess(process, BuildOutput, true, (outputLine) =>
                    {
                        if (outputLine != null && outputLine.Contains(".exe") && outputLine.Contains("->"))
                        {
                            executablePath = outputLine;
                            TrimExecutablePath(ref executablePath);
                        }
                    });
                    _runningProcesses.Remove(process);

                    if (executablePath != null)
                    {
                        SelectedOutputTabIndex = 1;
                        //Run the build
                        Process runProcess = new Process();
                        _runningProcesses.Add(process);
                        runProcess.StartInfo.FileName = executablePath;
                        runProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(SelectedSolutionPath);
                        await RunAndMonitorProcess(runProcess, BuildExecutionOutput, true, null);
                        _runningProcesses.Remove(process);
                    }


                }
            }
            catch (Exception exc)
            {
                SelectedOutputTabIndex = 0;
                BuildOutput.WriteLine("===========ERROR==========");
                BuildOutput.WriteLine("Build and Run encountered an exception: " + exc.Message);

            }
        }

        private async Task RunAndMonitorProcess(Process process, TextLog_VM console, bool hideConsole, Action<string?>? receivedOutput)
        {
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = hideConsole;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            console.Clear();

            Action<string> appendText = (txt) => { console.WriteLine(txt); };

            process.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data)) appendText(e.Data);
                receivedOutput?.Invoke(e.Data);
            });
            process.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
            {

                if (!string.IsNullOrEmpty(e.Data)) appendText(e.Data);

            });

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

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

        private void TrimExecutablePath(ref string executablePath)
        {
            int exeIndex = executablePath.IndexOf(".exe");
            int beforeExeIndex = executablePath.IndexOf("->", 0, exeIndex);
            if (beforeExeIndex != -1)
            {
                beforeExeIndex += 3;
                executablePath = executablePath.Substring(beforeExeIndex, exeIndex - beforeExeIndex + 4);
            }

        }

        private bool IsCMakeProject()
        {
            return SelectedSolutionPath != null && SelectedSolutionPath.EndsWith("CMakeLists.txt");
        }

        private async Task GenerateCMakeProject()
        {
            /*set -e

mkdir -p build; cd build
cmake -DCMAKE_BUILD_TYPE=Release ..
cmake --build . -j`nproc`
cd .. && SDL_VIDEODRIVER=wayland build/game */
           
        }

        private async Task BuildAndRun_CMake(bool release)
        {
            SelectedOutputTabIndex = 0;
            try
            {
                if (!string.IsNullOrEmpty(SelectedSolutionPath))
                {
                    string? executablePath = null;
                    Process process = new Process();
                    process.StartInfo.FileName = Preferences.MSBuildPath;
                    process.StartInfo.Arguments = $"\"{SelectedSolutionPath}\"";
                    if (!string.IsNullOrWhiteSpace(Preferences.BuildParams))
                    {
                        process.StartInfo.Arguments += " " + Preferences.BuildParams;
                    }

                    if (!process.StartInfo.Arguments.Contains("Configuration"))
                    {
                        process.StartInfo.Arguments += " /property:Configuration=" + (release ? "Release" : "Debug");
                    }

                    _runningProcesses.Add(process);
                    await RunAndMonitorProcess(process, BuildOutput, true, (outputLine) =>
                    {
                        if (outputLine != null && outputLine.Contains(".exe") && outputLine.Contains("->"))
                        {
                            executablePath = outputLine;
                            TrimExecutablePath(ref executablePath);
                        }
                    });
                    _runningProcesses.Remove(process);

                    if (executablePath != null)
                    {
                        SelectedOutputTabIndex = 1;
                        //Run the build
                        Process runProcess = new Process();
                        _runningProcesses.Add(process);
                        runProcess.StartInfo.FileName = executablePath;
                        runProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(SelectedSolutionPath);
                        await RunAndMonitorProcess(runProcess, BuildExecutionOutput, true, null);
                        _runningProcesses.Remove(process);
                    }


                }
            }
            catch (Exception exc)
            {
                SelectedOutputTabIndex = 0;
                BuildOutput.WriteLine("===========ERROR==========");
                BuildOutput.WriteLine("Build and Run encountered an exception: " + exc.Message);

            }
        }
    }
}
