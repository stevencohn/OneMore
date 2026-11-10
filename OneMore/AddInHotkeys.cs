//************************************************************************************************
// Copyright © 2019 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Settings;
	using System.Threading.Tasks;
	using System.Windows.Forms;


	public partial class AddIn
	{
		/// <summary>
		/// Dynamically discover and register hotkey sequences for commands decorated with
		/// the CommandAttribute attribute
		/// </summary>
		/// <returns></returns>
		private async Task RegisterHotkeys()
		{
			await HotkeyManager.Initialize();

			// allow Settings to re-register after the user changes key bindings
			HotkeyManager.Rebuild = () =>
			{
				RegisterCommandHotkeys();
				RegisterMarkdownEnterHotkey();
			};

			RegisterCommandHotkeys();
		}


		private void RegisterCommandHotkeys()
		{
			// On non-US keyboards (e.g. Italian, UK) AltGr is Ctrl+Alt, so Ctrl+Alt+OemPlus
			// conflicts with AltGr+= which produces a square bracket on those layouts; that
			// default binding is skipped by KeyBindings.Load
			var count = 0;

			foreach (var binding in KeyBindings.Load())
			{
				if (binding.Keys == Keys.None || (binding.Keys & Keys.KeyCode) == Keys.None)
				{
					continue;
				}

				// all commands should be of the form:
				// public async Task NAME(IRibbonControl control)

				var method = binding.Method;
				if (method.GetParameters().Length > 0)
				{
					HotkeyManager.RegisterHotKey(async () =>
					{
						await (Task)method.Invoke(this, new object[] { null });
					},
					new Hotkey(binding.Keys));

					count++;
				}
			}

			logger.WriteLine($"Startup: defined {count} hotkeys");
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

			if (enabled)
			{
				HotkeyManager.RegisterHotKey(
					async () => await factory.Run<Commands.ConvertLineOnEnterCommand>(),
					new Hotkey(Keys.Enter));

				logger.WriteLine("Startup: registered markdown convert-on-Enter");
			}
		}
	}
}
