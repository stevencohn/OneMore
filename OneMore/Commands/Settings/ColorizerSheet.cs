//************************************************************************************************
// Copyright © 2023 teven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using River.OneMoreAddIn.Styles;
	using Resx = Properties.Resources;


	internal partial class ColorizerSheet : SheetBase
	{
		public ColorizerSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Name = nameof(ColorizerSheet);
			Title = Resx.ColorizeSheet_Title;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"applyBox",
					"fontLabel",
					"font2Label",
					"fixedBox"
				});
			}

			familyBox.LoadFontFamilies();
			family2Box.LoadFontFamilies();

			if (AddIn.Culture.NumberFormat.NumberDecimalSeparator != ".")
			{
				for (int i = 0; i < sizeBox.Items.Count; i++)
				{
					var text = sizeBox.Items[i].ToString()
						.Replace(".", AddIn.Culture.NumberFormat.NumberDecimalSeparator);

					sizeBox.Items[i] = text;
					size2Box.Items[i] = text;
				}
			}

			var settings = provider.GetCollection(Name);

			applyBox.Checked = settings.Get("apply", false);

			var family = settings.Get("family", StyleBase.DefaultCodeFamily);
			familyBox.SelectedIndex = familyBox.Items.IndexOf(family);

			var size = settings.Get("size", StyleBase.DefaultCodeSize);
			sizeBox.SelectedIndex = sizeBox.Items.IndexOf(size.ToString());

			family = settings.Get("family2", StyleBase.DefaultCodeFamily);
			family2Box.SelectedIndex = family2Box.Items.IndexOf(family);

			size = settings.Get("size2", StyleBase.DefaultCodeSize);
			size2Box.SelectedIndex = size2Box.Items.IndexOf(size.ToString());

			applyBox.Focus();
		}


		private void FilterFontsOnCheckedChanged(object sender, System.EventArgs e)
		{
			familyBox.Items.Clear();
			familyBox.LoadFontFamilies(fixedBox.Checked);

			family2Box.Items.Clear();
			family2Box.LoadFontFamilies(fixedBox.Checked);
		}


		public override bool CollectSettings()
		{
			var settings = provider.GetCollection(Name);

			if (applyBox.Checked)
			{
				settings.Add("apply", true);
				settings.Add("family", familyBox.Text);
				settings.Add("size", sizeBox.Text);
				settings.Add("family2", family2Box.Text);
				settings.Add("size2", size2Box.Text);
				provider.SetCollection(settings);
			}
			else
			{
				provider.RemoveCollection(Name);
			}

			// does not require a restart
			return false;
		}
	}
}
