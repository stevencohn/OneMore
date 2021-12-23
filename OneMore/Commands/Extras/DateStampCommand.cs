//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Threading.Tasks;


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


		public DateStampCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (one = new OneNote())
			{
				var section = one.GetSection();
				var ns = one.GetNamespace(section);

				var infos = section.Elements(ns + "Page")
					.Select(e => new PageInfo
					{
						ID = e.Attribute("ID").Value,
						Name = e.Attribute("name").Value,
						Date = e.Attribute("dateTime").Value
					})
					.ToList();

				if (infos?.Count > 0)
				{
					logger.StartClock();

					using (progress = new UI.ProgressDialog())
					{
						progress.SetMaximum(infos.Count);
						progress.Show(owner);

						foreach (var info in infos)
						{
							progress.SetMessage(info.Name);
							await StampPage(info);
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

			var page = one.GetPage(info.ID, OneNote.PageDetail.Basic);
			page.Title = $"{stamp} {page.Title}";

			await one.Update(page);
		}
	}
}
