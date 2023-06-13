using CppSubmissionChecker_ViewModel.Data.Caching;
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
            _cachingProvider = new FallbackCachingProvider( new AppdataCachingProvider(), new RegistryCachingProvider());
        }
        public static string ProjectRootFolderName
        {
            get
            {
                var value = _cachingProvider.GetStringAsync("ProjectRootFolderName").GetAwaiter().GetResult();
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = "source";
                }
                return value;
            }
            set => _cachingProvider.SetStringAsync("ProjectRootFolderName", value);
        }
        public static string TempFolderPath
        {
            get => _cachingProvider.GetStringAsync("TempFolderPath").GetAwaiter().GetResult() ?? string.Empty;
            set => _cachingProvider.SetStringAsync("TempFolderPath", value);
        }

        public static string VisualStudioPath
        {
            get => _cachingProvider.GetStringAsync("VisualStudioPath").GetAwaiter().GetResult() ?? string.Empty;
            set => _cachingProvider.SetStringAsync("VisualStudioPath", value);
        }

        public static string MSBuildPath
        {
            get => _cachingProvider.GetStringAsync("MSBuildPath").GetAwaiter().GetResult() ?? string.Empty;
            set => _cachingProvider.SetStringAsync("MSBuildPath", value);
        }
        public static bool KeepSubmissions
        {
            get => _cachingProvider.GetStringAsync("KeepSubmissions").GetAwaiter().GetResult() == "true";
            set => _cachingProvider.SetStringAsync("KeepSubmissions", value ? "true" : "false");
        }

        public static string BuildParams
        {
            get => _cachingProvider.GetStringAsync(nameof(BuildParams)).GetAwaiter().GetResult() ?? string.Empty;
            set => _cachingProvider.SetStringAsync(nameof(BuildParams), value);
        }

        public static string UnityInstallation
        {
            get => _cachingProvider.GetStringAsync(nameof(UnityInstallation)).GetAwaiter().GetResult() ?? string.Empty;
            set => _cachingProvider.SetStringAsync(nameof(UnityInstallation), value);
        }

        public static string GetValue(string key)
        {
            return _cachingProvider.GetStringAsync(key).Result ?? string.Empty;
        }

        public static void SetValue(string key, string value)
        {
            _cachingProvider.SetStringAsync(key, value);
        }

        //TODO: MOVE VALIDATION TO SEPARATE SETTINGS FOR SELECTED SUBMISSION TYPES
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
            //if (string.IsNullOrWhiteSpace(UnityInstallation) || !File.Exists(UnityInstallation) || !UnityInstallation.EndsWith("Unity.exe"))
            //{
            //    return false;
            //}
            return true;
        }

    }
}
