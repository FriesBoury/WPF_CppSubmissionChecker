using CppSubmissionChecker_ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;


namespace CppSubmissionChecker_View.UserControls
{
    /// <summary>
    /// Interaction logic for StudentSubmissionDetails.xaml
    /// </summary>
    public partial class StudentSubmissionDetails : UserControl
    {
        List<string> _markedFiles = new List<string>(20);

        StudentSubmission? _submissionViewModel;

        private List<Process> _runningProcesses = new List<Process>();
        public StudentSubmissionDetails()
        {
            this.DataContextChanged += StudentSubmissionDetails_DataContextChanged;
            InitializeComponent();

        }

        private void StudentSubmissionDetails_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            _markedFiles.Clear();
            if (_submissionViewModel != null)
            {
                _submissionViewModel.FinishedLoading -= _submissionViewModel_FinishedLoading;
                _submissionViewModel.UnloadRequested -= _submissionViewModel_Unloaded;
                if (_submissionViewModel.DirectoryTree != null && !string.IsNullOrEmpty(_submissionViewModel.FullDirPath))
                {
                    FindAllMarkedFiles(ref _markedFiles, _submissionViewModel.DirectoryTree);
                    string rootDir = _submissionViewModel.FullDirPath;

                    for (int i = 0; i < _markedFiles.Count; ++i)
                    {
                        int sourceIndex = _markedFiles[i].IndexOf(Preferences.ProjectRootFolderName, StringComparison.InvariantCultureIgnoreCase);
                        if (sourceIndex >= 0)
                        {

                            _markedFiles[i] = _markedFiles[i].Substring(sourceIndex);
                        }
                    }
                }
            }

            _submissionViewModel = DataContext as StudentSubmission;
            if (_submissionViewModel != null)
            {
                _submissionViewModel.FinishedLoading += _submissionViewModel_FinishedLoading;
                _submissionViewModel.UnloadRequested += _submissionViewModel_Unloaded;
            }
            _console.Text = "";
            _console2.Text = "";
            _codeViewer?.CloseAllFiles();


        }

        private void _submissionViewModel_Unloaded(StudentSubmission submission)
        {
            UnloadSubmission(submission);
        }

        async void UnloadSubmission(StudentSubmission submission)
        {
            submission.IsUnloading = true;
            for(int i= _runningProcesses.Count-1; i >= 0; --i)
            {
                _runningProcesses[i].Kill();
            }
           
            while (_runningProcesses.Count > 0)
            {
                await Task.Delay(1000);
            }
            
            submission.Loaded = false;
            submission.IsUnloading = false;
        }

        private void _submissionViewModel_FinishedLoading()
        {
            if (_submissionViewModel?.DirectoryTree != null)
            {
                OpenAllMarkedFiles(ref _markedFiles, _submissionViewModel.DirectoryTree);
            }
        }

        private void FindAllMarkedFiles(ref List<string> markedFiles, UserDirectory dir)
        {
            foreach (var file in dir.Files)
            {
                if (file.IsMarked)
                {
                    markedFiles.Add(file.FilePath);
                }
            }
            foreach (var subDir in dir.Subfolders)
            {
                FindAllMarkedFiles(ref markedFiles, subDir);
            }
        }
        private void OpenAllMarkedFiles(ref List<string> markedFiles, UserDirectory dir)
        {
            foreach (var file in dir.Files)
            {
                string? fileMatch = markedFiles.FirstOrDefault(x => file.FilePath.EndsWith(x, StringComparison.InvariantCultureIgnoreCase));
                if (fileMatch != null)
                {
                    markedFiles.Remove(fileMatch);
                    file.IsMarked = true;
                    _codeViewer.OpenFile(file.FilePath);
                }
            }
            foreach (var subDir in dir.Subfolders)
            {
                OpenAllMarkedFiles(ref markedFiles, subDir);
            }
        }

        private void _directoryTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is UserDirectory dir)
            {
                //do nothing
            }
            else if (e.NewValue is UserFile file)
            {
                SelectFile(file.FilePath);
            }
        }

        void SelectFile(string? path)
        {
            _codeViewer.OpenFile(path);
        }

        private async void BuildAndRun_Click(object sender, RoutedEventArgs e)
        {
            
            try
            {
                if (_submissionViewModel != null && !string.IsNullOrEmpty(_submissionViewModel.SelectedSolutionPath))
                {

                    _tabcontrol.SelectedIndex = 0;
                    _console.Text = "";
                    _console2.Text = "";
                    string? executablePath = null;
                    Process process = new Process();
                    process.StartInfo.FileName = Preferences.MSBuildPath;
                    process.StartInfo.Arguments = $"\"{_submissionViewModel.SelectedSolutionPath}\"";
                    if (!string.IsNullOrWhiteSpace(Preferences.BuildParams))
                    {
                        process.StartInfo.Arguments += " " + Preferences.BuildParams;
                    }
                    _runningProcesses.Add(process);
                    await RunAndMonitorProcess(process, _console, true, (outputLine) =>
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
                        _tabcontrol.SelectedIndex = 1;
                        //Run the build
                        Process runProcess = new Process();
                        _runningProcesses.Add(process);
                        runProcess.StartInfo.FileName = executablePath;
                        runProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(_submissionViewModel.SelectedSolutionPath);
                        await RunAndMonitorProcess(runProcess, _console2, true, null);
                        _runningProcesses.Remove(process);
                    }


                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Build and Run encountered an exception: " + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private async Task RunAndMonitorProcess(Process process, TextBox console, bool hideConsole, Action<string?>? receivedOutput)
        {
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = hideConsole;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            console.Clear();

            Action<string> appendText = (txt) => { console.AppendText(txt); };

            process.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
            {

                Dispatcher.Invoke(() =>
                {
                    if (!string.IsNullOrEmpty(e.Data)) appendText(e.Data + "\n");
                });
                if (receivedOutput != null)
                {
                    receivedOutput(e.Data);
                }
            });
            process.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
            {

                Dispatcher.Invoke(() => { if (!string.IsNullOrEmpty(e.Data)) appendText(e.Data); });

            });

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

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

        private async void OpenInVS_Click(object sender, RoutedEventArgs e)
        {
            if (_submissionViewModel != null && !string.IsNullOrEmpty(_submissionViewModel.SelectedSolutionPath))
            {
                Process process = new Process();
                process.StartInfo.FileName = Preferences.VisualStudioPath;
                process.StartInfo.Arguments = $"\"{_submissionViewModel.SelectedSolutionPath}\"";
                _runningProcesses.Add(process);
                process.Start();
                await process.WaitForExitAsync();

                _runningProcesses.Remove(process);

            }
        }

        private void FindInExplorer_Click(object sender, RoutedEventArgs e)
        {
            if (_submissionViewModel != null && !string.IsNullOrEmpty(_submissionViewModel.SelectedSolutionPath))
            {
                Process process = new Process();
                process.StartInfo.FileName = "explorer";
                process.StartInfo.Arguments = "/select,  " + $"\"{_submissionViewModel.SelectedSolutionPath}\"";
                process.Start();
            }
        }

        private void _directoryTree_SelectedItemChanged_1(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }
    }
}
