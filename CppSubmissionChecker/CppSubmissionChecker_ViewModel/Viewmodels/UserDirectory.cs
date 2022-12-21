using System.Collections;
using System.Collections.ObjectModel;

namespace CppSubmissionChecker_ViewModel
{
    public class UserItem : ViewModelBase
    {
        private bool _isSelected;

        public UserItem(string name)
        {
            Name = name;
        }
        public string Name { get; private set; }
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
    }
    public class UserFile : UserItem
    {
        public UserFile(string path) : base(Path.GetFileName(path))
        {
            FilePath = path;
        }
        public string FilePath { get; set; }
        public bool IsMarked { get; set; }
    }

    public class UserDirectory : UserItem
    {
        private bool _isOpen;

        public List<UserFile> Files { get; private set; } = new List<UserFile>();
        public List<UserDirectory> Subfolders { get; private set; } = new List<UserDirectory>();
        public List<Object> Items { get; set; }
        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                _isOpen = value;
                OnPropertyChanged(nameof(IsOpen));
            }
        }

        public UserDirectory(string path) : base(Path.GetFileName(path))
        {
            DirectoryPath = path;

            foreach (var dir in Directory.GetDirectories(path))
            {
                Subfolders.Add(new UserDirectory(dir));
            }

            foreach (var file in Directory.GetFiles(path))
            {
                Files.Add(new UserFile(file));
            }

            Items = Subfolders.Cast<Object>().Concat(Files).ToList();
        }
        public string DirectoryPath { get; set; }

    }
}

