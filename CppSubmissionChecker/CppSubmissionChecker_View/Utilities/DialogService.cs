using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CppSubmissionChecker_ViewModel.Interfaces;

namespace CppSubmissionChecker_View.Utilities
{
	class DialogService : IDialogService
	{
		public string? OpenFile()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				Filter = "JSON files (*.json)|*.json",
				Multiselect = false
			};

			return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
		}

		public string? SaveFile()
		{
			SaveFileDialog dialog = new SaveFileDialog()
			{
				Filter = "JSON files (*.json)|*.json",
				FileName = "Matches.json"
			};
			return dialog.ShowDialog() == true ? dialog.FileName : null;
		}
	}
}
