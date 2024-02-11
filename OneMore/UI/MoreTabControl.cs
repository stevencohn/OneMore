//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System.ComponentModel;
	using System.Drawing;
	using System.Security.Permissions;
	using System.Windows.Forms;


	internal class MoreTabControl : TabControl
	{
		private const int Airgap = 4;
		private const int DefaultTabHeight = 25;
		private const int IndicatorSize = 3;
		private readonly ThemeManager manager;


		public MoreTabControl()
		{
			SetStyle(
				ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
				ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.CacheText,
				true);

			Alignment = TabAlignment.Top;
			Margin = new Padding(0);
			Padding = new Point(0, 0);

			manager = ThemeManager.Instance;
		}


		[Category("More"),
		 Description("Active tab indicator"), DefaultValue(typeof(string), "MenuHighlight")]
		public string ActiveIndicator { get; set; } = "MenuHighlight";


		[Category("More"),
		 Description("Active tab background"), DefaultValue(typeof(string), "ControlLight")]
		public string ActiveTabBack { get; set; } = "ControlLight";


		[Category("More"),
		 Description("Active tab foreground"), DefaultValue(typeof(string), "ControlText")]
		public string ActiveTabFore { get; set; } = "ControlText";


		[Category("More"),
		 Description("Background color, tab well"), DefaultValue(typeof(string), "Control")]
		public string Background { get; set; } = "Control";


		[Category("More"),
		 Description("Border color"), DefaultValue(typeof(string), "ActiveBorder")]
		public string Border { get; set; } = "ActiveBorder";


		[Category("More"),
		 Description("Inactive tab background"), DefaultValue(typeof(string), "Control")]
		public string InactiveTabBack { get; set; } = "Control";


		[Category("More"),
		 Description("Inactive tab foreground"), DefaultValue(typeof(string), "ControlText")]
		public string InactiveTabFore { get; set; } = "ControlText";



		protected override CreateParams CreateParams
		{
			[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED to stop flicker
				return cp;
			}
		}

		protected override void OnValidating(CancelEventArgs e)
		{
			foreach (TabPage page in TabPages)
			{
				if (page.Tag == null)
				{
					page.Tag = page.Text;
					page.Text = $"\u00A0{page.Text}\u00A0\u00A0";
				}
			}

			base.OnValidating(e);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			// background will become the Tabs Well bg as well as the internal bg
			var background = manager.GetColor(Background);
			e.Graphics.Clear(background);

			using var activeBack = new SolidBrush(manager.GetColor(ActiveTabBack));
			using var activeFore = new SolidBrush(manager.GetColor(ActiveTabFore));
			using var inactiveBack = new SolidBrush(manager.GetColor(InactiveTabBack));
			using var inactiveFore = new SolidBrush(manager.GetColor(InactiveTabFore));
			using var indicator = new SolidBrush(manager.GetColor(ActiveIndicator));

			var format = new StringFormat
			{
				Trimming = StringTrimming.EllipsisCharacter,
				FormatFlags = StringFormatFlags.LineLimit | StringFormatFlags.NoWrap
			};

			var top = 0;

			for (var i = 0; i < TabPages.Count; i++)
			{
				var page = TabPages[i];
				var bounds = GetTabRect(i);
				bounds.Inflate(0, 2);

				Brush fore;

				if (top == 0)
				{
					top = bounds.Height;
				}

				// background and indicator...

				if (i == SelectedIndex)
				{
					e.Graphics.FillRectangle(activeBack, bounds);
					e.Graphics.FillRectangle(indicator,
						bounds.X, bounds.Y, bounds.Width - Airgap, IndicatorSize);

					fore = activeFore;
				}
				else
				{
					e.Graphics.FillRectangle(inactiveBack,
						bounds.X, bounds.Y + 1,
						bounds.Width - Airgap, bounds.Height - IndicatorSize);

					fore = inactiveFore;
				}

				// text and image...

				var size = e.Graphics.MeasureString(page.Text, Font).ToSize();
				if (ImageList != null &&
					page.ImageIndex >= 0 && page.ImageIndex < ImageList.Images.Count)
				{
					e.Graphics.DrawImage(ImageList.Images[page.ImageIndex],
						bounds.X + 2, (bounds.Height - ImageList.ImageSize.Height) / 2);

					e.Graphics.DrawString(page.Text, Font, fore,
						(bounds.X + 2 + ImageList.ImageSize.Width) + 3,
						(bounds.Height - size.Height) - 3,
						format);
				}
				else
				{
					e.Graphics.DrawString(page.Text, Font, fore,
						bounds.X + 2 + (bounds.Width - size.Width) / 2f,
						(bounds.Height - size.Height) - 3,
						format);
				}
			}

			top = top == 0 ? DefaultTabHeight - 2 : top - 2;
			using var pen = new Pen(manager.GetColor(Border), 2);

			// outline: left, bottom, right
			e.Graphics.DrawLine(pen, 0, top, 0, e.ClipRectangle.Bottom);
			e.Graphics.DrawLine(pen, 0, e.ClipRectangle.Bottom, e.ClipRectangle.Right, e.ClipRectangle.Bottom);
			e.Graphics.DrawLine(pen, e.ClipRectangle.Right, e.ClipRectangle.Bottom, e.ClipRectangle.Right, top);

			e.Graphics.FillRectangle(indicator, 0, top, e.ClipRectangle.Width, IndicatorSize);
		}
	}
}