using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppSubmissionChecker_ViewModel
{

    public interface ICachingProvider
    {
        Task<string> GetStringAsync(string key);
        Task SetStringAsync(string key, string value);
        Task RemoveAsync(string key);
    }

}
