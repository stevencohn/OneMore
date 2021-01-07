﻿//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml;
	using System.Xml.Linq;
	using System.Xml.Schema;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class RunPluginCommand : Command
	{
		private const int MaxTimeoutSeconds = 15;

		private Plugin plugin;

		private UI.ProgressDialog progressDialog = null;
		private CancellationTokenSource source = null;


		public RunPluginCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out _, OneNote.PageDetail.All))
			{
				if (args.Length > 0 && (args[0] is string arg) && !string.IsNullOrEmpty(arg))
				{
					plugin = await new PluginsProvider().Load(arg);
				}
				else if (!PromptForPlugin(page.Title) || string.IsNullOrEmpty(plugin.Command))
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
					logger.WriteLine("error writing to temp file", exc);
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

					if (updated == original && !plugin.CreateNewPage)
					{
						UIHelper.ShowInfo(Resx.Plugin_NoChanges);
						return;
					}

					if (!Validated(root, one.GetNamespace(root)))
					{
						UIHelper.ShowInfo(Resx.Plugin_InvalidSchema);
						return;
					}

					if (plugin.CreateNewPage)
					{
						await CreatePage(one, root, page);
					}
					else
					{
						await one.Update(root);
					}
				}
				catch (Exception exc)
				{
					logger.WriteLine("error updating page", exc);
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
			using (var dialog = new PluginDialog())
			{
				name = XElement.Parse($"<wrapper>{name}</wrapper>").Value;

				dialog.PageName = name + " (2)";

				if (dialog.ShowDialog(owner) == DialogResult.Cancel)
				{
					return false;
				}

				plugin = dialog.Plugin;
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
			logger.WriteLine($"plugin working file is {path}");

			return path;
		}


		private bool Execute(string workPath)
		{
			using (source = new CancellationTokenSource())
			{
				using (progressDialog = new UI.ProgressDialog(source))
				{
					progressDialog.SetMaximum(MaxTimeoutSeconds);
					progressDialog.SetMessage(string.Format(
						Resx.Plugin_Running, plugin.Command, plugin.Arguments, workPath));

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
							logger.WriteLine("clicked cancel");
							process.Kill();
							return false;
						}
					}
					catch (Exception exc)
					{
						logger.WriteLine("error running Execute(string)", exc);
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
			logger.WriteLine($"running {plugin.Command} {plugin.Arguments} \"{path}\"");

			Process process = null;

			try
			{
				process = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = plugin.Command,
						Arguments = $"{plugin.Arguments} \"{path}\"",
						CreateNoWindow = true,
						UseShellExecute = false,
						RedirectStandardOutput = true,
						RedirectStandardError = true
					},

					EnableRaisingEvents = true
				};

				process.Exited += Process_Exited;
				process.OutputDataReceived += Process_OutputDataReceived;
				process.ErrorDataReceived += Process_ErrorDataReceived;

				process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();

				logger.WriteLine($"plugin process started PID:{process.Id}");
				logger.StartClock();
			}
			catch (Exception exc)
			{
				logger.WriteLine("error running RunPlugin(string)", exc);

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
			logger.WriteTime("plugin process exited");
			progressDialog.DialogResult = DialogResult.OK;
			progressDialog.Close();
		}


		private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			logger.WriteLine("| " + e.Data);
		}


		private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			logger.WriteLine("! " + e.Data);
		}


		private bool Validated(XElement root, XNamespace ns)
		{
			var schemas = new XmlSchemaSet();
			schemas.Add(ns.ToString(), XmlReader.Create(new StringReader(Resx._0336_OneNoteApplication_2013)));

			var document = new XDocument(root);

			bool valid = true;
			document.Validate(schemas, (o, e) =>
			{
				logger.WriteLine($"schema validation {e.Severity}", e.Exception);
				valid = false;
			});

			return valid;
		}


		private async Task CreatePage(OneNote one, XElement page, Page parent)
		{
			var section = one.GetSection();
			var sectionId = section.Attribute("ID").Value;

			one.CreatePage(sectionId, out var pageId);

			// set the page ID to the new page's ID
			page.Attribute("ID").Value = pageId;

			// set the page name to user-entered name
			if (!string.IsNullOrEmpty(plugin.PageName.Trim()))
			{
				new Page(page).Title = plugin.PageName;
			}

			// remove all objectID values and let OneNote generate new IDs
			page.Descendants().Attributes("objectID").Remove();

			await one.Update(page);

			if (plugin.AsChildPage)
			{
				// get current section again after new page is created
				section = one.GetSection();

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

				one.UpdateHierarchy(section);
			}

			await one.NavigateTo(pageId);
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
					logger.WriteLine($"error deleting {workPath}", exc);
				}
			}
		}
	}
}
