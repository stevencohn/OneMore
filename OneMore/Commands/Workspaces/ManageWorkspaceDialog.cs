//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Workspaces
{
	using System;
	using System.Drawing;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Which tab should be active when ManageWorkspaceDialog is shown.
	/// </summary>
	internal enum WorkspaceTab
	{
		Favorites,
		Layouts
	}


	/// <summary>
	/// Thin dialog wrapper hosting the Favorites and Layouts managers as tabs. Owns
	/// OK/Cancel; each tab's control owns its own editing logic and in-memory staging of
	/// changes.
	/// </summary>
	internal partial class ManageWorkspaceDialog : UI.MoreForm
	{
		private const int NoteVisibleMilliseconds = 2400;
		private const int NoteFadeSteps = 20;
		private const int NoteFadeStepMilliseconds = 30;

		private Color noteForeColor;
		private CancellationTokenSource noteAnimationCancellation;


		public ManageWorkspaceDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ManageWorkspaceDialog_Text;

				Localize(new string[]
				{
					"okButton=word_OK",
					"cancelButton=word_Cancel",
					"favoritesTab=word_Favorites",
					"layoutsTab=word_Layouts"
				});
			}

			managerControl.FavoritesChecked += ShowCheckCompletedNote;
			layoutsControl.LayoutsChecked += ShowCheckCompletedNote;
		}


		/// <summary>
		/// The tab to make active when the dialog is shown. Set by the caller, before
		/// ShowDialog, based on how the dialog was invoked.
		/// </summary>
		public WorkspaceTab ActiveTab { get; set; } = WorkspaceTab.Favorites;


		private void BindOnLoad(object sender, EventArgs e)
		{
			managerControl.LoadFavorites();
			layoutsControl.LoadLayouts();

			var index = ActiveTab == WorkspaceTab.Layouts ? 1 : 0;
			tabs.SelectedIndex = index;
			DefaultControl = index == 1 ? (Control)layoutsControl : managerControl;

			noteForeColor = noteLabel.ForeColor;
		}


		private async void ShowCheckCompletedNote(object sender, EventArgs e)
		{
			noteAnimationCancellation?.Cancel();
			var cancellation = new CancellationTokenSource();
			noteAnimationCancellation = cancellation;
			var token = cancellation.Token;

			noteLabel.ForeColor = noteForeColor;
			noteLabel.Visible = true;

			try
			{
				await Task.Delay(NoteVisibleMilliseconds, token);

				for (var i = 1; i <= NoteFadeSteps; i++)
				{
					noteLabel.ForeColor = Blend(noteForeColor, noteLabel.BackColor, (float)i / NoteFadeSteps);
					await Task.Delay(NoteFadeStepMilliseconds, token);
				}
			}
			catch (TaskCanceledException)
			{
				return;
			}

			noteLabel.Visible = false;
			noteLabel.ForeColor = noteForeColor;
		}


		private static Color Blend(Color from, Color to, float ratio)
		{
			return Color.FromArgb(
				from.R + (int)((to.R - from.R) * ratio),
				from.G + (int)((to.G - from.G) * ratio),
				from.B + (int)((to.B - from.B) * ratio));
		}


		private void OK(object sender, EventArgs e)
		{
			if (!managerControl.Save())
			{
				UI.MoreMessageBox.ShowError(this, "Could not save favorites");
				return;
			}

			if (!layoutsControl.Save())
			{
				UI.MoreMessageBox.ShowError(this, "Could not save layouts");
				return;
			}

			DialogResult = DialogResult.OK;
			Close();
		}


		private void ConfirmClosing(object sender, FormClosingEventArgs e)
		{
			noteAnimationCancellation?.Cancel();

			if (DialogResult == DialogResult.OK)
			{
				return;
			}

			if ((managerControl.IsDirty() || layoutsControl.IsDirty()) &&
				UI.MoreMessageBox.Show(this, Resx.ManageWorkspaceDialog_discard,
					MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
			{
				e.Cancel = true;
			}
		}
	}
}
