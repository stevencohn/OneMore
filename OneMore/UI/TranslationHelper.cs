//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************	

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.ComponentModel;
	using System.Linq;
	using System.Reflection;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal static class TranslationHelper
	{
		/// <summary>
		/// Determines if the main OneNote thread culture differs from our default design-time
		/// language, English. If true, then the Localize method should be called.
		/// </summary>
		/// <returns></returns>
		public static bool NeedsLocalizing()
		{
			return AddIn.Culture.TwoLetterISOLanguageName != "en";
		}


		/// <summary>
		/// Set the Text property, or specified property, for each named control.
		/// </summary>
		/// <param name="keys">
		/// A string array, each item can be one of these formats:
		///   - "control"            sets the Text property of the named control
		///   - "control.prop"       sets the prop property of the named control
		///   - "control=resid"      sets the Text property of the named control to resid
		///   - "control.prop=resid" sets the prop property of the named control to resid
		/// resid override can be used to target common word_ phrases
		/// </param>
		public static void Localize(ContainerControl owner, string[] keys)
		{
			foreach (var key in keys)
			{
				var controlName = key;
				var propName = "Text";

				string resid = null;
				var marker = controlName.IndexOf('=');
				if (marker > 0)
				{
					// override with explicit resid
					resid = controlName.Substring(marker + 1);
					controlName = controlName.Substring(0, marker);
				}

				marker = controlName.IndexOf('.');
				if (marker > 0)
				{
					// override property name
					propName = controlName.Substring(marker + 1);
					controlName = controlName.Substring(0, marker);
				}

				if (resid == null)
				{
					// default resid form if not explicitly overriden
					resid = $"{owner.Name}_{controlName}.{propName}";
				}

				string text;
				try
				{
					text = Resx.ResourceManager.GetString(resid, AddIn.Culture);
					if (string.IsNullOrEmpty(text))
					{
						Logger.Current.WriteLine($"resource not found {resid}");
						continue;
					}
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine($"error loading resource {resid}", exc);
					continue;
				}

				var control = owner.Controls.Find(controlName, true).FirstOrDefault();
				if (control != null)
				{
					if (control is ComboBox box)
					{
						box.Items.Clear();
						box.Items.AddRange(text.Split(
							// for some Github cloners, multiline items in the Resx file are
							// delimeted wth only CR instead of NLCR so allow for any possibility
							new string[] { Environment.NewLine, "\r", "\n" },
							StringSplitOptions.RemoveEmptyEntries));
					}
					else
					{
						var prop = control.GetType().GetProperty(propName);
						if (prop != null)
						{
							//logger.WriteLine($"resx {controlName}.{propName} = {resid} = {text}");
							prop.SetValue(control, text, null);
						}
						else
							Logger.Current.WriteLine($"cannot find control property {controlName}.{propName}");
					}
				}
				else
				{
					var bindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
					var component = owner.GetType().GetField(controlName, bindings)?.GetValue(owner);

					if (component is Component comp)
					{
						var prop = comp.GetType().GetProperty(propName, bindings);
						if (prop != null)
							prop.SetValue(comp, text, null);
						else
							Logger.Current.WriteLine($"cannot find Component property {controlName}.{propName}");
					}
					else if (component is TreeNode node)
					{
						var prop = node.GetType().GetProperty(propName, bindings);
						if (prop != null)
							prop.SetValue(node, text, null);
						else
							Logger.Current.WriteLine($"cannot find TreeNode property {controlName}.{propName}");
					}
					else
					{
						Logger.Current.WriteLine($"cannot translate {controlName}, name not found");
					}
				}
			}
		}
	}
}
