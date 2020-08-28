//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Dialogs;
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
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
							bool updated = false;
							ns = page.Namespace;

							headings = page.GetHeadings(manager);
							if (dialog.CleanupNumbering)
							{
								RemoveOutlineNumbering();
								updated = true;
							}

							if (dialog.NumericNumbering)
							{
								AddOutlineNumbering(true);
								updated = true;
							}
							else if (dialog.AlphaNumbering)
							{
								AddOutlineNumbering(false);
								updated = true;
							}

							if (dialog.Indent)
							{
								IndentContent(dialog.IndentTagged, dialog.TagSymbol);
								updated = true;
							}

							if (updated)
							{
								manager.UpdatePageContent(page.Root);
							}
						}
					}
				}
			}
		}


		private void RemoveOutlineNumbering()
		{
			var npattern = new Regex(@"^(\d+\.\s).+");
			var apattern = new Regex(@"^([a-z]+\.\s).+");
			var ipattern = new Regex(@"^((?:xc|xl|l?x{0,3})(?:ix|iv|v?i{0,3})\.\s).+");


			foreach (var heading in headings)
			{
				var match = npattern.Match(heading.Text);
				if (match.Success)
				{

				}
				else
				{
					match = apattern.Match(heading.Text);
					if (match.Success)
					{

					}
					else
					{
						match = ipattern.Match(heading.Text);
						if (match.Success)
						{
						}
					}
				}
			}
		}


		private void AddOutlineNumbering(bool numeric)
		{
		}


		private void IndentContent(bool indentTagged, int tagSymbol)
		{
		}
	}
}
