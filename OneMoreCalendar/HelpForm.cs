//************************************************************************************************
// Copyright © 2026 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// Present a popup window that lists the keyboard and mouse shortcuts, shown with F1.
	/// The layout is a static list so it is built in code rather than with the designer.
	/// </summary>
	internal class HelpForm : RoundedForm
	{
		private const int EdgePadding = 4;      // border ring around the content
		private const int ContentPadding = 20;  // space between the content and the border

		private static readonly (string Heading, (string Keys, string Action)[] Rows)[] Sections =
		{
			("Navigation", new[]
			{
				("PageUp  /  Ctrl+Left", "Previous month"),
				("PageDown  /  Ctrl+Right", "Next month"),
				("Home", "Jump to today"),
				("Ctrl+Tab", "Switch between month and day views"),
				("F5", "Refresh from OneNote")
			}),
			("Filter box", new[]
			{
				("Enter", "Apply the filter now"),
				("Esc", "Clear the filter")
			}),
			("Day view", new[]
			{
				("Up  /  Down", "Scroll the list of pages")
			}),
			("Mouse", new[]
			{
				("Click a page", "Open it in OneNote"),
				("Right-click a page", "Preview a snapshot of it"),
				("Click a day heading", "Open that day in the day view"),
				("Click the month and year", "Choose a different year"),
				("Mouse wheel", "Scroll the pages within a day")
			}),
			("General", new[]
			{
				("F1", "Show or hide this help"),
				("Esc", "Close a popup window")
			})
		};


		private Font titleFont;
		private Font headingFont;
		private Font keyFont;
		private Font textFont;
		private Label closeLabel;
		private bool closing;


		public HelpForm()
			: base()
		{
			AutoScaleMode = AutoScaleMode.None;
			FormBorderStyle = FormBorderStyle.None;
			KeyPreview = true;
			ShowIcon = false;
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.Manual;
			TopMost = true;
		}


		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				titleFont?.Dispose();
				headingFont?.Dispose();
				keyFont?.Dispose();
				textFont?.Dispose();
			}

			base.Dispose(disposing);
		}


		protected override void OnLoad(EventArgs e)
		{
			// size the form before calling base.OnLoad so RoundedForm's rounded region uses the
			// final size, like YearsForm
			BuildContent();

			// call RoundedForm.base to apply the rounded region and the theme
			base.OnLoad(e);
		}


		/// <summary>
		/// Creates the title, close button, and table of shortcuts, and sizes the form to fit
		/// </summary>
		private void BuildContent()
		{
			var pad = this.Scaled(ContentPadding);
			var edge = this.Scaled(EdgePadding);

			titleFont = new Font("Segoe UI", 12F);
			headingFont = new Font("Segoe UI", 10F, FontStyle.Bold);
			keyFont = new Font("Segoe UI Semibold", 9F);
			textFont = new Font("Segoe UI", 9F);

			var title = new Label
			{
				AutoSize = true,
				Font = titleFont,
				Text = "Keyboard shortcuts",
				Location = new Point(pad, pad)
			};

			var table = new TableLayoutPanel
			{
				AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink,
				ColumnCount = 2
			};

			var row = 0;
			foreach (var section in Sections)
			{
				var heading = new Label
				{
					AutoSize = true,
					Font = headingFont,
					Text = section.Heading,
					Margin = new Padding(0, row == 0 ? 0 : this.Scaled(14), 0, this.Scaled(4))
				};

				table.Controls.Add(heading, 0, row);
				table.SetColumnSpan(heading, 2);
				row++;

				foreach (var (keys, action) in section.Rows)
				{
					table.Controls.Add(new Label
					{
						AutoSize = true,
						Font = keyFont,
						Text = keys,
						Margin = new Padding(0, this.Scaled(2), this.Scaled(28), this.Scaled(2))
					}, 0, row);

					table.Controls.Add(new Label
					{
						AutoSize = true,
						Font = textFont,
						Text = action,
						Margin = new Padding(0, this.Scaled(2), 0, this.Scaled(2))
					}, 1, row);

					row++;
				}
			}

			table.RowCount = row;
			table.Location = new Point(pad, title.PreferredSize.Height + pad + this.Scaled(12));

			var tableSize = table.GetPreferredSize(Size.Empty);
			var content = new Size(
				Math.Max(tableSize.Width, title.PreferredSize.Width) + (pad * 2),
				table.Top + tableSize.Height + pad);

			var inset = this.Scaled(8);
			closeLabel = new Label
			{
				Cursor = Cursors.Hand,
				Size = new Size(this.Scaled(28), this.Scaled(28)),
				Text = "✕",
				TextAlign = ContentAlignment.MiddleCenter
			};

			closeLabel.Location = new Point(content.Width - closeLabel.Width - inset, inset);
			closeLabel.Click += (sender, args) => Close();
			closeLabel.MouseEnter += HoverClose;
			closeLabel.MouseLeave += HoverClose;

			var panel = new Panel { Dock = DockStyle.Fill };
			panel.Controls.Add(title);
			panel.Controls.Add(table);
			panel.Controls.Add(closeLabel);

			Padding = new Padding(edge);
			ClientSize = new Size(content.Width + (edge * 2), content.Height + (edge * 2));
			Controls.Add(panel);

			if (Owner is not null)
			{
				// center over the calendar
				Location = new Point(
					Owner.Left + ((Owner.Width - Width) / 2),
					Owner.Top + ((Owner.Height - Height) / 2));
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Handlers...

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.F1)
			{
				e.Handled = true;
				Close();
			}
		}


		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			closing = true;
			base.OnFormClosing(e);
		}


		/// <summary>
		/// Like the other popups, close when the user clicks away
		/// </summary>
		protected override void OnDeactivate(EventArgs e)
		{
			base.OnDeactivate(e);

			if (!closing)
			{
				Close();
			}
		}


		private void HoverClose(object sender, EventArgs e)
		{
			closeLabel.ForeColor = closeLabel.ClientRectangle.Contains(closeLabel.PointToClient(Cursor.Position))
				? Theme.HoverColor
				: Theme.ForeColor;
		}
	}
}
