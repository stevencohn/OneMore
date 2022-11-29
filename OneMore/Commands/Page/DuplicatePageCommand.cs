//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Simple and direct duplication of current page, inserting the new page immediate
	/// after the current page in the section. Adds (#) after the page title.
	/// </summary>
	internal class DuplicatePageCommand : Command
	{

		public DuplicatePageCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			// need All detail to copy images and Ink
			using var one = new OneNote(out var page, out var ns, OneNote.PageDetail.All);
			var originalId = page.PageId;
			var sectionId = one.CurrentSectionId;

			// create a new page with new ID and update its title
			one.CreatePage(sectionId, out var newId);

			// set the page ID to the new page's ID
			page.Root.Attribute("ID").Value = newId;
			// remove all objectID values and let OneNote generate new IDs
			page.Root.Descendants().Attributes("objectID").Remove();
			page = new Page(page.Root); // reparse to refresh PageId

			var section = one.GetSection(sectionId);
			SetUniquePageTitle(ns, section, page);

			await one.Update(page);

			MovePageAfterOriginal(one, ns, section, page, originalId);

			await one.NavigateTo(page.PageId, string.Empty);
		}


		private void SetUniquePageTitle(XNamespace ns, XElement section, Page page)
		{
			// extract just name part without tag
			var match = Regex.Match(page.Title, $@"([^(]+)(?:\s*\((\d+)\))?");
			var title = match.Groups[1].Success ? match.Groups[1].Value.Trim() : page.Title;

			// now match all pages in section on <name>[(tag)]
			var regex = new Regex($@"{title}(?:\s*\((\d+)\))?");

			var last = section.Elements(ns + "Page")
				.Select(e => new
				{
					Element = e,
					Name = e.Attribute("name").Value,
					Match = regex.Match(e.Attribute("name").Value)
				})
				.Where(m => m.Match.Success)
				.OrderBy(e => e.Name)
				.LastOrDefault();

			// make unique title
			if (last != null && last.Match.Success && last.Match.Groups[1].Success)
			{
				var tag = int.Parse(last.Match.Groups[1].Value) + 1;
				page.Title = $"{title} ({tag})";
			}
			else
			{
				page.Title = $"{title} (1)";
			}
		}


		private void MovePageAfterOriginal(
			OneNote one, XNamespace ns, XElement section, Page page, string originalId)
		{
			var original = section.Elements(ns + "Page")
				.FirstOrDefault(e => e.Attribute("ID").Value == originalId);

			if (original != null)
			{
				var entry = section.Elements(ns + "Page")
					.FirstOrDefault(e => e.Attribute("ID").Value == page.PageId);

				if (entry != null)
				{
					entry.Remove();
					entry.SetAttributeValue("pageLevel", original.Attribute("pageLevel").Value);
					original.AddAfterSelf(entry);

					one.UpdateHierarchy(section);
				}
			}
		}
	}
}
