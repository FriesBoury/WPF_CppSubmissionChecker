using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppSubmissionChecker_ViewModel.Viewmodels.FilePreview
{
    public class MediaFile_VM : FilePreviewBaseVM
    {
        public RelayCommand? TogglePlayMedia { get; private set; }

        private int _totalMilliseconds;

        public bool IsPlaying
        {
            get => _isPlaying; set
            {
                _isPlaying = value;
                OnPropertyChanged(nameof(IsPlaying));
            }
        }
        public int TotalMilliseconds
        {
            get => _totalMilliseconds;
            set
            {
                if (_totalMilliseconds == value) return;
                _totalMilliseconds = value;
                TotalTimeStr = new TimeSpan(0, 0,0, 0, _totalMilliseconds).ToString("hh\\:mm\\:ss");
                OnPropertyChanged(nameof(TotalMilliseconds));
            }
        }
        private int _currentMs;
        public int CurrentMilliseconds
        {
            get => _currentMs;
            set
            {
                _currentMs = value;
                CurrentTimeStr = new TimeSpan(0, 0,0, 0, _currentMs).ToString("hh\\:mm\\:ss");
                OnPropertyChanged(nameof(CurrentMilliseconds));
            }
        }

        string _totalTimeStr = "00:00";
        public string TotalTimeStr
        {
            get => _totalTimeStr;
            set
            {
                if(_totalTimeStr == value) return;  
                _totalTimeStr = value;
                OnPropertyChanged(nameof(TotalTimeStr));
            }
        }

        string _currentTimeStr = "00:00";
        private bool _isPlaying;

        public string CurrentTimeStr
        {
            get => _currentTimeStr;
            set
            {
                if (_currentTimeStr == value) return;
                _currentTimeStr = value;
                OnPropertyChanged(nameof(CurrentTimeStr));
            }
        }
        public MediaFile_VM(string path) : base(path)
        {
            TogglePlayMedia = new RelayCommand(() => { IsPlaying = !IsPlaying; });
        }

        public MediaFile_VM():base("")
        {
           
        }
    }
}
