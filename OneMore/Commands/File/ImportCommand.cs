//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Dialogs;
	using River.OneMoreAddIn.Helpers.Office;
	using System;
	using System.IO;
	using System.Threading;
	using System.Windows.Forms;


	internal class ImportCommand : Command
	{
		private ProgressDialog progressDialog;


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

				if (dialog.WordFile)
				{
					ImportWord(dialog.FilePath, dialog.AppendToPage);
				}
				else
				{
					ImportPowerpoint(dialog.FilePath, dialog.AppendToPage, dialog.SplitSlides);
				}
			}
		}

		private void ImportWord(string path, bool append)
		{
			if (!Office.IsWordInstalled())
			{
				UIHelper.ShowMessage("Word is not installed");
			}

			logger.StartClock();

			var completed = ConvertWordFile(path, () =>
			{
				using (var word = new Word())
				{
					var html = word.ConvertFileToHtml(path);

					progressDialog.DialogResult = DialogResult.OK;
					progressDialog.Close();

					if (append)
					{
						using (var one = new OneNote(out var page, out var ns))
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

							page.Title = Path.GetFileName(path);
							page.AddHtmlContent(html);
							one.Update(page);
							one.NavigateTo(page.PageId);
						}
					}
				}
			});

			if (completed)
			{
				logger.WriteTime("word file converted");
			}
			else
			{
				logger.StopClock();
			}
		}


		private bool ConvertWordFile(string path, Action action)
		{
			using (var source = new CancellationTokenSource())
			{
				using (progressDialog = new ProgressDialog(source))
				{
					progressDialog.SetMaximum(15);
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
							logger.WriteLine("Clicked cancel");
							thread.Abort();
							return false;
						}
					}
					catch (Exception exc)
					{
						logger.WriteLine("Error running Execute(string)", exc);
					}
				}
			}

			return true;
		}


		private void ImportPowerpoint(string path, bool append, bool split)
		{
			//
		}
	}
}
