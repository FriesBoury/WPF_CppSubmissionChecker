using CommunityToolkit.Mvvm.Input;
using CppSubmissionChecker_ViewModel.DataClasses;
using CppSubmissionChecker_ViewModel.Viewmodels;
using CppSubmissionChecker_ViewModel.Viewmodels.Submissions;
using CppSubmissionChecker_ViewModel.Viewmodels.TextLog;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using SharpCompress.Common;
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

    public class FileEntry : ViewmodelBase
    {
        private string _filePath = string.Empty;

        public event EventHandler? DeleteRequested;
        public FileEntry(string path)
        {
            DeleteCommand = new RelayCommand(() => DeleteRequested?.Invoke(this, EventArgs.Empty));
            FilePath = path;
        }
        public string FilePath
        {
            get => _filePath; set
            {
                _filePath = value;
                OnPropertyChanged(nameof(FilePath));
            }
        }

        public RelayCommand DeleteCommand { get; private set; }
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

        public RelayCommand AddFileCommand { get; private set; }


        public TextLog_VM Logs { get; private set; }

        private const float MATCH_TRESHOLD = .75f
            ;
        private const int AMT_STUDENTS_FOR_GLOBAL = 8;
        private readonly MultiSubmissionZipArchive? _archive;
        private readonly MarkedFileTracker? _fileTracker;
        private string _diffFolderPath;

        public ObservableCollection<FileEntry> FilesToCheck { get; private set; }


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

            FilesToCheck = new ObservableCollection<FileEntry>(_fileTracker.MarkedFiles.Select(f => new FileEntry(f)));

            foreach (var f in FilesToCheck)
            {
                f.DeleteRequested += FileEntry_DeleteRequested;
            }

            AddFileCommand = new RelayCommand(AddFile);

        }

        private void AddFile()
        {
            FileEntry f = new FileEntry("");
            f.DeleteRequested += FileEntry_DeleteRequested;
            FilesToCheck.Add(f);

        }
        private void FileEntry_DeleteRequested(object? sender, EventArgs e)
        {
            if (sender is FileEntry f)
            {
                FilesToCheck.Remove(f);
            }
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
            foreach (var file in FilesToCheck)
            {
                await BuildDiff(file.FilePath);
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
            int amtLeft = AMT_STUDENTS_FOR_GLOBAL;

            List<StudentSubmission> submissionsForDiff = _archive.StudentSubmissions.Where(s => !string.IsNullOrEmpty(s.GetRelativeFile(markedFilePath))).OrderBy(s => rand.Next()).ToList();
            Logs.WriteLine($"found {submissionsForDiff.Count} submissions with matching file");
            if (submissionsForDiff.Count > AMT_STUDENTS_FOR_GLOBAL)
            {
                submissionsForDiff = submissionsForDiff.Take(AMT_STUDENTS_FOR_GLOBAL).ToList();
            }
            if (submissionsForDiff.Count == 0) return;

            foreach(var submission in submissionsForDiff) { 
                string? filePath = submission.GetRelativeFile(markedFilePath);
                if (string.IsNullOrEmpty(filePath))
                    continue;

                if (string.IsNullOrEmpty(diffText))
                {
                    diffText = File.ReadAllText(filePath);
                    continue;
                }

                string otherFileText = File.ReadAllText(filePath);

                var model = diffBuilder.BuildDiffModel(diffText,otherFileText);

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
                    Logs.WriteLine($"Diff was reduced with {numDiffLines - numlines} lines (prev: {numDiffLines} - now: {numlines} - student: {submission.StudentName})");
                }

                numDiffLines = numlines;
                diffText = sb.ToString();

            }
            Logs.WriteLine("Final diff:");
            Logs.WriteLine(diffText);
            Logs.WriteLine("==End of file");
            CreateDirectoriesRecursive(_diffFolderPath, markedFilePath);
            await File.WriteAllTextAsync(Path.Combine(_diffFolderPath, markedFilePath.Replace('*', '_')), diffText);
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
            string diffFile = File.ReadAllText(Path.Combine(_diffFolderPath, markedFilePath.Replace('*', '_')));
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

            foreach (var fileEntry in FilesToCheck)
            {
                string file = fileEntry.FilePath;
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
            float maxMatchPct = 0f;
            for (int idx1 = 0; idx1 < DetectionResults.Count - 1; ++idx1)
            {
                var dh1 = DetectionResults[idx1]._diffHashies.FirstOrDefault(c => c.FileName == markedFilePath);
                if (dh1 == null) continue;
                if (dh1.ModifiedLines.Count == 0) continue;

                

                for (int idx2 = idx1 + 1; idx2 < DetectionResults.Count; ++idx2)
                {
                    var dh2 = DetectionResults[idx2]._diffHashies.FirstOrDefault(c => c.FileName == markedFilePath);
                    if (dh2 == null) continue;
                    if (dh2.ModifiedLines.Count == 0) continue;
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

                    Logs.WriteLine($"= Diff analyzed: -MatchPct: {matchPct:P2} - {DetectionResults[idx1].Submission.StudentName} vs {DetectionResults[idx2].Submission.StudentName}  ");
                    if (maxMatchPct < matchPct)
                    {
                        maxMatchPct = matchPct;
                    }
                    if (matchPct >= MATCH_TRESHOLD)
                    {
                        results.Add(new FraudCase()
                        {
                            Item1 = DetectionResults[idx1],
                            Item2 = DetectionResults[idx2],
                            MatchPct = matchPct
                        });
                    }
                }
            }

            Logs.WriteLine($"=====Finished cross referencing diffs. Max match percent: {maxMatchPct:P2}");
            

            return results;
        }

        public async Task<FraudDetectionResult?> AnalyzeSubmission(StudentSubmission submission)
        {
            if (_archive == null || _fileTracker == null) return null;
            var result = new FraudDetectionResult(submission);
            foreach (var tf in FilesToCheck)
            {
                await AnalyzeFile(tf.FilePath, result);
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
