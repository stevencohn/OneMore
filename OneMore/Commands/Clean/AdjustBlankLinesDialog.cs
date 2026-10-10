//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant
#pragma warning disable IDE1006 // Words must begin with upper case

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	/// <summary>
	/// Prompts for how blank lines between paragraphs should be adjusted.
	/// </summary>
	internal partial class AdjustBlankLinesDialog : UI.MoreForm
	{
		private const string ModeKey = "mode";
		private const string HeadingKey = "headingBlank";


		public AdjustBlankLinesDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.AdjustBlankLinesDialog_Text;

				Localize(new string[]
				{
					"removeRadio",
					"keepRadio",
					"exactRadio",
					"headingBox",
					"noteLabel",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			var collection = new SettingsProvider().GetCollection(nameof(AdjustBlankLinesDialog));

			switch ((BlankLineMode)collection.Get(ModeKey, (int)BlankLineMode.KeepOne))
			{
				case BlankLineMode.RemoveAll:
					removeRadio.Checked = true;
					break;

				case BlankLineMode.ExactlyOne:
					exactRadio.Checked = true;
					break;

				default:
					keepRadio.Checked = true;
					break;
			}

			headingBox.Checked = collection.Get(HeadingKey, false);
		}


		public BlankLineMode Mode =>
			removeRadio.Checked ? BlankLineMode.RemoveAll :
			exactRadio.Checked ? BlankLineMode.ExactlyOne :
			BlankLineMode.KeepOne;


		public bool HeadingBlank => headingBox.Checked;


		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (DialogResult == DialogResult.OK)
			{
				// remember choices for next time
				var settings = new SettingsProvider();
				var collection = settings.GetCollection(nameof(AdjustBlankLinesDialog));
				collection.Add(ModeKey, (int)Mode);
				collection.Add(HeadingKey, HeadingBlank);
				settings.SetCollection(collection);
				settings.Save();
			}

			base.OnFormClosing(e);
		}
	}
}
