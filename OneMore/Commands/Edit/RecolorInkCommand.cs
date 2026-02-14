//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Windows.Ink;
    using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Select a new color and apply it to all selected ink drawings on a OneNote page.
	/// </summary>
	/// <remarks>
	/// This command is typically used to recolor ink strokes that are currently selected in
	/// the OneNote page. It prompts the user with a color selection dialog and updates the color
	/// of all matching ink strokes if a new color is chosen. The command does not perform any
	/// action if no ink drawings are selected or if the user cancels the color selection.
	/// </remarks>
	internal class RecolorInkCommand : Command
    {
		private Page page;
		private XNamespace ns;
		private System.Windows.Media.Color badColor;


		public RecolorInkCommand()
        {
		}


        public override async Task Execute(params object[] args)
        {
			await using var one = new OneNote(out page, out ns, OneNote.PageDetail.All);

			var ink = page.Root.Descendants(ns + "InkDrawing")
                .FirstOrDefault(e => e.Attributes("selected").Any(a => a.Value == "all"));

			if (ink is null)
			{
				UI.MoreMessageBox.ShowError(owner, Resx.RecolorInkCommand_noselection);
                return;
			}

            var strokes = DeserializeInkFromBase64(ink.Element(ns + "Data").Value);
            if (strokes.Count == 0)
            {
				UI.MoreMessageBox.ShowError(owner, Resx.RecolorInkCommand_noselection);
				return;
            }

			badColor = strokes[0].DrawingAttributes.Color;

			var color = GetNewColor(Color.FromArgb(badColor.A, badColor.R, badColor.G, badColor.B));
			if (color == Color.Empty)
			{
				return;
			}

			var modified = Recolor(color);
			if (modified)
			{
				await one.Update(page);
			}
		}


		private Color GetNewColor(Color sampleColor)
		{
			if (!Native.GetCursorPos(out var location))
			{
				location = new Native.Point() { X = 100, Y = 100 };
			}

			// adjust for offset of context menu item that invokes this command
			location.Y -= SystemInformation.MenuHeight * 4;
			location.X -= 30;

			using var dialog = new UI.MoreColorDialog(
				"Select New Color", location.X, location.Y);

			dialog.Color = sampleColor;
			dialog.FullOpen = true;

			// use the elevator to force ColorDialog to top-most first time used
			// otherwise, it will be hidden by the OneMore window
			using var elevator = new UI.WindowElevator(dialog);
			var result = elevator.ShowDialog(owner);
			return result == DialogResult.OK ? dialog.Color : Color.Empty;
		}


		private bool Recolor(Color color)
		{
			var goodColor = new System.Windows.Media.Color
			{
				A = color.A,
				R = color.R,
				G = color.G,
				B = color.B
			};

			var modified = false;

			var drawings = page.Root.Descendants(ns + "InkDrawing");
			foreach (var drawing in drawings)
			{
				var data = drawing.Element(ns + "Data").Value;
				var strokes = DeserializeInkFromBase64(data);
				var mod = false;

				foreach (var stroke in strokes
					.Where(s => s.DrawingAttributes.Color == badColor))
				{
					stroke.DrawingAttributes.Color = goodColor;
					mod = true;
				}

				if (mod)
				{
					drawing.Element(ns + "Data").Value = SerializeToBase64(strokes);
					modified = true;
				}
			}

			return modified;
		}


		private static StrokeCollection DeserializeInkFromBase64(string data)
		{
			if (string.IsNullOrWhiteSpace(data))
            {
                return new StrokeCollection();
            }

			// OneNote stores ink as base64-encoded ISF (Ink Serialized Format) 
			// StrokeCollection can directly read ISF 

			var bytes = Convert.FromBase64String(data);
            using var ms = new MemoryStream(bytes);
            return new StrokeCollection(ms);
        }


		private static string SerializeToBase64(StrokeCollection strokes)
		{
			// save as ISF (Ink Serialized Format) 
			using var ms = new MemoryStream();
            strokes.Save(ms);
            var bytes = ms.ToArray();
            return Convert.ToBase64String(bytes);
        }
	}
}
