using SharpCompress.Archives.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppSubmissionChecker_ViewModel.Helpers
{
    public interface IArchiveUnpacker
    {
        public abstract void Unpack(ZipArchiveEntry entry, string targetDirectory, Action<float>? pctCallback);

    }
    public static class ArchiveUnpacker
    {
        
        public static IArchiveUnpacker CreateUnpacker(ZipArchiveEntry entry)
        {
            return new ArchiveUnpacker_Zip();
        }
    }

    public class ArchiveUnpacker_Zip : IArchiveUnpacker
    {
        public void Unpack(ZipArchiveEntry entry, string targetDirectory, Action<float>? pctCallback)
        {
            throw new NotImplementedException();
        }
    }

}
