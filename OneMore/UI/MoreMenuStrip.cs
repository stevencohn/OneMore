//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using River.OneMoreAddIn.Commands;
	using System;
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// A MenuStrip that provides a themed view
	/// </summary>

	internal class MoreMenuStrip : MenuStrip, ILoadControl, IDpiAware
	{
		public MoreMenuStrip()
		{
			Renderer = new MoreMenuRenderer(new ThemedColorTable());
		}


		void ILoadControl.OnLoad()
		{
			AutoSize = false;
			ApplyScale();
		}


		void IDpiAware.OnDpiChanged(int oldDpi, int newDpi)
		{
			ApplyScale();
		}


		private void ApplyScale()
		{
			ImageScalingSize = new Size(
				Scaling.GetSystemMetric(Native.SM_CXSMICON, Scaling.GetDpi(this)),
				Scaling.GetSystemMetric(Native.SM_CYSMICON, Scaling.GetDpi(this)));
		}
	}


	/// <summary>
	/// A ContextMenuStrip that provides a themed view, matching MoreMenuStrip styling
	/// </summary>
	internal class MoreContextMenuStrip : ContextMenuStrip
	{
		private Font scaledFont;


		public MoreContextMenuStrip()
		{
			Renderer = new MoreMenuRenderer(new ThemedColorTable());
		}


		/// <summary>
		/// Scales the font and image size to the DPI of the window hosting the owner. A popup
		/// menu is not a child of its owner so otherwise uses the stale system DPI.
		/// </summary>
		/// <param name="owner">The control that will show this menu</param>
		public void ApplyScale(Control owner)
		{
			var dpi = Scaling.GetDpi(owner);

			var old = scaledFont;
			var baseFont = old is null ? Font : SystemFonts.MenuFont;

			scaledFont = new Font(
				baseFont.FontFamily,
				baseFont.SizeInPoints * Scaling.GetAmbientFontScale(dpi),
				baseFont.Style, GraphicsUnit.Point, baseFont.GdiCharSet);

			Font = scaledFont;
			old?.Dispose();

			ImageScalingSize = new Size(
				Scaling.GetSystemMetric(Native.SM_CXSMICON, dpi),
				Scaling.GetSystemMetric(Native.SM_CYSMICON, dpi));
		}


		/// <summary>
		/// Scales this menu for the owner's DPI and then shows it.
		/// </summary>
		public void ShowScaled(Control owner, Point location)
		{
			ApplyScale(owner);
			Show(owner, location);
		}


		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				scaledFont?.Dispose();
				scaledFont = null;
			}

			base.Dispose(disposing);
		}
	}


	internal class MoreMenuRenderer : ToolStripProfessionalRenderer
	{
		private readonly Color menuBarColor;
		private readonly Color menuHighlightColor;
		private readonly Color menuTextColor;

		public MoreMenuRenderer(ProfessionalColorTable colorTable)
			: base(colorTable)
		{
			var manager = ThemeManager.Instance;
			menuBarColor = manager.GetColor("MenuBar");
			menuHighlightColor = manager.GetColor("MenuHighlight");
			menuTextColor = manager.GetColor("MenuText");
		}

		protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
		{
			e.ToolStrip.BackColor = menuBarColor;
			base.OnRenderToolStripBackground(e);
		}

		protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
		{
			if (e.Item.Selected && e.Item.Enabled)
			{
				using var brush = new SolidBrush(menuHighlightColor);
				e.Graphics.FillRectangle(brush, new Rectangle(Point.Empty, e.Item.Size));
			}
			else
			{
				base.OnRenderMenuItemBackground(e);
			}
		}

		protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
		{
			e.Item.BackColor = Color.Green;
			base.OnRenderDropDownButtonBackground(e);
		}

		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
		{
			e.TextColor = menuTextColor;
			base.OnRenderItemText(e);
		}

		protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
		{
			e.ArrowColor = menuTextColor;
			base.OnRenderArrow(e);
		}
	}


	internal class MoreMenuItem : ToolStripMenuItem
	{
		private readonly ThemeManager manager = ThemeManager.Instance;

		public MoreMenuItem() : base() { }

		public MoreMenuItem(string text) : base(text) { }

		public MoreMenuItem(Image image) : base(image) { }

		public MoreMenuItem(string text, Image image) : base(text, image) { }

		public MoreMenuItem(string text, Image image, EventHandler onClick)
			: base(text, image, onClick) { }

		public MoreMenuItem(string text, Image image, ToolStripDropDownItem[] dropDownItems)
			: base(text, image, dropDownItems) { }

		public MoreMenuItem(string text, Image image, EventHandler onClick, Keys shortcutKeys)
			: base(text, image, onClick, shortcutKeys) { }

		public MoreMenuItem(string text, Image image, EventHandler onClick, string name)
			: base(text, image, onClick, name) { }

		public override Image Image
		{
			get => base.Image;

			set
			{
				if (ThemeManager.Instance.DarkMode && value != null)
				{
					var editor = new ImageEditor() { Style = ImageEditor.Stylization.Invert };
					base.Image = editor.Apply(value);
				}
				else
				{
					base.Image = value;
				}
			}
		}

		protected override void OnCheckStateChanged(EventArgs e)
		{
			base.OnCheckStateChanged(e);
			if (this.Checked)
			{
				BackColor = manager.GetColor("MenuHighlight");
			}
			else
			{
				BackColor = manager.GetColor("MenuBar");
			}
		}
	}


	internal class MoreSplitButton : ToolStripSplitButton
	{
		public MoreSplitButton() : base() { }

		public MoreSplitButton(string text) : base(text) { }

		public MoreSplitButton(Image image) : base(image) { }

		public MoreSplitButton(string text, Image image) : base(text, image) { }

		public MoreSplitButton(string text, Image image, EventHandler onClick)
			: base(text, image, onClick) { }

		public MoreSplitButton(string text, Image image, ToolStripDropDownItem[] dropDownItems)
			: base(text, image, dropDownItems) { }

		public MoreSplitButton(string text, Image image, EventHandler onClick, string name)
			: base(text, image, onClick, name) { }

		public override Image Image
		{
			get => base.Image;

			set
			{
				if (ThemeManager.Instance.DarkMode && value != null)
				{
					var editor = new ImageEditor() { Style = ImageEditor.Stylization.Invert };
					base.Image = editor.Apply(value);
				}
				else
				{
					base.Image = value;
				}
			}
		}
	}
}
