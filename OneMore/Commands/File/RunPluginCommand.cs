//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
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
		private Plugin plugin;
		private ProgressDialog progress = null;
		private Page page;
		private string workpath;


		public RunPluginCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (!await LoadPlugin(args))
			{
				return;
			}

			var content = plugin.TargetPage ? PreparePageCache() : await PrepareHierarchyCache();
			if (content == null)
			{
				return;
			}

			try
			{
				if (!Execute())
				{
					UIHelper.ShowError(Resx.Plugin_Unsuccessful);
					Cleanup(workpath);
					return;
				}

				var root = LoadUpdates(content);
				if (root == null)
				{
					return;
				}

				if (plugin.TargetPage)
				{
					await SavePage(root);
				}
				else
				{
					SaveHierarchy(root);
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
				var element = args.FirstOrDefault(a => a is XElement e && e.Name.LocalName == "path") as XElement;
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
				PageName = "$name (2)"
			};

			if (dialog.ShowDialog(owner) == DialogResult.Cancel)
			{
				plugin = null;
				return false;
			}

			plugin = dialog.Plugin;
			return true;
		}


		private string PreparePageCache()
		{
			using var one = new OneNote(out page, out _, OneNote.PageDetail.All);

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
				UIHelper.ShowError(Resx.Plugin_WritingTemp);
				logger.WriteLine("error writing to temp file", exc);
				return null;
			}

			return content;
		}


		private async Task<string> PrepareHierarchyCache()
		{
			using var one = new OneNote();
			var notebook = await one.GetNotebook(OneNote.Scope.Sections);

			// derive a temp file name from the notebook ID which is of the form {ID}{}{}
			// so grab just the ID part which should be a hyphenated Guid value
			var name = notebook.Attribute("ID").Value;
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
				UIHelper.ShowError(Resx.Plugin_WritingTemp);
				logger.WriteLine("error writing to temp file", exc);
				return null;
			}

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

				result = progress.ShowTimedDialog(owner, ExecuteWorker);
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
				logger.WriteLine($"running {plugin.Command} {plugin.Arguments} \"{path}\"");

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
				if (process != null)
				{
					process.Dispose();
				}
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
					UIHelper.ShowInfo(Resx.Plugin_NoChanges);
					return null;
				}

				if (plugin.TargetPage && !ValidPageSchema(root))
				{
					UIHelper.ShowInfo(Resx.Plugin_InvalidSchema);
					return null;
				}

				return root;
			}
			catch (Exception exc)
			{
				logger.WriteLine("error updating page", exc);
				UIHelper.ShowError(Resx.Plugin_NoUpdate);
				return null;
			}
		}


		private bool ValidPageSchema(XElement root)
		{
			var schemas = new XmlSchemaSet();
			var ns = root.GetNamespaceOfPrefix(OneNote.Prefix);
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


		private async Task SavePage(XElement root)
		{
			try
			{
				using var one = new OneNote();

				if (plugin.CreateNewPage)
				{
					await CreatePage(one, root);
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
		}


		private async Task CreatePage(OneNote one, XElement childRoot)
		{
			var section = one.GetSection();
			var sectionId = section.Attribute("ID").Value;

			one.CreatePage(sectionId, out var pageId);

			// set the page ID to the new page's ID
			childRoot.Attribute("ID").Value = pageId;
			var child = new Page(childRoot);

			var childTitle = child.Title.Trim();
			var parentTitle = page.Title.Trim();

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

			if (plugin.AsChildPage)
			{
				// get current section again after new page is created
				section = one.GetSection();

				var parentElement = section.Elements(page.Namespace + "Page")
					.FirstOrDefault(e => e.Attribute("ID").Value == page.PageId);

				var childElement = section.Elements(page.Namespace + "Page")
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


		private void SaveHierarchy(XElement root)
		{
			try
			{
				using var one = new OneNote();
				one.UpdateHierarchy(root);
			}
			catch (Exception exc)
			{
				logger.WriteLine("error updating hierarchy", exc);
				UIHelper.ShowError(Resx.Plugin_NoUpdate);
			}
		}


		private void Cleanup(string workPath)
		{
			if (File.Exists(workPath))
			{
				try
				{
					//File.Delete(workPath);
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
