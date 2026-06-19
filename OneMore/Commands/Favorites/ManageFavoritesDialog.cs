//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Favorites
{
	using System;
	using System.Drawing;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Thin dialog wrapper around FavoritesManagerControl. Owns OK/Cancel; the control owns
	/// all favorites/folder editing logic and the in-memory staging of changes.
	/// </summary>
	internal partial class ManageFavoritesDialog : UI.MoreForm
	{
		private const int NoteVisibleMilliseconds = 2400;
		private const int NoteFadeSteps = 20;
		private const int NoteFadeStepMilliseconds = 30;

		private Color noteForeColor;
		private CancellationTokenSource noteAnimationCancellation;


		public ManageFavoritesDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ManageFavoritesDialog_Text;

				Localize(new string[]
				{
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			DefaultControl = managerControl;
			managerControl.FavoritesChecked += ShowCheckCompletedNote;
		}


		private void BindOnLoad(object sender, EventArgs e)
		{
			managerControl.LoadFavorites();
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

			if (managerControl.IsDirty())
			{
				if (UI.MoreMessageBox.Show(this, Resx.ManageFavoritesDialog_discard,
					MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
				{
					e.Cancel = true;
				}
			}
		}
	}
}
