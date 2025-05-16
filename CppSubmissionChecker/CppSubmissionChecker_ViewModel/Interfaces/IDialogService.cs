using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppSubmissionChecker_ViewModel.Interfaces
{
    public interface IDialogService
    {
		string? OpenFile();
		string? SaveFile();

	}
}
