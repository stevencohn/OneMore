//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using Resx = Properties.Resources;


	internal partial class FileImportSheet : SheetBase
	{
		private const decimal DefaultWidth = 600M;

		public FileImportSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Name = nameof(FileImportSheet);
			Title = Resx.FileImportSheet_Title;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"widthLabel"
				});
			}

			widthBox.Value = provider.GetCollection(Name).Get("width", DefaultWidth);
		}


		public override bool CollectSettings()
		{
			var settings = provider.GetCollection(Name);
			settings.Add("width", widthBox.Value.ToString());
			provider.SetCollection(settings);

			// restart not required
			return false;
		}
	}
}
