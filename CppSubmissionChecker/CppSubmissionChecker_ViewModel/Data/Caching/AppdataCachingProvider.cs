using Newtonsoft.Json;

namespace CppSubmissionChecker_ViewModel.Data.Caching
{
    internal class AppdataCachingProvider : ICachingProvider
    {
        Dictionary<string, string>? _cachedData = null;

        public async Task<string?> GetStringAsync(string key)
        {
            if (_cachedData == null)
            {
                _cachedData = await Load();
            }
            if (_cachedData.TryGetValue(key, out string? value))
                return value;
            return null;
        }

        public async Task RemoveAsync(string key)
        {
            _cachedData?.Remove(key);
            await Save();

        }

        public async Task SetStringAsync(string key, string value)
        {
            if (_cachedData != null)
            {
                _cachedData[key] = value;
            }
            await Save();
        }

        private Task<Dictionary<string, string>> Load()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DAESubmissionChecker\\config.json");
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            if (!File.Exists(path))
            {
                return Task.FromResult(new Dictionary<string, string>());
            }

            using (var reader = File.OpenText(path))
            {
                var jsonTxt = reader.ReadToEnd();
                return Task.FromResult(JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonTxt) ?? new Dictionary<string, string>());
            }

        }

        private async Task Save()
        {
            if (_cachedData == null)
                return;
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DAESubmissionChecker\\config.json");

            var jsonTxt = JsonConvert.SerializeObject(_cachedData, Formatting.Indented);
            await File.WriteAllTextAsync(path, jsonTxt);
        }
    }
}
