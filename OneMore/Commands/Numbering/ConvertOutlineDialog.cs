//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.Styles;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class ConvertOutlineDialog : UI.MoreForm
	{
		public ConvertOutlineDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ConvertOutlineDialog_Text;

				Localize(new string[]
				{
					"depthLabel",
					"themeLabel",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			themeBox.Items.Add(Resx.ConvertOutlineDialog_themeBox_BuiltIn);
			themeBox.Items.Add(new ThemeProvider().Theme.Name);

			RestoreSettings();
		}


		private void RestoreSettings()
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection("convertOutline");

			depthBox.Value = settings?.Get("depth", 1) ?? 1;
			themeBox.SelectedIndex = settings?.Get("theme", 0) ?? 0;
		}


		/// <summary>
		/// Gets the number of top levels to convert to headings, 1..6
		/// </summary>
		public int Depth => (int)depthBox.Value;


		/// <summary>
		/// Gets a Boolean indicating whether to use the currently loaded custom style
		/// theme instead of the built-in OneNote heading styles
		/// </summary>
		public bool UseCustomTheme => themeBox.SelectedIndex == 1;


		private void okButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.OK;

			var settings = new SettingsCollection("convertOutline");
			settings.Add("depth", Depth);
			settings.Add("theme", themeBox.SelectedIndex);

			var provider = new SettingsProvider();
			provider.SetCollection(settings);
			provider.Save();
		}
	}
}
