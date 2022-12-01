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
			// start by extracting just name part without tag
			// note that page.Title gets plain text

			var match = Regex.Match(page.Title, $@"([^(]+)(?:\s*\((\d+)\))?");
			var title = match.Groups[1].Success ? match.Groups[1].Value.Trim() : page.Title;

			// match all pages in section on <name>[(tag)]

			var regex = new Regex($@"{title}(?:\s*\((\d+)\))?");

			// this will also include the current page so should result in a max value
			var index = section.Elements(ns + "Page")
				.Select(e => regex.Match(e.Attribute("name").Value))
				.Where(m => m.Success)
				.Max(m => m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 0) + 1;

			// get the sytlized content so we can update the tag in place

			var run = page.Root.Elements(ns + "Title")
				.Elements(ns + "OE")
				.Elements(ns + "T")
				.LastOrDefault(e => !string.IsNullOrWhiteSpace(e.GetCData().Value));

			if (run == null)
			{
				// shouldn't happen?
				page.Title = $"{page.Title} ({index})";
				return;
			}

			var wrapper = run.GetCData().GetWrapper();
			var node = wrapper.Nodes().Last();

			var text = node.NodeType == System.Xml.XmlNodeType.Text
				? (XText)node
				: ((XElement)node).Nodes().OfType<XText>().Last();

			regex = new Regex(@"\(\d+\)\s*$");
			if (regex.IsMatch(text.Value))
			{
				text.Value = regex.Replace(text.Value, $"({index})");
			}
			else
			{
				text.Value = $"{text.Value} ({index})";
			}

			page.Title = wrapper.GetInnerXml();
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
