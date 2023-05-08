using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppSubmissionChecker_ViewModel.DataClasses
{
    internal class RegistryCachingProvider : ICachingProvider
    {
        private const string _registrykey = "SOFTWARE\\DAE\\SubmissionChecker";

        RegistryKey? _userPrefs;
        public RegistryCachingProvider()
        {
            _userPrefs = Registry.CurrentUser.OpenSubKey(_registrykey, true);
            if (_userPrefs == null)
            {
                _userPrefs = Registry.CurrentUser.CreateSubKey(_registrykey, true);
            }

        }
        public string? GetString(string key)
        {
            var value = _userPrefs.GetValue(key)?.ToString();
            if (value != null)
            {
                return value;
            }
            return null;
        }

        public Task<string?> GetStringAsync(string key)
        {
            return Task.FromResult(GetString(key));
        }

        public void Remove(string key)
        {
            try
            {
                _userPrefs.DeleteValue(key);
            }
            catch
            {

            }
        }

        public Task RemoveAsync(string key)
        {
            Remove(key);
            return Task.CompletedTask;
        }

        public void SetString(string key, string value)
        {
            _userPrefs.SetValue(key, value);
        }

        public Task SetStringAsync(string key, string value)
        {
            SetString(key, value);
            return Task.CompletedTask;
        }


    }
}
