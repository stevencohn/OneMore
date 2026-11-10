//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Windows.Forms;
	using System.Xml.Linq;


	/// <summary>
	/// Resolves the effective key binding of each command, combining the defaults declared
	/// by CommandAttribute with user overrides from the Keyboard settings sheet. Also provides
	/// display text for ribbon screentips.
	/// </summary>
	internal static class KeyBindings
	{
		private const int en_US_Locale = 1033;

		// captured on first use, which happens at startup on the add-in thread, so that later
		// calls from other threads (e.g. Settings dialog) use the same keyboard layout
		private static readonly int layout =
			System.Threading.Thread.CurrentThread.CurrentCulture.KeyboardLayoutId;

		private static readonly object locker = new();
		private static Dictionary<string, string> labels;


		internal sealed class Binding
		{
			public MethodInfo Method { get; set; }
			public CommandAttribute Command { get; set; }

			/// <summary>
			/// The effective keys including modifiers; Keys.None if unbound
			/// </summary>
			public Keys Keys { get; set; }
		}


		/// <summary>
		/// Loads the effective binding of every command method decorated with CommandAttribute,
		/// reading user overrides fresh from settings
		/// </summary>
		public static List<Binding> Load()
		{
			var settings = new SettingsProvider()
				.GetCollection(KeyboardSheet.CollectionName)?
				.Get<XElement>(KeyboardSheet.SettingsName);

			var bindings = new List<Binding>();

			foreach (var method in typeof(AddIn).GetMethods())
			{
				var attribute = (CommandAttribute)method.GetCustomAttribute(
					typeof(CommandAttribute), false);

				if (attribute is null)
				{
					continue;
				}

				var keys = attribute.DefaultKeys;

				var setting = settings?.Elements("command")
					.FirstOrDefault(e => e.Attribute("command")?.Value == method.Name);

				if (setting is not null &&
					Enum.TryParse<Keys>(setting.Attribute("keys")?.Value, true, out var custom))
				{
					// Back means the user deliberately cleared the binding
					keys = (custom & Keys.KeyCode) == Keys.Back ? Keys.None : custom;
				}
				else if ((keys & Keys.KeyCode) == Keys.Oemplus && layout != en_US_Locale)
				{
					// Ctrl+Alt is AltGr on non-US keyboards and conflicts with AltGr+=; this
					// applies only to default bindings, a user-chosen key is always honored
					keys = Keys.None;
				}

				bindings.Add(new Binding { Method = method, Command = attribute, Keys = keys });
			}

			return bindings;
		}


		/// <summary>
		/// Gets the display text of the binding for the given ribbon control, e.g. "Ctrl+E",
		/// or null if the control has no command or no binding
		/// </summary>
		/// <param name="controlId">The ribbon control ID, e.g. ribSearchButton</param>
		public static string GetText(string controlId)
		{
			lock (locker)
			{
				if (labels is null)
				{
					var map = new Dictionary<string, string>();

					foreach (var binding in Load())
					{
						if (binding.Keys == Keys.None ||
							!binding.Command.ResID.EndsWith("_Label"))
						{
							continue;
						}

						var text = new Hotkey(binding.Keys).ToString();
						if (text.Length > 0)
						{
							var id = binding.Command.ResID.Substring(
								0, binding.Command.ResID.Length - "_Label".Length);

							map[id] = text;
						}
					}

					labels = map;
				}

				return labels.TryGetValue(controlId, out var value) ? value : null;
			}
		}


		/// <summary>
		/// Discards cached display text so it is rebuilt from current settings
		/// </summary>
		public static void Reset()
		{
			lock (locker)
			{
				labels = null;
			}
		}
	}
}
