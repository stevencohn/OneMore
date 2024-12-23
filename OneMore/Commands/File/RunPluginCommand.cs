//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal class RunPluginCommand : Command
	{
		private Plugin plugin;
		private ProgressDialog progress = null;
		private Page page;
		private OneNote.HierarchyInfo hierarchyInfo;
		private string workpath;
		private bool trialRun;


		public RunPluginCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (!await LoadPlugin(args))
			{
				return;
			}

			var content = plugin.Target == PluginTarget.Page
				? await PreparePageCache()
				: await PrepareHierarchyCache();

			if (content == null)
			{
				return;
			}

			try
			{
				if (Execute())
				{
					var root = LoadUpdates(content);
					if (root != null)
					{
						if (plugin.Target == PluginTarget.Page)
						{
							await SavePage(root);
						}
						else
						{
							await SaveHierarchy(root);
						}
					}
				}
				else
				{
					ShowError(Resx.Plugin_Unsuccessful);
				}
			}
			finally
			{
				Cleanup(workpath);
			}
		}


		private async Task<bool> LoadPlugin(params object[] args)
		{
			// check for replay
			if (args != null && args.Length > 0)
			{
				var element = Array.Find(args,
					a => a is XElement e && e.Name.LocalName == "path") as XElement;

				if (!string.IsNullOrEmpty(element?.Value))
				{
					plugin = await new PluginsProvider().Load(element.Value);
				}
				else if ((args[0] is string arg) && !string.IsNullOrEmpty(arg))
				{
					plugin = await new PluginsProvider().Load(arg);
				}
			}

			if (plugin == null)
			{
				if (!PromptForPlugin() || string.IsNullOrEmpty(plugin.Command))
				{
					IsCancelled = true;
					return false;
				}
			}

			return true;
		}


		private bool PromptForPlugin()
		{
			using var dialog = new PluginDialog
			{
				PageName = PluginDialog.DefaultCreatedPageName
			};

			if (dialog.ShowDialog(owner) == DialogResult.Cancel)
			{
				plugin = null;
				return false;
			}

			plugin = dialog.Plugin;
			trialRun = dialog.TrialRun;
			return true;
		}


		private async Task<string> PreparePageCache()
		{
			await using var one = new OneNote(out page, out _, OneNote.PageDetail.All);

			// derive a temp file name from PageId which is of the form {sectionID}{}{pageId}
			// so grab just the pageId part which should be a Guid spanning the last 32 digits
			var name = page.PageId.Substring(page.PageId.LastIndexOf('{') + 1, 32);
			workpath = Path.Combine(Path.GetTempPath(), $"{name}.xml");
			logger.WriteLine($"plugin working file is {workpath}");

			var content = page.Root.ToString(SaveOptions.DisableFormatting);

			try
			{
				// write the page XML to the working path
				page.Root.Save(workpath, SaveOptions.DisableFormatting);
			}
			catch (Exception exc)
			{
				ShowError(Resx.Plugin_WritingTemp);
				logger.WriteLine("error writing to temp file", exc);
				return null;
			}

			hierarchyInfo = await one.GetPageInfo(page.PageId);
			hierarchyInfo.PageId = page.PageId;

			return content;
		}


		private async Task<string> PrepareHierarchyCache()
		{
			await using var one = new OneNote();

			XElement notebook = null;
			switch (plugin.Target)
			{
				case PluginTarget.Section:
					notebook = await one.GetSection();
					break;

				case PluginTarget.Notebook:
					notebook = await one.GetNotebook(OneNote.Scope.Sections);
					break;

				case PluginTarget.NotebookPages:
					notebook = await one.GetNotebook(OneNote.Scope.Pages);
					break;

				default: // case PluginTarget.Notebooks:
					notebook = await one.GetNotebooks(OneNote.Scope.Pages);
					break;
			}

			//var notebook = await one.GetNotebook(OneNote.Scope.Sections);

			// look for locked sections and warn user...
			var ns = one.GetNamespace(notebook);
			if (notebook.Attribute("locked") != null /* section */ ||
				notebook.Descendants(ns + "Section").Any(e => e.Attribute("locked") != null))
			{
				using var box = new MoreMessageBox();
				box.SetIcon(MessageBoxIcon.Warning);
				box.SetButtons(MessageBoxButtons.YesNo);
				box.AppendMessage("This notebook contains locked sections.",
					ThemeManager.Instance.GetColor("ErrorText"));

				box.AppendMessage(plugin.SkipLocked
					? " These sections may be skipped by the plugin."
					: " These sections may cause the plugin to fail.");

				box.AppendMessage(" Do you wish to continue?");

				if (box.ShowDialog(owner) == DialogResult.No)
				{
					return null;
				}
			}

			// derive a temp file name from the notebook ID which is of the form {ID}{}{}
			// so grab just the ID part which should be a hyphenated Guid value
			var name = notebook.Name.LocalName == "Notebooks"
				? notebook.Elements(ns + "Notebook").FirstOrDefault()?.Attribute("ID").Value
				: notebook.Attribute("ID").Value;

			// shouldn't happen, but fall back to generic Guid
			name ??= Guid.NewGuid().ToString("D");

			name = name.Substring(1, name.IndexOf('}') - 1).Replace("-", string.Empty);
			workpath = Path.Combine(Path.GetTempPath(), $"{name}.xml");

			var content = notebook.ToString(SaveOptions.DisableFormatting);

			try
			{
				// write the hierarchy XML to the working path
				notebook.Save(workpath, SaveOptions.DisableFormatting);
			}
			catch (Exception exc)
			{
				ShowError(Resx.Plugin_WritingTemp);
				logger.WriteLine("error writing to temp file", exc);
				return null;
			}

			hierarchyInfo = await one.GetPageInfo();
			hierarchyInfo.PageId = one.CurrentPageId;

			return content;
		}


		private bool Execute()
		{
			var timeout = plugin.Timeout == 0 ? Plugin.MaxTimeout : plugin.Timeout;
			DialogResult result;

			// plugin will be executed in an STA thread...

			using (progress = new ProgressDialog(timeout))
			{
				progress.Tag = workpath;
				progress.SetMessage(string.Format(
					Resx.Plugin_Running, plugin.Command, plugin.Arguments, workpath));

				result = progress.ShowTimedDialog(ExecuteWorker);
			}

			return result == DialogResult.OK;
		}


		private async Task<bool> ExecuteWorker(ProgressDialog progress, CancellationToken token)
		{
			var path = (string)progress.Tag;

			Process process = null;
			var ok = true;

			try
			{
				var abscmd = Environment.ExpandEnvironmentVariables(plugin.Command);
				var absargs = Environment.ExpandEnvironmentVariables(plugin.Arguments);
				var userargs = Environment.ExpandEnvironmentVariables(plugin.UserArguments);

				var op = trialRun ? "trialing" : "running";
				logger.WriteLine($"{op} {abscmd} {absargs} \"{path}\" {userargs}");

				var info = new ProcessStartInfo
				{
					FileName = abscmd,
					Arguments = $"{absargs} \"{path}\" {userargs}",
					CreateNoWindow = true,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true
				};

				info.Environment["PLUGIN_SKIPLOCK"] = plugin.SkipLocked.ToString();
				info.Environment["PLUGIN_CREATE"] = plugin.CreateNewPage.ToString();
				info.Environment["PLUGIN_PAGENAME"] = plugin.PageName;
				info.Environment["PLUGIN_ASCHILD"] = plugin.AsChildPage.ToString();

				info.Environment["PLUGIN_SOURCE_PAGEID"] = hierarchyInfo.PageId;
				info.Environment["PLUGIN_SOURCE_SECTIONID"] = hierarchyInfo.SectionId;
				info.Environment["PLUGIN_SOURCE_NOTEBOOKID"] = hierarchyInfo.NotebookId;
				info.Environment["PLUGIN_SOURCE_PAGENAME"] = hierarchyInfo.Name;
				info.Environment["PLUGIN_SOURCE_PAGEPATH"] = hierarchyInfo.Path;
				info.Environment["PLUGIN_SOURCE_PAGEURL"] = hierarchyInfo.Link;

				process = new Process
				{
					StartInfo = info,
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

				await process.WaitForExitAsync(token);
				ok = !token.IsCancellationRequested;
			}
			catch (Exception exc)
			{
				logger.WriteLine("error running Execute(string)", exc);
				ok = false;
			}
			finally
			{
				process?.Dispose();
			}

			return ok;
		}


		private void Process_Exited(object sender, EventArgs e)
		{
			logger.WriteTime("plugin process exited");
			progress.DialogResult = DialogResult.OK;
			progress.Close();
		}


		private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			logger.WriteLine("| " + e.Data);
		}


		private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			logger.WriteLine("! " + e.Data);
		}


		private XElement LoadUpdates(string content)
		{
			try
			{
				var root = XElement.Load(workpath);

				var updated = root.ToString(SaveOptions.DisableFormatting);
				if (updated == content && !plugin.CreateNewPage)
				{
					InformNoChange();
					return null;
				}

				if (plugin.Target == PluginTarget.Page)
				{
					var candidate = new Page(root);
					// must optimize before we can validate schema...
					candidate.OptimizeForSave(true);

					if (!OneNote.ValidateSchema(root))
					{
						ShowInfo(Resx.Plugin_InvalidSchema);
						return null;
					}
				}

				return root;
			}
			catch (Exception exc)
			{
				logger.WriteLine("error updating page", exc);
				ShowError(Resx.Plugin_NoUpdate);
				return null;
			}
		}


		private void InformNoChange()
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection("plugins");
			if (settings.Get("hideNoChange", false))
			{
				return;
			}

			var box = new MoreMessageBox();
			box.SetIcon(MessageBoxIcon.Information);
			box.SetMessage(Resx.Plugin_NoChanges);
			box.SetButtons(MessageBoxButtons.OK);
			box.EnableSuppression();
			var result = box.ShowDialog(owner);

			if (result == DialogResult.OK && box.SuppressMessage)
			{
				settings.Add("hideNoChange", "true");
				provider.SetCollection(settings);
				provider.Save();
			}
		}


		private async Task SavePage(XElement root)
		{
			if (!OneNote.ValidateSchema(root))
			{
				ShowError($"{Resx.Plugin_InvalidSchema}\n\nCache: {workpath}");
				return;
			}

			if (trialRun)
			{
				ShowInfo(string.Format(Resx.Plugin_trialCompleted, workpath));
				return;
			}

			try
			{
				await using var one = new OneNote();

				if (plugin.CreateNewPage)
				{
					await CreatePage(one, root);
				}
				else
				{
					var candidate = new Page(root);
					candidate.OptimizeForSave(true);

					await one.Update(candidate);
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error updating page", exc);
				ShowError(Resx.Plugin_NoUpdate);
			}
		}


		private async Task CreatePage(OneNote one, XElement childRoot)
		{
			var section = await one.GetSection();
			var sectionId = section.Attribute("ID").Value;

			one.CreatePage(sectionId, out var pageId);

			// set the page ID to the new page's ID
			childRoot.Attribute("ID").Value = pageId;
			var child = new Page(childRoot);

			var childTitle = child.Title.Trim();
			var parentTitle = page.Title.Trim();

			// every new page adds hashes so need to remove them
			child.OptimizeForSave(true);

			// if plugin has modified the page title then accept that
			// otherwise apply the name template defined by this plugin...
			if (childTitle == parentTitle)
			{
				// set the page name to user-entered name
				var template = plugin.PageName.Trim();

				if (!string.IsNullOrEmpty(template) && template.Contains("$name"))
				{
					// grab only text from parent's title
					template = template.Replace("$name", XElement.Parse($"<w>{parentTitle}</w>").Value);
				}

				// override child title with expanded template
				logger.WriteLine($"setting page title from name template [{template}]");
				child.Title = template;
			}
			else
			{
				logger.WriteLine($"keeping page title set by plugin [{childTitle}]");
			}

			// remove all objectID values and let OneNote generate new IDs
			child.Root.Descendants().Attributes("objectID").Remove();

			await one.Update(child);

			section = await one.GetSection();
			var editor = new SectionEditor(section);

			if (plugin.AsChildPage)
			{
				// get current section again after new page is created
				if (editor.MovePageToParent(pageId, page.PageId))
				{
					one.UpdateHierarchy(section);
				}
			}
			else if (editor.MovePageAfterAnchor(pageId, page.PageId))
			{
				one.UpdateHierarchy(section);
			}

			await one.NavigateTo(pageId);
		}


		private async Task SaveHierarchy(XElement root)
		{
			if (trialRun)
			{
				ShowInfo(string.Format(Resx.Plugin_trialCompleted, workpath));
				return;
			}

			try
			{
				await using var one = new OneNote();
				one.UpdateHierarchy(root);
			}
			catch (Exception exc)
			{
				logger.WriteLine("error updating hierarchy", exc);
				ShowError(Resx.Plugin_NoUpdate);
			}
		}


		private void Cleanup(string workPath)
		{
			if (File.Exists(workPath) && !trialRun)
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


		public override XElement GetReplayArguments()
		{
			if (!string.IsNullOrEmpty(plugin?.Path))
			{
				return new XElement("path", plugin.Path);
			}

			return null;
		}
	}
}
