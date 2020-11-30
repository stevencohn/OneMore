//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Xml.Linq;


	internal class ToCaseCommand : Command
	{
		public ToCaseCommand ()
		{
		}


		public override void Execute(params object[] args)
		{
			bool upper = (bool)args[0];

			using (var one = new OneNote(out var page, out var ns))
			{
				var updated = page.EditSelected((s) =>
				{
					if (s is XText text)
					{
						text.Value = upper ? text.Value.ToUpper() : text.Value.ToLower();
						return text;
					}

					var element = (XElement)s;
					element.Value = upper ? element.Value.ToUpper() : element.Value.ToLower();
					return element;
				});

				if (updated)
				{
					one.Update(page);
				}
			}
		}
	}
}
