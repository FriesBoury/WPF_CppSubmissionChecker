using CppSubmissionChecker_ViewModel;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Readers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace CppSubmissionChecker_ViewModel
{

    public class StudentSubmission : ViewModelBase
    {
        // Public Properties
        public string Name { get; private set; }
        public string StudentName { get; private set; }

        public string? FullDirPath { get; private set; }
        public long Size { get; private set; }
        public UserDirectory? DirectoryTree { get; private set; }
        public ObservableCollection<string> SolutionPaths { get; private set; } = new ObservableCollection<string>();
        public string? SelectedSolutionPath
        {
            get => _selectedSolutionPath;
            set
            {
                _selectedSolutionPath = value;
                OnPropertyChanged(nameof(SelectedSolutionPath));
            }
        }

        // Private fields
        private ZipArchiveEntry? _archiveEntry;
        private string? _selectedSolutionPath;

        // Constructors

        public StudentSubmission()
        {
            //Default constructor is only used for Designer in WPF
            Name = "SubmissionName_001.zip";
            SolutionPaths.Add("SomeDir/SomeSubDir/solutionName1.sln");
            SolutionPaths.Add("SomeDir/SomeSubDir/solutionName2.sln");

            FullDirPath = "SomeDir";
            StudentName = Name.Substring(0, Name.IndexOf('_', 2));
        }
        public StudentSubmission(string studentName, ZipArchiveEntry archiveEntry)
        {
            _archiveEntry = archiveEntry;
            Name = studentName;
            int separatorIndex = Name.IndexOf('_');
            if (separatorIndex == -1)
            {
                StudentName = Name;
            }
            else
            {
                StudentName = Name.Substring(0, separatorIndex);
            }
        }

        //TODO: Move extraction functionality to a separate SubmissionArchive class per type (based on file extension)
        public bool ExtractToPath(string dirPath, bool deleteExistingFiles, Action<float>? pctCallback = null)
        {
            if (_archiveEntry == null) return false;
            try
            {
                //Clear directory
                foreach (var file in Directory.GetFiles(dirPath))
                {
                    File.Delete(file);
                }

                foreach (var dir in Directory.GetDirectories(dirPath))
                {
                    Directory.Delete(dir, true);
                }

                string archiveName = _archiveEntry.Name;
                archiveName = Path.GetFileNameWithoutExtension(archiveName);
                FullDirPath = Path.GetFullPath(Path.Combine(dirPath, archiveName));
                FullDirPath = Regex.Replace(FullDirPath, @"[^0-9a-zA-Z\._\\/:]", "");
                if (!Directory.Exists(FullDirPath))
                {
                    Directory.CreateDirectory(FullDirPath);
                }
                float progress = 0f;
                pctCallback?.Invoke(progress);
                if (_archiveEntry.Name.EndsWith(".zip"))
                {
                    using (ZipArchive archive = new ZipArchive(_archiveEntry.Open(), ZipArchiveMode.Read))
                    {
                        archive.ExtractToDirectory(FullDirPath);
                        Size = archive.Entries.Sum(x => x.Length);
                    }

                }
                else if (_archiveEntry.Name.EndsWith(".rar"))
                {
                    string tempPath = Path.GetFullPath(Path.Combine(dirPath, _archiveEntry.Name));
                    _archiveEntry.ExtractToFile(tempPath);

                    using (RarArchive archive = RarArchive.Open(tempPath))
                    {
                        long totalSize = archive.TotalUncompressSize;
                        long readSize = 0;
                        var reader = archive.ExtractAllEntries();

                        while (reader.MoveToNextEntry())
                        {

                            if (reader.Entry.IsDirectory == false)
                            {
                                readSize += reader.Entry.Size;
                                pctCallback?.Invoke((float)readSize / totalSize);
                                reader.WriteEntryToDirectory(FullDirPath, new SharpCompress.Common.ExtractionOptions() { ExtractFullPath = true, Overwrite = true });

                            }
                        };
                    }
                }
                else if (_archiveEntry.Name.EndsWith(".7z"))
                {
                    string tempPath = Path.GetFullPath(Path.Combine(dirPath, _archiveEntry.Name));
                    _archiveEntry.ExtractToFile(tempPath);
                    using (SevenZipArchive archive = SevenZipArchive.Open(tempPath))
                    {
                        long totalSize = archive.TotalUncompressSize;
                        long readSize = 0;
                        var reader = archive.ExtractAllEntries();

                        while (reader.MoveToNextEntry())
                        {

                            if (reader.Entry.IsDirectory == false)
                            {
                                readSize += reader.Entry.Size;
                                pctCallback?.Invoke((float)readSize / totalSize);
                                reader.WriteEntryToDirectory(FullDirPath, new SharpCompress.Common.ExtractionOptions() { ExtractFullPath = true, Overwrite = true });

                            }
                        };
                    }

                }
                else
                {
                    FullDirPath = Path.GetFullPath(Path.Combine(dirPath, archiveName));
                    FullDirPath = Regex.Replace(FullDirPath, @"[^0-9a-zA-Z\._\\/:]", "");
                    _archiveEntry.ExtractToFile(FullDirPath);
                }
            }
            catch (Exception e)
            {
                FullDirPath = null;
                MainDispatcher?.Invoke(() =>
                {
                    SolutionPaths.Clear();
                });
                OnPropertyChanged(nameof(DirectoryTree));

                //rethrow exception to handle in view
                throw;
            }


            if (FullDirPath != null)
            {
                //Find SLNPath
                List<string> paths = new List<string>();
                FindSolutionsInDirectory(FullDirPath, ref paths);
                MainDispatcher?.Invoke(() =>
                {
                    foreach (string p in paths)
                    {
                        SolutionPaths.Add(p);
                    }

                    SelectedSolutionPath = SolutionPaths.FirstOrDefault();
                    DirectoryTree = new UserDirectory(FullDirPath);

                    OnPropertyChanged(nameof(DirectoryTree));

                });
                return true;
            }

            return false;


        }

        //TODO: move to seperate class
        public void FindSolutionsInDirectory(string directory, ref List<string> solutions)
        {
            foreach (var file in Directory.GetFiles(directory))
            {
                if (file.EndsWith(".sln"))
                {
                    solutions.Add(file);
                }
            }

            foreach (var dir in Directory.GetDirectories(directory))
            {
                FindSolutionsInDirectory(dir, ref solutions);

            }
        }

        public string? FindSolutionInDirectory(string directory)
        {
            foreach (var file in Directory.GetFiles(directory))
            {
                if (file.EndsWith(".sln"))
                {
                    return file;
                }
            }

            foreach (var dir in Directory.GetDirectories(directory))
            {
                string? slnFile = FindSolutionInDirectory(dir);
                if (!string.IsNullOrEmpty(slnFile))
                {
                    return slnFile;
                }
            }
            return null;
        }

    }
}

