using CppSubmissionChecker_ViewModel.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppSubmissionChecker_ViewModel
{
    public static class Preferences
    {
        static ICachingProvider _cachingProvider;
        static Preferences()
        {
            _cachingProvider = new RegistryCachingProvider();
        }

        public static string TempFolderPath
        {
            get => _cachingProvider.GetStringAsync("TempFolderPath").GetAwaiter().GetResult();
            set => _cachingProvider.SetStringAsync("TempFolderPath", value);
        }

        public static string VisualStudioPath
        {
            get => _cachingProvider.GetStringAsync("VisualStudioPath").GetAwaiter().GetResult();
            set => _cachingProvider.SetStringAsync("VisualStudioPath", value);
        }

        public static string MSBuildPath
        {
            get => _cachingProvider.GetStringAsync("VisualStudioPath").GetAwaiter().GetResult();
            set => _cachingProvider.SetStringAsync("VisualStudioPath", value);
        }

        public static bool Validate()
        {
            if (string.IsNullOrWhiteSpace(TempFolderPath) || !Directory.Exists(TempFolderPath))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(VisualStudioPath) || !File.Exists(VisualStudioPath))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(MSBuildPath) || !File.Exists(MSBuildPath))
            {
                return false;
            }
            return true;
        }

    }
}
