//************************************************************************************************
// Copyright © 2018 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Styles;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class ReorderDialog : UI.MoreForm
	{
		private Image headingGlyph;
		private Image headingGlyphSelected;


		public ReorderDialog(ComboBox.ObjectCollection items)
		{
			InitializeComponent();

			styleList.GetCellImage = GetStyleCellImage;

			foreach (var style in items.Cast<GraphicStyle>())
			{
				styleList.Items.Add(new ListViewItem(style.Name) { Tag = style });
			}

			styleList.SetColumnProportions(1f);

			if (styleList.Items.Count > 0)
			{
				styleList.Items[0].Selected = true;
			}

			if (NeedsLocalizing())
			{
				Text = Resx.ReorderDialog_Text;

				Localize(new string[]
				{
					"okButton=word_OK",
					"cancelButton=word_Cancel",
					"label"
				});
			}
		}


		protected override void OnClosed(System.EventArgs e)
		{
			headingGlyph?.Dispose();
			headingGlyphSelected?.Dispose();
			base.OnClosed(e);
		}


		public GraphicStyle[] GetItems()
		{
			return styleList.Items.Cast<ListViewItem>()
				.Select(i => (GraphicStyle)i.Tag)
				.ToArray();
		}


		private Image GetStyleCellImage(ListViewItem item, int columnIndex)
		{
			if (item.Tag is not GraphicStyle { StyleType: StyleType.Heading })
			{
				return null;
			}

			return item.Selected
				? headingGlyphSelected ??= BuildHeadingGlyph(UI.ThemeManager.Instance.GetColor("HighlightText"))
				: headingGlyph ??= BuildHeadingGlyph(UI.ThemeManager.Instance.GetColor("ControlText"));
		}


		private static Image BuildHeadingGlyph(Color color)
		{
			var bitmap = new Bitmap(16, 16);
			using var g = Graphics.FromImage(bitmap);
			using var font = new Font(DefaultFont.FontFamily, DefaultFont.Size - 2f, FontStyle.Bold | FontStyle.Italic);
			using var brush = new SolidBrush(color);
			g.DrawString("H", font, brush, 0, 0);
			return bitmap;
		}


		private void ChangeSelection(object sender, System.EventArgs e)
		{
			var item = styleList.SelectedItems.Count > 0 ? styleList.SelectedItems[0] : null;
			upButton.Enabled = item != null && item.Index > 0;
			downButton.Enabled = item != null && item.Index < styleList.Items.Count - 1;
		}


		private void MoveUp(object sender, System.EventArgs e)
		{
			Move(-1);
		}


		private void MoveDown(object sender, System.EventArgs e)
		{
			Move(1);
		}


		private void Move(int direction)
		{
			if (styleList.SelectedItems.Count == 0)
			{
				return;
			}

			var item = styleList.SelectedItems[0];
			var newIndex = item.Index + direction;
			if (newIndex < 0 || newIndex >= styleList.Items.Count)
			{
				return;
			}

			styleList.BeginUpdate();
			styleList.Items.RemoveAt(item.Index);
			styleList.Items.Insert(newIndex, item);
			styleList.EndUpdate();

			for (var i = 0; i < styleList.Items.Count; i++)
			{
				((GraphicStyle)styleList.Items[i].Tag).Index = i;
			}

			item.Selected = true;
			item.EnsureVisible();
			ChangeSelection(this, System.EventArgs.Empty);
		}
	}
}
