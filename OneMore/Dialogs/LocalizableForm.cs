//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Dialogs
{
	using System;
	using System.ComponentModel;
	using System.Linq;
	using System.Reflection;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class LocalizableForm : Form, IOneMoreWindow
	{

		protected static bool NeedsLocalizing()
		{
			return AddIn.Culture.TwoLetterISOLanguageName != "en";
		}


		/// <summary>
		/// Set the Text property, or specified property, for each named control.
		/// </summary>
		/// <param name="keys">
		/// A string array; each item is the name of the control to set the Text property, or can
		/// specified the controlName.propertyName to set a named property rather than Text
		/// </param>
		protected void Localize(string[] keys)
		{
			foreach (var key in keys)
			{
				// named control and default Text property
				var k = key;
				var p = "Text";

				var dot = key.IndexOf('.');
				if (dot > 0)
				{
					// specific named property for this control
					k = key.Substring(0, dot);
					p = key.Substring(dot + 1);
				}

				// Name will be the dialog class name, k is the control name, p is the property
				var resid = $"{Name}_{k}.{p}";
				string text;

				try
				{
					text = Resx.ResourceManager.GetString(resid, AddIn.Culture);
					if (string.IsNullOrEmpty(text))
					{
						Logger.Current.WriteLine($"resource not found {resid}");
						return;
					}
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine($"error loading resource {resid}", exc);
					return;
				}

				var control = Controls.Find(k, true).FirstOrDefault();
				if (control != null)
				{
					var prop = control.GetType().GetProperty(p);
					if (prop != null)
						prop.SetValue(control, text, null);
					else
						Logger.Current.WriteLine($"cannot find control property {k}.{p}");
				}
				else
				{
					var bindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
					var component = GetType().GetField(k, bindings)?.GetValue(this);

					if (component is Component comp)
					{
						var prop = comp.GetType().GetProperty(p, bindings);
						if (prop != null)
							prop.SetValue(comp, text, null);
						else
							Logger.Current.WriteLine($"cannot find Component property {k}.{p}");
					}
					else if (component is TreeNode node)
					{
						var prop = node.GetType().GetProperty(p, bindings);
						if (prop != null)
							prop.SetValue(node, text, null);
						else
							Logger.Current.WriteLine($"cannot find TreeNode property {k}.{p}");
					}
					else
					{
						Logger.Current.WriteLine($"cannot translate {k}, name not found");
					}
				}
			}
		}
	}
}
