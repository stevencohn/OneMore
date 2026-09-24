//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using River.OneMoreAddIn;
	using System;
	using System.ComponentModel;
	using System.Linq;
	using System.Reflection;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Applies strings from Properties\Resources.resx to designer-created controls. This is a
	/// simplified version of OneMore's UI\Translator that reads the Calendar's own resources
	/// using the current UI culture.
	/// </summary>
	internal static class Translator
	{
		/// <summary>
		/// Set the Text property, or specified property, for each named control or component.
		/// </summary>
		/// <param name="owner">The form or control that hosts the named controls</param>
		/// <param name="keys">
		/// A string array, each item can be one of these formats:
		///   - "control"            sets the Text property of the named control
		///   - "control.prop"       sets the prop property of the named control
		///   - "control=resid"      sets the Text property of the named control to resid
		///   - "control.prop=resid" sets the prop property of the named control to resid
		/// The special name "this" targets the owner itself, e.g. its title bar Text.
		/// The default resid is "Owner_control.prop", or "Owner.prop" for "this", where Owner is
		/// the type name of the owner.
		/// </param>
		public static void Localize(ContainerControl owner, string[] keys)
		{
			var ownerName = owner.GetType().Name;

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

				// default resid form if not explicitly overriden
				resid ??= controlName == "this"
					? $"{ownerName}.{propName}"
					: $"{ownerName}_{controlName}.{propName}";

				string text;
				try
				{
					text = Resx.ResourceManager.GetString(resid, Resx.Culture);
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

				var target = FindTarget(owner, controlName);
				if (target is null)
				{
					Logger.Current.WriteLine($"cannot translate {controlName}, name not found");
					continue;
				}

				var bindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
				var prop = target.GetType().GetProperty(propName, bindings);
				if (prop is null)
				{
					Logger.Current.WriteLine($"cannot find property {controlName}.{propName}");
					continue;
				}

				prop.SetValue(target, text, null);
			}
		}


		private static object FindTarget(ContainerControl owner, string name)
		{
			if (name == "this")
			{
				return owner;
			}

			var control = owner.Controls.Find(name, true).FirstOrDefault();
			if (control is not null)
			{
				return control;
			}

			// components that are not Controls, such as ToolStripItems, are only fields
			var bindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			return owner.GetType().GetField(name, bindings)?.GetValue(owner) as Component;
		}
	}
}
