//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Dialogs;
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Threading;
	using System.Windows.Forms;


	internal class RunPluginCommand : Command
	{
		private const int OneSecond = 1000;
		private const int MaxTimeoutSeconds = 10;
		private const int MaxTimeout = OneSecond * MaxTimeoutSeconds;

		private int ticks = 0;
		private Process process = null;
		private ProgressDialog progressDialog = null;
		private CancellationTokenSource source = null;

		private delegate void TickDelegate(object sender, EventArgs e);


		public RunPluginCommand()
		{
		}


		public void Execute()
		{
			string path = null;
			using (var dialog = new RunPluginDialog())
			{
				if (dialog.ShowDialog(owner) == DialogResult.OK)
				{
					path = dialog.PluginPath;	
				}
			}

			if (!string.IsNullOrEmpty(path) && File.Exists(path))
			{
				source = new CancellationTokenSource();

				progressDialog = new ProgressDialog(source)
				{
					Maximum = MaxTimeoutSeconds,
					Message = $"Running {path}"
				};

				progressDialog.Show(owner);

				try
				{
					var thread = new Thread(() =>
					{
						using (var timer = new System.Threading.Timer(Timer_Tick, null, 0, 1000))
						{
							RunPlugin(path);
						}
					});

					thread.SetApartmentState(ApartmentState.STA);
					thread.IsBackground = true;
					thread.Start();
					thread.Join();
				}
				finally
				{
					if (progressDialog != null)
					{
						progressDialog.Close();
						progressDialog.Dispose();
					}

					if (source != null)
					{
						source.Dispose();
					}
				}
			}
		}
		//private void Timer_Tick(object sender, EventArgs e)
		private void Timer_Tick(object state)
		{
			if (progressDialog.InvokeRequired)
			{
				progressDialog.Invoke(new Action(() => Timer_Tick(state)));
			}

			logger.WriteLine($"ticking {ticks}");

			if (ticks < MaxTimeoutSeconds && !process.HasExited)
			{
				if (source.Token.IsCancellationRequested)
				{
					ticks = int.MaxValue;
					logger.WriteLine("cancelled");
				}
				else
				{
					ticks++;

					progressDialog.Increment();
					logger.WriteLine("increment");
				}
			}
		}


		private void RunPlugin(string path)
		{
			try
			{
				process = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = path,
						CreateNoWindow = true,
						UseShellExecute = false,
						RedirectStandardOutput = true
					}
				};

				process.OutputDataReceived += Process_OutputDataReceived;

				process.Start();
				process.BeginOutputReadLine();

				logger.WriteLine($"started process {process.Id}");

				if (!process.WaitForExit(MaxTimeout))
				{
					logger.WriteLine("killing processing");
					process.Kill();
				}
				else
				{
					logger.WriteLine("finished");
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
			}
			finally
			{
				if (process != null)
				{
					process.Dispose();
				}
			}
		}


		private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			logger.WriteLine("OUT: " + e.Data);
		}
	}
}
