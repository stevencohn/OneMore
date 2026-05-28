//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Cli
{
	/// <summary>
	/// Marks a CLI command as requiring OneNote to be running as a full interactive UI
	/// process before execution. Some OneNote APIs — notably <c>Publish</c> — are only
	/// available when OneNote's UI-side services have been initialised, which does not
	/// happen when OneNote is activated purely as a headless COM server via
	/// <c>new Application()</c>. The CLI framework will launch <c>ONENOTE.EXE</c>
	/// interactively and wait for its main window to appear before invoking the command.
	/// </summary>
	public interface ICliInteractiveCommand : ICliCommand
	{
	}
}
