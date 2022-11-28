using CppSubmissionChecker_ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using System.Windows.Shapes;

namespace CppSubmissionChecker_View.UserControls
{
    /// <summary>
    /// Interaction logic for StudentSubmissionDetails.xaml
    /// </summary>
    public partial class StudentSubmissionDetails : UserControl
    {
        const string MSBuildPath = "C:\\Program Files\\Microsoft Visual Studio\\2022\\Enterprise\\MSBuild\\Current\\Bin\\MSBuild.exe";
        const string VSPath = "C:\\Program Files\\Microsoft Visual Studio\\2022\\Enterprise\\Common7\\IDE\\devenv.exe";
        StudentSubmission? _submissionViewModel;
        public StudentSubmissionDetails()
        {
            this.DataContextChanged += StudentSubmissionDetails_DataContextChanged;
            InitializeComponent();

        }

        private void StudentSubmissionDetails_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _submissionViewModel = DataContext as StudentSubmission;
            _console.Text = "";
            _console2.Text = "";

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
                    process.StartInfo.FileName = MSBuildPath;
                    process.StartInfo.Arguments = $"\"{_submissionViewModel.SelectedSolutionPath}\"";

                    await RunAndMonitorProcess(process, _console, true, (outputLine) =>
                    {
                        if (outputLine != null && outputLine.Contains(".exe") && outputLine.Contains("->"))
                        {
                            executablePath = outputLine;
                            TrimExecutablePath(ref executablePath);
                        }
                    });

                    if (executablePath != null)
                    {
                        _tabcontrol.SelectedIndex = 1;
                        //Run the build
                        Process runProcess = new Process();
                        runProcess.StartInfo.FileName = executablePath;
                        await RunAndMonitorProcess(runProcess, _console2, true, null);
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

            StringBuilder sb = new StringBuilder();
            Action updateTxt = () => { console.Text = sb.ToString(); };

            process.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                sb.AppendLine(e.Data);
                Dispatcher.Invoke(updateTxt);
                if (receivedOutput != null)
                {
                    receivedOutput(e.Data);
                }
            });
            process.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                sb.AppendLine(e.Data);
                Dispatcher.Invoke(updateTxt);

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

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {

        }

        private void OpenInVS_Click(object sender, RoutedEventArgs e)
        {
            if (_submissionViewModel != null && !string.IsNullOrEmpty(_submissionViewModel.SelectedSolutionPath))
            {
                Process process = new Process();
                process.StartInfo.FileName = VSPath;
                process.StartInfo.Arguments = $"\"{_submissionViewModel.SelectedSolutionPath}\"";

                process.Start();
            }
        }

        private void FindInExplorer_Click(object sender, RoutedEventArgs e)
        {
            if (_submissionViewModel != null &&!string.IsNullOrEmpty(_submissionViewModel.SelectedSolutionPath))
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
