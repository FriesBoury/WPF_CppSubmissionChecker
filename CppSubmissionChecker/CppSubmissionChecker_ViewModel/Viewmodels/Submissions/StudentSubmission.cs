using CommunityToolkit.Mvvm.Input;
using CppSubmissionChecker_ViewModel.Data;
using CppSubmissionChecker_ViewModel.Interfaces;
using CppSubmissionChecker_ViewModel.Viewmodels.TextLog;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Readers;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace CppSubmissionChecker_ViewModel.Viewmodels.Submissions
{
    public class SubmissionCommand : ViewmodelBase
    {
        public string CommandText { get; private set; } = "Build and Run";
        public RelayCommand Command { get; private set; }

        public SubmissionCommand(string text, Action command, Func<bool>? condition = null)
        {
            CommandText = text;
            Command = condition == null ? new RelayCommand(command) : new RelayCommand(command, condition);
        }
    }

    public abstract class StudentSubmission : ViewmodelBase
    {
        public event Action<string, bool> FileMarkedChanged;
        public event Action<StudentSubmission>? UnloadRequested;
        public event Action? FinishedLoading;

        public bool Loaded { get; set; } = false;
        // Public Properties
        public string Name { get; private set; }
        public string StudentName { get; private set; }
        public string? FullDirPath { get; private set; }
        public long Size { get; private set; }
        public UserDirectory? DirectoryTree { get; private set; }
        public ObservableCollection<string> SolutionPaths { get; private set; } = new ObservableCollection<string>();
        public ObservableCollection<Executable> Executables { get; private set; } = new ObservableCollection<Executable>();


        //To override in inherited classes
        public ReadOnlyCollection<SubmissionCommand> SubmissionCommands => _submissionCommands.AsReadOnly();
        protected List<SubmissionCommand> _submissionCommands = new List<SubmissionCommand>();

        public TextLog_VM ExecutionOutput
        {
            get; private set;
        } = new TextLog_VM();

        public bool IsUnloading
        {
            get => _isUnloading; set
            {
                _isUnloading = value;
                OnPropertyChanged(nameof(IsUnloading));
            }
        }

        public string? SelectedSolutionPath
        {
            get => _selectedSolutionPath;
            set
            {
                _selectedSolutionPath = value;
                OnPropertyChanged(nameof(SelectedSolutionPath));
            }
        }

        public MarkedFileTracker Tracker => _tracker;

        // Private fields
        private ZipArchiveEntry? _archiveEntry;
        private readonly MarkedFileTracker _tracker;
        private string? _selectedSolutionPath;
        private bool _isUnloading;

        // Constructors

        public StudentSubmission()
        {
            //Default constructor is only used for Designer in WPF
            Name = "SubmissionName_001.zip";
            SolutionPaths.Add("SomeDir/SomeSubDir/solutionName1.sln");
            SolutionPaths.Add("SomeDir/SomeSubDir/solutionName2.sln");

            Executables.Add(new Executable("C:\\Test\\TestExecutable.exe"));
            Executables.Add(new Executable("C:\\Test2\\TestExecutable2.exe"));

            FullDirPath = "SomeDir";
            StudentName = Name.Substring(0, Name.IndexOf('_', 2));
        }
        public StudentSubmission(string studentName, ZipArchiveEntry archiveEntry, MarkedFileTracker tracker)
        {
            _archiveEntry = archiveEntry;
            _tracker = tracker;
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

        public virtual Task KillRunningProcesses()
        {
            return Task.CompletedTask;
        }

        public virtual async Task Unload()
        {
            UnloadRequested?.Invoke(this);


            while (Loaded)
            {
                await Task.Delay(1000);
            }
        }
        //TODO: Move extraction functionality to a separate SubmissionArchive class per type (based on file extension)
        public bool ExtractToPath(string dirPath, bool deleteExistingFiles, Action<float>? pctCallback = null)
        {
            if (_archiveEntry == null)
                return false;

            try
            {
                if (deleteExistingFiles)
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
                }

                string archiveName = _archiveEntry.Name;
                archiveName = Path.GetFileNameWithoutExtension(archiveName);
                if (archiveName.Count(c => c == '_') > 1) //truncate to 2nd _
                {
                    int firstIdx = archiveName.IndexOf('_');
                    string shortName = archiveName.Substring(0, archiveName.IndexOf('_', firstIdx + 1));
                    archiveName = shortName;
                }

                FullDirPath = Path.GetFullPath(Path.Combine(dirPath, archiveName));
                FullDirPath = Regex.Replace(FullDirPath, @"[^0-9a-zA-Z\._\\/:]", "");
                if (!Directory.Exists(FullDirPath))
                {
                    Directory.CreateDirectory(FullDirPath);

                    float progress = 0f;
                    pctCallback?.Invoke(progress);
                    if (_archiveEntry.Name.EndsWith(".zip"))
                    {
                        using ( ZipArchive archive = new ZipArchive(_archiveEntry.Open(), ZipArchiveMode.Read))
                        {
                            archive.ExtractToDirectory(FullDirPath);
                            Size = archive.Entries.Sum(x => x.Length);
                        }

                    }
                    else if (_archiveEntry.Name.EndsWith(".rar"))
                    {
                        string tempPath = Path.GetFullPath(Path.Combine(dirPath, _archiveEntry.Name));
                        _archiveEntry.ExtractToFile(tempPath, true);

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
                        _archiveEntry.ExtractToFile(tempPath, true);
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
                    else //not an archive -> extract to file
                    {
                        string tempPath = Path.GetFullPath(Path.Combine(FullDirPath, _archiveEntry.Name));
                        _archiveEntry.ExtractToFile(tempPath, true);


                    }
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
                FindFilesWithExtension(FullDirPath, ".sln", ref paths);

                List<string> cmake = new List<string>();
                //Find CMAkeLists.txt
                FindFilesWithExtension(FullDirPath, "CMakelists.txt", ref cmake);

                //Find Exe's
                List<string> executables = new List<string>();
                FindFilesWithExtension(FullDirPath, ".exe", ref executables);

                MainDispatcher?.Invoke(() =>
                {
                    SolutionPaths.Clear();
                    Executables.Clear();
                    foreach (string exe in executables)
                    {
                        var exeObj = new Executable(exe);
                        exeObj.Clicked += Executable_Clicked;
                        Executables.Add(exeObj);
                    }

                    foreach (string p in paths)
                    {
                        SolutionPaths.Add(p);
                    }

                    foreach (string p in cmake)
                    {
                        SolutionPaths.Add(p);
                    }

                    if (cmake.Any())
                    {
                        SelectedSolutionPath = cmake.FirstOrDefault();
                    }
                    else
                    {
                        SelectedSolutionPath = SolutionPaths.FirstOrDefault();
                    }
                    DirectoryTree = new UserDirectory(FullDirPath);
                    MarkFilesInDirectory(DirectoryTree);
                    DirectoryTree.FileMarkedChanged += DirectoryTree_FileMarkedChanged;
                    FinishedLoading?.Invoke();
                    OnPropertyChanged(nameof(DirectoryTree));

                });


                Loaded = true;
                return true;
            }

            return false;
        }
        private bool MarkFilesInDirectory(UserDirectory directory)
        {
            bool markedAny = false;
            foreach (var file in directory.Files)
            {
                if (_tracker.GetFileTracking(file.FilePath, this.FullDirPath!))
                {
                    file.IsMarked = true;
                    markedAny = true;
                }
            }
            foreach (var dir in directory.Subfolders)
            {
                markedAny |= MarkFilesInDirectory(dir);
            }

            directory.IsOpen = markedAny;
            return markedAny;
        }

        private void DirectoryTree_FileMarkedChanged(string path, bool marked)
        {
            _tracker.SetFileTracking(path, marked, this.FullDirPath!);
        }

        private void Executable_Clicked(object? sender, EventArgs e)
        {
            RunExecutable(sender as Executable);
        }

        public async void RunExecutable(Executable? exe)
        {
            if (exe == null)
                return;
            if (string.IsNullOrEmpty(exe.FullPath))
                return;

            ProcessStartInfo p = new ProcessStartInfo(exe.FullPath);
            p.RedirectStandardError = true;
            p.RedirectStandardOutput = true;
            p.UseShellExecute = false;
            p.CreateNoWindow = true;
            p.StandardOutputEncoding = Encoding.UTF8;

            ExecutionOutput.Clear();

            var process = Process.Start(p);
            if (process == null)
            {
                return;
            }

            process.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
            {

                ExecutionOutput.WriteLine(e.Data);

            });
            process.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
            {

                ExecutionOutput.WriteLine(e.Data);

            });

            //process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();
        }

        //TODO: move to seperate class
        public string? FindSubDirectory(string rootDirectory, string subDirectorName)
        {
            if (rootDirectory.EndsWith(subDirectorName, StringComparison.InvariantCultureIgnoreCase))
            {
                return rootDirectory;
            }
            foreach (var dir in Directory.GetDirectories(rootDirectory))
            {
                string? match = FindSubDirectory(dir, subDirectorName);
                if (!string.IsNullOrEmpty(match))
                {
                    return match;
                }

            }
            return null;
        }
        public void FindFilesWithExtension(string directory, string extension, ref List<string> solutions)
        {
            foreach (var file in Directory.GetFiles(directory))
            {
                if (file.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase))
                {
                    solutions.Add(file);
                }
            }

            foreach (var dir in Directory.GetDirectories(directory))
            {
                FindFilesWithExtension(dir, extension, ref solutions);

            }
        }

        public string? FindSolutionInDirectory(string directory, string extension)
        {
            foreach (var file in Directory.GetFiles(directory))
            {
                if (file.EndsWith(extension))
                {
                    return file;
                }
            }

            foreach (var dir in Directory.GetDirectories(directory))
            {
                string? slnFile = FindSolutionInDirectory(dir, extension);
                if (!string.IsNullOrEmpty(slnFile))
                {
                    return slnFile;
                }
            }
            return null;
        }

        public abstract Task RunProcessAsync(Process process);

        internal string? GetRelativeFile(string markedFilePath)
        {
            if (FullDirPath == null) return null;

            string rootFolderPath = FullDirPath;    
            if (!string.IsNullOrEmpty(Preferences.ProjectRootFolderName))
            {
                rootFolderPath = FindSubDirectory(FullDirPath, Preferences.ProjectRootFolderName) ?? rootFolderPath;
            }
            string path = Path.GetFullPath( Path.Combine(rootFolderPath, markedFilePath));
            if(File.Exists(path)) { return path; }
            return null;
        }
    }
}

