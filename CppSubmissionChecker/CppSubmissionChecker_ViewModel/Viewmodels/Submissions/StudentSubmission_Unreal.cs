using CppSubmissionChecker_ViewModel.Viewmodels.TextLog;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
namespace CppSubmissionChecker_ViewModel
{
	public static partial class Preferences
	{
		public static string UnrealBuildPath
		{
			get => _cachingProvider.GetStringAsync("UnrealBuildPath").GetAwaiter().GetResult() ?? string.Empty;
			set => _cachingProvider.SetStringAsync("UnrealBuildPath", value);
		}
		public static string RiderPath
		{
			get => _cachingProvider.GetStringAsync("RiderPath").GetAwaiter().GetResult() ?? string.Empty;
			set => _cachingProvider.SetStringAsync("RiderPath", value);
		}
	}
}
namespace CppSubmissionChecker_ViewModel.Viewmodels.Submissions
{

	public class StudentSubmission_Unreal : StudentSubmission
	{
		private int _selectedOutputTab;

		public TextLog_VM BuildOutput
		{
			get; private set;
		} = new TextLog_VM();

		public TextLog_VM BuildExecutionOutput
		{
			get; private set;
		} = new TextLog_VM();

		public int SelectedOutputTabIndex
		{
			get => _selectedOutputTab;
			set
			{
				_selectedOutputTab = value;
				OnPropertyChanged(nameof(SelectedOutputTabIndex));
			}
		}

		public override string OpenInIDEText => "Open in Rider";

		protected List<Process> _runningProcesses = new List<Process>();
		public StudentSubmission_Unreal(string longName, string studentName, ZipArchiveEntry archiveEntry, MarkedFileTracker tracker) : base(longName, studentName, archiveEntry, tracker)
		{

			_submissionCommands.Add(new SubmissionCommand("Build And Run [Development]", () => { BuildAndRun(false); }));
			_submissionCommands.Add(new SubmissionCommand("Open in Unreal", async () => { OpenInUnreal(); }));
		}

		private async void BuildAndRun(bool release)
		{
			await BuildAndRun_Unreal(release);
		}

		private async Task OpenInUnreal()
		{
			SelectedOutputTabIndex = 0;
			try
			{
				if (!string.IsNullOrEmpty(SelectedSolutionPath))
				{
					var process = Process.Start("explorer", "\"" + SelectedSolutionPath + "\"");

					_runningProcesses.Add(process);
					await RunAndMonitorProcess(process, BuildOutput, true, null);
					_runningProcesses.Remove(process);
				}
			}
			catch (Exception exc)
			{
				SelectedOutputTabIndex = 0;
				BuildOutput.WriteLine("===========ERROR==========");
				BuildOutput.WriteLine("Open in Unreal encountered an exception: " + exc.Message);

			}
		}

		public async Task BuildAndRun_Unreal(bool release)
		{
			SelectedOutputTabIndex = 0;
			try
			{
				if (!string.IsNullOrEmpty(SelectedSolutionPath))
				{
					string targetName = Path.GetFileNameWithoutExtension(SelectedSolutionPath) + "Editor";
					string? executablePath = null;
					Process process = new Process();
					process.StartInfo.FileName = Preferences.UnrealBuildPath;
					process.StartInfo.Arguments = $"{targetName} -Project=\"{SelectedSolutionPath}\"";
					process.StartInfo.Arguments += " -Platform: Win64";
					if (!string.IsNullOrWhiteSpace(Preferences.BuildParams))
					{
						process.StartInfo.Arguments += " " + Preferences.BuildParams;
					}

					if (!process.StartInfo.Arguments.Contains("Configuration"))
					{
						process.StartInfo.Arguments += " -Configuration:" + (release ? " Release" : " Development");
					}

					_runningProcesses.Add(process);
					await RunAndMonitorProcess(process, BuildOutput, true, (outputLine) =>
					{
						if (outputLine != null && (outputLine.Contains(".exe") || outputLine.Contains(".dll")) && outputLine.Contains("->"))
						{
							executablePath = outputLine;
							TrimExecutablePath(ref executablePath);
						}
					});
					_runningProcesses.Remove(process);

					if (!string.IsNullOrEmpty(executablePath) && executablePath.EndsWith(".dll"))
					{
						executablePath = executablePath.Replace(".dll", ".exe");
					}

					if (executablePath != null)
					{
						SelectedOutputTabIndex = 1;
						//Run the build
						Process runProcess = new Process();
						_runningProcesses.Add(process);
						runProcess.StartInfo.FileName = executablePath;
						runProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(executablePath);
						await RunAndMonitorProcess(runProcess, BuildExecutionOutput, true, null);
						_runningProcesses.Remove(process);
					}


				}
			}
			catch (Exception exc)
			{
				SelectedOutputTabIndex = 0;
				BuildOutput.WriteLine("===========ERROR==========");
				BuildOutput.WriteLine("Build and Run encountered an exception: " + exc.Message);

			}
		}

		protected async Task RunAndMonitorProcess(Process process, TextLog_VM console, bool hideConsole, Action<string?>? receivedOutput)
		{
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = hideConsole;
			process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
			console.Clear();

			Action<string> appendText = (txt) => { console.WriteLine(txt); };

			process.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
			{
				if (!string.IsNullOrEmpty(e.Data)) appendText(e.Data);
				receivedOutput?.Invoke(e.Data);
			});
			process.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
			{

				if (!string.IsNullOrEmpty(e.Data)) appendText(e.Data);

			});

			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			await process.WaitForExitAsync();

		}

		public override async Task RunProcessAsync(Process process)
		{
			_runningProcesses.Add(process);
			process.Start();
			await process.WaitForExitAsync();

			_runningProcesses.Remove(process);
		}

		public override async Task KillRunningProcesses()
		{
			for (int i = _runningProcesses.Count - 1; i >= 0; --i)
			{
				_runningProcesses[i].Kill();
			}

			while (_runningProcesses.Count > 0)
			{
				for (int idx = _runningProcesses.Count - 1; idx >= 0; --idx)
				{
					if (_runningProcesses[idx] == null || _runningProcesses[idx].HasExited)
					{
						_runningProcesses.RemoveAt(idx);
					}
				}
				await Task.Delay(1000);
			}
		}

		private void TrimExecutablePath(ref string executablePath)
		{

			int exeIndex = Math.Max(executablePath.IndexOf(".exe"), executablePath.IndexOf(".dll"));
			int beforeExeIndex = executablePath.IndexOf("->", 0, exeIndex);
			if (beforeExeIndex != -1)
			{
				beforeExeIndex += 3;
				executablePath = executablePath.Substring(beforeExeIndex, exeIndex - beforeExeIndex + 4);
			}

		}

		protected override async Task OpenInVS()
		{

			if (!string.IsNullOrEmpty(SelectedSolutionPath))
			{
				Process process = new Process();
				process.StartInfo.FileName = Preferences.RiderPath;
				process.StartInfo.Arguments = $"\"{SelectedSolutionPath}\"";
				await RunProcessAsync(process);
			}
		}

	}
}
