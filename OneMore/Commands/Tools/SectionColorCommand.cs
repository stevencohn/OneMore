//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
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

			await using var one = new OneNote();

			var section = await one.GetSection();
			var sectionColor = section.Attribute("color").Value;
			if (sectionColor == "none")
			{
				// close approximation of none
				sectionColor = "#F0F0F0";
			}

			using var picker = new UI.MoreColorPicker(
				location.X, location.Y, Resx.SectionColor_Title)
			{
				Color = ColorTranslator.FromHtml(sectionColor),
				AutomaticVisible = false
			};

			// use the elevator to force the picker to top-most first time used
			// otherwise, it will be hidden by the OneMore window
			using var elevator = new UI.WindowElevator(picker);
			var result = elevator.ShowDialog(owner);
			if (result == DialogResult.OK)
			{
				section.SetAttributeValue("color", picker.Color.ToRGBHtml());
				one.UpdateHierarchy(section);
			}

			await Task.Yield();
		}
	}
}
