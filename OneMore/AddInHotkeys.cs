//************************************************************************************************
// Copyright © 2019 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	public partial class AddIn
	{
		private const int en_US_Locale = 1033;


		/// <summary>
		/// Dynamically discover and register hotkey sequences for commands decorated with
		/// the CommandAttribute attribute
		/// </summary>
		/// <returns></returns>
		private async Task RegisterHotkeys()
		{
			// discover all command methods with CommandAttribute...

			var methods = typeof(AddIn).GetMethods()
				.Select(m => new 
				{ 
					Method = m, 
					Attributes = m.GetCustomAttributes(typeof(CommandAttribute), false)
				})
				.Where(m => m.Attributes.Any())
				.Select(m => new 
				{
					m.Method,
					Command = (CommandAttribute)m.Attributes.First()
				});

			// an awful hack to avoid a conflict with Italian keyboard (FIGS and likely UK) that
			// use AltGr as Ctrl+Alt. This means users pressing AltGr+OemPlus to get a square
			// bracket would instead end up increasing the font size of the page when they didn't
			// mean to! So here we only register these hot keys for the US keyboard input layout.
			var locale = System.Threading.Thread.CurrentThread.CurrentCulture.KeyboardLayoutId;
			if (locale != en_US_Locale)
			{
				methods = methods.Where(m => m.Method.Name != nameof(IncreaseFontSizeCmd));
			}

			if (!methods.Any())
			{
				logger.WriteLine("no hotkey definitions found");
				await Task.Yield();
				return;
			}

			logger.WriteLine($"defining {methods.Count()} hotkeys for input locale {locale}");

			var settings = new SettingsProvider()
				.GetCollection("KeyboardSheet")?.Get<XElement>("commands");

			// register hotkey for each discovered command...

			HotkeyManager.Initialize();

			methods.ForEach((m) =>
			{
				var hotkey = new Hotkey(m.Command.DefaultKeys);

				// look for user override for the command
				var setting = settings?.Elements("command")
					.FirstOrDefault(e => e.Attribute("command")?.Value == m.Method.Name);

				if (setting != null)
				{
					if (Enum.TryParse<Keys>(setting.Attribute("keys")?.Value, true, out var keys))
					{
						// has user deliberately cleared command binding (?HasFlag doesn't work?)
						if ((keys & Keys.KeyCode) == Keys.Back)
						{
							hotkey = null;
						}
						// has user overridden default binding
						else if ((hotkey.Keys & hotkey.Modifiers) != keys)
						{
							hotkey = new Hotkey(keys);
						}
					}
				}

				if (hotkey != null)
				{
					// all commands should be of the form:
					// public async Task NAME(IRibbonControl control)

					var parameterType = m.Method.GetParameters().FirstOrDefault()?.GetType();
					if (parameterType != null)
					{
						HotkeyManager.RegisterHotKey(async () =>
						{
							await (Task)m.Method.Invoke(this, new object[] { null });

						},
						hotkey);
					}
				}
			});
		}
	}
}
