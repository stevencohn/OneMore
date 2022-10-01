//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Scan pages looking for and removing duplicates
	/// </summary>
	internal class RemoveDuplicatesCommand : Command
	{
		public RemoveDuplicatesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var cruncher = new MD5CryptoServiceProvider();

			using (var one = new OneNote(out _, out var ns))
			{
				var hierarchy = one.GetSection();

				hierarchy.Descendants(ns + "Page").ForEach(p =>
				{
					var page = one.GetPage(p.Attribute("ID").Value, OneNote.PageDetail.Basic);

					// EditedByAttributes and the page ID
					page.Root.DescendantsAndSelf().Attributes().Where(a =>
						a.Name.LocalName == "ID"
						|| a.Name.LocalName == "dateTime"
						|| a.Name.LocalName == "callbackID"
						|| a.Name.LocalName == "author"
						|| a.Name.LocalName == "authorInitials"
						|| a.Name.LocalName == "authorResolutionID"
						|| a.Name.LocalName == "lastModifiedBy"
						|| a.Name.LocalName == "lastModifiedByInitials"
						|| a.Name.LocalName == "lastModifiedByResolutionID"
						|| a.Name.LocalName == "creationTime"
						|| a.Name.LocalName == "lastModifiedTime"
						|| a.Name.LocalName == "objectID")
						.Remove();

					var xml = page.Root.ToString(SaveOptions.DisableFormatting);
					var pageHash = Convert.ToBase64String(
						cruncher.ComputeHash(UnicodeEncoding.Default.GetBytes(xml)));

					var text = page.Root.Value;					
					var textHash = Convert.ToBase64String(
						cruncher.ComputeHash(UnicodeEncoding.Default.GetBytes(text)));

					logger.WriteLine($"page hash [{pageHash}] text hash [{textHash}]");

				});
			}

			await Task.Yield();
		}
	}
}
