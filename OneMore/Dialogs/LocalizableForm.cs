//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Dialogs
{
	using System.Linq;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class LocalizableForm : Form, IOneMoreWindow
	{

		protected static bool NeedsLocalizing()
		{
			return AddIn.Culture.TwoLetterISOLanguageName != "en";
		}


		protected void Localize(string[] keys)
		{
			foreach (var key in keys)
			{
				var control = Controls.Find(key, true).FirstOrDefault();
				if (control != null)
				{
					var resid = $"{Name}_{key}.Text";
					try
					{
						var text = Resx.ResourceManager.GetString(resid, AddIn.Culture);
						if (!string.IsNullOrEmpty(text))
						{
							control.Text = text;
						}
					}
					catch
					{
						Logger.Current.WriteLine($"Error translating resource {resid}");
					}
				}
			}
		}
	}
}
