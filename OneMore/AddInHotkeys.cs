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
					Command = (CommandAttribute)m.Attributes[0]
				});

			// On non-US keyboards (e.g. Italian, UK) AltGr is Ctrl+Alt, so Ctrl+Alt+OemPlus
			// conflicts with AltGr+= which produces a square bracket on those layouts.
			// We skip registration of any OemPlus-based hotkey on non-US locales below,
			// but only for the default binding — if the user has configured a different key
			// we honor that choice regardless of locale.
			var locale = System.Threading.Thread.CurrentThread.CurrentCulture.KeyboardLayoutId;

			if (!methods.Any())
			{
				logger.WriteLine("Startup: no hotkey definitions found");
				await Task.Yield();
				return;
			}

			var settings = new SettingsProvider()
				.GetCollection(nameof(KeyboardSheet))?.Get<XElement>("commands");

			// register hotkey for each discovered command...

			await HotkeyManager.Initialize();
			var count = 0;

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

				// Skip OemPlus-based bindings on non-US keyboards to avoid AltGr conflict
				if (locale != en_US_Locale && hotkey?.Keys == Keys.Oemplus)
				{
					hotkey = null;
				}

				if (hotkey != null && hotkey.Keys != Keys.None)
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

						count++;
					}
				}
			});

			logger.WriteLine($"Startup: defined {count} hotkeys for input locale {locale}");
		}


		/// <summary>
		/// Registers a global hotkey for the bare Enter key that triggers live markdown
		/// conversion of the current line. This is a background behavior rather than a
		/// user-facing command, so it is registered directly here instead of through the
		/// CommandAttribute/RegisterHotkeys discovery mechanism, keeping it out of the
		/// Keyboard settings sheet, Command Palette, and wiki keyboard reference.
		/// </summary>
		/// <remarks>
		/// This is an experimental feature, off by default; its checkbox on the Markdown
		/// settings sheet is itself only shown when the General sheet's experimental
		/// features flag is enabled. See MarkdownSheet.
		/// </remarks>
		private void RegisterMarkdownEnterHotkey()
		{
			var enabled = new SettingsProvider()
				.GetCollection(nameof(MarkdownSheet)).Get("convertOnEnter", false);

			if (!enabled)
			{
				return;
			}

			HotkeyManager.RegisterHotKey(
				async () => await factory.Run<Commands.ConvertLineOnEnterCommand>(),
				new Hotkey(Keys.Enter));

			logger.WriteLine("Startup: registered markdown convert-on-Enter hotkey");
		}
	}
}
