//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Compare
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Shows a surface-level diff of two OneNote hierarchy branches (notebook, section, or
	/// section group) - names, node types, and created/modified timestamps only - and lets
	/// the user act on the selected node or page. Runs modeless since Open Left/Right (and,
	/// in later phases, Copy/Mirror/Delete) call back into OneNote while this dialog stays
	/// open.
	/// </summary>
	internal partial class CompareDialog : MoreForm
	{
		private readonly DiffNode root;
		private readonly OneNote.NodeType nodeType;

		private Panel hierarchyActionsPanel;
		private Panel pageActionsPanel;

		private MoreButton copyRightButton;
		private MoreButton copyLeftButton;
		private MoreButton mirrorRightButton;
		private MoreButton mirrorLeftButton;

		private MoreButton openLeftButton;
		private MoreButton openRightButton;
		private MoreButton pageCopyRightButton;
		private MoreButton pageCopyLeftButton;
		private MoreButton deleteLeftButton;
		private MoreButton deleteRightButton;
		private MoreButton deleteBothButton;
		private MoreButton compareContentsButton;


		public CompareDialog(DiffNode root, OneNote.NodeType nodeType, string sourceName, string targetName)
		{
			InitializeComponent();

			this.root = root;
			this.nodeType = nodeType;

			Text = Resx.CompareDialog_title;

			BuildTopPanel(sourceName, targetName);
			BuildFooterPanel();

			diffView.SelectionChanged += DiffViewSelectionChanged;
			diffView.SetRoot(root);
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Layout...

		private void BuildTopPanel(string sourceName, string targetName)
		{
			var manager = ThemeManager.Instance;

			var topPanel = new TableLayoutPanel
			{
				Dock = DockStyle.Top,
				AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink,
				ColumnCount = 1,
				RowCount = 3,
				Padding = new Padding(16, 10, 16, 6)
			};
			topPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			topPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			topPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

			var headerFlow = new FlowLayoutPanel
			{
				AutoSize = true,
				FlowDirection = FlowDirection.LeftToRight,
				WrapContents = false
			};

			var sourceLabel = new MoreLabel
			{
				AutoSize = true,
				Font = new Font(Font, FontStyle.Bold),
				Text = sourceName,
				Margin = new Padding(0, 0, 8, 0)
			};

			var arrowLabel = new MoreLabel
			{
				AutoSize = true,
				Text = "↔",
				ThemedFore = "GrayText",
				Margin = new Padding(0, 0, 8, 0)
			};

			var targetLabel = new MoreLabel
			{
				AutoSize = true,
				Font = new Font(Font, FontStyle.Bold),
				Text = targetName,
				Margin = new Padding(0, 0, 16, 0)
			};

			var badgeLabel = new MoreLabel
			{
				AutoSize = true,
				Text = GetBadgeText(nodeType),
				ThemedFore = "HotTrack",
				Font = new Font(Font, FontStyle.Bold),
				Margin = new Padding(0, 2, 0, 0)
			};

			headerFlow.Controls.Add(sourceLabel);
			headerFlow.Controls.Add(arrowLabel);
			headerFlow.Controls.Add(targetLabel);
			headerFlow.Controls.Add(badgeLabel);

			var scopeHintLabel = new MoreLabel
			{
				AutoSize = true,
				Text = Resx.CompareDialog_scopeHint,
				ThemedFore = "GrayText",
				Margin = new Padding(0, 6, 0, 0)
			};

			var legendFlow = new FlowLayoutPanel
			{
				AutoSize = true,
				FlowDirection = FlowDirection.LeftToRight,
				WrapContents = false,
				Margin = new Padding(0, 8, 0, 0)
			};

			// mirror DrawWell's three unselected states exactly: Same is a green equals
			// sign, DifferentTimestamps is a filled circle, Orphan is a dashed outline circle
			legendFlow.Controls.Add(CreateEqualsSwatch(manager.GetColor("SuccessFill")));
			legendFlow.Controls.Add(CreateLegendLabel(Resx.CompareDialog_legendSame));
			legendFlow.Controls.Add(CreateSwatch(fill: manager.GetColor("CompareWarningFill")));
			legendFlow.Controls.Add(CreateLegendLabel(Resx.CompareDialog_legendDifferent));
			legendFlow.Controls.Add(CreateSwatch(fill: null, border: manager.GetColor("ErrorText"), dashed: true));
			legendFlow.Controls.Add(CreateLegendLabel(Resx.CompareDialog_legendOrphan));

			topPanel.Controls.Add(headerFlow, 0, 0);
			topPanel.Controls.Add(scopeHintLabel, 0, 1);
			topPanel.Controls.Add(legendFlow, 0, 2);

			Controls.Add(topPanel);
		}


		// draws as a circle to match the diff tree's own center-well glyph (DrawWell)
		private static Panel CreateSwatch(Color? fill, Color? border = null, bool dashed = false)
		{
			var panel = new Panel
			{
				Size = new Size(10, 10),
				BackColor = Color.Transparent,
				Margin = new Padding(0, 4, 4, 0)
			};

			panel.Paint += (s, e) =>
			{
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

				if (fill.HasValue)
				{
					using var brush = new SolidBrush(fill.Value);
					e.Graphics.FillEllipse(brush, 0, 0, panel.Width - 1, panel.Height - 1);
				}

				if (border.HasValue)
				{
					using var pen = new Pen(border.Value, dashed ? 2f : 1f);
					if (dashed)
					{
						// Dot (not Dash) + a round cap: long flat-capped dashes tile
						// unevenly and look chunky around a circle this small
						pen.DashStyle = DashStyle.Dot;
						pen.DashCap = DashCap.Round;
					}

					e.Graphics.DrawEllipse(pen, 0, 0, panel.Width - 1, panel.Height - 1);
				}
			};

			return panel;
		}


		// small green equals sign, matching DrawWell's glyph for the Same (unselected) state
		private static Panel CreateEqualsSwatch(Color color)
		{
			var panel = new Panel
			{
				Size = new Size(10, 10),
				BackColor = Color.Transparent,
				Margin = new Padding(0, 4, 4, 0)
			};

			panel.Paint += (s, e) =>
			{
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

				using var pen = new Pen(color, 1.5f);
				var cy = panel.Height / 2f;
				e.Graphics.DrawLine(pen, 1, cy - 2, panel.Width - 1, cy - 2);
				e.Graphics.DrawLine(pen, 1, cy + 2, panel.Width - 1, cy + 2);
			};

			return panel;
		}


		private static MoreLabel CreateLegendLabel(string text)
		{
			return new MoreLabel
			{
				AutoSize = true,
				Text = text,
				ThemedFore = "GrayText",
				Margin = new Padding(0, 0, 16, 0)
			};
		}


		private static string GetBadgeText(OneNote.NodeType type)
		{
			return type switch
			{
				OneNote.NodeType.Notebook => Resx.CompareDialog_badgeNotebook,
				OneNote.NodeType.SectionGroup => Resx.CompareDialog_badgeSectionGroup,
				_ => Resx.CompareDialog_badgeSection
			};
		}


		private void BuildFooterPanel()
		{
			var footerPanel = new Panel
			{
				Dock = DockStyle.Bottom,
				Height = 122,
				Padding = new Padding(16, 8, 16, 8)
			};

			hierarchyActionsPanel = BuildHierarchyActionsPanel();
			pageActionsPanel = BuildPageActionsPanel();

			footerPanel.Controls.Add(hierarchyActionsPanel);
			footerPanel.Controls.Add(pageActionsPanel);

			hierarchyActionsPanel.Visible = false;
			pageActionsPanel.Visible = false;

			Controls.Add(footerPanel);
		}


		private Panel BuildHierarchyActionsPanel()
		{
			var panel = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 1,
				RowCount = 2
			};
			panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

			var title = new MoreLabel
			{
				AutoSize = true,
				Font = new Font(Font, FontStyle.Bold),
				Text = Resx.CompareDialog_hierarchyActionsTitle,
				Margin = new Padding(0, 0, 0, 6)
			};

			var buttonFlow = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				WrapContents = true
			};

			copyRightButton = CreateActionButton(Resx.CompareDialog_copyRight);
			copyLeftButton = CreateActionButton(Resx.CompareDialog_copyLeft);
			mirrorRightButton = CreateActionButton(Resx.CompareDialog_mirrorRight);
			mirrorLeftButton = CreateActionButton(Resx.CompareDialog_mirrorLeft);

			buttonFlow.Controls.Add(copyRightButton);
			buttonFlow.Controls.Add(copyLeftButton);
			buttonFlow.Controls.Add(mirrorRightButton);
			buttonFlow.Controls.Add(mirrorLeftButton);

			panel.Controls.Add(title, 0, 0);
			panel.Controls.Add(buttonFlow, 0, 1);

			return panel;
		}


		private Panel BuildPageActionsPanel()
		{
			var panel = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 1,
				RowCount = 3
			};
			panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
			panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

			var title = new MoreLabel
			{
				AutoSize = true,
				Font = new Font(Font, FontStyle.Bold),
				Text = Resx.CompareDialog_pageActionsTitle,
				Margin = new Padding(0, 0, 0, 6)
			};

			var buttonFlow = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				WrapContents = true
			};

			openLeftButton = CreateActionButton(Resx.CompareDialog_openLeft);
			openLeftButton.Click += OpenLeftClick;

			openRightButton = CreateActionButton(Resx.CompareDialog_openRight);
			openRightButton.Click += OpenRightClick;

			pageCopyRightButton = CreateActionButton(Resx.CompareDialog_copyRight);
			pageCopyLeftButton = CreateActionButton(Resx.CompareDialog_copyLeft);

			deleteLeftButton = CreateActionButton(Resx.CompareDialog_deleteLeft, danger: true);
			deleteRightButton = CreateActionButton(Resx.CompareDialog_deleteRight, danger: true);
			deleteBothButton = CreateActionButton(Resx.CompareDialog_deleteBoth, danger: true);

			compareContentsButton = CreateActionButton(Resx.CompareDialog_compareContents, wide: true);

			buttonFlow.Controls.Add(openLeftButton);
			buttonFlow.Controls.Add(openRightButton);
			buttonFlow.Controls.Add(pageCopyRightButton);
			buttonFlow.Controls.Add(pageCopyLeftButton);
			buttonFlow.Controls.Add(deleteLeftButton);
			buttonFlow.Controls.Add(deleteRightButton);
			buttonFlow.Controls.Add(deleteBothButton);
			buttonFlow.Controls.Add(compareContentsButton);

			var caption = new MoreLabel
			{
				AutoSize = true,
				ThemedFore = "GrayText",
				Text = Resx.CompareDialog_deleteBothCaption,
				Margin = new Padding(0, 4, 0, 0)
			};

			panel.Controls.Add(title, 0, 0);
			panel.Controls.Add(buttonFlow, 0, 1);
			panel.Controls.Add(caption, 0, 2);

			return panel;
		}


		private static MoreButton CreateActionButton(string text, bool danger = false, bool wide = false)
		{
			return new MoreButton
			{
				Text = text,
				Size = new Size(wide ? 180 : 100, 30),
				Margin = new Padding(0, 0, 8, 8),
				ThemedFore = danger ? "ErrorText" : "HotTrack",

				// Copy/Mirror/Delete/Compare contents are wired up in later phases; Open
				// left/right are enabled dynamically as the selection changes below
				Enabled = false
			};
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Selection and actions...

		private void DiffViewSelectionChanged(object sender, EventArgs e)
		{
			var node = diffView.SelectedNode;

			if (node is null)
			{
				hierarchyActionsPanel.Visible = false;
				pageActionsPanel.Visible = false;
				return;
			}

			var isPage = node.NodeType == OneNote.NodeType.Page;
			hierarchyActionsPanel.Visible = !isPage;
			pageActionsPanel.Visible = isPage;

			if (isPage)
			{
				openLeftButton.Enabled = node.LeftId is not null;
				openRightButton.Enabled = node.RightId is not null;
			}
		}


		private async void OpenLeftClick(object sender, EventArgs e)
		{
			await OpenPage(diffView.SelectedNode?.LeftId);
		}


		private async void OpenRightClick(object sender, EventArgs e)
		{
			await OpenPage(diffView.SelectedNode?.RightId);
		}


		private async Task OpenPage(string pageId)
		{
			if (string.IsNullOrEmpty(pageId))
			{
				return;
			}

			await using var one = new OneNote();
			var link = one.GetHyperlink(pageId, string.Empty);
			if (!string.IsNullOrEmpty(link))
			{
				await one.NavigateTo(link, true);
			}
		}


		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Escape)
			{
				Close();
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}
	}
}
