//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Compare;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	/// <summary>
	/// Lets the user review the duplicate/similar page groups a RemoveDuplicatesCommand scan
	/// found, and cherrypick which pages to delete - one card-like group per scanned title,
	/// each row showing a similarity chip reusing Compare Hierarchy's own scoring/popup UI.
	/// </summary>
	internal partial class RemoveDuplicatesNavigator : UI.MoreForm
	{
		/// <summary>
		/// One row's worth of a group: the underlying scanned node, plus how (or whether) its
		/// similarity chip should render. Members are ordered latest-first (see BuildGroups),
		/// and only the non-first ("secondary") instances ever show a chip - the newest is
		/// treated as the reference (the one "Keep Newest" would keep), so there's nothing
		/// useful to compare it to itself.
		/// </summary>
		private sealed class MemberModel
		{
			public RemoveDuplicatesCommand.HashNode Node;
			public bool ShowChip;
			public bool IsExactChip;

			// only meaningful when ShowChip && !IsExactChip - see BuildGroups for why these
			// aren't always just "this node's own Result vs the group's scan anchor"
			public SimilarityResult Result;
			public string CompareLeftName;
			public string CompareRightName;
		}


		private sealed class GroupModel
		{
			public string Title;
			public bool IsEmptyGroup;
			public List<MemberModel> Members;
		}


		private const int TopPad = 8;
		private const int CardPadH = 12;
		private const int CardPadV = 8;
		private const int HeaderHeight = 30;
		private const int RowHeight = 44;
		private const int TitleLineHeight = 18;
		private const int CardGap = 10;
		private const int DeleteWidth = 32;
		private const int GapSm = 6;

		// horizontal breathing room inside the similarity chip's pill, both sides combined
		private const int ChipPadding = 24;

		// a worst-case "MMM d, yyyy h:mm tt" sample (2-digit day and hour) - measured with the
		// actual date font rather than guessed as a pixel constant, so the reserved date column
		// never clips regardless of font metrics/DPI, yet is still a fixed width every row can
		// share (see dateFont/dateBoxWidth, computed once per Rebuild)
		private const string DateMeasureSample = "Sep 22, 2026 11:45 PM";

		private readonly OneNote one;
		private readonly ToolTip tooltip;
		private readonly List<GroupModel> groups;

		private Font dateFont;
		private int dateBoxWidth;
		private int chipWidth;


		public RemoveDuplicatesNavigator()
			: base()
		{
			InitializeComponent();

			tooltip = new ToolTip();

			// dragging the scrollbar thumb (unlike mouse-wheel scrolling) drives
			// ScrollableControl to BitBlt the existing pixels and invalidate only the newly
			// exposed strip, which leaves owner-drawn children (MoreButton/SimilarityChip)
			// garbled at their old position until something forces a full repaint
			resultsPanel.Scroll += (s, e) => resultsPanel.Refresh();

			if (NeedsLocalizing())
			{
				Text = Resx.RemoveDuplicatesDialog_Text;

				Localize(new string[]
				{
					"cancelButton=word_Close"
				});
			}
		}


		public RemoveDuplicatesNavigator(List<RemoveDuplicatesCommand.HashNode> hashes)
			: this()
		{
			groups = BuildGroups(hashes);
			one = new OneNote();
		}


		protected override void OnLoad(EventArgs e)
		{
			resultsPanel.BackColor = manager.GetColor("Window");
			BackColor = manager.GetColor("Control");
			ForeColor = manager.GetColor("ControlText");

			base.OnLoad(e);

			Rebuild();
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Model...

		private static List<GroupModel> BuildGroups(List<RemoveDuplicatesCommand.HashNode> hashes)
		{
			var groups = new List<GroupModel>();

			foreach (var head in hashes)
			{
				var isEmptyGroup = head.PageID == null;

				var nodes = new List<RemoveDuplicatesCommand.HashNode>();
				if (!isEmptyGroup)
				{
					nodes.Add(head);
				}
				nodes.AddRange(head.Siblings);

				// latest first: the newest instance - the one "Keep Newest" would actually keep
				// - is treated as the reference and never shows a chip; every older instance
				// does, whether it exact-hash-matched or was scored as a near-duplicate (see
				// the ShowChip assignment below)
				nodes.Sort((a, b) => b.LastModified.CompareTo(a.LastModified));

				// ScoreNearDuplicates only ever records "head vs sibling" (a star topology,
				// head being the page the exact-hash pass happened to scan first - unrelated
				// to LastModified), never sibling-vs-sibling. So once sorted, head itself can
				// land anywhere, not just position 0: when it does, and some sibling is newer,
				// that sibling's own MatchKind/Result *is* "head vs newest" (the relationship
				// is symmetric) - just recorded on the sibling's side - so head's own row
				// borrows it (with names swapped) rather than showing nothing/mislabeled data
				var newest = nodes.Count > 0 ? nodes[0] : null;

				var members = new List<MemberModel>();
				for (var i = 0; i < nodes.Count; i++)
				{
					var node = nodes[i];
					var member = new MemberModel { Node = node, ShowChip = !isEmptyGroup && i > 0 };

					if (member.ShowChip)
					{
						if (ReferenceEquals(node, head) && !ReferenceEquals(newest, head))
						{
							member.IsExactChip =
								newest.MatchKind == RemoveDuplicatesCommand.MatchKind.Exact;
							member.Result = newest.Result;
							member.CompareLeftName = newest.Title;
							member.CompareRightName = head.Title;
						}
						else
						{
							member.IsExactChip =
								node.MatchKind == RemoveDuplicatesCommand.MatchKind.Exact;
							member.Result = node.Result;
							member.CompareLeftName = head.Title;
							member.CompareRightName = node.Title;
						}
					}

					members.Add(member);
				}

				var hasSimilar = head.Siblings.Any(
					s => s.MatchKind == RemoveDuplicatesCommand.MatchKind.Similar);

				var title = isEmptyGroup
					? head.Title
					: string.Format(hasSimilar
						? Resx.RemoveDuplicatesNavigator_pagesSimilarTo
						: Resx.RemoveDuplicatesNavigator_duplicatesOf,
						head.Title);

				groups.Add(new GroupModel
				{
					Title = title,
					IsEmptyGroup = isEmptyGroup,
					Members = members
				});
			}

			return groups;
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Layout...

		private void Rebuild()
		{
			resultsPanel.SuspendLayout();
			resultsPanel.Controls.Clear();

			dateFont?.Dispose();
			dateFont = new Font("Consolas", Math.Max(7f, Font.SizeInPoints - 1f));
			dateBoxWidth = TextRenderer.MeasureText(DateMeasureSample, dateFont).Width + 6;

			// measured, not guessed, so a translation longer than English (the exact-match
			// chip's "100% · identical" text in particular) still fits without overflowing
			chipWidth = Math.Max(
				TextRenderer.MeasureText(Resx.RemoveDuplicatesNavigator_identicalChip, Font).Width,
				TextRenderer.MeasureText(
					string.Format(Resx.RemoveDuplicatesNavigator_similarChipFormat, 100), Font).Width)
				+ ChipPadding;

			var y = TopPad;
			foreach (var group in groups)
			{
				var cardHeight = (CardPadV * 2) + HeaderHeight + (group.Members.Count * RowHeight);
				var card = CreateCardPanel(y, cardHeight);

				AddGroupHeader(card, group);

				var rowY = CardPadV + HeaderHeight;
				foreach (var member in group.Members)
				{
					AddMemberRow(card, group, member, rowY);
					rowY += RowHeight;
				}

				resultsPanel.Controls.Add(card);
				y += cardHeight + CardGap;
			}

			resultsPanel.AutoScrollMinSize = new Size(0, y);
			resultsPanel.ResumeLayout();

			if (groups.Count == 0)
			{
				Close();
			}
		}


		private int ContentWidth =>
			Math.Max(400, resultsPanel.ClientSize.Width - (SystemInformation.VerticalScrollBarWidth + 4));


		/// <summary>
		/// One duplicate/similar group's card: a themed "alternate background" surface with a
		/// thin border, so consecutive groups read as visually distinct cards rather than one
		/// undifferentiated scroll of rows.
		/// </summary>
		private Panel CreateCardPanel(int y, int height)
		{
			var borderColor = manager.GetColor("ButtonBorder");

			var card = new Panel
			{
				BackColor = manager.GetColor("Control"),
				Location = new Point(TopPad, y),
				Size = new Size(ContentWidth - (TopPad * 2), height),
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			};

			card.Paint += (s, e) =>
			{
				using var pen = new Pen(borderColor);
				e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
			};

			return card;
		}


		private void AddGroupHeader(Panel card, GroupModel group)
		{
			var width = card.Width - (CardPadH * 2);

			// dynamically-created controls miss MoreForm's ILoadControl walk (it only runs once,
			// during the form's own Load), so colors are set explicitly here rather than via
			// ThemedFore/ThemedBack - this runs on every Rebuild, not just the first
			var titleLabel = new MoreLabel
			{
				AutoSize = false,
				Font = new Font(Font, FontStyle.Bold),
				ForeColor = manager.GetColor("ControlText"),
				BackColor = card.BackColor,
				Location = new Point(CardPadH, CardPadV + 2),
				Size = new Size(width - 150, HeaderHeight - 8),
				Text = group.Title,
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			};
			card.Controls.Add(titleLabel);

			if (!group.IsEmptyGroup && group.Members.Count >= 2)
			{
				var keepButton = MakeKeepNewestButton(group);
				keepButton.Location = new Point(card.Width - CardPadH - keepButton.Width, CardPadV - 2);
				keepButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
				card.Controls.Add(keepButton);
			}
		}


		private void AddMemberRow(Panel card, GroupModel group, MemberModel member, int y)
		{
			var node = member.Node;
			var midY = y + (RowHeight / 2);

			var deleteButton = MakeDeleteButton();
			deleteButton.Location = new Point(card.Width - CardPadH - DeleteWidth, midY - 12);
			deleteButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			deleteButton.Click += (s, e) => DeleteMember(group, member);
			if (group.IsEmptyGroup)
			{
				tooltip.SetToolTip(deleteButton, Resx.RemoveDuplicatesNavigator_emptyPageTip);
			}
			card.Controls.Add(deleteButton);

			// the chip slot's width is reserved unconditionally - even on a row that doesn't
			// show one - so the date column's X position is identical on every row in every
			// card, rather than shifting left/right depending on whether that particular row
			// happens to have a chip
			var chipLeft = card.Width - CardPadH - DeleteWidth - GapSm - chipWidth;
			if (member.ShowChip)
			{
				var chip = new SimilarityChip
				{
					BackColor = card.BackColor,
					Location = new Point(chipLeft, midY - 11),
					Size = new Size(chipWidth, 22),
					Anchor = AnchorStyles.Top | AnchorStyles.Right,
					HostForm = this
				};

				if (member.IsExactChip)
				{
					chip.SetExact();
				}
				else
				{
					chip.SetSimilar(member.Result, member.CompareLeftName, member.CompareRightName);
				}

				card.Controls.Add(chip);
			}

			var dateLeft = chipLeft - GapSm - dateBoxWidth;

			// full date/time, matching Compare Hierarchy's and Search Titles' own timestamps.
			// Anchor=Right, same as the chip/delete button, so it floats together with that
			// cluster as the dialog resizes; a *fixed*, measured width (dateBoxWidth, computed
			// once in Rebuild - not per-row AutoSize) is what lets every row's date still line
			// up on its own left edge despite being right-anchored, and lets it be left-aligned
			// within its own box without risking the clipping a guessed pixel width once caused
			var dateLabel = new MoreLabel
			{
				AutoSize = false,
				Font = dateFont,
				ForeColor = manager.GetColor("GrayText"),
				BackColor = card.BackColor,
				Text = node.LastModified == DateTime.MinValue
					? string.Empty : node.LastModified.ToShortFriendlyString(),
				TextAlign = ContentAlignment.MiddleLeft,
				Location = new Point(dateLeft, midY - 9),
				Size = new Size(dateBoxWidth, 18),
				Anchor = AnchorStyles.Top | AnchorStyles.Right
			};
			card.Controls.Add(dateLabel);

			var textWidth = Math.Max(60, dateLeft - GapSm - CardPadH);

			// AutoSize + MaximumSize (rather than a stretched, fixed-width label) so the
			// clickable region matches the rendered text itself, not the full row width
			var link = MakeNavigationLink(node, node.Title);
			link.Font = new Font(Font, FontStyle.Regular);
			link.MaximumSize = new Size(textWidth, 0);
			link.Location = new Point(CardPadH, y + 3);
			link.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			link.BackColor = card.BackColor;
			link.LinkColor = manager.GetColor(group.IsEmptyGroup ? "GrayText" : "HotTrack");
			card.Controls.Add(link);

			if (!string.IsNullOrEmpty(node.Path))
			{
				// plain text, not a link - only the title itself navigates
				var pathLabel = new MoreLabel
				{
					AutoSize = true,
					Font = new Font(Font.FontFamily, 7.5f),
					MaximumSize = new Size(textWidth, 0),
					Location = new Point(CardPadH, y + 3 + TitleLineHeight),
					ForeColor = manager.GetColor("GrayText"),
					BackColor = card.BackColor,
					Text = node.Path,
					Anchor = AnchorStyles.Top | AnchorStyles.Left
				};
				card.Controls.Add(pathLabel);
			}
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Controls...

		/// <summary>
		/// A link (used for both the title and, separately, the full path row) that navigates
		/// to node's page when clicked. AutoSize=true so the clickable region always matches
		/// whatever text is passed in, not some wider fixed bound.
		/// </summary>
		private MoreLinkLabel MakeNavigationLink(RemoveDuplicatesCommand.HashNode node, string text)
		{
			var label = new MoreLinkLabel
			{
				AutoSize = true,
				Text = text
			};

			label.Click += (s, e) =>
			{
				if (node.PageID != null && !node.PageID.Equals(one.CurrentPageId))
				{
					Task.Run(async () => { await one.NavigateTo(node.PageID); });
				}
			};

			return label;
		}


		private Button MakeDeleteButton()
		{
			Image image = Resx.m_Delete;
			if (manager.DarkMode)
			{
				using var original = image;
				image = new ImageEditor { Style = ImageEditor.Stylization.Invert }.Apply(original);
			}

			var button = new Button
			{
				Image = image,
				Padding = new Padding(0),
				Margin = new Padding(0),
				FlatStyle = FlatStyle.Flat,
				Width = DeleteWidth,
				Height = 24,
				BackColor = manager.GetColor("ButtonFace")
			};

			button.FlatAppearance.BorderColor = manager.GetColor("ButtonBorder");
			return button;
		}


		private MoreButton MakeKeepNewestButton(GroupModel group)
		{
			var button = new MoreButton
			{
				AutoSize = true,
				Text = Resx.RemoveDuplicatesNavigator_keepNewest,
				Padding = new Padding(10, 2, 10, 2),
				Margin = new Padding(0),
				ShowBorder = true,
				Height = 26,
				BackColor = manager.GetColor("ButtonFace"),
				ForeColor = manager.GetColor("HotTrack")
			};

			button.Click += (s, e) => KeepNewest(group);
			return button;
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Actions...

		private void KeepNewest(GroupModel group)
		{
			if (group.Members.Count < 2)
			{
				return;
			}

			var newest = group.Members.OrderByDescending(m => m.Node.LastModified).First();
			var toDelete = group.Members.Where(m => m != newest).ToList();

			var msg = string.Format(Resx.RemoveDuplicatesNavigator_confirmAll, toDelete.Count);
			if (MoreMessageBox.Show(Owner, msg, MessageBoxButtons.YesNo, MessageBoxIcon.Question)
				!= DialogResult.Yes)
			{
				return;
			}

			foreach (var member in toDelete)
			{
				logger.WriteLine($"deleting page '{member.Node.Title}'; moved to recyclebin");
				one.DeleteHierarchy(member.Node.PageID);
			}

			// the group is resolved down to a single survivor; nothing left to compare
			groups.Remove(group);
			Rebuild();
		}


		private void DeleteMember(GroupModel group, MemberModel member)
		{
			var msg = Resx.RemoveDuplicatesNavigator_confirm1;
			if (MoreMessageBox.Show(Owner, msg, MessageBoxButtons.YesNo, MessageBoxIcon.Question)
				!= DialogResult.Yes)
			{
				return;
			}

			logger.WriteLine($"deleting page '{member.Node.Title}'; moved to recyclebin");
			one.DeleteHierarchy(member.Node.PageID);
			group.Members.Remove(member);

			// once fewer than two pages remain, there's nothing left to call a duplicate
			if (group.IsEmptyGroup ? group.Members.Count == 0 : group.Members.Count < 2)
			{
				groups.Remove(group);
			}

			Rebuild();
		}


		private void CloseDialog(object sender, EventArgs e)
		{
			Close();
		}
	}
}
