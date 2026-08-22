//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Drawing.Text;
	using System.Runtime.InteropServices;
	using System.Windows.Forms;


	/// <summary>
	/// A themed ComboBox that integrates with ThemeManager and supports an optional image
	/// prefix on each item. Items may be plain strings or <see cref="ComboItem"/> instances.
	/// </summary>
	internal class MoreComboBox : ComboBox, ILoadControl
	{
		private const int ImagePad = 4;

		protected StringFormat stringFormat;


		/// <summary>
		/// An item carrying optional image prefix alongside display text.
		/// </summary>
		public sealed class ComboItem
		{
			public ComboItem(string text, Image image = null)
			{
				Text = text ?? string.Empty;
				Image = image;
			}

			public string Text { get; }

			public Image Image { get; }

			// Required: ComboBox calls ToString() to render the edit-area text
			public override string ToString() => Text;
		}


		// Lifecycle

		public MoreComboBox()
		{
			DrawMode = DrawMode.OwnerDrawFixed;
			MakeStringFormat();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				stringFormat?.Dispose();
				stringFormat = null;
			}
			base.Dispose(disposing);
		}


		// designer-hidden property overrides

		[Browsable(false),
		 DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
		 EditorBrowsable(EditorBrowsableState.Never)]
		public new DrawMode DrawMode
		{
			get => base.DrawMode;
			set => base.DrawMode = value;
		}


		// theming

		public string ThemedBack { get; set; }

		public string ThemedFore { get; set; }

		void ILoadControl.OnLoad()
		{
			var manager = ThemeManager.Instance;
			BackColor = manager.GetColor("Window", ThemedBack);
			ForeColor = Enabled
				? manager.GetColor("WindowText", ThemedFore)
				: manager.GetColor("GrayText");

			if (manager.DarkMode)
			{
				FlatStyle = FlatStyle.Popup;
			}
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			((ILoadControl)this).OnLoad();
		}


		// owner-draw - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private int GetReservedImageWidth()
		{
			int max = 0;
			foreach (var raw in Items)
			{
				if (raw is ComboItem ci && ci.Image != null)
				{
					int w = ci.Image.Width + ImagePad * 2;
					if (w > max) max = w;
				}
			}
			return max;
		}

		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			base.OnDrawItem(e);

			if (e.Index < 0 || e.Index >= Items.Count)
				return;

			var manager = ThemeManager.Instance;
			bool isEdit = (e.State & DrawItemState.ComboBoxEdit) != 0;
			bool isSel  = !isEdit && (e.State & DrawItemState.Selected) != 0;

			Color back, fore;
			if (!Enabled)
			{
				back = manager.GetColor("InactiveWindow");
				fore = manager.GetColor("GrayText");
			}
			else if (isSel)
			{
				back = manager.GetColor("Highlight");
				fore = manager.GetColor("HighlightText");
			}
			else
			{
				back = manager.GetColor("Window", ThemedBack);
				fore = manager.GetColor("WindowText", ThemedFore);
			}

			using var backBrush = new SolidBrush(back);
			e.Graphics.FillRectangle(backBrush, e.Bounds);

			int reservedWidth = GetReservedImageWidth();
			var raw = Items[e.Index];
			int textX = e.Bounds.X + 2;

			if (reservedWidth > 0)
			{
				textX = e.Bounds.X + reservedWidth;
				if (raw is ComboItem ci && ci.Image != null)
				{
					int imgY = e.Bounds.Y + (e.Bounds.Height - ci.Image.Height) / 2;
					e.Graphics.DrawImage(ci.Image, e.Bounds.X + ImagePad, imgY);
				}
			}

			var textBounds = new Rectangle(
				textX, e.Bounds.Y, e.Bounds.Right - textX - 2, e.Bounds.Height);

			using var foreBrush = new SolidBrush(fore);
			e.Graphics.DrawString(
				// use GetItemText here to pick up DisplayMember when there is a DataSource
				GetItemText(raw),
				GetItemFont(raw), foreBrush, textBounds, stringFormat);

			if (!isEdit && (e.State & DrawItemState.Focus) != 0)
				e.DrawFocusRectangle();
		}


		/// <summary>
		/// Gets the font to use when drawing the given item's text. Overridable so subclasses
		/// like FontComboBox can render each item in its own font family.
		/// </summary>
		protected virtual Font GetItemFont(object item) => Font;


		protected virtual void MakeStringFormat()
		{
			stringFormat?.Dispose();
			stringFormat = new StringFormat(StringFormatFlags.NoWrap)
			{
				Trimming      = StringTrimming.EllipsisCharacter,
				HotkeyPrefix  = HotkeyPrefix.None,
				Alignment     = StringAlignment.Near,
				LineAlignment = StringAlignment.Center
			};
		}


		// dark-mode dropdown button theming - - - - - - - - - - - - - - - - - - - - - - - - - - -

		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);

			if (m.Msg != Native.WM_PAINT || !ThemeManager.Instance.DarkMode)
				return;

			var info = new Native.COMBOBOXINFO { cbSize = Marshal.SizeOf<Native.COMBOBOXINFO>() };
			if (!Native.GetComboBoxInfo(Handle, ref info))
				return;

			var btnRect = Rectangle.FromLTRB(
				info.rcButton.Left, info.rcButton.Top, info.rcButton.Right, info.rcButton.Bottom);

			if (btnRect.IsEmpty)
				return;

			var manager = ThemeManager.Instance;
			bool pressed = info.stateButton == 2;

			using var g = Graphics.FromHwnd(Handle);

			var backColor = pressed ? manager.ButtonPressBorder : manager.ButtonBack;
			using var backBrush = new SolidBrush(backColor);
			g.FillRectangle(backBrush, btnRect);

			using var borderPen = new Pen(manager.ButtonBorder);
			g.DrawLine(borderPen, btnRect.Left, btnRect.Top, btnRect.Left, btnRect.Bottom - 1);

			var arrowColor = Enabled ? ForeColor : manager.GetColor("GrayText");
			using var arrowBrush = new SolidBrush(arrowColor);
			const int aw = 8, ah = 4;
			int ax = btnRect.Left + (btnRect.Width - aw) / 2;
			int ay = btnRect.Top  + (btnRect.Height - ah) / 2;
			g.FillPolygon(arrowBrush, new[]
			{
				new Point(ax,          ay),
				new Point(ax + aw,     ay),
				new Point(ax + aw / 2, ay + ah)
			});
		}
	}
}
