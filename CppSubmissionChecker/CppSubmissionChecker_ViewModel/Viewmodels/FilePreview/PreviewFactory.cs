using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppSubmissionChecker_ViewModel.Viewmodels.FilePreview
{
    public class FilePreviewFactory
    {
        private static readonly string[] _codeFileExtensions = new string[] { ".cs", ".h", ".cpp", ".json", ".txt" };
        private static readonly string[] _mediaFileExtensions = new string[] { ".mp4", ".mov", "mkv" };

        public FilePreviewBaseVM? BuildFilePreviewVM(string filePath)
        {
            
            if (IsTextFile(filePath))
            {
                return new CodeFile_VM(filePath);
            }
            if (IsMediaFile(filePath))
            {
                return new MediaFile_VM(filePath);
            }
            return null;
        }

        bool IsTextFile(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            return _codeFileExtensions.Any(ext => path.EndsWith(ext));

        }

        bool IsMediaFile(string? path)
        {
            return !string.IsNullOrEmpty(path) && _mediaFileExtensions.Any(ext => path.EndsWith(ext));
        }
    }
}
