//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// Disposables taken care of in OnClosed.
	/// </remarks>

	internal partial class EmojiDialog : Form, IOneMoreWindow
	{
		private (string code, string html, string name, Image image)[] icons =
		{
			("🚩", "&#128681;", "Important", Resx.Flag),			// 🚩 1F6A9 Triangular Flag On Post (Important)
			("📐", "&#128208;", "Architecture", Resx.Architecture),	// 📐 1F4D0 Triangular Ruler (Architecture)
			("●", "&#9679;", "Bullet", Resx.Bullet),					// ●  25CF Black Circle (Bullet)
			("📆", "&#‭128198‬;", "Calendar", Resx.Calendar),			// 📆 1F4C6 Tear-Off Calendar (Calendar)
			("⭕", "&#2B55;", "Circle", Resx.Circle),				// ⭕  2B55 Heavy Large Circle (Circle)
			("❌", "&#‭11093‬;", "Cross Mark", Resx.Cross),			// ❌ 274C Cross Mark
			("🚴", "&#‭128644‬;", "Cycling", Resx.Cyclist),			// 🚴 1F684 Bicyclist (Cycling)
			("✉", "&#‭9993‬;", "Email", Resx.Envelope),				// ✉  2709 Envelope (Email)
			("🔨", "&#‭128296‬;", "Hammer", Resx.Hammer),				// 🔨 1F528 Hammer
			("📷", "&#‭128247‬;", "Images", Resx.Camera),				// 📷 1F4F7 Camera (Images)
			("📓", "&#128211;", "Journal", Resx.Journal),			// 📓 1F4D3 Notebook (Journal)
			("✏", "&#‭9999‬;", "Pencil", Resx.Pencil),				// ✏  270F Pencil
			("📌", "&#‭128204‬;", "Pushpin", Resx.Pushpin),			// 📌 1F4CC Pushpin
			("🙂", "&#‭128578‬;", "Smiley", Resx.Smiley),				// 🙂 1F642 Smiley
			("⭐", "&#‭11088‬;", "Star", Resx.Star),					// ⭐  2B50 White Medium Star (Star)
			("∑", "&#‭8721‬;", "Summary", Resx.Summary),				// ∑  2211 N-Ary Summation (Summary)
			("⌚", "&#‭8986‬;", "Watch", Resx.Watch)					// ⌚ 231A Watch
		};


		public EmojiDialog ()
		{
			InitializeComponent();

			iconBox.ItemHeight = 26;
			iconBox.Items.AddRange(icons.Select(e => e.name).ToArray());
			iconBox.SelectedIndex = 0;
		}

		protected override void OnShown (EventArgs e)
		{
			UIHelper.SetForegroundWindow(this);
		}

		protected override void OnClosed (EventArgs e)
		{
			foreach (var icon in icons)
			{
				icon.image?.Dispose();
			}
		}

		private void okButton_Click (object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		private void cancelButton_Click (object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}


		public string[] GetSelectedCodes ()
		{
			var list = new List<int>();
			foreach (int index in iconBox.SelectedIndices)
			{
				list.Add(index);
			}

			return list.Select(e => icons[e].code).ToArray();
			//return list.Select(e => Get32BitUnicode(icons[e].html)).ToArray();
		}


		//private string Get32BitUnicode (string desc)
		//{
		//	// Exception calling "ConvertFromUtf32" with "1" argument(s): "A valid UTF32 value
		//	// is between 0x000000 and 0x10ffff, inclusive, and should  not include surrogate
		//	// codepoint values (0x00d800 ~ 0x00dfff).

		//	if (int.TryParse(desc.Substring(2, desc.Length - 3), out var num))
		//	{
		//		if ((num > 0) && (num <= 0xFFFF))
		//		{
		//			return ((char)num).ToString();
		//		}
		//		else if (num <= 0x10FFFF)
		//		{
		//			return Char.ConvertFromUtf32(num);
		//		}
		//	}

		//	return "*";
		//}


		private void iconBox_MeasureItem (object sender, MeasureItemEventArgs e)
		{
			e.ItemHeight = 26;
		}

		private void iconBox_DrawItem (object sender, DrawItemEventArgs e)
		{
			var icon = icons[e.Index];

			if (DialogResult == DialogResult.OK)
			{
				// double-click exit
				return;
			}

			Brush brush;

			if ((e.State & (DrawItemState.Selected | DrawItemState.Focus)) > 0)
			{
				e.Graphics.FillRectangle(SystemBrushes.HotTrack, e.Bounds);
				brush = SystemBrushes.HighlightText;
			}
			else
			{
				e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
				brush = SystemBrushes.ControlText;
			}

			try
			{
				e.Graphics.DrawImageUnscaled(icon.image, e.Bounds.Location.X + 1, e.Bounds.Location.Y + 1);
				e.Graphics.DrawString(icon.name, DefaultFont, brush, e.Bounds.Location.X + 28, e.Bounds.Location.Y + 1);
			}
			catch
			{
				// closing?
			}
		}

		private void iconBox_DoubleClick (object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
