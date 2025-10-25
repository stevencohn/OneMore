//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Hosted control to be used in the SearchResults MoreListView.
	/// </summary>
	/// <remarks>
	/// ListView group headings cannot be fully controlled via WndProc or Paint overrides.
	/// So this class implements an approximation of a group header
	/// </remarks>
	internal class SearchGroupControl : UserControl, IChameleon, IThemedControl
	{
		private readonly PictureBox picture;
		private readonly MoreLabel label;


		public SearchGroupControl(string htmlColor, string text)
		{
			picture = new PictureBox
			{
				Image = Resx.SectionMask.MapColor(Color.Black, ColorHelper.FromHtml(htmlColor)),
				Dock = DockStyle.Left,
				Padding = new(5, 4, 0, 2),
				Width = 30
			};

			label = new MoreLabel
			{
				Dock = DockStyle.Fill,
				Text = text,
				Font = new("Segoe UI", 8.5f, FontStyle.Bold, GraphicsUnit.Point),
				Padding = new(0),
				Margin = new(4, 6, 0, 0)
			};

			BackColor = Color.Transparent;
			Width = 1000;
			Height = 24;
			Margin = new Padding(0, 6, 0, 2);

			BackColorChanged += new EventHandler((s, e) =>
			{
				picture.BackColor = ((Control)s).BackColor;
				label.BackColor = ((Control)s).BackColor;
			});

			Controls.Add(label);
			Controls.Add(picture);
		}


		protected override void Dispose(bool disposing)
		{
			picture?.Dispose();
			base.Dispose(disposing);
		}


		public override string Text { get => label.Text; set => label.Text = value; }


		public string ThemedBack { get; set; }


		public string ThemedFore { get; set; }


		public void ApplyBackground(Color color)
		{
			BackColor = color;
			label.BackColor = color;
		}


		public void ApplyTheme(ThemeManager manager)
		{
			//BackColor = manager.GetColor("Info");
			picture.BackColor = BackColor;
			label.BackColor = BackColor;

			var color = manager.GetColor("InfoText");
			label.ForeColor = color;
		}


		public void ResetBackground()
		{
			BackColor = Color.Transparent;
			label.BackColor = Color.Transparent;
		}


		public void SetTitle(string title)
		{
			label.Text = title;
		}
	}
}
