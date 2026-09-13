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
				// Never explicitly assign BackColor at design time: the VS designer would bake
				// the (always-light) snapshot into the consumer's InitializeComponent the next
				// time its Designer.cs is opened and saved.
				if (!ThemeManager.IsDesignTime)
				{
					e.ToolStrip.BackColor = ThemeManager.Instance.GetColor("MenuBar");
				}
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


			protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
			{
				if (e.Item.Enabled && IsHighlightValid(e.Item))
				{
					base.OnRenderButtonBackground(e);
				}
			}


			protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
			{
				if (e.Item.Enabled && IsHighlightValid(e.Item))
				{
					base.OnRenderMenuItemBackground(e);
				}
			}


			/// <summary>
			/// A disabled ToolStripItem's Selected flag can get stuck true: ToolStrip
			/// only clears hover state by firing MouseLeave on the item, and that firing
			/// is a no-op while the item is disabled. So an item hovered while disabled
			/// and then re-enabled after the mouse has moved away can still report
			/// Selected == true even though nothing is really hovering it. Confirm the
			/// mouse is really over the item (or the toolstrip has keyboard focus, in
			/// which case Selected reflects real keyboard navigation) before trusting it.
			/// </summary>
			private static bool IsHighlightValid(ToolStripItem item)
			{
				if (!item.Selected)
				{
					return true;
				}

				return item.Owner is not null &&
					(item.Owner.ContainsFocus ||
					item.Bounds.Contains(item.Owner.PointToClient(Cursor.Position)));
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
