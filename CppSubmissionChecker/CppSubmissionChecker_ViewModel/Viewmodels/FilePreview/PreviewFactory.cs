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
        private static readonly string[] _codeFileExtensions = new string[] { ".cs", ".h", ".cpp", ".json", ".txt", ".xaml", ".htm", ".url" };
        private static readonly string[] _mediaFileExtensions = new string[] { ".mp4", ".mov", ".mkv" };
        private static readonly string[] _imageFileExtensions = new string[] { ".png", ".jpg", ".jpeg" };

        private static readonly Dictionary<string, Type> _typesByExtension;
        static FilePreviewFactory()
        {
            _typesByExtension = new Dictionary<string, Type>()
            {
                //codefiles
                {".cs", typeof(CodeFile_VM)},
                {".h", typeof(CodeFile_VM)},
                {".cpp", typeof(CodeFile_VM)},
                {".json", typeof(CodeFile_VM)},
                {".txt", typeof(CodeFile_VM)},
                {".htm", typeof(CodeFile_VM)},
                {".html", typeof(CodeFile_VM)},
                {".url", typeof(CodeFile_VM)},
                {".xaml", typeof(CodeFile_VM)},
                {".axaml", typeof(CodeFile_VM)},
                {".gitignore", typeof(CodeFile_VM)},
                {".md", typeof(CodeFile_VM)},
                {".prefab", typeof(CodeFile_VM)},
                {".meta", typeof(CodeFile_VM)},

                //mediafiles
                {".mp4", typeof(MediaFile_VM)},
                {".mov", typeof(MediaFile_VM)},
                {".mkv", typeof(MediaFile_VM)},

                //Imagefiles
                { ".png", typeof(ImageFile_VM)},
                {".jpg", typeof(ImageFile_VM)},
                {".jpeg", typeof(ImageFile_VM)},

                //Excel files
                {".xlsx", typeof(ExcelFile_VM) },
            };
        }

        public FilePreviewBaseVM? BuildFilePreviewVM(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return null;
            int extIndex = filePath.LastIndexOf('.');
            if (extIndex < 0) return null;

            string extension = filePath.Substring(extIndex);
            Type? typeMatch = _typesByExtension!.GetValueOrDefault<string, Type?>(extension, null);
            if (typeMatch == null) return new UnhandledFilePreview_VM(filePath, "File type is not handled");

            object[] constructorArgs = new[] { filePath };
            try
            {
                return Activator.CreateInstance(typeMatch, constructorArgs) as FilePreviewBaseVM;
            }
            catch (Exception e)
            {
                return new UnhandledFilePreview_VM(filePath, e.Message);
            }
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

        bool IsImageFile(string? path)
        {
            return !string.IsNullOrEmpty(path) && _imageFileExtensions.Any(ext => path.EndsWith(ext));
        }
    }
}
