//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	/// <summary>
	/// Applies numbering to headings, with options to indent content below headings or
	/// indent tagged content.
	/// </summary>
	internal class OutlineCommand : Command
	{
		private XNamespace ns;
		private List<Heading> headings;


		public OutlineCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var dialog = new OutlineDialog();
			if (dialog.ShowDialog() != DialogResult.OK)
			{
				return;
			}

			using var one = new OneNote(out var page, out ns);
			if (!page.IsValid)
			{
				return;
			}

			headings = page.GetHeadings(one);
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

			if (dialog.Indent || dialog.IndentTagged)
			{
				IndentContent(page,
					dialog.Indent,
					dialog.IndentTagged,
					dialog.TagSymbol,
					dialog.RemoveTags);
			}

			// if OK then something must have happened, so save it
			await one.Update(page);
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
		private void IndentContent(
			Page page, bool indentHeadings, bool indentTagged, int tagSymbol, bool removeTags)
		{
			string tagIndex = null;
			if (indentTagged)
			{
				// find the index of the tagdef of tagSymbol
				tagIndex = page.Root.Elements(ns + "TagDef")
					.Where(e => e.Attribute("symbol").Value == tagSymbol.ToString())
					.Select(e => e.Attribute("index").Value).FirstOrDefault();
			}

			List<XElement> elements;

			if (indentHeadings)
			{
				// only below headings
				elements = headings.Select(h => h.Root).ToList();
			}
			else
			{
				// below any tagged paragraph
				elements = page.Root.Elements(ns + "Outline")
					.Descendants(ns + "Tag")
					.Where(t => t.Attribute("index").Value == tagIndex)
					.Select(e => e.Parent)
					.ToList();
			}

			for (int i = 0; i < elements.Count; i++)
			{
				var element = elements[i];
				XElement tag = null;

				if (tagIndex != null)
				{
					tag = element.Elements(ns + "Tag")
						.FirstOrDefault(e => e.Attribute("index").Value == tagIndex);

					if (tag == null)
					{
						// this heading is not tagged with the specified tag
						continue;
					}

					if (removeTags)
					{
						tag.Remove();
					}
				}

				var siblings = element.ElementsAfterSelf()?.ToList();
				if (siblings?.Count > 0)
				{
					var next = i < elements.Count - 1 ? elements[i + 1] : null;

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
						element.Add(children);
					}
				}
			}
		}
	}
}
