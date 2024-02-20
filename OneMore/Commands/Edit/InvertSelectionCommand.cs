﻿//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Inverts the text selections on the current page, affecting the entire page.
	/// </summary>
	internal class InvertSelectionCommand : Command
	{
		public InvertSelectionCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out var ns);
			var hidden = page.Root.Elements(ns + "Outline")
				.Descendants(ns + "OE")
				.Where(e => e.Attribute("collapsed")?.Value == "1")
				.Elements(ns + "OEChildren")
				.Descendants(ns + "T");

			var pos = page.Root.Elements(ns + "Outline")
				.Descendants(ns + "T")
				.Where(e => e.Attribute("selected")?.Value == "all")
				.ToList();

			var neg = page.Root.Elements(ns + "Outline")
				.Descendants(ns + "T")
				.Except(hidden)
				.Except(pos)
				.ToList();

			pos.ForEach((e) =>
			{
				e.Attributes("selected").Remove();
			});

			neg.ForEach((e) =>
			{
				e.SetAttributeValue("selected", "all");
			});

			if (pos.Count > 0 || neg.Count > 0)
			{
				await one.Update(page);
			}
		}
	}
}
