using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppSubmissionChecker_ViewModel.Viewmodels
{
    public class MarkedFileTracker : ObservableObject
    {
        public ObservableCollection<string> MarkedFiles { get; private set; } = new ObservableCollection<string>();
        public void SetFileTracking(string path, bool tracked, string rootPath)
        {
            path = FilterPath(path, rootPath);
            if (tracked && MarkedFiles.Contains(path)) return;

            if (tracked)
            {
                MarkedFiles.Add(path);
            }
            else
            {
                MarkedFiles.Remove(path);
            }
        }

        string FilterPath(string path, string rootPath)
        {
            if (string.IsNullOrWhiteSpace(Preferences.ProjectRootFolderName)) return path;
            int startIndex = path.IndexOf(Preferences.ProjectRootFolderName, StringComparison.InvariantCultureIgnoreCase);
            int length = Preferences.ProjectRootFolderName.Length;
            if (startIndex < 0)
            {
                startIndex = path.IndexOf(rootPath, StringComparison.InvariantCultureIgnoreCase);
                length = rootPath.Length;
            }

            if (startIndex < 0) return path;

            return path.Substring(startIndex + length);
        }

        public bool GetFileTracking(string path, string rootPath)
        {
            path = FilterPath(path, rootPath);
            return MarkedFiles.Any(p => path.EndsWith(p, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
