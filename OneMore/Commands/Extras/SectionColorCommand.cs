//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Drawing;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Available as a command from the Section context menu, sets the color of the section's tab
	/// </summary>
	internal class SectionColorCommand : Command
	{

		public SectionColorCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			if (!Native.GetCursorPos(out var location))
			{
				location = new Native.Point() { X = 100, Y = 100 };
			}

			// adjust for offset of context menu item that invokes this command
			location.Y -= SystemInformation.MenuHeight * 4;
			location.X -= 30;

			using var one = new OneNote();

			var section = one.GetSection();
			var sectionColor = section.Attribute("color").Value;
			if (sectionColor == "none")
			{
				// close approximation of none
				sectionColor = "#F0F0F0";
			}

			using var dialog = new UI.MoreColorDialog(
				Resx.SectionColor_Title, location.X, location.Y);

			dialog.Color = ColorTranslator.FromHtml(sectionColor);
			dialog.FullOpen = true;

			// use the elevator to force ColorDialog to top-most first time used
			// otherwise, it will be hidden by the OneMore window
			using var elevator = new UI.WindowElevator(dialog);
			var result = elevator.ShowDialog();
			if (result == DialogResult.OK)
			{
				section.SetAttributeValue("color", dialog.Color.ToRGBHtml());
				one.UpdateHierarchy(section);
			}

			await Task.Yield();
		}
	}
}
