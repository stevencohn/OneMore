//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal enum StatusColor
	{
		Blue,
		Gray,
		Green,
		Red,
		Yellow
	}


	#region Wrappers
	internal class InsertBlueStatusCommand : InsertStatusCommand
	{
		public InsertBlueStatusCommand() : base() { }
		public override Task Execute(params object[] args)
		{
			return base.Execute(StatusColor.Blue);
		}
	}
	internal class InsertGrayStatusCommand : InsertStatusCommand
	{
		public InsertGrayStatusCommand() : base() { }
		public override Task Execute(params object[] args)
		{
			return base.Execute(StatusColor.Gray);
		}
	}
	internal class InsertGreenStatusCommand : InsertStatusCommand
	{
		public InsertGreenStatusCommand() : base() { }
		public override Task Execute(params object[] args)
		{
			return base.Execute(StatusColor.Green);
		}
	}
	internal class InsertRedStatusCommand : InsertStatusCommand
	{
		public InsertRedStatusCommand() : base() { }
		public override Task Execute(params object[] args)
		{
			return base.Execute(StatusColor.Red);
		}
	}
	internal class InsertYellowStatusCommand : InsertStatusCommand
	{
		public InsertYellowStatusCommand() : base() { }
		public override Task Execute(params object[] args)
		{
			return base.Execute(StatusColor.Yellow);
		}
	}
	#endregion Wrappers


	internal class InsertStatusCommand : Command
	{

		public InsertStatusCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var statusColor = (StatusColor)args[0];

			await using var one = new OneNote(out var page, out var ns);
			if (!page.ConfirmBodyContext())
			{
				ShowError(Resx.Error_BodyContext);
				return;
			}

			var elements = page.Root.Descendants(ns + "T")
				.Where(e => e.Attribute("selected")?.Value == "all");

			if (elements.Any())
			{
				string color = "black";
				string background = "yellow";

				switch (statusColor)
				{
					case StatusColor.Gray:
						color = "white";
						background = "#42526E";
						break;

					case StatusColor.Red:
						color = "white";
						background = "#BF2600";
						break;

					case StatusColor.Yellow:
						color = "172B4D";
						background = "#FF991F";
						break;

					case StatusColor.Green:
						color = "white";
						background = "#00875A";
						break;

					case StatusColor.Blue:
						color = "white";
						background = "#0052CC";
						break;
				}

				var colors = $"color:{color};background:{background}";
				var text = "     STATUS     ";

				var content = new XElement(ns + "T",
					new XCData(
						new XElement("span",
							new XAttribute("style",
								$"font-size:10.0pt;font-weight:bold;{colors}"),
							text
						).ToString(SaveOptions.DisableFormatting) + "&#160;")
					);

				var editor = new PageEditor(page);
				editor.ReplaceSelectedWith(content);

				await one.Update(page);
			}
		}
	}
}
