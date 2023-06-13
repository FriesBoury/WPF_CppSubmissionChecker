namespace CppSubmissionChecker_ViewModel.Data.Caching
{
    internal class FallbackCachingProvider : ICachingProvider
    {
        private readonly ICachingProvider[] _providers;

        public FallbackCachingProvider(params ICachingProvider[] providers)
        {
            _providers = providers;
        }
        public async Task<string?> GetStringAsync(string key)
        {
            string? result = null;
            foreach (var p in _providers)
            {
                result = await p.GetStringAsync(key);
                if (result != null) return result;
            }
            return result;
        }

        public async Task RemoveAsync(string key)
        {
            await _providers.First().RemoveAsync(key);
        }

        public async Task SetStringAsync(string key, string value)
        {
            await _providers.First().SetStringAsync(key, value);
        }
    }
}
