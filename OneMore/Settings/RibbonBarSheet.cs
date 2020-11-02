//************************************************************************************************
// Copyright © 2020 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	internal partial class RibbonBarSheet : SheetBase
	{
		public RibbonBarSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Title = "Ribbon Bar Options";
		}


		public override void CollectSettings()
		{
			//
		}
	}
}
