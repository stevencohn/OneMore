//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class ProofingCommand : Command
	{
		public const string NoLang = "yo";


		public ProofingCommand()
		{
		}


		/*
		 * Note that if the lang is ambiguous for some text that is not the CurrentCulture
		 * codec then OneNote will infer the language and inject a lang attribute in the CDATA
		 */


		public override async Task Execute(params object[] args)
		{
			var cultureName = (string)args[0];

			logger.StartClock();

			using (var one = new OneNote(out var page, out var ns))
			{
				if (page == null)
				{
					logger.WriteTime("spell check cancelled; no page context");
					return;
				}

				var partial = false;
				if (page.GetTextCursor() == null && page.SelectionScope == SelectionScope.Region)
				{
					// update only selected text...

					var selections = page.Root.Elements(ns + "Outline")
						.Descendants(ns + "T")
						.Where(e => e.Attributes("selected").Any(a => a.Value.Equals("all")));

					if (selections.Any())
					{
						selections.ForEach(e =>
						{
							// remove all "lang=" attributes within selected CDATA
							var wrapper = e.GetCData().GetWrapper();
							var atts = wrapper.Descendants().Attributes("lang");
							if (atts.Any())
							{
								atts.Remove();
								e.Value = wrapper.GetInnerXml();
							}

							// set lang on T element
							e.SetAttributeValue("lang", cultureName);
						});

						partial = true;
					}
					else
					{
						logger.WriteLine("selections not found; setting entire page");
					}
				}

				if (!partial)
				{
					// remove all occurances of the "lang=" attribute across the entire page
					page.Root.DescendantsAndSelf()
						.Where(e => e.Name.LocalName != "OCRData")
						.Attributes("lang")
						.Remove();

					// remove all occurances of the "lang=" attribute within all CDATA
					page.Root.Descendants(ns + "T").ForEach(e =>
					{
						var wrapper = e.GetCData().GetWrapper();
						var atts = wrapper.Descendants().Attributes("lang");
						if (atts.Any())
						{
							atts.Remove();
							e.Value = wrapper.GetInnerXml();
						}
					});

					// set lang for entire page
					page.Root.Add(new XAttribute("lang", cultureName));
					page.Root.Element(ns + "Title")?.Add(new XAttribute("lang", cultureName));
				}

				//logger.WriteLine(page.Root);

				await one.Update(page);

				var area = partial ? "selection" : "full page";
				var abled = cultureName == NoLang ? "disabled" : "enabled";
				logger.WriteTime($"spell check {abled} for {cultureName} on {area}");
			}
		}
	}
}
