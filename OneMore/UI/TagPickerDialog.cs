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
	using System.Linq;
	using System.Windows.Forms;


	public partial class TagPickerDialog : Form
	{
		private class Zone
		{
			public int Symbol;
			public Rectangle Bounds;
		}

		private readonly Graphics graphics;
		private readonly float xScalingFactor;
		private readonly float yScalingFactor;

		private readonly List<Zone> zones;
		private Zone active;


		public TagPickerDialog(int x, int y)
		{
			InitializeComponent();

			graphics = Graphics.FromImage(pictureBox.Image);

			zones = new List<Zone>
			{
				#region Zones
				// column 1
				new Zone { Symbol = 3, Bounds = new Rectangle { X=4, Y=4, Width=20, Height=20 } },		// BlueCheckbox
				new Zone { Symbol = 6, Bounds = new Rectangle { X=4, Y=26, Width=20, Height=20 } },		// BlueStarCheckbox
				new Zone { Symbol = 9, Bounds = new Rectangle { X=4, Y=47, Width=20, Height=20 } },		// BlueExclamationCheckbox
				new Zone { Symbol = 12, Bounds = new Rectangle { X=4, Y=70, Width=20, Height=20 } },	// BlueArrowCheckbox
				new Zone { Symbol = 28, Bounds = new Rectangle { X=4, Y=92, Width=20, Height=20 } },	// BlueCheckbox1
				new Zone { Symbol = 30, Bounds = new Rectangle { X=4, Y=114, Width=20, Height=20 } },	// BlueCheckbox2
				new Zone { Symbol = 32, Bounds = new Rectangle { X=4, Y=136, Width=20, Height=20 } },	// BlueCheckbox3
				new Zone { Symbol = 94, Bounds = new Rectangle { X=4, Y=158, Width=20, Height=20 } },	// TodoBluePerson
				new Zone { Symbol = 97, Bounds = new Rectangle { X=4, Y=180, Width=20, Height=20 } },	// TudoBlueFlag

				// column 2
				new Zone { Symbol = 2, Bounds = new Rectangle { X=26, Y=4, Width=20, Height=20 } },		// YellowCheckbox
				new Zone { Symbol = 5, Bounds = new Rectangle { X=26, Y=26, Width=20, Height=20 } },	// YellowStarCheckbox
				new Zone { Symbol = 8, Bounds = new Rectangle { X=26, Y=49, Width=20, Height=20 } },	// YellowExclamationCheckbox
				new Zone { Symbol = 11, Bounds = new Rectangle { X=26, Y=71, Width=20, Height=20 } },	// YellowArrowCheckbox
				new Zone { Symbol = 69, Bounds = new Rectangle { X=26, Y=92, Width=20, Height=20 } },	// YellowCheckbox1
				new Zone { Symbol = 71, Bounds = new Rectangle { X=26, Y=114, Width=20, Height=20 } },	// YellowCheckbox2
				new Zone { Symbol = 73, Bounds = new Rectangle { X=26, Y=136, Width=20, Height=20 } },	// YellowCheckbox3
				new Zone { Symbol = 95, Bounds = new Rectangle { X=26, Y=158, Width=20, Height=20 } },	// TodoYellowPerson
				new Zone { Symbol = 98, Bounds = new Rectangle { X=26, Y=180, Width=20, Height=20 } },	// TodoYellowFlag

				// column 3
				new Zone { Symbol = 1, Bounds = new Rectangle { X=48, Y=4, Width=20, Height=20 } },		// GreenCheckbox
				new Zone { Symbol = 4, Bounds = new Rectangle { X=48, Y=26, Width=20, Height=20 } },	// GreenStarCheckbox
				new Zone { Symbol = 7, Bounds = new Rectangle { X=48, Y=49, Width=20, Height=20 } },	// GreenExclamationCheckbox
				new Zone { Symbol = 10, Bounds = new Rectangle { X=48, Y=71, Width=20, Height=20 } },	// GreenArrowCheckbox
				new Zone { Symbol = 48, Bounds = new Rectangle { X=48, Y=92, Width=20, Height=20 } },	// GreenCheckbox1
				new Zone { Symbol = 50, Bounds = new Rectangle { X=48, Y=114, Width=20, Height=20 } },	// GreenCheckbox2
				new Zone { Symbol = 52, Bounds = new Rectangle { X=48, Y=136, Width=20, Height=20 } },	// GreenCheckbox3
				new Zone { Symbol = 96, Bounds = new Rectangle { X=48, Y=158, Width=20, Height=20 } },	// TodoGreenPerson
				new Zone { Symbol = 99, Bounds = new Rectangle { X=48, Y=180, Width=20, Height=20 } },	// TodoGreenFlag

				// column 4
				new Zone { Symbol = 40, Bounds = new Rectangle { X=89, Y=4, Width=20, Height=20 } },	// BlueStar
				new Zone { Symbol = 43, Bounds = new Rectangle { X=89, Y=26, Width=20, Height=20 } },	// BlueTriangle
				new Zone { Symbol = 36, Bounds = new Rectangle { X=89, Y=49, Width=20, Height=20 } },	// BlueCircle
				new Zone { Symbol = 39, Bounds = new Rectangle { X=89, Y=71, Width=20, Height=20 } },	// BlueTarget
				new Zone { Symbol = 42, Bounds = new Rectangle { X=89, Y=92, Width=20, Height=20 } },	// BlueSolidTarget
				new Zone { Symbol = 29, Bounds = new Rectangle { X=89, Y=114, Width=20, Height=20 } },	// BlueCircle1
				new Zone { Symbol = 31, Bounds = new Rectangle { X=89, Y=136, Width=20, Height=20 } },	// BlueCircle2
				new Zone { Symbol = 33, Bounds = new Rectangle { X=89, Y=158, Width=20, Height=20 } },	// BlueCircle3
				new Zone { Symbol = 35, Bounds = new Rectangle { X=89, Y=180, Width=20, Height=20 } },	// BlueCheckmark
				new Zone { Symbol = 44, Bounds = new Rectangle { X=89, Y=202, Width=20, Height=20 } },	// BlueUmbrella

				// column 5
				new Zone { Symbol = 13, Bounds = new Rectangle { X=111, Y=4, Width=20, Height=20 } },	// YellowStar
				new Zone { Symbol = 84, Bounds = new Rectangle { X=111, Y=26, Width=20, Height=20 } },	// YellowTriangle
				new Zone { Symbol = 77, Bounds = new Rectangle { X=111, Y=49, Width=20, Height=20 } },	// YellowCircle
				new Zone { Symbol = 83, Bounds = new Rectangle { X=111, Y=71, Width=20, Height=20 } },	// YellowTarget
				new Zone { Symbol = 81, Bounds = new Rectangle { X=111, Y=92, Width=20, Height=20 } },	// YellowSolidTarget
				new Zone { Symbol = 70, Bounds = new Rectangle { X=111, Y=114, Width=20, Height=20 } },	// YellowCircle1
				new Zone { Symbol = 72, Bounds = new Rectangle { X=111, Y=136, Width=20, Height=20 } },	// YellowCircle2
				new Zone { Symbol = 74, Bounds = new Rectangle { X=111, Y=158, Width=20, Height=20 } },	// YellowCircle3
				new Zone { Symbol = 76, Bounds = new Rectangle { X=111, Y=180, Width=20, Height=20 } },	// YellowCheckmark
				new Zone { Symbol = 85, Bounds = new Rectangle { X=111, Y=202, Width=20, Height=20 } },	// YellowUmbrella

				// column 6
				new Zone { Symbol = 61, Bounds = new Rectangle { X=133, Y=4, Width=20, Height=20 } },	// GreenStar
				new Zone { Symbol = 64, Bounds = new Rectangle { X=133, Y=26, Width=20, Height=20 } },	// GreenTriangle
				new Zone { Symbol = 56, Bounds = new Rectangle { X=133, Y=49, Width=20, Height=20 } },	// GreenCircle
				new Zone { Symbol = 63, Bounds = new Rectangle { X=133, Y=71, Width=20, Height=20 } },	// GreenTarget
				new Zone { Symbol = 60, Bounds = new Rectangle { X=133, Y=92, Width=20, Height=20 } },	// GreenSolidTarget
				new Zone { Symbol = 49, Bounds = new Rectangle { X=133, Y=114, Width=20, Height=20 } },	// GreenCircle1
				new Zone { Symbol = 51, Bounds = new Rectangle { X=133, Y=136, Width=20, Height=20 } },	// GreenCircle2
				new Zone { Symbol = 53, Bounds = new Rectangle { X=133, Y=158, Width=20, Height=20 } },	// GreenCircle3
				new Zone { Symbol = 55, Bounds = new Rectangle { X=133, Y=180, Width=20, Height=20 } },	// GreenCheckmark
				new Zone { Symbol = 65, Bounds = new Rectangle { X=133, Y=202, Width=20, Height=20 } },	// GreenUmbrella

				// column 7
				new Zone { Symbol = 41, Bounds = new Rectangle { X=174, Y=4, Width=20, Height=20 } },	// BlueSun
				new Zone { Symbol = 34, Bounds = new Rectangle { X=174, Y=26, Width=20, Height=20 } },	// Blue8Star
				new Zone { Symbol = 46, Bounds = new Rectangle { X=174, Y=49, Width=20, Height=20 } },	// BlueXDots
				new Zone { Symbol = 47, Bounds = new Rectangle { X=174, Y=71, Width=20, Height=20 } },	// BlueX
				new Zone { Symbol = 38, Bounds = new Rectangle { X=174, Y=92, Width=20, Height=20 } },	// BlueLeftArrow
				new Zone { Symbol = 16, Bounds = new Rectangle { X=174, Y=114, Width=20, Height=20 } },	// BlueRightArrow
				new Zone { Symbol = 37, Bounds = new Rectangle { X=174, Y=136, Width=20, Height=20 } },	// BlueDownArrow
				new Zone { Symbol = 45, Bounds = new Rectangle { X=174, Y=158, Width=20, Height=20 } },	// BlueUpArrow
				new Zone { Symbol = 100, Bounds = new Rectangle { X=174, Y=180, Width=20, Height=20 } },// RedSquare
				new Zone { Symbol = 103, Bounds = new Rectangle { X=174, Y=202, Width=20, Height=20 } },// GreenSquare

				// column 8
				new Zone { Symbol = 82, Bounds = new Rectangle { X=196, Y=4, Width=20, Height=20 } },	// YellowSun
				new Zone { Symbol = 75, Bounds = new Rectangle { X=196, Y=26, Width=20, Height=20 } },	// Yellow8Star
				new Zone { Symbol = 87, Bounds = new Rectangle { X=196, Y=49, Width=20, Height=20 } },	// YellowXDots
				new Zone { Symbol = 88, Bounds = new Rectangle { X=196, Y=71, Width=20, Height=20 } },	// YellowX
				new Zone { Symbol = 79, Bounds = new Rectangle { X=196, Y=92, Width=20, Height=20 } },	// YellowLeftArrow
				new Zone { Symbol = 80, Bounds = new Rectangle { X=196, Y=114, Width=20, Height=20 } },	// YellowRightArrow
				new Zone { Symbol = 78, Bounds = new Rectangle { X=196, Y=136, Width=20, Height=20 } },	// YellowDownArrow
				new Zone { Symbol = 86, Bounds = new Rectangle { X=196, Y=158, Width=20, Height=20 } },	// YellowUpArrow
				new Zone { Symbol = 101, Bounds = new Rectangle { X=196, Y=180, Width=20, Height=20 } },// YellowSquare
				new Zone { Symbol = 104, Bounds = new Rectangle { X=196, Y=202, Width=20, Height=20 } },// OrangeSquare

				// column 9
				new Zone { Symbol = 62, Bounds = new Rectangle { X=218, Y=4, Width=20, Height=20 } },	// GreenSun
				new Zone { Symbol = 54, Bounds = new Rectangle { X=218, Y=26, Width=20, Height=20 } },	// Green8Star
				new Zone { Symbol = 67, Bounds = new Rectangle { X=218, Y=49, Width=20, Height=20 } },	// GreenXDots
				new Zone { Symbol = 68, Bounds = new Rectangle { X=218, Y=71, Width=20, Height=20 } },	// GreenX
				new Zone { Symbol = 58, Bounds = new Rectangle { X=218, Y=92, Width=20, Height=20 } },	// GreenLeftArrow
				new Zone { Symbol = 59, Bounds = new Rectangle { X=218, Y=114, Width=20, Height=20 } },	// GreenRightArrow
				new Zone { Symbol = 57, Bounds = new Rectangle { X=218, Y=136, Width=20, Height=20 } },	// GreenDownArrow
				new Zone { Symbol = 66, Bounds = new Rectangle { X=218, Y=158, Width=20, Height=20 } },	// GreenUpArrow
				new Zone { Symbol = 102, Bounds = new Rectangle { X=218, Y=180, Width=20, Height=20 } },// BlueSquare
				new Zone { Symbol = 105, Bounds = new Rectangle { X=218, Y=202, Width=20, Height=20 } },// PinkSquare

				// column 10
				new Zone { Symbol = 106, Bounds = new Rectangle { X=259, Y=4, Width=20, Height=20 } },	// Email
				new Zone { Symbol = 109, Bounds = new Rectangle { X=259, Y=26, Width=20, Height=20 } },	// Phone
				new Zone { Symbol = 14, Bounds = new Rectangle { X=259, Y=49, Width=20, Height=20 } },	// Followup
				new Zone { Symbol = 21, Bounds = new Rectangle { X=259, Y=71, Width=20, Height=20 } },	// Lightbulb
				new Zone { Symbol = 112, Bounds = new Rectangle { X=259, Y=92, Width=20, Height=20 } },	// Paperclip
				new Zone { Symbol = 113, Bounds = new Rectangle { X=259, Y=114, Width=20, Height=20 } },// Frowning
				new Zone { Symbol = 114, Bounds = new Rectangle { X=259, Y=136, Width=20, Height=20 } },// IMContact
				new Zone { Symbol = 117, Bounds = new Rectangle { X=259, Y=158, Width=20, Height=20 } },// ReminderBell
				new Zone { Symbol = 120, Bounds = new Rectangle { X=259, Y=180, Width=20, Height=20 } },// Date
				new Zone { Symbol = 121, Bounds = new Rectangle { X=259, Y=202, Width=20, Height=20 } },// MusicNote

				// column 11
				new Zone { Symbol = 107, Bounds = new Rectangle { X=281, Y=4, Width=20, Height=20 } },	// EnvelopeClosed
				new Zone { Symbol = 18, Bounds = new Rectangle { X=281, Y=26, Width=20, Height=20 } },	// Telephone
				new Zone { Symbol = 15, Bounds = new Rectangle { X=281, Y=49, Width=20, Height=20 } },	// Question
				new Zone { Symbol = 24, Bounds = new Rectangle { X=281, Y=71, Width=20, Height=20 } },	// Comment
				new Zone { Symbol = 22, Bounds = new Rectangle { X=281, Y=92, Width=20, Height=20 } },	// Pushpin
				new Zone { Symbol = 25, Bounds = new Rectangle { X=281, Y=114, Width=20, Height=20 } },	// Smiley
				new Zone { Symbol = 115, Bounds = new Rectangle { X=281, Y=136, Width=20, Height=20 } },// Person
				new Zone { Symbol = 118, Bounds = new Rectangle { X=281, Y=158, Width=20, Height=20 } },// Contact
				new Zone { Symbol = 19, Bounds = new Rectangle { X=281, Y=180, Width=20, Height=20 } },	// Calendar
				new Zone { Symbol = 122, Bounds = new Rectangle { X=281, Y=202, Width=20, Height=20 } },// MovieClip

				// column 12
				new Zone { Symbol = 108, Bounds = new Rectangle { X=303, Y=4, Width=20, Height=20 } },	// EnvelopeOpened
				new Zone { Symbol = 110, Bounds = new Rectangle { X=303, Y=26, Width=20, Height=20 } },	// TelephoneClock
				new Zone { Symbol = 17, Bounds = new Rectangle { X=303, Y=49, Width=20, Height=20 } },	// HighPriority
				new Zone { Symbol = 111, Bounds = new Rectangle { X=303, Y=71, Width=20, Height=20 } },	// QuestionBaloon
				new Zone { Symbol = 23, Bounds = new Rectangle { X=303, Y=92, Width=20, Height=20 } },	// Home
				new Zone { Symbol = 26, Bounds = new Rectangle { X=303, Y=114, Width=20, Height=20 } },	// AwardRibbon
				new Zone { Symbol = 116, Bounds = new Rectangle { X=303, Y=136, Width=20, Height=20 } },// TwoPeople
				new Zone { Symbol = 119, Bounds = new Rectangle { X=303, Y=158, Width=20, Height=20 } },// FlowersBoquet
				new Zone { Symbol = 20, Bounds = new Rectangle { X=303, Y=180, Width=20, Height=20 } },	// Clock
				new Zone { Symbol = 123, Bounds = new Rectangle { X=303, Y=202, Width=20, Height=20 } },// QuoteMark

				// column 13
				new Zone { Symbol = 124, Bounds = new Rectangle { X=343, Y=4, Width=20, Height=20 } },	// Globe
				new Zone { Symbol = 127, Bounds = new Rectangle { X=343, Y=26, Width=20, Height=20 } },	// Plane
				new Zone { Symbol = 129, Bounds = new Rectangle { X=343, Y=49, Width=20, Height=20 } },	// Binoculars
				new Zone { Symbol = 132, Bounds = new Rectangle { X=343, Y=71, Width=20, Height=20 } },	// BookOpen
				new Zone { Symbol = 135, Bounds = new Rectangle { X=343, Y=92, Width=20, Height=20 } },	// Research
				new Zone { Symbol = 138, Bounds = new Rectangle { X=343, Y=114, Width=20, Height=20 } },// CoinWindow
				new Zone { Symbol = 141, Bounds = new Rectangle { X=343, Y=136, Width=20, Height=20 } },// Cloud

				// column 14
				new Zone { Symbol = 125, Bounds = new Rectangle { X=366, Y=4, Width=20, Height=20 } },	// Link
				new Zone { Symbol = 128, Bounds = new Rectangle { X=366, Y=26, Width=20, Height=20 } },	// Car
				new Zone { Symbol = 130, Bounds = new Rectangle { X=366, Y=49, Width=20, Height=20 } },	// Presentation
				new Zone { Symbol = 133, Bounds = new Rectangle { X=366, Y=71, Width=20, Height=20 } },	// Notebook
				new Zone { Symbol = 136, Bounds = new Rectangle { X=366, Y=92, Width=20, Height=20 } },	// Marker
				new Zone { Symbol = 139, Bounds = new Rectangle { X=366, Y=114, Width=20, Height=20 } },// ScheduledTask
				new Zone { Symbol = 142, Bounds = new Rectangle { X=366, Y=136, Width=20, Height=20 } },// Heart

				// column 15
				new Zone { Symbol = 126, Bounds = new Rectangle { X=388, Y=4, Width=20, Height=20 } },	// Laptop
				new Zone { Symbol = 27, Bounds = new Rectangle { X=388, Y=26, Width=20, Height=20 } },	// Key
				new Zone { Symbol = 131, Bounds = new Rectangle { X=388, Y=49, Width=20, Height=20 } },	// Padlock
				new Zone { Symbol = 134, Bounds = new Rectangle { X=388, Y=71, Width=20, Height=20 } },	// Paper
				new Zone { Symbol = 137, Bounds = new Rectangle { X=388, Y=92, Width=20, Height=20 } },	// Dollar
				new Zone { Symbol = 140, Bounds = new Rectangle { X=388, Y=114, Width=20, Height=20 } },// Lightning
				new Zone { Symbol = 143, Bounds = new Rectangle { X=388, Y=136, Width=20, Height=20 } } // Flower
				#endregion Zones
			};

			(xScalingFactor, yScalingFactor) = UIHelper.GetScalingFactors();

			Left = x;
			Top = y + (Height / 2);
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
			active = zones.FirstOrDefault(z => z.Symbol == symbol);
			if (active != null)
			{
				return GetGlyph();
			}

			return null;
		}


		public void Select(int symbol)
		{
			var zone = zones.FirstOrDefault(z => z.Symbol == symbol);
			if (zone != null)
			{
				active = zone;
				using (var pen = new Pen(Color.Magenta, 2f))
				{
					graphics.DrawRectangle(pen, active.Bounds);
				}
			}
		}


		private void pictureBox_MouseMove(object sender, MouseEventArgs e)
		{
			var mouseX = xScalingFactor == 0.0 ? e.X : (int)Math.Round(e.X / xScalingFactor);
			var mouseY = yScalingFactor == 0.0 ? e.Y : (int)Math.Round(e.Y / yScalingFactor);

			var zone = zones.FirstOrDefault(z =>
				mouseX >= z.Bounds.Left && mouseX <= z.Bounds.Right &&
				mouseY >= z.Bounds.Top && mouseY <= z.Bounds.Bottom);

			if (zone != null)
			{
				if ((active != null) && (zone != active))
				{
					// erase previous selection box
					using (var pen = new Pen(Color.White, 2f))
					{
						graphics.DrawRectangle(pen, active.Bounds);
					}
				}

				active = zone;

				// draw new selection box
				using (var pen = new Pen(Color.Magenta, 2f))
				{
					graphics.DrawRectangle(pen, zone.Bounds);
				}

				pictureBox.Refresh();
			}
		}


		private void pictureBox_MouseUp(object sender, MouseEventArgs e)
		{
			var mouseX = xScalingFactor == 0.0 ? e.X : (int)Math.Round(e.X / xScalingFactor);
			var mouseY = yScalingFactor == 0.0 ? e.Y : (int)Math.Round(e.Y / yScalingFactor);

			var zone = zones.FirstOrDefault(z =>
				mouseX >= z.Bounds.Left && mouseX <= z.Bounds.Right &&
				mouseY >= z.Bounds.Top && mouseY <= z.Bounds.Bottom);

			if (zone != null)
			{
				active = zone;

				// erase selection box before exiting (and maybe cloning glyph)
				using (var pen = new Pen(Color.White, 2f))
				{
					graphics.DrawRectangle(pen, active.Bounds);
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
