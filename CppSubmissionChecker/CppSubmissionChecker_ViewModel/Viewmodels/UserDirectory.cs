using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection.Metadata.Ecma335;

namespace CppSubmissionChecker_ViewModel
{
    public class UserItem : ViewmodelBase
    {
        public event Action<string, bool>? FileMarkedChanged;

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

        protected void OnFileMarkedChanged(string filepath, bool marked)
        {
            FileMarkedChanged?.Invoke(filepath, marked);
        }
    }
    public class UserFile : UserItem
    {
        private bool _isMarked;

        public UserFile(string path) : base(Path.GetFileName(path))
        {
            FilePath = path;
        }
        public string FilePath { get; set; }
        public bool IsMarked
        {
            get => _isMarked;
            set
            {
                if (_isMarked == value) return;
                _isMarked = value;
                OnFileMarkedChanged(FilePath, _isMarked);
            }
        }
    }

    public class UserDirectory : UserItem
    {

        private bool _isOpen;

        public List<UserFile> Files { get; private set; } = new List<UserFile>();
        public List<UserDirectory> Subfolders { get; private set; } = new List<UserDirectory>();
        public List<UserItem> Items { get; set; }
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

           

            Items = Subfolders.Cast<UserItem>().Concat(Files).ToList();
            foreach(var item in Items)
            {
                item.FileMarkedChanged += OnFileMarkedChanged;
            }
        }

     

        public string DirectoryPath { get; set; }



    }
}

