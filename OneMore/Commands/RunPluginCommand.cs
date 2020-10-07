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
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class RunPluginCommand : Command
	{
		private const int MaxTimeoutSeconds = 15;

		private string command;
		private string arguments;

		private bool createNewPage;
		private bool asChildPage;
		private string pageName;

		private ProgressDialog progressDialog = null;
		private CancellationTokenSource source = null;


		public RunPluginCommand()
		{
		}


		public void Execute()
		{
			using (var manager = new ApplicationManager())
			{
				var page = new Page(manager.CurrentPage(Microsoft.Office.Interop.OneNote.PageInfo.piAll));

				if (!PromptForPlugin(page.PageName) || string.IsNullOrEmpty(command))
				{
					return;
				}

				var workPath = MakeWorkingFilePath(page.PageId);

				// copy contents for comparison later
				string original = page.Root.ToString(SaveOptions.DisableFormatting);

				try
				{
					// write the page XML to the working path
					page.Root.Save(workPath, SaveOptions.DisableFormatting);
				}
				catch (Exception exc)
				{
					UIHelper.ShowError(Resx.Plugin_WritingTemp);
					logger.WriteLine("Error writing to temp file", exc);
					return;
				}

				// execute the plugin in an STA thread
				if (!Execute(workPath))
				{
					UIHelper.ShowError(Resx.Plugin_Unsuccessful);
					Cleanup(workPath);
					return;
				}

				try
				{
					var root = XElement.Load(workPath);
					var updated = root.ToString(SaveOptions.DisableFormatting);

					if (createNewPage)
					{
						CreatePage(manager, root, page);
					}
					else
					{
						if (updated != original)
						{
							manager.UpdatePageContent(root);
						}
						else
						{
							UIHelper.ShowMessage(Resx.Plugin_NoChanges);
						}
					}
				}
				catch (Exception exc)
				{
					logger.WriteLine("Error updating page", exc);
					UIHelper.ShowError(Resx.Plugin_NoUpdate);
				}
				finally
				{
					Cleanup(workPath);
				}
			}
		}


		private bool PromptForPlugin(string name)
		{
			using (var dialog = new RunPluginDialog())
			{
				name = XElement.Parse($"<wrapper>{name}</wrapper>").Value;

				dialog.PageName = name + " (2)";

				if (dialog.ShowDialog(owner) == DialogResult.Cancel)
				{
					return false;
				}

				command = dialog.Command;
				arguments = dialog.Arguments;
				createNewPage = !dialog.UpdatePage;
				asChildPage = dialog.CreateChild;
				pageName = dialog.PageName;
			}

			return true;
		}


		private string MakeWorkingFilePath(string pageId)
		{
			// Page.ID is of the form {sectionID}{}{pageId} so grab just the pageId part
			// which should be a Guid spanning the last 32 digits

			var name = pageId.Replace("{", string.Empty).Replace("}", string.Empty);
			name = name.Substring(Math.Max(0, name.Length - 32));

			var path = Path.Combine(Path.GetTempPath(), $"{name}.xml");
			logger.WriteLine($"Plugin working file is {path}");

			return path;
		}


		private bool Execute(string workPath)
		{
			using (source = new CancellationTokenSource())
			{
				using (progressDialog = new ProgressDialog(source)
				{
					Maximum = MaxTimeoutSeconds,
					Message = string.Format(Resx.Plugin_Running, command, arguments, workPath)
				})
				{
					Process process = null;

					try
					{
						// process should run in an STA thread otherwise it will conflict with
						// the OneNote MTA thread environment
						var thread = new Thread(() =>
						{
							process = StartPlugin(workPath);
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
							process = null;
						}
					}
				}
			}

			return true;
		}


		private Process StartPlugin(string path)
		{
			logger.WriteLine($"Running {command} {arguments} \"{path}\"");

			Process process = null;

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

				if (process != null)
				{
					process.Dispose();
					process = null;
				}
			}

			return process;
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


		private void CreatePage(ApplicationManager manager, XElement page, Page parent)
		{
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

			if (asChildPage)
			{
				// get current section again after new page is created
				section = manager.CurrentSection();

				var parentElement = section.Elements(parent.Namespace + "Page")
					.FirstOrDefault(e => e.Attribute("ID").Value == parent.PageId);

				var childElement = section.Elements(parent.Namespace + "Page")
					.FirstOrDefault(e => e.Attribute("ID").Value == pageId);

				if (childElement != parentElement.NextNode)
				{
					// move new page immediately after its original in the section
					childElement.Remove();
					parentElement.AddAfterSelf(childElement);
				}

				var level = int.Parse(parentElement.Attribute("pageLevel").Value);
				childElement.Attribute("pageLevel").Value = (level + 1).ToString();

				manager.UpdateHierarchy(section);
			}

			manager.Application.NavigateTo(pageId);
		}


		private void Cleanup(string workPath)
		{
			if (File.Exists(workPath))
			{
				try
				{
					File.Delete(workPath);
				}
				catch (Exception exc)
				{
					logger.WriteLine($"Error deleting {workPath}", exc);
				}
			}
		}
	}
}
