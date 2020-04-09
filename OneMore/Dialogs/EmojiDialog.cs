//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable IDE0042 // variable can be deconstructed

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
		private readonly (string code, string html, string name, Image image)[] icons =
		{
			("🚩", "&#128681;", "Important", Resx.Flag),			// 🚩 1F6A9 Triangular Flag On Post
			("📐", "&#128208;", "Architecture", Resx.Architecture),	// 📐 1F4D0 Triangular Ruler
			("🚗", "&#1F697;", "Automobile", Resx.Automobile),		// 🚗 1F697 Automobile
			("●", "&#9679;", "Bullet", Resx.Bullet),					// ● 25CF Black Circle
			("📆", "&#‭128198‬;", "Calendar", Resx.Calendar),			// 📆 1F4C6 Tear-Off Calendar
			("⭕", "&#2B55;", "Circle", Resx.Circle),				// ⭕ 2B55 Heavy Large Circle
			("❌", "&#‭11093‬;", "Cross Mark", Resx.Cross),			// ❌ 274C Cross Mark
			("🚴", "&#‭128644‬;", "Cycling", Resx.Cyclist),			// 🚴 1F684 Bicyclist
			("✉", "&#‭9993‬;", "Email", Resx.Envelope),				// ✉ 2709 Envelope
			("👪", "&#1F46A;", "Family", Resx.Family),				// 👪 1F46A Family
			("💲", "&#1F4B2;", "Financial", Resx.Financial),			// 💲 1F4B2 Heavy Dollar Sign
			("🔨", "&#‭128296‬;", "Hammer", Resx.Hammer),				// 🔨 1F528 Hammer
			("📷", "&#‭128247‬;", "Images", Resx.Camera),				// 📷 1F4F7 Camera
			("📓", "&#128211;", "Journal", Resx.Journal),			// 📓 1F4D3 Notebook
			("📝", "&#1F4DD;", "Memo", Resx.Memo),					// 📝 1F4DD Memo
			("🔑", "&#1F511;", "Passwords", Resx.Passwords),		// 🔑 1F511 Key
			("✏", "&#‭9999‬;", "Pencil", Resx.Pencil),				// ✏ 270F Pencil
			("📌", "&#‭128204‬;", "Pushpin", Resx.Pushpin),			// 📌 1F4CC Pushpin
			("⚡", "&#26A1;", "Shazam!", Resx.Shazam),				// ⚡ 26A1 Lightning
			("🙂", "&#‭128578‬;", "Smiley", Resx.Smiley),				// 🙂 1F642 Smiley
			("💾", "&#1F4BE;", "Software", Resx.Software),			// 💾 1F4BE Floppy Disk
			("⚾", "&#26BE;", "Sports", Resx.Sports),				// ⚾ 26BE Baseball
			("⭐", "&#‭11088‬;", "Star", Resx.Star),					// ⭐ 2B50 White Medium Star
			("∑", "&#‭8721‬;", "Summary", Resx.Summary),				// ∑ 2211 N-Ary Summation
			("☑", "&#2611;", "Tasks", Resx.Tasks),					// ☑ 2611 Ballot Box
			("🛩", "&#1F6E9;", "Travel", Resx.Travel),				// Airplane
			("⌚", "&#‭8986‬;", "Watch", Resx.Watch)					// ⌚ 231A Watch
		};


		public EmojiDialog()
		{
			InitializeComponent();

			iconBox.ItemHeight = 22;
			iconBox.Items.AddRange(icons.Select(e => e.name).ToArray());
			iconBox.SelectedIndex = 0;
		}

		protected override void OnShown(EventArgs e)
		{
			Location = new System.Drawing.Point(Location.X, Location.Y - (Height / 2));
			UIHelper.SetForegroundWindow(this);
		}

		protected override void OnClosed(EventArgs e)
		{
			foreach (var icon in icons)
			{
				icon.image?.Dispose();
			}
		}

		private void OK(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		private void Cancel(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}


		public string[] GetSelectedCodes()
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


		private void MeasureIconItemSIze(object sender, MeasureItemEventArgs e)
		{
			e.ItemHeight = 22;
		}

		private void DrawIconItem(object sender, DrawItemEventArgs e)
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
				e.Graphics.DrawImage(icon.image, e.Bounds.Location.X + 1, e.Bounds.Location.Y + 1);
				e.Graphics.DrawString(icon.name, DefaultFont, brush, e.Bounds.Location.X + 28, e.Bounds.Location.Y + 1);
			}
			catch
			{
				// closing?
			}
		}

		private void DoubleClickItem(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
