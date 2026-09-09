//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Header ("Box Types" + "+ New box type") and scrollable stack of BoxTypeCard controls,
	/// shown on the "Box Types" tab of the Snippets settings sheet. Fills whatever space its
	/// host gives it (Dock = Fill is expected of this control), so cards stretch to the full
	/// width and the scrollable region to the full height of the tab.
	/// </summary>
	internal class BoxTypesPanel : MoreUserControl
	{
		private const int ButtonTopMargin = 8;
		private const int ButtonBottomMargin = 24;
		private const int CardGap = 12;

		private readonly List<BoxTypeCard> cards = new();
		private readonly Panel cardsHost;
		private readonly MoreButton newButton;
		private readonly MoreContextMenuStrip duplicateMenu = new();


		public BoxTypesPanel(IEnumerable<BoxType> boxTypes)
		{
			var headerRow = new Panel
			{
				Dock = DockStyle.Top
			};

			var titleLabel = new MoreLabel
			{
				AutoSize = true,
				Font = new Font(Font, FontStyle.Bold),
				Location = new Point(0, ButtonTopMargin),
				Text = Resx.BoxTypesPanel_Title
			};

			newButton = new MoreButton
			{
				AutoSize = true,
				Location = new Point(0, ButtonTopMargin),
				Text = Resx.BoxTypesPanel_NewBoxType
			};
			newButton.Click += ShowDuplicateMenu;

			headerRow.Height = newButton.Bottom + ButtonBottomMargin;

			headerRow.Controls.Add(titleLabel);
			headerRow.Controls.Add(newButton);
			headerRow.Resize += (s, e) =>
				newButton.Location = new Point(headerRow.ClientSize.Width - newButton.Width, ButtonTopMargin);

			// Cards are positioned manually (no Dock, no FlowLayoutPanel) and stacked by
			// explicitly setting Location/Width and AutoScrollMinSize below in LayoutCards -
			// both Dock=Top-in-an-AutoScroll-Panel and FlowLayoutPanel's Dock-driven stretch
			// produced squeezed or invisible cards here, so this avoids relying on either.
			cardsHost = new Panel
			{
				Dock = DockStyle.Fill,
				AutoScroll = true,
				BackColor = manager.GetColor("Control")
			};
			cardsHost.Resize += (s, e) => LayoutCards();

			Controls.Add(cardsHost);
			Controls.Add(headerRow);

			cardsHost.SuspendLayout();

			foreach (var box in boxTypes)
			{
				var card = WireCard(new BoxTypeCard(box));
				cards.Add(card);
				cardsHost.Controls.Add(card);
			}

			cardsHost.ResumeLayout(false);

			LayoutCards();
		}


		/// <summary>
		/// Positions every card top-to-bottom at the full width of the cards host, and sets
		/// AutoScrollMinSize to the total stacked height so the host scrolls correctly. Re-run
		/// whenever the host resizes or any card's own height changes (expand/collapse).
		/// </summary>
		private void LayoutCards()
		{
			cardsHost.SuspendLayout();

			var width = cardsHost.ClientSize.Width;
			var y = 0;

			foreach (var card in cards)
			{
				card.Width = width;
				card.Location = new Point(0, y);
				y += card.Height + CardGap;
			}

			cardsHost.AutoScrollMinSize = new Size(0, y);
			cardsHost.ResumeLayout(true);
		}


		public event EventHandler Changed;


		/// <summary>
		/// Returns the current box types in top-to-bottom (persisted) order
		/// </summary>
		public List<BoxType> GetBoxTypes()
		{
			return cards.Select(c => c.Box).ToList();
		}


		private BoxTypeCard WireCard(BoxTypeCard card)
		{
			card.Duplicate += (s, e) => DuplicateBox(card.Box);
			card.Delete += (s, e) => RemoveCard(card);
			card.Changed += (s, e) => Changed?.Invoke(this, EventArgs.Empty);
			card.Resize += (s, e) => LayoutCards();
			return card;
		}


		private void ShowDuplicateMenu(object sender, EventArgs e)
		{
			duplicateMenu.Items.Clear();

			var header = new MoreMenuItem(Resx.BoxTypesPanel_DuplicateFrom)
			{
				Enabled = false
			};
			duplicateMenu.Items.Add(header);
			duplicateMenu.Items.Add(new ToolStripSeparator());

			foreach (var card in cards)
			{
				var box = card.Box;
				var item = new MoreMenuItem(box.Title);
				item.Click += (s, e) => DuplicateBox(box);
				duplicateMenu.Items.Add(item);
			}

			duplicateMenu.Show(newButton, new Point(0, newButton.Height));
		}


		private void DuplicateBox(BoxType source)
		{
			var clone = source.Clone();
			clone.Id = Guid.NewGuid().ToString("N");
			clone.Title = string.Format(Resx.BoxTypesPanel_CopyTitle, source.Title);
			clone.IsBuiltin = false;

			var card = WireCard(new BoxTypeCard(clone));
			cards.Add(card);
			cardsHost.Controls.Add(card);
			LayoutCards();

			card.Expanded = true;
			cardsHost.ScrollControlIntoView(card);

			Changed?.Invoke(this, EventArgs.Empty);
		}


		private void RemoveCard(BoxTypeCard card)
		{
			cards.Remove(card);
			cardsHost.Controls.Remove(card);
			card.Dispose();

			LayoutCards();
			Changed?.Invoke(this, EventArgs.Empty);
		}
	}
}
