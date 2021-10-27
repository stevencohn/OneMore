//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable IDE0042 // variable can be deconstructed

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Text;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// Disposables taken care of in OnClosed.
	/// </remarks>

	internal partial class AddTitleIconDialog : UI.LocalizableForm
	{
		private readonly (string code, string html, string name, Image image)[] icons =
		{
			("🚩", "&#128681;", Resx.Emoji_Important, Resx.Flag),				// 🚩 1F6A9 Triangular Flag On Post
			("📐", "&#128208;", Resx.Emoji_Architecture, Resx.Architecture),	// 📐 1F4D0 Triangular Ruler
			("🚗", "&#1F697;", Resx.Emoji_Automobile, Resx.Automobile),			// 🚗 1F697 Automobile
			("●", "&#9679;", Resx.Emoji_Bullet, Resx.Bullet),					// ● 25CF Black Circle
			("📆", "&#‭128198‬;", Resx.Emoji_Calendar, Resx.Calendar),			// 📆 1F4C6 Tear-Off Calendar
			("⭕", "&#2B55;", Resx.Emoji_Circle, Resx.Circle),					// ⭕ 2B55 Heavy Large Circle
			("❌", "&#‭11093‬;", Resx.Emoji_CrossMark, Resx.Cross),				// ❌ 274C Cross Mark
			("🚴", "&#‭128644‬;", Resx.Emoji_Cycling, Resx.Cyclist),				// 🚴 1F684 Bicyclist
			("✉", "&#‭9993‬;", Resx.Emoji_Email, Resx.Envelope),					// ✉ 2709 Envelope
			("👪", "&#1F46A;", Resx.Emoji_Family, Resx.Family),					// 👪 1F46A Family
			("💲", "&#1F4B2;", Resx.Emoji_Financial, Resx.Financial),			// 💲 1F4B2 Heavy Dollar Sign
			("🔨", "&#‭128296‬;", Resx.Emoji_Hammer, Resx.Hammer),				// 🔨 1F528 Hammer
			("📷", "&#‭128247‬;", Resx.Emoji_Images, Resx.Camera),				// 📷 1F4F7 Camera
			("📓", "&#128211;", Resx.Emoji_Journal, Resx.Journal),				// 📓 1F4D3 Notebook
			("📝", "&#1F4DD;", Resx.Emoji_Memo, Resx.Memo),						// 📝 1F4DD Memo
			("🔑", "&#1F511;", Resx.Emoji_Passwords, Resx.Passwords),			// 🔑 1F511 Key
			("✏", "&#‭9999‬;", Resx.Emoji_Pencil, Resx.Pencil),					// ✏ 270F Pencil
			("📌", "&#‭128204‬;", Resx.Emoji_Pushpin, Resx.Pushpin),				// 📌 1F4CC Pushpin
			("⚡", "&#26A1;", Resx.Emoji_Shazam, Resx.Shazam),					// ⚡ 26A1 Lightning
			("🙂", "&#‭128578‬;", Resx.Emoji_Smiley, Resx.Smiley),				// 🙂 1F642 Smiley
			("💾", "&#1F4BE;", Resx.Emoji_Software, Resx.Software),				// 💾 1F4BE Floppy Disk
			("⚾", "&#26BE;", Resx.Emoji_Sports, Resx.Sports),					// ⚾ 26BE Baseball
			("⭐", "&#‭11088‬;", Resx.Emoji_Star, Resx.Star),						// ⭐ 2B50 White Medium Star
			("∑", "&#‭8721‬;", Resx.Emoji_Summary, Resx.Summary),					// ∑ 2211 N-Ary Summation
			("☑", "&#2611;", Resx.Emoji_Tasks, Resx.Tasks),						// ☑ 2611 Ballot Box
			("🛩", "&#1F6E9;", Resx.Emoji_Travel, Resx.Travel),					// Airplane
			("⌚", "&#‭8986‬;", Resx.Emoji_Watch, Resx.Watch)						// ⌚ 231A Watch
		};


		public AddTitleIconDialog()
		{
			InitializeComponent();

			iconBox.ItemHeight = 22;
			iconBox.Items.AddRange(icons.Select(e => e.name).ToArray());
			iconBox.SelectedIndex = 0;

			if (NeedsLocalizing())
			{
				Text = Resx.AddTitleIconDialog_Text;

				Localize(new string[]
				{
					"introLabel",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
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


		public static string RemoveEmojis(string value)
		{
			// single-char emojis
			var singles = new List<char> {
				(char)0x2211, (char)0x231A, (char)0x25CF, (char)0x2611, (char)0x26A1, (char)0x26BE,
				(char)0x2709, (char)0x270F, (char)0x274C, (char)0x2B50, (char)0x2B55,
			};

			// double-char emojis, all have first char 0xD83D
			var doubleMarker = (char)0xD83D;

			var doubles = new List<char> {
				(char)0xDC6A, (char)0xDCB2, (char)0xDCBE, (char)0xDCC6, (char)0xDCCC, (char)0xDCD0,
				(char)0xDCD3, (char)0xDCDD, (char)0xDCF7, (char)0xDD11, (char)0xDD28, (char)0xDE42,
				(char)0xDE97, (char)0xDEA9, (char)0xDEB4, (char)0xDEE9
			};

			var builder = new StringBuilder(value);
			int i;

			foreach (var c in singles)
			{
				if ((i = builder.IndexOf(c)) >= 0)
				{
					builder.Remove(i, 1);
				}
			}

			while (((i = builder.IndexOf(doubleMarker)) >= 0) && (i < builder.Length - 1))
			{
				if (doubles.Contains(builder[i + 1]))
				{
					builder.Remove(i, 2);
				}
			}

			return builder.ToString();
		}
	}
}
