﻿//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable IDE0042 // variable can be deconstructed

namespace River.OneMoreAddIn.Commands
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

	internal partial class EmojiDialog : UI.LocalizableForm
	{
		private readonly Emojis emojis;
		private readonly List<int> selections;


		public EmojiDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.EmojiDialog_Text;

				Localize(new string[]
				{
					"introLabel",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			// must be defined before initializing SelectedIndex
			selections = new();

			emojis = new Emojis();
			emojis.LoadImages();

			emojiBox.ItemHeight = 26;
			emojiBox.Items.AddRange(emojis.GetNames());
			emojiBox.SelectedIndex = 0;
		}


		protected override void OnClosed(EventArgs e)
		{
			emojis.Dispose();
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


		/// <summary>
		/// Gets collection of user selected emojis
		/// </summary>
		/// <returns>A collection of IEmoji</returns>
		public IEnumerable<IEmoji> GetEmojis()
		{
			// pre-dispose images so caller doesn't have to
			emojis.Dispose();

			foreach (int index in selections)
			{
				yield return emojis[index];
			}
		}


		private void MeasureIconItemSIze(object sender, MeasureItemEventArgs e)
		{
			e.ItemHeight = 26;
		}


		private void DrawIconItem(object sender, DrawItemEventArgs e)
		{
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
				e.Graphics.DrawImage(emojis[e.Index].Image, new Rectangle
				{
					X = e.Bounds.Location.X + 5,
					Y = e.Bounds.Location.Y + 1,
					Width = e.Bounds.Height - 2,
					Height = e.Bounds.Height - 2
				});

				e.Graphics.DrawString(
					emojis[e.Index].Name, DefaultFont, brush,
					e.Bounds.Location.X + 40, e.Bounds.Location.Y + 1);
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


		private void DoSelectedIndexChanged(object sender, EventArgs e)
		{
			if (emojiBox.SelectedIndex > -1)
			{
				// maintains a list of selection in the order in which they are selected...
				// with one caveat: doesn't know when a user drags across multiple items
				// with the mouse bottom-up

				selections.AddRange(emojiBox.SelectedIndices.OfType<int>().Except(selections));
				selections.RemoveRange(0, selections.Count - emojiBox.SelectedItems.Count);
			}
		}
	}
}
