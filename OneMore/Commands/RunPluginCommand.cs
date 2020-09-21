//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Dialogs;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Threading;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class RunPluginCommand : Command
	{
		private const int MaxTimeoutSeconds = 10;

		private Process process = null;
		private ProgressDialog progressDialog = null;
		private CancellationTokenSource source = null;


		public RunPluginCommand()
		{
		}


		public void Execute()
		{
			string pluginPath = null;
			using (var dialog = new RunPluginDialog())
			{
				if (dialog.ShowDialog(owner) == DialogResult.OK)
				{
					pluginPath = dialog.PluginPath;
				}
			}

			if (!string.IsNullOrEmpty(pluginPath) && File.Exists(pluginPath))
			{
				using (var manager = new ApplicationManager())
				{
					var page = new Page(manager.CurrentPage());
					var name = page.PageId.Replace("{", string.Empty).Replace("}", string.Empty);

					// pageId is of the form {sectionID}{}{pageId} so grab last 32 digits
					name = name.Substring(Math.Max(0, name.Length - 32));

					var xmlPath = Path.Combine(Path.GetTempPath(), $"{name}.xml");
					logger.WriteLine($"Plugin input file is {xmlPath}");

					try
					{
						var original = page.Root.ToString(SaveOptions.DisableFormatting);
						page.Root.Save(xmlPath, SaveOptions.DisableFormatting);

						if (Execute(pluginPath, xmlPath))
						{
							var root = XElement.Load(xmlPath);
							var updated = root.ToString(SaveOptions.DisableFormatting);

							if (updated != original)
							{
								logger.WriteLine("Updating content modified by plugin");
								manager.UpdatePageContent(root);
							}

							if (File.Exists(xmlPath))
							{
								File.Delete(xmlPath);
							}
						}
					}
					catch (Exception exc)
					{
						logger.WriteLine("Error running plugin", exc);
						UIHelper.ShowError("Error running plugin. See log file");
					}
				}
			}
		}


		private bool Execute(string pluginPath, string xmlPath)
		{
			using (source = new CancellationTokenSource())
			{
				using (progressDialog = new ProgressDialog(source)
				{
					Maximum = MaxTimeoutSeconds,
					Message = $"Running {pluginPath}"
				})
				{
					try
					{
						// process should run in an STA thread otherwise it will conflict with
						// the OneNote MTA thread environment
						var thread = new Thread(() =>
						{
							RunPlugin(pluginPath, xmlPath);
						});

						thread.SetApartmentState(ApartmentState.STA);
						thread.IsBackground = true;
						thread.Start();

						progressDialog.StartTimer();
						var result = progressDialog.ShowDialog(owner);

						if (result == DialogResult.Cancel)
						{
							logger.WriteLine("Clicked cancel");
							process.Kill();
							return false;
						}
					}
					catch (Exception exc)
					{
						logger.WriteLine("Error running Execute(string)", exc);
					}
					finally
					{
						if (process != null)
						{
							process.Dispose();
						}
					}
				}
			}

			return true;
		}


		private void RunPlugin(string path, string xmlPath)
		{
			try
			{
				process = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = path,
						Arguments = xmlPath,
						CreateNoWindow = true,
						UseShellExecute = false,
						RedirectStandardOutput = true,
					},

					EnableRaisingEvents = true
				};

				process.Exited += Process_Exited;
				process.OutputDataReceived += Process_OutputDataReceived;

				process.Start();
				process.BeginOutputReadLine();

				logger.WriteLine($"Plugin process started PID:{process.Id}");
			}
			catch (Exception exc)
			{
				logger.WriteLine("Error running RunPlugin(string)", exc);
			}
		}


		private void Process_Exited(object sender, EventArgs e)
		{
			logger.WriteLine("Plugin process exited");
			progressDialog.DialogResult = DialogResult.OK;
			progressDialog.Close();
		}


		private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			logger.WriteLine("| " + e.Data);
		}
	}
}
