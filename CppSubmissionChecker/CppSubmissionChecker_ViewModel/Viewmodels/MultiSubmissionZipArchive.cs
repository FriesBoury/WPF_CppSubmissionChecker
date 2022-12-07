using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppSubmissionChecker_ViewModel.DataClasses
{
    public class MultiSubmissionZipArchive : ViewModelBase
    {

        public event Action<Exception>? ExceptionFired;
        //Private fields
        private const string _subDirName = "SubmissionChecker";
        private ZipArchive _zipArchive;
        private List<StudentSubmission> _studentSubmissions;
        private StudentSubmission? _selectedStudentSubmission;

        //Public Properties
        public bool HasData => _studentSubmissions.Count > 0;
        public bool Loading { get; private set; }

        public float LoadingProgress { get; private set; }

        public bool HasSelectedSubmission => _selectedStudentSubmission != null && !Loading;

        public List<StudentSubmission> StudentSubmissions
        {
            get => _studentSubmissions; private set
            {
                _studentSubmissions = value;
                OnPropertyChanged(nameof(StudentSubmissions));
                OnPropertyChanged(nameof(HasData));
            }
        }
        public StudentSubmission? SelectedSubmission
        {
            get => _selectedStudentSubmission;
            set
            {
                SwapSubmission(value);
            }
        }

        async void SwapSubmission(StudentSubmission? value)
        {
            if (_selectedStudentSubmission != null)
            {
                await UnloadSubmission(_selectedStudentSubmission);
            }
            _selectedStudentSubmission = value;
            if (_selectedStudentSubmission != null)
            {
                LoadSubmission(_selectedStudentSubmission);
            }
            OnPropertyChanged(nameof(SelectedSubmission));
            OnPropertyChanged(nameof(HasSelectedSubmission));
        }

        //Constructor
        public MultiSubmissionZipArchive(ZipArchive archive)
        {
            _zipArchive = archive;
            _studentSubmissions = new List<StudentSubmission>();

            foreach (var entry in archive.Entries)
            {
                if (entry == null)
                {
                    continue;
                }
                string name = entry.Name;
                StudentSubmissions.Add(new StudentSubmission(name, entry));
            }

            StudentSubmissions = StudentSubmissions.OrderBy(x => x.StudentName).ToList();
        }

        private void SetLoadingProgress(float progress)
        {
            LoadingProgress = progress*100;
            OnPropertyChanged(nameof(LoadingProgress));
        }

        async Task UnloadSubmission(StudentSubmission submission)
        {
            await submission.Unload();
        }
        async void LoadSubmission(StudentSubmission submission)
        {

            Loading = true;
            OnPropertyChanged(nameof(Loading));
            OnPropertyChanged(nameof(HasSelectedSubmission));

            try
            {
                string fullPath = Path.GetFullPath(Path.Combine(Preferences.TempFolderPath, _subDirName));
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }
                await Task.Run(() =>
                {
                    submission.ExtractToPath(fullPath, true, SetLoadingProgress);
                });
            }
            catch (Exception e)
            {
                ExceptionFired?.Invoke(e);
            }

            Loading = false;
            OnPropertyChanged(nameof(Loading));
            OnPropertyChanged(nameof(HasSelectedSubmission));
        }
    }
}
