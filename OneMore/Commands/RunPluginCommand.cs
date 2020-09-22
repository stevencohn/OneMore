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
	using System.Linq;
	using System.Threading;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class RunPluginCommand : Command
	{
		private const int MaxTimeoutSeconds = 10;

		private string command;
		private string arguments;

		private bool createNewPage;
		private bool createChildPage;
		private string pageName;

		private Process process = null;
		private ProgressDialog progressDialog = null;
		private CancellationTokenSource source = null;


		public RunPluginCommand()
		{
		}


		public void Execute()
		{
			using (var manager = new ApplicationManager())
			{
				var page = new Page(manager.CurrentPage());

				using (var dialog = new RunPluginDialog())
				{
					dialog.PageName = page.PageName + " (2)";

					if (dialog.ShowDialog(owner) == DialogResult.Cancel)
					{
						return;
					}

					command = dialog.Command;
					arguments = dialog.Arguments;
					createNewPage = !dialog.UpdatePage;
					createChildPage = dialog.CreateChild;
					pageName = dialog.PageName;
				}

				if (!string.IsNullOrEmpty(command))
				{
					var name = page.PageId.Replace("{", string.Empty).Replace("}", string.Empty);

					// pageId is of the form {sectionID}{}{pageId} so grab last 32 digits
					name = name.Substring(Math.Max(0, name.Length - 32));

					var path = Path.Combine(Path.GetTempPath(), $"{name}.xml");
					logger.WriteLine($"Plugin input file is {path}");

					string original = null;
					try
					{
						original = page.Root.ToString(SaveOptions.DisableFormatting);
						page.Root.Save(path, SaveOptions.DisableFormatting);
					}
					catch (Exception exc)
					{
						logger.WriteLine("Error saving page", exc);
						return;
					}

					var success = false;
					try
					{
						success = Execute(path);
					}
					catch (Exception exc)
					{
						logger.WriteLine("Error executing plugin", exc);
						return;
					}

					if (!success)
					{
						UIHelper.ShowError("Plugin did not complete successfully. See log file");
						return;
					}

					try
					{
						var root = XElement.Load(path);
						var updated = root.ToString(SaveOptions.DisableFormatting);

						if (updated != original)
						{
							if (createNewPage)
							{
								CreatePage(manager, root, page);
							}
							else
							{
								logger.WriteLine("Updating page by plugin");
								manager.UpdatePageContent(root);
							}
						}
						else
						{
							logger.WriteLine("No changes found");
						}

						if (File.Exists(path))
						{
							File.Delete(path);
						}
					}
					catch (Exception exc)
					{
						logger.WriteLine("Error updating page", exc);
						UIHelper.ShowError("Error updating page. See log file");
					}
				}
			}
		}


		private void CreatePage(ApplicationManager manager, XElement page, Page parent)
		{
			logger.WriteLine("Creating page by plugin");

			var section = manager.CurrentSection();
			var sectionId = section.Attribute("ID").Value;

			manager.Application.CreateNewPage(sectionId, out var pageId);

			// set the page ID to the new page's ID
			page.Attribute("ID").Value = pageId;
			// set the page name to user-entered name
			new Page(page).PageName = pageName;
			// remove all objectID values and let OneNote generate new IDs
			page.Descendants().Attributes("objectID").Remove();

			manager.UpdatePageContent(page);

			// get current section again after new page is created
			section = manager.CurrentSection();

			var parentElement = section.Elements(parent.Namespace + "Page")
				.Where(e => e.Attribute("ID").Value == parent.PageId)
				.FirstOrDefault();

			// TODO: childPage?
		}


		private bool Execute(string path)
		{
			using (source = new CancellationTokenSource())
			{
				using (progressDialog = new ProgressDialog(source)
				{
					Maximum = MaxTimeoutSeconds,
					Message = $"Running {command} {arguments} \"{path}\""
				})
				{
					try
					{
						// process should run in an STA thread otherwise it will conflict with
						// the OneNote MTA thread environment
						var thread = new Thread(() =>
						{
							RunPlugin(path);
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


		private void RunPlugin(string path)
		{
			logger.WriteLine($"Running {command} {arguments} \"{path}\"");

			try
			{
				process = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = command,
						Arguments = $"{arguments} \"{path}\"",
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
