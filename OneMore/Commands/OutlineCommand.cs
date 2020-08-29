//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Dialogs;
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class OutlineCommand : Command
	{
		private XNamespace ns;
		private List<Heading> headings;


		public OutlineCommand() : base()
		{
		}


		public void Execute()
		{
			using (var dialog = new OutlineDialog())
			{
				if (dialog.ShowDialog(owner) == DialogResult.OK)
				{
					using (var manager = new ApplicationManager())
					{
						var page = new Page(manager.CurrentPage());
						if (page.IsValid)
						{
							ns = page.Namespace;

							headings = page.GetHeadings(manager);
							if (dialog.CleanupNumbering)
							{
								RemoveOutlineNumbering();
							}

							if (dialog.NumericNumbering)
							{
								AddOutlineNumbering(true, 0, 0, 1, string.Empty);
							}
							else if (dialog.AlphaNumbering)
							{
								AddOutlineNumbering(false, 0, 0, 1, string.Empty);
							}

							if (dialog.Indent)
							{
								IndentContent(page,
									dialog.IndentTagged,
									dialog.TagSymbol,
									dialog.RemoveTags);
							}

							// if OK then something must have happened, so save it
							manager.UpdatePageContent(page.Root);
						}
					}
				}
			}
		}


		//* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
		// Remove outline numbering...

		private void RemoveOutlineNumbering()
		{
			var npattern = new Regex(@"^((?:\d+\.)+\s).+");
			var apattern = new Regex(@"^([a-z]+\.\s).+");
			var ipattern = new Regex(@"^((?:xc|xl|l?x{0,3})(?:ix|iv|v?i{0,3})\.\s).+");

			foreach (var heading in headings)
			{
				var cdata = heading.Root.Element(ns + "T").GetCData();

				var wrapper = cdata.GetWrapper();
				var text = wrapper.DescendantNodes().OfType<XText>().FirstOrDefault();
				if (text == null)
				{
					continue;
				}

				// numeric 1.
				var match = npattern.Match(text.Value);

				// alpha a.
				if (!match.Success)
				{
					match = apattern.Match(text.Value);
				}

				// alpha i.
				if (!match.Success)
				{
					match = ipattern.Match(text.Value);
				}

				if (match.Success)
				{
					text.Value = text.Value.Substring(match.Groups[1].Length);
					cdata.Value = wrapper.GetInnerXml();
				}
			}
		}


		//* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
		// Add outline numbering...

		int AddOutlineNumbering(bool numeric, int index, int level, int counter, string prefix)
		{
			if (index > headings.Count)
			{
				return index;
			}

			while (index < headings.Count)
			{
				PrefixHeader(headings[index].Root, numeric, level, counter, prefix);

				index++;
				counter++;

				if (index < headings.Count && headings[index].Level < level)
				{
					break;
				}

				if (index < headings.Count && headings[index].Level > level)
				{
					index = AddOutlineNumbering(numeric, index, headings[index].Level, 1, $"{prefix}{counter - 1}.");
					if (index < headings.Count && headings[index].Level < level)
					{
						break;
					}
				}
			}

			return index;
		}


		private void PrefixHeader(XElement root, bool numeric, int level, int counter, string prefix)
		{
			var cdata = root.Element(ns + "T").GetCData();
			var wrapper = cdata.GetWrapper();
			var text = wrapper.DescendantNodes().OfType<XText>().FirstOrDefault();

			if (numeric)
			{
				text.Value = $"{prefix}{counter}. {text.Value} ";
			}
			else
			{
				switch (level % 3)
				{
					case 0:
						text.Value = $"{counter}. {text.Value}";
						break;

					case 1:
						text.Value = $"{counter.ToAlphabetic().ToLower()}. {text.Value}";
						break;

					case 2:
						text.Value = $"{counter.ToRoman().ToLower()}. {text.Value}";
						break;
				}
			}

			cdata.Value = wrapper.GetInnerXml();
		}


		//* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
		// Indent...

		/*
		<one:OE>
		  <one:OEChildren>
			<one:OE>
			  <one:T><![CDATA[tfox]]></one:T>
			</one:OE>
		  </one:OEChildren>
		</one:OE>


	    <one:TagDef index="0" symbol="143"/>

	    <one:OE>
		  <one:Tag index="0"/>
		  <one:T><![CDATA[Heading1]]></one:T>
		</one:OE>

		*/
		private void IndentContent(Page page, bool indentTagged, int tagSymbol, bool removeTags)
		{
			string tagIndex = null;
			if (indentTagged)
			{
				// find the index of the tagdef of tagSymbol
				tagIndex = page.Root.Elements(ns + "TagDef")
					.Where(e => e.Attribute("symbol").Value == tagSymbol.ToString())
					.Select(e => e.Attribute("index").Value).FirstOrDefault();
			}

			for (int i = 0; i < headings.Count; i++)
			{
				var heading = headings[i];
				XElement tag = null;

				if (tagIndex != null)
				{
					tag = heading.Root.Element(ns + "Tag");
					if (tag == null ||
						tag.Name.LocalName != "Tag" ||
						tag.Attribute("index").Value != tagIndex)
					{
						// this heading is not tagged
						continue;
					}
				}

				if (tag != null && removeTags)
				{
					tag.Remove();
				}

				var siblings = heading.Root.ElementsAfterSelf()?.ToList();
				if (siblings != null && siblings.Count > 0)
				{
					var next = i < headings.Count - 1 ? headings[i + 1].Root : null;

					var children = new XElement(ns + "OEChildren");

					foreach (var sibling in siblings)
					{
						// did we hit the next heading?
						if (sibling == next)
						{
							break;
						}

						// move content into new OEChildren
						sibling.Remove();
						children.Add(sibling);
					}

					if (children.HasElements)
					{
						heading.Root.AddAfterSelf(new XElement(ns + "OE", children));
					}
				}
			}
		}
	}
}
