//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003 // Type is not CLS-compliant
#pragma warning disable IDE1006 // Words must begin with upper case

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Windows.Forms;


	public partial class TagPickerDialog : Form
	{
		private sealed class Zone
		{
			public int Symbol;
			public Rectangle Bounds;
		}

		private readonly Graphics graphics;
		private readonly float xScalingFactor;
		private readonly float yScalingFactor;

		private readonly List<Zone> zones;
		private Zone preset;
		private Zone active;


		public TagPickerDialog(int x, int y)
		{
			InitializeComponent();

			graphics = Graphics.FromImage(pictureBox.Image);

			zones = new List<Zone>
			{
				#region Zones
				// column 1
				new() { Symbol = 3, Bounds = new Rectangle { X=4, Y=4, Width=20, Height=20 } },		// BlueCheckbox
				new() { Symbol = 6, Bounds = new Rectangle { X=4, Y=26, Width=20, Height=20 } },	// BlueStarCheckbox
				new() { Symbol = 9, Bounds = new Rectangle { X=4, Y=47, Width=20, Height=20 } },	// BlueExclamationCheckbox
				new() { Symbol = 12, Bounds = new Rectangle { X=4, Y=70, Width=20, Height=20 } },	// BlueArrowCheckbox
				new() { Symbol = 28, Bounds = new Rectangle { X=4, Y=92, Width=20, Height=20 } },	// BlueCheckbox1
				new() { Symbol = 30, Bounds = new Rectangle { X=4, Y=114, Width=20, Height=20 } },	// BlueCheckbox2
				new() { Symbol = 32, Bounds = new Rectangle { X=4, Y=136, Width=20, Height=20 } },	// BlueCheckbox3
				new() { Symbol = 94, Bounds = new Rectangle { X=4, Y=158, Width=20, Height=20 } },	// TodoBluePerson
				new() { Symbol = 97, Bounds = new Rectangle { X=4, Y=180, Width=20, Height=20 } },	// TudoBlueFlag

				// column 2
				new() { Symbol = 2, Bounds = new Rectangle { X=26, Y=4, Width=20, Height=20 } },	// YellowCheckbox
				new() { Symbol = 5, Bounds = new Rectangle { X=26, Y=26, Width=20, Height=20 } },	// YellowStarCheckbox
				new() { Symbol = 8, Bounds = new Rectangle { X=26, Y=49, Width=20, Height=20 } },	// YellowExclamationCheckbox
				new() { Symbol = 11, Bounds = new Rectangle { X=26, Y=71, Width=20, Height=20 } },	// YellowArrowCheckbox
				new() { Symbol = 69, Bounds = new Rectangle { X=26, Y=92, Width=20, Height=20 } },	// YellowCheckbox1
				new() { Symbol = 71, Bounds = new Rectangle { X=26, Y=114, Width=20, Height=20 } },	// YellowCheckbox2
				new() { Symbol = 73, Bounds = new Rectangle { X=26, Y=136, Width=20, Height=20 } },	// YellowCheckbox3
				new() { Symbol = 95, Bounds = new Rectangle { X=26, Y=158, Width=20, Height=20 } },	// TodoYellowPerson
				new() { Symbol = 98, Bounds = new Rectangle { X=26, Y=180, Width=20, Height=20 } },	// TodoYellowFlag

				// column 3
				new() { Symbol = 1, Bounds = new Rectangle { X=48, Y=4, Width=20, Height=20 } },	// GreenCheckbox
				new() { Symbol = 4, Bounds = new Rectangle { X=48, Y=26, Width=20, Height=20 } },	// GreenStarCheckbox
				new() { Symbol = 7, Bounds = new Rectangle { X=48, Y=49, Width=20, Height=20 } },	// GreenExclamationCheckbox
				new() { Symbol = 10, Bounds = new Rectangle { X=48, Y=71, Width=20, Height=20 } },	// GreenArrowCheckbox
				new() { Symbol = 48, Bounds = new Rectangle { X=48, Y=92, Width=20, Height=20 } },	// GreenCheckbox1
				new() { Symbol = 50, Bounds = new Rectangle { X=48, Y=114, Width=20, Height=20 } },	// GreenCheckbox2
				new() { Symbol = 52, Bounds = new Rectangle { X=48, Y=136, Width=20, Height=20 } },	// GreenCheckbox3
				new() { Symbol = 96, Bounds = new Rectangle { X=48, Y=158, Width=20, Height=20 } },	// TodoGreenPerson
				new() { Symbol = 99, Bounds = new Rectangle { X=48, Y=180, Width=20, Height=20 } },	// TodoGreenFlag

				// column 4
				new() { Symbol = 40, Bounds = new Rectangle { X=89, Y=4, Width=20, Height=20 } },	// BlueStar
				new() { Symbol = 43, Bounds = new Rectangle { X=89, Y=26, Width=20, Height=20 } },	// BlueTriangle
				new() { Symbol = 36, Bounds = new Rectangle { X=89, Y=49, Width=20, Height=20 } },	// BlueCircle
				new() { Symbol = 39, Bounds = new Rectangle { X=89, Y=71, Width=20, Height=20 } },	// BlueTarget
				new() { Symbol = 42, Bounds = new Rectangle { X=89, Y=92, Width=20, Height=20 } },	// BlueSolidTarget
				new() { Symbol = 29, Bounds = new Rectangle { X=89, Y=114, Width=20, Height=20 } },	// BlueCircle1
				new() { Symbol = 31, Bounds = new Rectangle { X=89, Y=136, Width=20, Height=20 } },	// BlueCircle2
				new() { Symbol = 33, Bounds = new Rectangle { X=89, Y=158, Width=20, Height=20 } },	// BlueCircle3
				new() { Symbol = 35, Bounds = new Rectangle { X=89, Y=180, Width=20, Height=20 } },	// BlueCheckmark
				new() { Symbol = 44, Bounds = new Rectangle { X=89, Y=202, Width=20, Height=20 } },	// BlueUmbrella

				// column 5
				new() { Symbol = 13, Bounds = new Rectangle { X=111, Y=4, Width=20, Height=20 } },	// YellowStar
				new() { Symbol = 84, Bounds = new Rectangle { X=111, Y=26, Width=20, Height=20 } },	// YellowTriangle
				new() { Symbol = 77, Bounds = new Rectangle { X=111, Y=49, Width=20, Height=20 } },	// YellowCircle
				new() { Symbol = 83, Bounds = new Rectangle { X=111, Y=71, Width=20, Height=20 } },	// YellowTarget
				new() { Symbol = 81, Bounds = new Rectangle { X=111, Y=92, Width=20, Height=20 } },	// YellowSolidTarget
				new() { Symbol = 70, Bounds = new Rectangle { X=111, Y=114, Width=20, Height=20 } },// YellowCircle1
				new() { Symbol = 72, Bounds = new Rectangle { X=111, Y=136, Width=20, Height=20 } },// YellowCircle2
				new() { Symbol = 74, Bounds = new Rectangle { X=111, Y=158, Width=20, Height=20 } },// YellowCircle3
				new() { Symbol = 76, Bounds = new Rectangle { X=111, Y=180, Width=20, Height=20 } },// YellowCheckmark
				new() { Symbol = 85, Bounds = new Rectangle { X=111, Y=202, Width=20, Height=20 } },// YellowUmbrella

				// column 6
				new() { Symbol = 61, Bounds = new Rectangle { X=133, Y=4, Width=20, Height=20 } },	// GreenStar
				new() { Symbol = 64, Bounds = new Rectangle { X=133, Y=26, Width=20, Height=20 } },	// GreenTriangle
				new() { Symbol = 56, Bounds = new Rectangle { X=133, Y=49, Width=20, Height=20 } },	// GreenCircle
				new() { Symbol = 63, Bounds = new Rectangle { X=133, Y=71, Width=20, Height=20 } },	// GreenTarget
				new() { Symbol = 60, Bounds = new Rectangle { X=133, Y=92, Width=20, Height=20 } },	// GreenSolidTarget
				new() { Symbol = 49, Bounds = new Rectangle { X=133, Y=114, Width=20, Height=20 } },// GreenCircle1
				new() { Symbol = 51, Bounds = new Rectangle { X=133, Y=136, Width=20, Height=20 } },// GreenCircle2
				new() { Symbol = 53, Bounds = new Rectangle { X=133, Y=158, Width=20, Height=20 } },// GreenCircle3
				new() { Symbol = 55, Bounds = new Rectangle { X=133, Y=180, Width=20, Height=20 } },// GreenCheckmark
				new() { Symbol = 65, Bounds = new Rectangle { X=133, Y=202, Width=20, Height=20 } },// GreenUmbrella

				// column 7
				new() { Symbol = 41, Bounds = new Rectangle { X=174, Y=4, Width=20, Height=20 } },	// BlueSun
				new() { Symbol = 34, Bounds = new Rectangle { X=174, Y=26, Width=20, Height=20 } },	// Blue8Star
				new() { Symbol = 46, Bounds = new Rectangle { X=174, Y=49, Width=20, Height=20 } },	// BlueXDots
				new() { Symbol = 47, Bounds = new Rectangle { X=174, Y=71, Width=20, Height=20 } },	// BlueX
				new() { Symbol = 38, Bounds = new Rectangle { X=174, Y=92, Width=20, Height=20 } },	// BlueLeftArrow
				new() { Symbol = 16, Bounds = new Rectangle { X=174, Y=114, Width=20, Height=20 } },// BlueRightArrow
				new() { Symbol = 37, Bounds = new Rectangle { X=174, Y=136, Width=20, Height=20 } },// BlueDownArrow
				new() { Symbol = 45, Bounds = new Rectangle { X=174, Y=158, Width=20, Height=20 } },// BlueUpArrow
				new() { Symbol = 100, Bounds = new Rectangle { X=174, Y=180, Width=20, Height=20 } },// RedSquare
				new() { Symbol = 103, Bounds = new Rectangle { X=174, Y=202, Width=20, Height=20 } },// GreenSquare

				// column 8
				new() { Symbol = 82, Bounds = new Rectangle { X=196, Y=4, Width=20, Height=20 } },	// YellowSun
				new() { Symbol = 75, Bounds = new Rectangle { X=196, Y=26, Width=20, Height=20 } },	// Yellow8Star
				new() { Symbol = 87, Bounds = new Rectangle { X=196, Y=49, Width=20, Height=20 } },	// YellowXDots
				new() { Symbol = 88, Bounds = new Rectangle { X=196, Y=71, Width=20, Height=20 } },	// YellowX
				new() { Symbol = 79, Bounds = new Rectangle { X=196, Y=92, Width=20, Height=20 } },	// YellowLeftArrow
				new() { Symbol = 80, Bounds = new Rectangle { X=196, Y=114, Width=20, Height=20 } },// YellowRightArrow
				new() { Symbol = 78, Bounds = new Rectangle { X=196, Y=136, Width=20, Height=20 } },// YellowDownArrow
				new() { Symbol = 86, Bounds = new Rectangle { X=196, Y=158, Width=20, Height=20 } },// YellowUpArrow
				new() { Symbol = 101, Bounds = new Rectangle { X=196, Y=180, Width=20, Height=20 } },// YellowSquare
				new() { Symbol = 104, Bounds = new Rectangle { X=196, Y=202, Width=20, Height=20 } },// OrangeSquare

				// column 9
				new() { Symbol = 62, Bounds = new Rectangle { X=218, Y=4, Width=20, Height=20 } },	// GreenSun
				new() { Symbol = 54, Bounds = new Rectangle { X=218, Y=26, Width=20, Height=20 } },	// Green8Star
				new() { Symbol = 67, Bounds = new Rectangle { X=218, Y=49, Width=20, Height=20 } },	// GreenXDots
				new() { Symbol = 68, Bounds = new Rectangle { X=218, Y=71, Width=20, Height=20 } },	// GreenX
				new() { Symbol = 58, Bounds = new Rectangle { X=218, Y=92, Width=20, Height=20 } },	// GreenLeftArrow
				new() { Symbol = 59, Bounds = new Rectangle { X=218, Y=114, Width=20, Height=20 } },// GreenRightArrow
				new() { Symbol = 57, Bounds = new Rectangle { X=218, Y=136, Width=20, Height=20 } },// GreenDownArrow
				new() { Symbol = 66, Bounds = new Rectangle { X=218, Y=158, Width=20, Height=20 } },// GreenUpArrow
				new() { Symbol = 102, Bounds = new Rectangle { X=218, Y=180, Width=20, Height=20 } },// BlueSquare
				new() { Symbol = 105, Bounds = new Rectangle { X=218, Y=202, Width=20, Height=20 } },// PinkSquare

				// column 10
				new() { Symbol = 106, Bounds = new Rectangle { X=259, Y=4, Width=20, Height=20 } },	// Email
				new() { Symbol = 109, Bounds = new Rectangle { X=259, Y=26, Width=20, Height=20 } },// Phone
				new() { Symbol = 14, Bounds = new Rectangle { X=259, Y=49, Width=20, Height=20 } },	// Followup
				new() { Symbol = 21, Bounds = new Rectangle { X=259, Y=71, Width=20, Height=20 } },	// Lightbulb
				new() { Symbol = 112, Bounds = new Rectangle { X=259, Y=92, Width=20, Height=20 } },// Paperclip
				new() { Symbol = 113, Bounds = new Rectangle { X=259, Y=114, Width=20, Height=20 } },// Frowning
				new() { Symbol = 114, Bounds = new Rectangle { X=259, Y=136, Width=20, Height=20 } },// IMContact
				new() { Symbol = 117, Bounds = new Rectangle { X=259, Y=158, Width=20, Height=20 } },// ReminderBell
				new() { Symbol = 120, Bounds = new Rectangle { X=259, Y=180, Width=20, Height=20 } },// Date
				new() { Symbol = 121, Bounds = new Rectangle { X=259, Y=202, Width=20, Height=20 } },// MusicNote

				// column 11
				new() { Symbol = 107, Bounds = new Rectangle { X=281, Y=4, Width=20, Height=20 } },	// EnvelopeClosed
				new() { Symbol = 18, Bounds = new Rectangle { X=281, Y=26, Width=20, Height=20 } },	// Telephone
				new() { Symbol = 15, Bounds = new Rectangle { X=281, Y=49, Width=20, Height=20 } },	// Question
				new() { Symbol = 24, Bounds = new Rectangle { X=281, Y=71, Width=20, Height=20 } },	// Comment
				new() { Symbol = 22, Bounds = new Rectangle { X=281, Y=92, Width=20, Height=20 } },	// Pushpin
				new() { Symbol = 25, Bounds = new Rectangle { X=281, Y=114, Width=20, Height=20 } },// Smiley
				new() { Symbol = 115, Bounds = new Rectangle { X=281, Y=136, Width=20, Height=20 } },// Person
				new() { Symbol = 118, Bounds = new Rectangle { X=281, Y=158, Width=20, Height=20 } },// Contact
				new() { Symbol = 19, Bounds = new Rectangle { X=281, Y=180, Width=20, Height=20 } },// Calendar
				new() { Symbol = 122, Bounds = new Rectangle { X=281, Y=202, Width=20, Height=20 } },// MovieClip

				// column 12
				new() { Symbol = 108, Bounds = new Rectangle { X=303, Y=4, Width=20, Height=20 } },	// EnvelopeOpened
				new() { Symbol = 110, Bounds = new Rectangle { X=303, Y=26, Width=20, Height=20 } },// TelephoneClock
				new() { Symbol = 17, Bounds = new Rectangle { X=303, Y=49, Width=20, Height=20 } },	// HighPriority
				new() { Symbol = 111, Bounds = new Rectangle { X=303, Y=71, Width=20, Height=20 } },// QuestionBaloon
				new() { Symbol = 23, Bounds = new Rectangle { X=303, Y=92, Width=20, Height=20 } },	// Home
				new() { Symbol = 26, Bounds = new Rectangle { X=303, Y=114, Width=20, Height=20 } },// AwardRibbon
				new() { Symbol = 116, Bounds = new Rectangle { X=303, Y=136, Width=20, Height=20 } },// TwoPeople
				new() { Symbol = 119, Bounds = new Rectangle { X=303, Y=158, Width=20, Height=20 } },// FlowersBoquet
				new() { Symbol = 20, Bounds = new Rectangle { X=303, Y=180, Width=20, Height=20 } },// Clock
				new() { Symbol = 123, Bounds = new Rectangle { X=303, Y=202, Width=20, Height=20 } },// QuoteMark

				// column 13
				new() { Symbol = 124, Bounds = new Rectangle { X=343, Y=4, Width=20, Height=20 } },	// Globe
				new() { Symbol = 127, Bounds = new Rectangle { X=343, Y=26, Width=20, Height=20 } },// Plane
				new() { Symbol = 129, Bounds = new Rectangle { X=343, Y=49, Width=20, Height=20 } },// Binoculars
				new() { Symbol = 132, Bounds = new Rectangle { X=343, Y=71, Width=20, Height=20 } },// BookOpen
				new() { Symbol = 135, Bounds = new Rectangle { X=343, Y=92, Width=20, Height=20 } },// Research
				new() { Symbol = 138, Bounds = new Rectangle { X=343, Y=114, Width=20, Height=20 } },// CoinWindow
				new() { Symbol = 141, Bounds = new Rectangle { X=343, Y=136, Width=20, Height=20 } },// Cloud

				// column 14
				new() { Symbol = 125, Bounds = new Rectangle { X=366, Y=4, Width=20, Height=20 } },	// Link
				new() { Symbol = 128, Bounds = new Rectangle { X=366, Y=26, Width=20, Height=20 } },// Car
				new() { Symbol = 130, Bounds = new Rectangle { X=366, Y=49, Width=20, Height=20 } },// Presentation
				new() { Symbol = 133, Bounds = new Rectangle { X=366, Y=71, Width=20, Height=20 } },// Notebook
				new() { Symbol = 136, Bounds = new Rectangle { X=366, Y=92, Width=20, Height=20 } },// Marker
				new() { Symbol = 139, Bounds = new Rectangle { X=366, Y=114, Width=20, Height=20 } },// ScheduledTask
				new() { Symbol = 142, Bounds = new Rectangle { X=366, Y=136, Width=20, Height=20 } },// Heart

				// column 15
				new() { Symbol = 126, Bounds = new Rectangle { X=388, Y=4, Width=20, Height=20 } },	// Laptop
				new() { Symbol = 27, Bounds = new Rectangle { X=388, Y=26, Width=20, Height=20 } },	// Key
				new() { Symbol = 131, Bounds = new Rectangle { X=388, Y=49, Width=20, Height=20 } },// Padlock
				new() { Symbol = 134, Bounds = new Rectangle { X=388, Y=71, Width=20, Height=20 } },// Paper
				new() { Symbol = 137, Bounds = new Rectangle { X=388, Y=92, Width=20, Height=20 } },// Dollar
				new() { Symbol = 140, Bounds = new Rectangle { X=388, Y=114, Width=20, Height=20 } },// Lightning
				new() { Symbol = 143, Bounds = new Rectangle { X=388, Y=136, Width=20, Height=20 } }// Flower
				#endregion Zones
			};

			(xScalingFactor, yScalingFactor) = UI.Scaling.GetScalingFactors();

			Left = x;
			Top = y + (Height / 2);

			preset = null;
		}


		/// <summary>
		/// 
		/// </summary>
		public int Symbol => active != null ? active.Symbol : 0;


		/// <summary>
		/// Return a Bitmap of the chosen symbol
		/// </summary>
		public Bitmap GetGlyph()
		{
			if (active == null)
				return null;

			// copy just the selected glyph from tagmap
			var bitmap = new Bitmap(pictureBox.Image);
			var glyph = bitmap.Clone(active.Bounds, bitmap.PixelFormat);
			return glyph;
		}


		/// <summary>
		/// Used by OutlineDialog to restore saved user settings and prepopulate
		/// the glyph on its tag symbol button
		/// </summary>
		/// <param name="symbol"></param>
		/// <returns></returns>
		public Bitmap GetGlyph(int symbol)
		{
			active = zones.Find(z => z.Symbol == symbol);
			if (active != null)
			{
				return GetGlyph();
			}

			return null;
		}


		public void Select(int symbol)
		{
			var zone = zones.Find(z => z.Symbol == symbol);
			if (zone != null)
			{
				active = preset = zone;
				using var pen = new Pen(Color.Magenta, 2f);
				graphics.DrawRectangle(pen, active.Bounds);
			}
		}


		private void pictureBox_MouseMove(object sender, MouseEventArgs e)
		{
			var mouseX = xScalingFactor.EstEquals(0f) ? e.X : (int)Math.Round(e.X / xScalingFactor);
			var mouseY = yScalingFactor.EstEquals(0f) ? e.Y : (int)Math.Round(e.Y / yScalingFactor);

			var zone = zones.Find(z =>
				mouseX >= z.Bounds.Left && mouseX <= z.Bounds.Right &&
				mouseY >= z.Bounds.Top && mouseY <= z.Bounds.Bottom);

			if (zone != null)
			{
				if ((active != null) && (zone != active))
				{
					if (active == preset)
					{
						using var pen = new Pen(Color.Magenta, 2f);
						graphics.DrawRectangle(pen, preset.Bounds);
					}
					else
					{
						// erase previous selection box
						using var pen = new Pen(Color.White, 2f);
						graphics.DrawRectangle(pen, active.Bounds);
					}
				}

				active = zone;

				// draw new selection box
				using (var pen = new Pen(Color.Magenta, 2f) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot })
				{
					graphics.DrawRectangle(pen, zone.Bounds);
				}

				pictureBox.Refresh();
			}
		}


		private void pictureBox_MouseUp(object sender, MouseEventArgs e)
		{
			var mouseX = xScalingFactor.EstEquals(0f) ? e.X : (int)Math.Round(e.X / xScalingFactor);
			var mouseY = yScalingFactor.EstEquals(0f) ? e.Y : (int)Math.Round(e.Y / yScalingFactor);

			var zone = zones.Find(z =>
				mouseX >= z.Bounds.Left && mouseX <= z.Bounds.Right &&
				mouseY >= z.Bounds.Top && mouseY <= z.Bounds.Bottom);

			if (zone != null)
			{
				active = zone;

				// erase selection box before exiting (and maybe cloning glyph)
				using (var pen = new Pen(Color.White, 2f))
				{
					graphics.DrawRectangle(pen, active.Bounds);

					if (preset != null)
					{
						graphics.DrawRectangle(pen, preset.Bounds);
					}
				}

				graphics.Dispose();

				DialogResult = DialogResult.OK;
				Close();
			}
		}


		private void TagPickerDialog_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				active = null;

				DialogResult = DialogResult.Cancel;
				Close();
			}
		}
	}
}
