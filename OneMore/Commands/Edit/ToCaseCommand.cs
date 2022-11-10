//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Globalization;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class ToCaseCommand : Command
	{
		public const int Lowercase = 0;
		public const int Uppercase = 1;
		public const int Titlecase = 3;

		private readonly TextInfo info;


		public ToCaseCommand ()
		{
			info = Thread.CurrentThread.CurrentCulture.TextInfo;
		}


		public override async Task Execute(params object[] args)
		{
			var casing = (int)args[0];

			using var one = new OneNote(out var page, out var ns);
			var updated = page.EditSelected((s) =>
			{
				if (s is XText text)
				{
					text.Value = Recase(text.Value, casing);
					return text;
				}

				var element = (XElement)s;
				element.Value = Recase(element.Value, casing);
				return element;
			});

			if (updated)
			{
				await one.Update(page);
			}
		}


		private string Recase(string text, int casing)
		{
			if (casing == Lowercase)
			{
				return info.ToLower(text);
			}

			if (casing == Uppercase)
			{
				return info.ToUpper(text);
			}

			return info.ToTitleCase(text);
		}
	}
}
