using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppSubmissionChecker_ViewModel.Viewmodels.TextLog
{
    public class AppendLineEventArgs : EventArgs
    {
        private readonly string _line;
        public string Line => _line;
        public AppendLineEventArgs(string line)
        {
            _line = line;
        }
    }
    public class TextLog_VM : ViewmodelBase
    {

        public event EventHandler<AppendLineEventArgs>? AppendedLine;
        public event EventHandler? Cleared;
        private StringBuilder _stringBuilder;


        public TextLog_VM()
        {
            _stringBuilder = new StringBuilder();
        }
        public void WriteLine(string? text)
        {
            if (string.IsNullOrEmpty(text)) return;

            _stringBuilder.AppendLine(text);
            OnPropertyChanged();
            AppendedLine?.Invoke(this, new AppendLineEventArgs(text));
        }

        public void Clear()
        {
            _stringBuilder.Clear();
            OnPropertyChanged();
            Cleared?.Invoke(this, EventArgs.Empty);
        }


        public override string ToString()
        {
            return _stringBuilder.ToString() ;
        }

        public static TextLog_VM operator + (TextLog_VM _vm, string value)
        {
            _vm.WriteLine(value);
            return _vm;
        }
    }
}
