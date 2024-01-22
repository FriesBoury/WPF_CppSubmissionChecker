using CommunityToolkit.Mvvm.Input;
using CppSubmissionChecker_ViewModel.DataClasses;
using CppSubmissionChecker_ViewModel.Viewmodels;
using CppSubmissionChecker_ViewModel.Viewmodels.Submissions;
using CppSubmissionChecker_ViewModel.Viewmodels.TextLog;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppSubmissionChecker_ViewModel.FraudDetection
{
    public class FraudCase
    {
        public FraudDetectionResult Item1 { get; set; }
        public FraudDetectionResult Item2 { get; set; }
        public float MatchPct { get; set; }
    }
    public class FraudDetection : ViewmodelBase
    {

        public bool AllSubmissionsExtracted { get; private set; }
        public bool GlobalDiffBuilt { get; private set; }
        public bool Working { get; private set; }

        public ObservableCollection<FraudDetectionResult> DetectionResults { get; private set; }
        public AsyncRelayCommand StartAnalyzeCommand { get; private set; }
        public AsyncRelayCommand ExtractCommand { get; private set; }
        public AsyncRelayCommand BuildDiffCommand { get; private set; }

        public TextLog_VM Logs { get; private set; }

        private const float MATCH_TRESHOLD = 0.85f;
        private const int AMT_STUDENTS_FOR_GLOBAL = 8;
        private readonly MultiSubmissionZipArchive? _archive;
        private readonly MarkedFileTracker? _fileTracker;
        private string _diffFolderPath;

        public FraudDetection()
        {
            _diffFolderPath = Path.GetFullPath(Path.Combine(Preferences.TempFolderPath, "globaldiff"));

            _archive = null;
            DetectionResults = new ObservableCollection<FraudDetectionResult>();
            StartAnalyzeCommand = new AsyncRelayCommand(Analyze, () => AllSubmissionsExtracted && GlobalDiffBuilt && !Working);
            ExtractCommand = new AsyncRelayCommand(ExtractAllSubmissions, () => !Working);
            BuildDiffCommand = new AsyncRelayCommand(BuildGlobalDiff, () => AllSubmissionsExtracted && !Working);
            Logs = new TextLog_VM();
        }
        public FraudDetection(MultiSubmissionZipArchive archive) : this()
        {
            if (!Directory.Exists(_diffFolderPath))
            {
                Directory.CreateDirectory(_diffFolderPath);
            }
            _archive = archive;
            _fileTracker = archive.MarkedFileTracker;
        }
        private void UpdateCommands()
        {
            StartAnalyzeCommand.NotifyCanExecuteChanged();
            ExtractCommand.NotifyCanExecuteChanged();
            BuildDiffCommand.NotifyCanExecuteChanged();
        }
        public async Task ExtractAllSubmissions()
        {
            if (_archive == null) return;

            Working = true;
            UpdateCommands();


            Logs.Clear();
            Logs.WriteLine("Starting to extract submissions...");
            foreach (var submission in _archive.StudentSubmissions)
            {
                Logs.WriteLine($"Extracting submission {submission.StudentName}");
                await _archive.ExtractSubmission(submission);

            }
            Logs.WriteLine($"{_archive.StudentSubmissions.Count(s => s.Loaded)} submissions extracted - {_archive.StudentSubmissions.Count(s => !s.Loaded)} failed.");

            AllSubmissionsExtracted = true;
            Working = false;
            UpdateCommands();
        }
        public async Task BuildGlobalDiff()
        {
            if (_archive == null || _fileTracker == null) return;

            Working = true;
            UpdateCommands();

            Logs.Clear();
            foreach (var file in _fileTracker.MarkedFiles)
            {
                await BuildDiff(file);
            }


            GlobalDiffBuilt = true;
            Working = false;
            UpdateCommands();
        }

        async Task BuildDiff(string markedFilePath)
        {
            if (markedFilePath.StartsWith('\\'))
            {
                markedFilePath = markedFilePath.Substring(1);
            }
            Logs.WriteLine($"Building diff for file \"{markedFilePath}\"");

            if (_archive == null) return;
            var diffBuilder = new InlineDiffBuilder();
            string diffText = string.Empty;
            int numDiffLines = -1;

            Random rand = new Random(DateTime.Now.Millisecond);
            for (int idx = 0; idx < AMT_STUDENTS_FOR_GLOBAL; ++idx)
            {
                int randIdx = rand.Next() % _archive.StudentSubmissions.Count();
                string? filePath = _archive.StudentSubmissions[randIdx].GetRelativeFile(markedFilePath);
                if (string.IsNullOrEmpty(filePath))
                    continue;

                if (string.IsNullOrEmpty(diffText))
                {
                    diffText = File.ReadAllText(filePath);
                    continue;
                }


                var model = diffBuilder.BuildDiffModel(diffText, File.ReadAllText(filePath));

                StringBuilder sb = new StringBuilder();
                int numlines = 0;
                foreach (var line in model.Lines)
                {
                    if (line.Type == DiffPlex.DiffBuilder.Model.ChangeType.Unchanged)
                    {
                        sb.AppendLine(line.Text);
                        ++numlines;
                    }
                }

                if (numDiffLines >= 0)
                {
                    Logs.WriteLine($"Diff was reduced with {numDiffLines - numlines} lines (prev: {numDiffLines} - now: {numlines} - student: {_archive.StudentSubmissions[randIdx].StudentName})");
                }

                numDiffLines = numlines;
                diffText = sb.ToString();

            }
            CreateDirectoriesRecursive(_diffFolderPath, markedFilePath);
            await File.WriteAllTextAsync(Path.Combine(_diffFolderPath, markedFilePath), diffText);
        }


        async Task AnalyzeFile(string markedFilePath, FraudDetectionResult result)
        {
            if (markedFilePath.StartsWith('\\'))
                markedFilePath = markedFilePath.Substring(1);



            var diffHash = new DiffHash() { FileName = markedFilePath };
            result._diffHashies.Add(diffHash);

            string? filePath = result.Submission.GetRelativeFile(markedFilePath);
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }
            string diffFile = File.ReadAllText(Path.Combine(_diffFolderPath, markedFilePath));
            var diff = InlineDiffBuilder.Diff(diffFile, File.ReadAllText(filePath));

            foreach (var line in diff.Lines)
            {
                if (line.Type == ChangeType.Unchanged)
                    continue;
                diffHash.ModifiedLines.Add(line.Text.Trim());
            }

        }
        public async Task Analyze()
        {
            if (_archive == null || _fileTracker == null) return;
            Logs.Clear();
            DetectionResults.Clear();
            Working = true;
            UpdateCommands();
            foreach (var submission in _archive.StudentSubmissions)
            {
                var result = await AnalyzeSubmission(submission);
                if (result != null)
                {
                    DetectionResults.Add(result);
                }
            }

            foreach (string file in _fileTracker.MarkedFiles)
            {
                string fs = file;
                if (fs.StartsWith('\\'))
                {
                    fs = file.Substring(1);
                }
                Logs.WriteLine("");
                Logs.WriteLine("");
                Logs.WriteLine($"//====================================================");
                Logs.WriteLine($"//===={fs}");
                Logs.WriteLine($"//====================================================");
                Logs.WriteLine("");

                foreach (var result in CrossReferenceMatches(fs))
                {

                    List<string> hashesA = result.Item1._diffHashies.First(h => h.FileName.Equals(fs)).ModifiedLines;
                    List<string> hashesB = result.Item2._diffHashies.First(h => h.FileName.Equals(fs)).ModifiedLines;
                    Logs.WriteLine("");
                    Logs.WriteLine($"====================================================");
                    Logs.WriteLine($"===Detected possible fraud between A: {result.Item1.Submission.StudentName} and B: {result.Item2.Submission.StudentName}");
                    Logs.WriteLine($"Pct: {result.MatchPct}");
                    Logs.WriteLine($" - Case A:\n|\t{string.Join("\n|\t", hashesA)}\n\n");
                    Logs.WriteLine($" - Case B:\n|\t{string.Join("\n|\t", hashesB)}");
                    Logs.WriteLine($"====================================================");
                    Logs.WriteLine("");
                }
                Logs.WriteLine("");
            }


            Working = false;
            UpdateCommands();
        }

        public List<FraudCase> CrossReferenceMatches(string markedFilePath)
        {
            List<FraudCase> results = new List<FraudCase>();

            for (int idx = 0; idx < DetectionResults.Count - 1; ++idx)
            {
                var dh1 = DetectionResults[idx]._diffHashies.FirstOrDefault(c => c.FileName == markedFilePath);
                if (dh1 == null) continue;


                for (int idx2 = idx + 1; idx2 < DetectionResults.Count; ++idx2)
                {
                    var dh2 = DetectionResults[idx2]._diffHashies.FirstOrDefault(c => c.FileName == markedFilePath);
                    if (dh2 == null) continue;

                    var el1 = new Queue<string>(dh1.ModifiedLines.OrderBy(x => x));
                    var el2 = new Queue<string>(dh2.ModifiedLines.OrderBy(x => x));
                    int amtMatches = 0;
                    int amtMisses = 0;
                    while (el1.Any() && el2.Any())
                    {
                        int compare = el1.Peek().CompareTo(el2.Peek());

                        if (compare > 0)
                        {
                            el2.Dequeue();

                            ++amtMisses;
                        }
                        else if (compare < 0)
                        {
                            el1.Dequeue();
                            ++amtMisses;
                        }
                        else //same
                        {
                            el1.Dequeue();
                            el2.Dequeue();
                            ++amtMatches;
                        }
                    }
                    float matchPct = (float)amtMatches / (float)(amtMatches + amtMisses);

                    if (matchPct >= MATCH_TRESHOLD)
                    {
                        results.Add(new FraudCase()
                        {
                            Item1 = DetectionResults[idx],
                            Item2 = DetectionResults[idx2],
                            MatchPct = matchPct
                        });
                    }
                }
            }
            return results;
        }

        public async Task<FraudDetectionResult?> AnalyzeSubmission(StudentSubmission submission)
        {
            if (_archive == null || _fileTracker == null) return null;
            var result = new FraudDetectionResult(submission);
            foreach (var tf in _fileTracker.MarkedFiles)
            {
                await AnalyzeFile(tf, result);
            }

            return result;
        }

        public async Task<string?> GetFileContentsAsync(string dirPath, string filename)
        {
            foreach (var file in Directory.GetFiles(dirPath))
            {
                if (file.EndsWith(filename, StringComparison.InvariantCultureIgnoreCase))
                {
                    return await File.ReadAllTextAsync(file);
                }
            }

            foreach (var subdir in Directory.GetDirectories(dirPath))
            {
                string? subDirResult = await GetFileContentsAsync(subdir, filename);
                if (subDirResult != null)
                {
                    return subDirResult;
                }
            }
            return null;
        }
        private void CreateDirectoriesRecursive(string dirPath, string relativePath)
        {
            if (!relativePath.Contains('\\'))
            {
                return;
            }

            string dirName = relativePath.Substring(0, relativePath.IndexOf('\\'));
            dirPath = Path.GetFullPath(Path.Combine(dirPath, dirName));
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            relativePath = relativePath.Substring(dirName.Length + 1);

            CreateDirectoriesRecursive(dirPath, relativePath);

        }
    }
}
