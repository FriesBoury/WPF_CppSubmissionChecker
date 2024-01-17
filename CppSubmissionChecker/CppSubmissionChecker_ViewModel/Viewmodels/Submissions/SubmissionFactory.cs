using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppSubmissionChecker_ViewModel.Viewmodels.Submissions
{
    public class SubmissionFactory
    {
        private const string UNITY = "Unity";
        private const string CPP = "C++";
        private const string CSHARPNET = "C# .NET";
        private const string GPP_EXAM = "GPP Exam";

        private string _selectedType;

        public string[] TypeNames { get; private set; } = new string[] { CPP, CSHARPNET, UNITY, GPP_EXAM };
        public string SelectedType
        {
            get => _selectedType;
            set
            {
                _selectedType = value;
                Preferences.SetValue("SubmissionType", value);
            }
        }

        public SubmissionFactory()
        {
            _selectedType = Preferences.GetValue("SubmissionType");
            if (string.IsNullOrEmpty(_selectedType))
            {
                _selectedType = "C++";
            }
        }

        public StudentSubmission CreateSubmission(string studentName, ZipArchiveEntry entry, MarkedFileTracker tracker)
        {
            switch (SelectedType)
            {
                default:
                case CPP:
                case CSHARPNET:
                    StudentSubmission cSharp =  new StudentSubmission_CSharp(studentName, entry, tracker);
                    return cSharp;
                case UNITY:
                    return new StudentSubmission_Unity(studentName, entry, tracker);
                case GPP_EXAM:
                    return new StudentSubmission_GPPExam(studentName, entry, tracker);
            }


        }
    }
}
