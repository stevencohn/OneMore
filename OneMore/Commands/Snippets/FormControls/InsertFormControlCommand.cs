//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Inserts a form control hyperlink at the cursor, replacing any selected text. The
	/// argument is the control kind, FormControlCommand.DateKind or FormControlCommand.ListKind.
	/// A list control asks which of the user's lists it should offer.
	/// </summary>
	internal class InsertFormControlCommand : Command
	{
		public InsertFormControlCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var kind = args.Length > 0 ? args[0] as string : null;

			string anchor;
			if (kind == FormControlCommand.DateKind)
			{
				anchor = FormControlCommand.MakeAnchor(
					kind, MakeID(), Resx.FormControl_ChooseDate);
			}
			else if (kind == FormControlCommand.ListKind)
			{
				var name = ChooseList();
				if (string.IsNullOrEmpty(name))
				{
					return;
				}

				anchor = FormControlCommand.MakeAnchor(
					kind, MakeID(), string.Format(Resx.FormControl_ChooseList, name), name);
			}
			else
			{
				return;
			}

			var clipboard = new ClipboardProvider();
			await clipboard.StashState();

			try
			{
				await clipboard.Clear();
				if (!await clipboard.SetHtml(ClipboardProvider.WrapWithHtmlPreamble(anchor)))
				{
					ShowInfo(Resx.Clipboard_locked);
					return;
				}

				await using var one = new OneNote();
				Native.SetForegroundWindow(one.WindowHandle);
				await ClipboardProvider.Paste();
			}
			finally
			{
				await clipboard.RestoreState();
			}
		}


		private static string MakeID()
		{
			return Guid.NewGuid().ToString("N");
		}


		private string ChooseList()
		{
			string[] names;
			using (var provider = new FormControlsProvider())
			{
				names = provider.ReadLists().Select(l => l.Name).ToArray();
			}

			if (names.Length == 0)
			{
				ShowInfo(Resx.FormControl_NoLists);
				return null;
			}

			using var dialog = new ChooseFormListDialog(names);
			return dialog.ShowDialog(owner) == DialogResult.OK ? dialog.SelectedList : null;
		}
	}
}
