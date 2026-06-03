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

	internal class MoreMenuStrip : MenuStrip, ILoadControl
	{
		public MoreMenuStrip()
		{
			Renderer = new MoreMenuRenderer(new ThemedColorTable());
		}


		void ILoadControl.OnLoad()
		{
			AutoSize = false;
			ImageScalingSize = SystemInformation.SmallIconSize;
		}
	}


	/// <summary>
	/// A ContextMenuStrip that provides a themed view, matching MoreMenuStrip styling
	/// </summary>
	internal class MoreContextMenuStrip : ContextMenuStrip
	{
		public MoreContextMenuStrip()
		{
			Renderer = new MoreMenuRenderer(new ThemedColorTable());
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
		private readonly ThemeManager manager;

		public MoreMenuItem() : base()
		{
			manager = ThemeManager.Instance;
		}

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
