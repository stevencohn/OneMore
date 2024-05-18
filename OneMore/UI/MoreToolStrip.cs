//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using River.OneMoreAddIn.Commands;
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;


	/// <summary>
	/// A ToolStrip that provides proper DPI scaling by applying a scaling factor
	/// to ImageScalingSize so button icons are drawn to correct size for the device.
	/// </summary>

	internal class MoreToolStrip : ToolStrip, ILoadControl
	{
		public MoreToolStrip()
		{
			GripStyle = ToolStripGripStyle.Hidden;
			Renderer = new CustomRenderer(new ThemedColorTable());
		}

		void ILoadControl.OnLoad()
		{
			AutoSize = false;

			(float scaleX, float scaleY) = UI.Scaling.GetScalingFactors();
			ImageScalingSize = new Size((int)(16 * scaleX), (int)(16 * scaleY));
			Height = (int)(24 * scaleY);

			Width = Items.OfType<ToolStripItem>().Sum(i => i.Width) + 16;
		}


		protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
		{
			base.ScaleControl(factor, specified);

			var items = Items.GetEnumerator();
			while (items.MoveNext())
			{
				if (items.Current is ToolStripControlHost host)
				{
					if (host.Placement == ToolStripItemPlacement.Overflow)
						host.Control.Scale(factor);
				}
			}
		}


		private sealed class CustomRenderer : ToolStripProfessionalRenderer
		{
			private readonly Color textColor;

			public CustomRenderer(ProfessionalColorTable colorTable)
				: base(colorTable)
			{
				var manager = ThemeManager.Instance;
				textColor = manager.GetColor("MenuText");
			}

			protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
			{
				e.ToolStrip.BackColor = ThemeManager.Instance.GetColor("MenuBar");
				base.OnRenderToolStripBackground(e);
			}

			protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
			{
				e.TextColor = textColor;
				base.OnRenderItemText(e);
			}


			protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
			{
				e.ArrowColor = textColor;
				base.OnRenderArrow(e);
			}
		}
	}


	internal class MoreToolStripButton : ToolStripButton
	{
		public MoreToolStripButton() : base() { }
		public MoreToolStripButton(Image image) : base(image) { }
		public MoreToolStripButton(string text) : base(text) { }
		public MoreToolStripButton(string text, Image image) : base(text, image) { }
		public MoreToolStripButton(string text, Image image, EventHandler onClick)
			: base(text, image, onClick) { }
		public MoreToolStripButton(string text, Image image, EventHandler onClick, string name)
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
