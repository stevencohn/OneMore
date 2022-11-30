//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal enum Expando
	{
		Collapse,
		Expand,
		Save,
		Restore
	}


	/// <summary>
	/// Manages collapsible elements, expanding, collapsing, and saving or restoring the current
	/// collapsed state of these elements.
	/// </summary>
	internal class ExpandoCommand : Command
	{
		private const string MetaName = "omExpando";

		private Page page;
		private XNamespace ns;


		public ExpandoCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var one = new OneNote(out page, out ns);

			bool updated = false;

			switch ((Expando)(args[0]))
			{
				case Expando.Collapse:
					updated = CollapseAll();
					break;

				case Expando.Expand:
					updated = ExpandAll();
					break;

				case Expando.Restore:
					updated = Restore();
					break;

				case Expando.Save:
					updated = Save();
					break;
			}

			if (updated)
			{
				await one.Update(page);
			}
		}


		private IEnumerable<XElement> FindContainers()
		{
			// find all OE/OEChildren where OEChildren is only preceded by T elements;
			// the T elements specify the title, the OEChildren contains the indented content
			return page.Root.Descendants(ns + "OE")
				.Where(e => e.Element(ns + "OEChildren") != null
					&& e.Element(ns + "OEChildren").ElementsBeforeSelf()
						.All(b => b.Name.LocalName == "T" || b.Name.LocalName == "Meta"));
		}


		private bool CollapseAll()
		{
			var containers = FindContainers();
			if (!containers.Any())
			{
				return false;
			}

			// will not collapse a container if cursor is within its contents

			foreach (var container in containers)
			{
				container.SetAttributeValue("collapsed", "1");
			}

			return true;
		}


		private bool ExpandAll()
		{
			var containers = page.Root.Descendants(ns + "OE")
				.Where(e => e.Attribute("collapsed") != null);

			if (!containers.Any())
			{
				return false;
			}

			foreach (var container in containers)
			{
				var collapsed = container.Attribute("collapsed");
				collapsed?.Remove();
			}

			return true;
		}


		private bool Restore()
		{
			var containers = FindContainers();
			if (!containers.Any())
			{
				return false;
			}

			foreach (var container in containers)
			{
				if (container.Elements(ns + "Meta")
					.Any(e => e.Attribute("name").Value == MetaName
					&& e.Attribute("content").Value == "1"))
				{
					if (container.Attribute("collapsed")?.Value != "1")
					{
						container.SetAttributeValue("collapsed", "1");
					}
				}
				else
				{
					container.Attributes("collapsed").Remove();
				}
			}

			return true;
		}


		private bool Save()
		{
			var containers = FindContainers();
			if (!containers.Any())
			{
				return false;
			}

			foreach (var container in containers)
			{
				if (container.Attribute("collapsed")?.Value == "1")
				{
					var meta = container.Elements(ns + "Meta")
						.FirstOrDefault(e => e.Attribute("name").Value == MetaName);

					if (meta == null)
					{
						container.AddFirst(new XElement(ns + "Meta",
							new XAttribute("name", MetaName),
							new XAttribute("content", "1")
							));
					}
					else if (meta.Attribute("content").Value != "1")
					{
						meta.SetAttributeValue("content", "1");
					}
				}
				else
				{
					var meta = container.Elements(ns + "Meta")
						.FirstOrDefault(e => e.Attribute("name").Value == MetaName);

					// meta.Remove() doesn't work
					meta?.SetAttributeValue("content", "0");
				}
			}

			UIHelper.ShowMessage(Resx.ExpandoCommand_Saved);

			return true;
		}
	}
}
