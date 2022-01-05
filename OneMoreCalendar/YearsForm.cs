//************************************************************************************************
// Copyright © 2022 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using River.OneMoreAddIn;
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;


	/// <summary>
	/// Present a popup window from which the user can choose a year
	/// </summary>
	internal partial class YearsForm : RoundedForm
	{

		private const int ItemRadius = 4;

		private int skipYear; // exclude currently displayed year
		private ListViewItem hoveredItem;


		/// <summary>
		/// Consumers should call YearsForm(int)
		/// </summary>
		public YearsForm()
			: base()
		{
			InitializeComponent();
		}


		/// <summary>
		/// Initialize the form
		/// </summary>
		/// <param name="year">The year to exclude from the list</param>
		public YearsForm(int year)
			: this()
		{
			skipYear = year;
		}


		/// <summary>
		/// Preload the years to display
		/// </summary>
		/// <param name="e"></param>
		protected override async void OnLoad(EventArgs e)
		{
			// call RoundForm.base to draw background
			base.OnLoad(e);

			if (!DesignMode)
			{
				var years = await new OneNoteProvider()
					.GetYears(await new SettingsProvider().GetNotebookIDs());

				years.ToList().ForEach(y =>
				{
					if (y != skipYear)
					{
						listView.Items.Add(y.ToString());
					}
				});
			}
		}


		/// <summary>
		/// Gets the chosen year
		/// </summary>
		public int Year { get; private set; }


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Handlers...

		private void EscapeForm(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				JustLeave(sender, e);
			}
		}


		private void JustLeave(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}


		private void DrawItem(object sender, DrawListViewItemEventArgs e)
		{
			if (hoveredItem == e.Item)
			{
				var size = e.Graphics.MeasureString(e.Item.Text, listView.Font);
				var bounds = new Rectangle(e.Bounds.X, e.Bounds.Y, (int)size.Width + 8, e.Bounds.Height);

				e.Graphics.FillRoundedRectangle(AppColors.HoverBrush, bounds, ItemRadius);
				e.Graphics.DrawRoundedRectangle(AppColors.HoverPen, bounds, ItemRadius);
			}
			else
			{
				e.Graphics.FillRectangle(Brushes.White, e.Bounds);
			}

			e.Graphics.DrawString(e.Item.Text, listView.Font, AppColors.TextBrush, e.Bounds);
		}


		private void HoverMouse(object sender, MouseEventArgs e)
		{
			var item = listView.GetItemAt(e.X, e.Y);
			if (hoveredItem == item)
			{
				return;
			}

			// unhighlight old

			int index;
			if (hoveredItem != null)
			{
				var oldItem = hoveredItem;
				hoveredItem = null;

				index = listView.Items.IndexOf(oldItem);

				DrawItem(sender, new DrawListViewItemEventArgs(
					listView.CreateGraphics(), oldItem, 
					listView.GetItemRect(index), index, ListViewItemStates.Default));
			}

			// highlight new

			hoveredItem = item;
			if (hoveredItem != null)
			{
				index = listView.Items.IndexOf(hoveredItem);

				DrawItem(sender, new DrawListViewItemEventArgs(
					listView.CreateGraphics(), hoveredItem,
					listView.GetItemRect(index), index, ListViewItemStates.Hot));
			}
		}


		private void ChooseYear(object sender, MouseEventArgs e)
		{
			var item = listView.GetItemAt(e.X, e.Y);
			if (item != null)
			{
				Year = int.Parse(item.Text);
				DialogResult = DialogResult.OK;
				Close();
			}
		}
	}
}
