//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Dialogs
{
	using System.Linq;
	using System.Threading;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class LocalizableForm : Form, IOneMoreWindow
	{

		protected static bool NeedsLocalizing()
		{
			return Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName != "en";
		}


		protected void Localize(string[] keys)
		{
			foreach (var key in keys)
			{
				var control = Controls.Find(key, true).FirstOrDefault();
				if (control != null)
				{
					control.Text = Resx.ResourceManager.GetString($"{Name}_{key}.Text");
				}
			}
		}
	}
}
