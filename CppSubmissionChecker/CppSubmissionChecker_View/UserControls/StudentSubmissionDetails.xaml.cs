using CppSubmissionChecker_ViewModel;
using CppSubmissionChecker_ViewModel.Viewmodels.Submissions;
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
using System.Windows.Threading;

namespace CppSubmissionChecker_View.UserControls
{
    /// <summary>
    /// Interaction logic for StudentSubmissionDetails.xaml
    /// </summary>
    public partial class StudentSubmissionDetails : UserControl
    {

        StudentSubmission? _submissionViewModel;


        public StudentSubmissionDetails()
        {
            this.DataContextChanged += StudentSubmissionDetails_DataContextChanged;
            InitializeComponent();

        }

        private void StudentSubmissionDetails_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_submissionViewModel != null)
            {
                _submissionViewModel.FinishedLoading -= _submissionViewModel_FinishedLoading;
                _submissionViewModel.UnloadRequested -= _submissionViewModel_Unloaded;
            }

            _submissionViewModel = DataContext as StudentSubmission;
            if (_submissionViewModel != null)
            {
                _submissionViewModel.FinishedLoading += _submissionViewModel_FinishedLoading;
                _submissionViewModel.UnloadRequested += _submissionViewModel_Unloaded;
                _submissionViewModel.SetMainDispatcher(new ViewmodelDispatcher(this.Dispatcher));
            }

            _codeViewer?.CloseAllFiles();


        }

        private void _submissionViewModel_Unloaded(StudentSubmission submission)
        {
            UnloadSubmission(submission);
        }

        async void UnloadSubmission(StudentSubmission submission)
        {
            submission.IsUnloading = true;

            await submission.KillRunningProcesses();

            submission.Loaded = false;
            submission.IsUnloading = false;
        }

        private void _submissionViewModel_FinishedLoading()
        {
            if (_submissionViewModel?.DirectoryTree != null)
            {
                OpenAllMarkedFiles(_submissionViewModel.DirectoryTree);
            }
        }

        private void OpenAllMarkedFiles(UserDirectory dir)
        {
            foreach (var file in dir.Files)
            {
                if (file.IsMarked)
                {
                    _codeViewer.OpenFile(file.FilePath);
                }
            }
            foreach (var subDir in dir.Subfolders)
            {
                OpenAllMarkedFiles(subDir);
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

        //TODO: Move to VM

        private async void OpenInVS_Click(object sender, RoutedEventArgs e)
        {
            if (_submissionViewModel != null && !string.IsNullOrEmpty(_submissionViewModel.SelectedSolutionPath))
            {
                Process process = new Process();
                process.StartInfo.FileName = Preferences.VisualStudioPath;
                process.StartInfo.Arguments = $"\"{_submissionViewModel.SelectedSolutionPath}\"";
                await _submissionViewModel?.RunProcessAsync(process);


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
