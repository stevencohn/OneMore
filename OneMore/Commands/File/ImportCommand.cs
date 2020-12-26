//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Helpers.Office;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Drawing;
	using System.IO;
	using System.Threading;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class ImportCommand : Command
	{
		private const int MaxWait = 15;
		private UI.ProgressDialog progressDialog;


		public ImportCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			using (var dialog = new ImportDialog())
			{
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				switch (dialog.Format)
				{
					case ImportDialog.Formats.Word:
						ImportWord(dialog.FilePath, dialog.AppendToPage);
						break;

					case ImportDialog.Formats.PowerPoint:
						ImportPowerPoint(dialog.FilePath, dialog.AppendToPage, dialog.CreateSection);
						break;

					case ImportDialog.Formats.Xml:
						ImportXml(dialog.FilePath);
						break;

					case ImportDialog.Formats.OneNote:
						ImportOneNote(dialog.FilePath);
						break;
				}
			}
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Word...

		private void ImportWord(string filepath, bool append)
		{
			if (!Office.IsWordInstalled())
			{
				UIHelper.ShowMessage("Word is not installed");
			}

			logger.StartClock();

			var completed = RunBackgroundTask(filepath, () =>
			{
				WordImporter(filepath, append);

				progressDialog.DialogResult = DialogResult.OK;
				progressDialog.Close();
			});

			if (completed)
			{
				logger.WriteTime("word file imported");
			}
			else
			{
				logger.StopClock();
			}
		}


		private void WordImporter(string filepath, bool append)
		{
			using (var word = new Word())
			{
				var html = word.ConvertFileToHtml(filepath);


				if (append)
				{
					using (var one = new OneNote(out var page, out _))
					{
						page.AddHtmlContent(html);
						one.Update(page);
					}
				}
				else
				{
					using (var one = new OneNote())
					{
						one.CreatePage(one.CurrentSectionId, out var pageId);
						var page = one.GetPage(pageId);

						page.Title = Path.GetFileName(filepath);
						page.AddHtmlContent(html);
						one.Update(page);
						one.NavigateTo(page.PageId);
					}
				}
			}
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Powerpoint...

		private void ImportPowerPoint(string filepath, bool append, bool split)
		{
			if (!Office.IsPowerPointInstalled())
			{
				UIHelper.ShowMessage("PowerPoint is not installed");
			}

			logger.StartClock();

			var completed = RunBackgroundTask(filepath, () =>
			{
				PowerPointImporter(filepath, append, split);

				progressDialog.DialogResult = DialogResult.OK;
				progressDialog.Close();
			});

			if (completed)
			{
				logger.WriteTime("powerpoint file imported");
			}
			else
			{
				logger.StopClock();
			}
		}


		private void PowerPointImporter(string filepath, bool append, bool split)
		{
			string outpath;
			using (var powerpoint = new PowerPoint())
			{
				outpath = powerpoint.ConvertFileToImages(filepath);
			}

			if (outpath == null)
			{
				logger.WriteLine($"failed to create output path");
				return;
			}

			if (split)
			{
				using (var one = new OneNote())
				{
					var section = one.CreateSection(Path.GetFileNameWithoutExtension(filepath));
					var sectionId = section.Attribute("ID").Value;
					var ns = one.GetNamespace(section);

					one.NavigateTo(sectionId);

					int i = 1;
					foreach (var file in Directory.GetFiles(outpath, "*.jpg"))
					{
						one.CreatePage(sectionId, out var pageId);
						var page = one.GetPage(pageId);
						page.Title = $"Slide {i}";
						var container = page.EnsureContentContainer();

						LoadImage(container, ns, file);

						one.Update(page);

						i++;
					}

					logger.WriteLine("created section");
				}
			}
			else
			{
				using (var one = new OneNote())
				{
					Page page;
					if (append)
					{
						page = one.GetPage();
					}
					else
					{
						one.CreatePage(one.CurrentSectionId, out var pageId);
						page = one.GetPage(pageId);
						page.Title = Path.GetFileName(filepath);
					}

					var container = page.EnsureContentContainer();

					foreach (var file in Directory.GetFiles(outpath, "*.jpg"))
					{
						using (var image = Image.FromFile(file))
						{
							LoadImage(container, page.Namespace, file);
						}
					}

					one.Update(page);

					if (!append)
					{
						one.NavigateTo(page.PageId);
					}
				}
			}

			try
			{
				Directory.Delete(outpath, true);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error cleaning up {outpath}", exc);
			}
		}


		private void LoadImage(XElement container, XNamespace ns, string filepath)
		{
			using (var image = Image.FromFile(filepath))
			{
				container.Add(
					new XElement(ns + "OE",
						new XElement(ns + "Image",
							new XElement(ns + "Size",
								new XAttribute("width", $"{image.Width:00}"),
								new XAttribute("height", $"{image.Height:00}"),
								new XAttribute("isSetByUser", "true")),
							new XElement(ns + "Data", image.ToBase64String())
						)),
					new XElement(ns + "OE",
						new XElement(ns + "T", new XCData(string.Empty))
						)
					);
			}
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Our trusty little worker

		private bool RunBackgroundTask(string path, Action action)
		{
			using (var source = new CancellationTokenSource())
			{
				using (progressDialog = new UI.ProgressDialog(source))
				{
					progressDialog.SetMaximum(MaxWait);
					progressDialog.SetMessage($"Importing {path}...");

					try
					{
						// process should run in an STA thread otherwise it will conflict with
						// the OneNote MTA thread environment
						var thread = new Thread(() =>
						{
							action();
						});

						thread.SetApartmentState(ApartmentState.STA);
						thread.IsBackground = true;
						thread.Start();

						progressDialog.StartTimer();
						var result = progressDialog.ShowDialog(owner);

						if (result == DialogResult.Cancel)
						{
							logger.WriteLine("clicked cancel");
							thread.Abort();
							return false;
						}
					}
					catch (Exception exc)
					{
						logger.WriteLine("error importing", exc);
					}
				}
			}

			return true;
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// XML...

		private void ImportXml(string filepath)
		{
			try
			{
				// load page-from-file
				var template = new Page(XElement.Load(filepath));

				using (var one = new OneNote())
				{
					one.CreatePage(one.CurrentSectionId, out var pageId);

					// remove any objectID values and let OneNote generate new IDs
					template.Root.Descendants().Attributes("objectID").Remove();

					// set the page ID to the new page's ID
					template.Root.Attribute("ID").Value = pageId;

					if (string.IsNullOrEmpty(template.Title))
					{
						template.Title = Path.GetFileNameWithoutExtension(filepath);
					}

					one.Update(template);
					one.NavigateTo(pageId);
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
				UIHelper.ShowMessage("Could not import. See log file for details");
			}
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// OneNote...

		private void ImportOneNote(string filepath)
		{
			try
			{
				using (var one = new OneNote())
				{
					/*
					 * TODO: Incomplete! See the OneNote.Import method for details :-(
					 */

					var pageId = one.Import(filepath);
					//one.NavigateTo(pageId);
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
				UIHelper.ShowMessage("Could not import. See log file for details");
			}
		}
	}
}
