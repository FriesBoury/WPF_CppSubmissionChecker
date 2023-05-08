using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppSubmissionChecker_ViewModel
{
    public class PreferencesWindow_VM : ViewmodelBase
    {
        public string VSLocation
        {
            get => Preferences.VisualStudioPath; set
            {
                Preferences.VisualStudioPath = value;
                OnPropertyChanged(nameof(VSLocation));
                OnPropertyChanged(nameof(IsValid));

            }
        }

        public string MSBuildLocation
        {
            get => Preferences.MSBuildPath;
            set
            {
                Preferences.MSBuildPath = value;
                OnPropertyChanged(nameof(MSBuildLocation));
                OnPropertyChanged(nameof(IsValid));

            }
        }

        public string TempFolderPath
        {
            get => Preferences.TempFolderPath;
            set
            {
                Preferences.TempFolderPath = value;
                OnPropertyChanged(nameof(TempFolderPath));
                OnPropertyChanged(nameof(IsValid));
            }
        }


        public string ProjectRootFolderName
        {
            get => Preferences.ProjectRootFolderName;
            set
            {
                Preferences.ProjectRootFolderName = value;
                OnPropertyChanged(nameof(TempFolderPath));
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public bool KeepSubmissions
        {
            get => Preferences.KeepSubmissions;
            set => Preferences.KeepSubmissions = value ;
        }

        public string BuildParams
        {
            get => Preferences.BuildParams;
            set => Preferences.BuildParams = value;
        }

        public string UnityInstallation
        {
            get => Preferences.UnityInstallation;
            set
            {
                Preferences.UnityInstallation = value;
                OnPropertyChanged(nameof(UnityInstallation));
            }
        }

        public bool IsValid => Preferences.Validate();
        public void SetVisualstudioFolder(string path)
        {
            string? vsPath = SearchFileRecursively("devenv.exe", path);
            if (!string.IsNullOrEmpty(vsPath))
            {
                VSLocation = vsPath;
            }
            string? msBuildPath = SearchFileRecursively("msbuild.exe", path);
            if (!string.IsNullOrEmpty(msBuildPath))
            {
                MSBuildLocation = msBuildPath;
            }
        }

        private static string? SearchFileRecursively(string fileName, string directoryPath)
        {
            fileName = fileName.ToLowerInvariant();
            foreach (var file in Directory.GetFiles(directoryPath))
            {
                if (file.ToLowerInvariant().EndsWith(fileName))
                {
                    return file;
                }
            }

            foreach (var dir in Directory.GetDirectories(directoryPath))
            {
                string? filePath = SearchFileRecursively(fileName, dir);
                if (!string.IsNullOrEmpty(filePath))
                {
                    return filePath;
                }
            }

            return null;
        }
    }
}
