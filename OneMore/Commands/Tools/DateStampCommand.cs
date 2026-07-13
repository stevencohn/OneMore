//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;


	/// <summary>
	/// Prepends the title of each page in the current section with the created date of the page
	/// using the form YYYY-MM-DD.If a page title already contains that value, no changes are made
	/// </summary>
	internal class DateStampCommand : Command
	{
		private sealed class PageInfo
		{
			public string ID;
			public string Name;
			public string Date;
		}


		private OneNote one;
		private UI.ProgressDialog progress;
		private Regex regex;


		public DateStampCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var stamping = (args.Length == 0) || ((args[0] is bool b) && b);

			if (!stamping)
			{
				regex = new Regex(@"^\d{4}-\d{2}-\d{2}\s.+", RegexOptions.Compiled);
			}

			await using (one = new OneNote())
			{
				var section = await one.GetSection();
				var ns = one.GetNamespace(section);

				var infos = section.Elements(ns + "Page")
					.Select(e => new PageInfo
					{
						ID = e.Attribute("ID").Value,
						Name = e.Attribute("name").Value,
						Date = e.Attribute("dateTime").Value
					})
					.ToList();

				if (infos.Any())
				{
					logger.StartClock();

					using (progress = new UI.ProgressDialog())
					{
						progress.SetMaximum(infos.Count);
						progress.Show();

						foreach (var info in infos)
						{
							progress.SetMessage(info.Name);

							if (stamping)
							{
								await StampPage(info);
							}
							else
							{
								await UnstampPage(info);
							}
						}

						progress.Close();
					}

					logger.StopClock();
					logger.WriteTime("datestamp pages");
				}
			}
		}


		private async Task StampPage(PageInfo info)
		{
			string stamp = info.Date.Substring(0, 10); // yyyy-mm-dd
			if (info.Name.Contains(stamp))
			{
				return;
			}

			var page = await one.GetPage(info.ID, OneNote.PageDetail.Basic);
			page.Title = $"{stamp} {page.Title}";

			await one.Update(page);
		}


		private async Task UnstampPage(PageInfo info)
		{
			if (regex.Match(info.Name).Success)
			{
				var page = await one.GetPage(info.ID, OneNote.PageDetail.Basic);
				page.Title = page.Title.Substring(11);
				await one.Update(page);
			}
		}
	}
}
