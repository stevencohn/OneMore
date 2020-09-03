//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;
	using System.Text.RegularExpressions;


	internal class RemovePageNumbersCommand : Command
	{
		public RemovePageNumbersCommand()
		{
		}


		public void Execute()
		{
			var npattern = new Regex(@"^(\(\d+\)\s).+");
			var apattern = new Regex(@"^(\([a-z]+\)\s).+");
			var ipattern = new Regex(@"^(\((?:xc|xl|l?x{0,3})(?:ix|iv|v?i{0,3})\)\s).+");

			using (var manager = new ApplicationManager())
			{
				var section = manager.CurrentSection();
				if (section != null)
				{
					var ns = section.GetDefaultNamespace();

					var pages = section.Elements(ns + "Page").ToList();

					bool modified = false;

					foreach (var page in pages)
					{
						var name = section.Attributes("name").FirstOrDefault();
						if (name != null)
						{
							if (string.IsNullOrEmpty(name.Value))
							{
								continue;
							}

							// numeric 1.
							var match = npattern.Match(name.Value);

							// alpha a.
							if (!match.Success)
							{
								match = apattern.Match(name.Value);
							}

							// alpha i.
							if (!match.Success)
							{
								match = ipattern.Match(name.Value);
							}

							if (match.Success)
							{
								name.Value = name.Value.Substring(match.Groups[1].Length);
								modified = true;
							}
						}
					}

					if (modified)
					{
						manager.UpdateHierarchy(section);
					}
				}
			}
		}
	}
}
