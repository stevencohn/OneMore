//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Prototype helper that creates a page with sample form control hyperlinks. Invoke it
	/// by clicking a hyperlink to onemore://FormControlsDemoCommand. To be removed once the
	/// real insert commands exist.
	/// </summary>
	[CommandService]
	internal class FormControlsDemoCommand : Command
	{
		private const string DemoList = "Fruit";


		public FormControlsDemoCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			SeedDemoList();

			await using var one = new OneNote();

			one.CreatePage(one.CurrentSectionId, out var pageId);
			var page = await one.GetPage(pageId);
			var ns = page.Namespace;

			page.Title = "Form Controls Demo";

			var container = page.EnsureContentContainer();
			container.Add(
				MakeLine(ns, "Start date: ", FormControlCommand.DateKind, "Choose date…"),
				MakeLine(ns, "Favorite fruit: ", FormControlCommand.ListKind, "Choose Fruit…", DemoList),
				MakeLine(ns, "End date: ", FormControlCommand.DateKind, "Choose date…"));

			await one.Update(page);
			await one.NavigateTo(pageId, string.Empty);
		}


		private static void SeedDemoList()
		{
			using var provider = new FormControlsProvider();
			var lists = provider.ReadLists();
			if (lists.Exists(l => l.Name.Equals(DemoList, StringComparison.OrdinalIgnoreCase)))
			{
				return;
			}

			var fruit = new FormList(DemoList);
			fruit.Items.AddRange(new[]
			{
				"Apple", "Banana", "Cherry", "Grape", "Lemon", "Mango", "Orange", "Peach"
			});

			lists.Add(fruit);
			provider.WriteLists(lists);
		}


		private static XElement MakeLine(
			XNamespace ns, string label, string kind, string prompt, string list = null)
		{
			var id = Guid.NewGuid().ToString("N");
			var anchor = FormControlCommand.MakeAnchor(kind, id, prompt, list);

			return new XElement(ns + "OE",
				new XElement(ns + "T", new XCData(label + anchor)));
		}
	}
}
