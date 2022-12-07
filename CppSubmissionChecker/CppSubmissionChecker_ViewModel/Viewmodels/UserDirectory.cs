using System.Collections;
using System.Collections.ObjectModel;

namespace CppSubmissionChecker_ViewModel
{
    public class UserFile
    {
        public UserFile(string path)
        {
            FilePath = path;
        }
        public string FilePath { get; set; }
        public string Name { get { return Path.GetFileName(FilePath); } }

        public bool IsMarked { get; set; }
    }

    public class UserDirectory
    {

        public ObservableCollection<UserFile> Files { get; private set; } = new ObservableCollection<UserFile>();
        public ObservableCollection<UserDirectory> Subfolders { get; private set; } = new ObservableCollection<UserDirectory>();
        public IEnumerable Items { get { return Subfolders.Cast<Object>().Concat(Files); } }

        public UserDirectory(string path)
        {
            DirectoryPath = path;
            Name = Path.GetFileName(path);

            foreach (var dir in Directory.GetDirectories(path))
            {
                Subfolders.Add(new UserDirectory(dir));
            }

            foreach (var file in Directory.GetFiles(path))
            {
                Files.Add(new UserFile(file));
            }
        }
        public string DirectoryPath { get; set; }
        public string Name { get; private set; }

    }
}

