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
	/// the user act on the selected node or page: Copy/Mirror a container node (see
	/// HierarchyDiffSync), Open/Copy/Delete a page (this class), or score two pages' content
	/// similarity (see SimilarityEngine and SimilarityPopup). Runs modeless since these all
	/// call back into OneNote while this dialog stays open.
	/// </summary>
	internal partial class CompareDialog : MoreForm
	{
		private DiffNode root;
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

			// modeless dialogs otherwise appear behind the OneNote window - sometimes on
			// first show (racing OneNote's own picker dialog closing), and always whenever
			// focus returns to OneNote (e.g. after Open left/right navigates to a page in a
			// new OneNote window); this tracks OneNote's focus and re-elevates on top of it,
			// same as SearchDialog/HashtagDialog
			ElevatedWithOneNote = true;

			Text = Resx.CompareDialog_title;

			BuildTopPanel(sourceName, targetName);
			BuildFooterPanel();

			diffView.SelectionChanged += DiffViewSelectionChanged;
			diffView.ContextMenuRequested += DiffViewContextMenuRequested;
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
			copyRightButton.Click += CopyRightClick;

			copyLeftButton = CreateActionButton(Resx.CompareDialog_copyLeft);
			copyLeftButton.Click += CopyLeftClick;

			mirrorRightButton = CreateActionButton(Resx.CompareDialog_mirrorRight);
			mirrorRightButton.Click += MirrorRightClick;

			mirrorLeftButton = CreateActionButton(Resx.CompareDialog_mirrorLeft);
			mirrorLeftButton.Click += MirrorLeftClick;

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
			pageCopyRightButton.Click += PageCopyRightClick;

			pageCopyLeftButton = CreateActionButton(Resx.CompareDialog_copyLeft);
			pageCopyLeftButton.Click += PageCopyLeftClick;

			deleteLeftButton = CreateActionButton(Resx.CompareDialog_deleteLeft, danger: true);
			deleteLeftButton.Click += DeleteLeftClick;

			deleteRightButton = CreateActionButton(Resx.CompareDialog_deleteRight, danger: true);
			deleteRightButton.Click += DeleteRightClick;

			deleteBothButton = CreateActionButton(Resx.CompareDialog_deleteBoth, danger: true);
			deleteBothButton.Click += DeleteBothClick;

			compareContentsButton = CreateActionButton(Resx.CompareDialog_compareContents, wide: true);
			compareContentsButton.Click += CompareContentsClick;

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

				// enabled dynamically as the selection changes, below
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

				// copy direction depends only on whether that side has a source to copy
				// from, same as the hierarchy actions below - not on which side is selected
				pageCopyRightButton.Enabled = node.LeftId is not null;
				pageCopyLeftButton.Enabled = node.RightId is not null;

				deleteLeftButton.Enabled = node.LeftId is not null;
				deleteRightButton.Enabled = node.RightId is not null;

				// per spec, "Delete both" and "Compare contents..." only enable when the row
				// was selected via the center well (both sides), not merely because both
				// sides happen to exist
				var bothSelected = node.LeftId is not null && node.RightId is not null
					&& diffView.SelectedSide == DiffSide.Both;

				deleteBothButton.Enabled = bothSelected;
				compareContentsButton.Enabled = bothSelected;
			}
			else
			{
				// enabled whenever there's something on that button's source side to act
				// on, regardless of which side (if any) is currently selected - direction
				// is chosen by the button, not by the well/name click
				copyRightButton.Enabled = node.LeftId is not null;
				mirrorRightButton.Enabled = node.LeftId is not null;
				copyLeftButton.Enabled = node.RightId is not null;
				mirrorLeftButton.Enabled = node.RightId is not null;
			}
		}


		// right-clicking a row selects it (see HierarchyDiffView.OnMouseUp), which raises
		// SelectionChanged synchronously above before this fires - so the toolbar buttons'
		// Enabled state already reflects the right-clicked row and can just be copied onto
		// the equivalent menu items below, keeping both in lockstep with one source of truth
		private void DiffViewContextMenuRequested(object sender, DiffNode node)
		{
			var menu = node.NodeType == OneNote.NodeType.Page
				? BuildPageContextMenu()
				: BuildNodeContextMenu();

			menu.Show(Cursor.Position);
		}


		private MoreContextMenuStrip BuildNodeContextMenu()
		{
			var menu = new MoreContextMenuStrip();

			menu.Items.Add(new MoreMenuItem(Resx.CompareDialog_copyRight, null, CopyRightClick)
			{
				Enabled = copyRightButton.Enabled
			});
			menu.Items.Add(new MoreMenuItem(Resx.CompareDialog_copyLeft, null, CopyLeftClick)
			{
				Enabled = copyLeftButton.Enabled
			});
			menu.Items.Add(new MoreMenuItem(Resx.CompareDialog_mirrorRight, null, MirrorRightClick)
			{
				Enabled = mirrorRightButton.Enabled
			});
			menu.Items.Add(new MoreMenuItem(Resx.CompareDialog_mirrorLeft, null, MirrorLeftClick)
			{
				Enabled = mirrorLeftButton.Enabled
			});

			return menu;
		}


		private MoreContextMenuStrip BuildPageContextMenu()
		{
			var menu = new MoreContextMenuStrip();

			menu.Items.Add(new MoreMenuItem(Resx.CompareDialog_openLeft, null, OpenLeftClick)
			{
				Enabled = openLeftButton.Enabled
			});
			menu.Items.Add(new MoreMenuItem(Resx.CompareDialog_openRight, null, OpenRightClick)
			{
				Enabled = openRightButton.Enabled
			});
			menu.Items.Add(new ToolStripSeparator());
			menu.Items.Add(new MoreMenuItem(Resx.CompareDialog_copyRight, null, PageCopyRightClick)
			{
				Enabled = pageCopyRightButton.Enabled
			});
			menu.Items.Add(new MoreMenuItem(Resx.CompareDialog_copyLeft, null, PageCopyLeftClick)
			{
				Enabled = pageCopyLeftButton.Enabled
			});
			menu.Items.Add(new ToolStripSeparator());
			menu.Items.Add(new MoreMenuItem(Resx.CompareDialog_deleteLeft, null, DeleteLeftClick)
			{
				Enabled = deleteLeftButton.Enabled
			});
			menu.Items.Add(new MoreMenuItem(Resx.CompareDialog_deleteRight, null, DeleteRightClick)
			{
				Enabled = deleteRightButton.Enabled
			});
			menu.Items.Add(new MoreMenuItem(Resx.CompareDialog_deleteBoth, null, DeleteBothClick)
			{
				Enabled = deleteBothButton.Enabled
			});
			menu.Items.Add(new ToolStripSeparator());
			menu.Items.Add(new MoreMenuItem(Resx.CompareDialog_compareContents, null, CompareContentsClick)
			{
				Enabled = compareContentsButton.Enabled
			});

			return menu;
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


		private async void CopyRightClick(object sender, EventArgs e)
			=> await RunHierarchyAction(SyncDirection.LeftToRight, mirror: false);


		private async void CopyLeftClick(object sender, EventArgs e)
			=> await RunHierarchyAction(SyncDirection.RightToLeft, mirror: false);


		private async void MirrorRightClick(object sender, EventArgs e)
			=> await RunHierarchyAction(SyncDirection.LeftToRight, mirror: true);


		private async void MirrorLeftClick(object sender, EventArgs e)
			=> await RunHierarchyAction(SyncDirection.RightToLeft, mirror: true);


		private async Task RunHierarchyAction(SyncDirection direction, bool mirror)
		{
			var node = diffView.SelectedNode;
			if (node is null)
			{
				return;
			}

			var directionWord = direction == SyncDirection.LeftToRight
				? Resx.CompareDialog_directionRight
				: Resx.CompareDialog_directionLeft;

			string confirmMessage;
			if (mirror)
			{
				var count = HierarchyDiffSync.CountTargetOnly(node, direction);
				confirmMessage = count > 0
					? string.Format(Resx.CompareDialog_confirmMirror, node.Name, directionWord, count)
					: string.Format(Resx.CompareDialog_confirmMirrorNoDeletions, node.Name, directionWord);
			}
			else
			{
				confirmMessage = string.Format(Resx.CompareDialog_confirmCopy, node.Name, directionWord);
			}

			if (MoreMessageBox.ShowQuestion(this, confirmMessage) != DialogResult.Yes)
			{
				return;
			}

			Exception error = null;

			using (var progress = new ProgressDialog())
			{
				progress.SetMessage(mirror
					? Resx.CompareDialog_mirroringMessage
					: Resx.CompareDialog_copyingMessage);

				progress.ShowDialogWithCancel(async (dialog, token) =>
				{
					try
					{
						await using var one = new OneNote();

						if (mirror)
						{
							await HierarchyDiffSync.Mirror(one, node, direction);
						}
						else
						{
							await HierarchyDiffSync.Copy(one, node, direction);
						}

						return true;
					}
					catch (Exception exc)
					{
						error = exc;
						return false;
					}
				}, cancelable: false);
			}

			if (error is not null)
			{
				MoreMessageBox.ShowError(this, error.Message);
			}

			try
			{
				await RefreshDiff();
			}
			catch (Exception exc)
			{
				MoreMessageBox.ShowError(this, exc.Message);
			}
		}


		private async void PageCopyRightClick(object sender, EventArgs e)
			=> await RunPageCopy(SyncDirection.LeftToRight);


		private async void PageCopyLeftClick(object sender, EventArgs e)
			=> await RunPageCopy(SyncDirection.RightToLeft);


		private async Task RunPageCopy(SyncDirection direction)
		{
			var node = diffView.SelectedNode;
			if (node is null)
			{
				return;
			}

			var directionWord = direction == SyncDirection.LeftToRight
				? Resx.CompareDialog_directionRight
				: Resx.CompareDialog_directionLeft;

			var confirmMessage = string.Format(Resx.CompareDialog_confirmCopyPage, node.Name, directionWord);

			if (MoreMessageBox.ShowQuestion(this, confirmMessage) != DialogResult.Yes)
			{
				return;
			}

			Exception error = null;

			using (var progress = new ProgressDialog())
			{
				progress.SetMessage(Resx.CompareDialog_copyingMessage);

				progress.ShowDialogWithCancel(async (dialog, token) =>
				{
					try
					{
						await using var one = new OneNote();
						await HierarchyDiffSync.Copy(one, node, direction);
						return true;
					}
					catch (Exception exc)
					{
						error = exc;
						return false;
					}
				}, cancelable: false);
			}

			if (error is not null)
			{
				MoreMessageBox.ShowError(this, error.Message);
			}

			try
			{
				await RefreshDiff();
			}
			catch (Exception exc)
			{
				MoreMessageBox.ShowError(this, exc.Message);
			}
		}


		private async void DeleteLeftClick(object sender, EventArgs e)
			=> await RunPageDelete(DiffSide.Left);


		private async void DeleteRightClick(object sender, EventArgs e)
			=> await RunPageDelete(DiffSide.Right);


		private async void DeleteBothClick(object sender, EventArgs e)
			=> await RunPageDelete(DiffSide.Both);


		private async Task RunPageDelete(DiffSide side)
		{
			var node = diffView.SelectedNode;
			if (node is null)
			{
				return;
			}

			string confirmMessage;
			if (side == DiffSide.Both)
			{
				confirmMessage = string.Format(Resx.CompareDialog_confirmDeleteBothPage, node.Name);
			}
			else
			{
				var directionWord = side == DiffSide.Left
					? Resx.CompareDialog_directionLeft
					: Resx.CompareDialog_directionRight;

				confirmMessage = string.Format(Resx.CompareDialog_confirmDeletePage, node.Name, directionWord);
			}

			if (MoreMessageBox.ShowQuestion(this, confirmMessage) != DialogResult.Yes)
			{
				return;
			}

			using (var progress = new ProgressDialog())
			{
				progress.SetMessage(Resx.CompareDialog_deletingMessage);

				progress.ShowDialogWithCancel(async (dialog, token) =>
				{
					await using var one = new OneNote();

					// DeleteHierarchy logs and swallows its own failures rather than
					// throwing, matching HierarchyDiffSync's Mirror delete step
					if (side is DiffSide.Left or DiffSide.Both && node.LeftId is not null)
					{
						one.DeleteHierarchy(node.LeftId);
					}

					if (side is DiffSide.Right or DiffSide.Both && node.RightId is not null)
					{
						one.DeleteHierarchy(node.RightId);
					}

					return true;
				}, cancelable: false);
			}

			try
			{
				await RefreshDiff();
			}
			catch (Exception exc)
			{
				MoreMessageBox.ShowError(this, exc.Message);
			}
		}


		private async void CompareContentsClick(object sender, EventArgs e)
		{
			var node = diffView.SelectedNode;
			if (node?.LeftId is null || node.RightId is null)
			{
				return;
			}

			Exception error = null;
			SimilarityResult result = null;

			using (var progress = new ProgressDialog())
			{
				progress.SetMessage(Resx.CompareDialog_comparingMessage);

				progress.ShowDialogWithCancel(async (dialog, token) =>
				{
					try
					{
						await using var one = new OneNote();
						var left = await one.GetPage(node.LeftId);
						var right = await one.GetPage(node.RightId);
						result = SimilarityEngine.Compare(left, right, one);
						return true;
					}
					catch (Exception exc)
					{
						error = exc;
						return false;
					}
				}, cancelable: false);
			}

			if (error is not null)
			{
				MoreMessageBox.ShowError(this, error.Message);
				return;
			}

			if (result is null)
			{
				return;
			}

			var popup = new SimilarityPopup(node.Name, result);
			popup.RunModeless(GetPopupLocation(popup), (s, ev) =>
			{
				popup.Dispose();

				// the popup's own OnFormClosed (MoreForm) unconditionally hands foreground
				// focus back to OneNote on close, since that's correct for a dialog invoked
				// directly from OneNote; here it submerges this still-open dialog behind
				// OneNote instead, so re-elevate on top of it
				Elevate();
			});
		}


		// positions the popup's bottom-left corner just above the invoking button's
		// top-left, like a tooltip, clamped so it never renders off the top or right edge
		// of the screen the dialog is on
		private Point GetPopupLocation(SimilarityPopup popup)
		{
			var anchor = compareContentsButton.PointToScreen(Point.Empty);
			var size = popup.PreferredSize;

			var x = anchor.X;
			var y = anchor.Y - size.Height - 6;

			var working = Screen.FromControl(compareContentsButton).WorkingArea;
			x = Math.Min(x, working.Right - size.Width);
			x = Math.Max(x, working.Left);
			y = Math.Max(y, working.Top);

			return new Point(x, y);
		}


		private async Task RefreshDiff()
		{
			await using var one = new OneNote();

			var left = nodeType == OneNote.NodeType.Notebook
				? await one.GetNotebook(root.LeftId, OneNote.Scope.Pages)
				: await one.GetSection(root.LeftId);

			var right = nodeType == OneNote.NodeType.Notebook
				? await one.GetNotebook(root.RightId, OneNote.Scope.Pages)
				: await one.GetSection(root.RightId);

			root = HierarchyDiffBuilder.Build(left, right);
			diffView.SetRoot(root);
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
