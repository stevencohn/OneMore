//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Dialogs
{
	using System.Threading;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class LocalizableForm : Form, IOneMoreWindow
	{

		public bool NeedsLocalizing()
		{
			return Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName != "en";
		}


		public void Localize(string[] keys)
		{
			foreach (var key in keys)
			{
				if (Controls.ContainsKey(key))
				{
					Controls[key].Text = Resx.ResourceManager.GetString($"{Name}_{key}.Text");
				}
			}
		}
	}
}
