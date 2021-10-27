//************************************************************************************************
// Copyright © 2018 Steven M Cohn.  Yada yada...
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Styles;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class ReorderDialog : UI.LocalizableForm
	{

		public ReorderDialog(ComboBox.ObjectCollection items)
		{
			InitializeComponent();

			var list = items.Cast<GraphicStyle>().ToArray();
			listBox.Items.AddRange(list);

			listBox.SelectedIndex = 0;

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


		public GraphicStyle[] GetItems()
		{
			var items = listBox.Items.Cast<GraphicStyle>().ToArray();
			return items;
		}



		private void ChangeSelection(object sender, System.EventArgs e)
		{
			upButton.Enabled = listBox.SelectedIndex > 0;
			downButton.Enabled = listBox.SelectedIndex < listBox.Items.Count - 1;
		}


		private void MoveUp(object sender, System.EventArgs e)
		{
			var index = listBox.SelectedIndex;
			var item = listBox.SelectedItem;
			listBox.Items.RemoveAt(index);

			index--;
			listBox.Items.Insert(index, item);

			for (var i = 0; i < listBox.Items.Count; i++)
			{
				(listBox.Items[i] as GraphicStyle).Index = i;
			}

			listBox.SelectedIndex = index;
		}


		private void MoveDown(object sender, System.EventArgs e)
		{
			var index = listBox.SelectedIndex;
			var item = listBox.SelectedItem;
			listBox.Items.RemoveAt(index);

			index++;
			listBox.Items.Insert(index, item);

			for (var i = 0; i < listBox.Items.Count; i++)
			{
				(listBox.Items[i] as GraphicStyle).Index = i;
			}

			listBox.SelectedIndex = index;
		}


		private void DrawItem(object sender, DrawItemEventArgs e)
		{
			var item = listBox.Items[e.Index] as GraphicStyle;

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
				if (item.StyleType == StyleType.Heading)
				{
					using (var hfont = new Font(DefaultFont.FontFamily, DefaultFont.Size - 2.0f, FontStyle.Bold | FontStyle.Italic))
					{
						e.Graphics.DrawString("H", hfont, brush, e.Bounds.Location.X + 2, e.Bounds.Location.Y + 1);
					}
				}

				e.Graphics.DrawString(item.Name, DefaultFont, brush, e.Bounds.Location.X + 18, e.Bounds.Location.Y);
			}
			catch
			{
				// closing?
			}
		}


		private void MeasureItem(object sender, MeasureItemEventArgs e)
		{
			e.ItemHeight = 24;
		}
	}
}
