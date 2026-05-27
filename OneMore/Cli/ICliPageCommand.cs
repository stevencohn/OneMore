//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Cli
{
	/// <summary>
	/// Marks a CLI command as operating on individual pages resolved from a notebook/section/page
	/// path. The CLI framework resolves the path to one or more page IDs via
	/// <see cref="OneNote.FindPagesByPath"/> and invokes the command once per page, injecting the
	/// resolved <c>pageId</c> into the <see cref="CliParameterSet"/> before each call.
	/// <para>
	/// Commands that implement this interface must declare <c>notebook</c> and <c>section</c>
	/// parameters in <see cref="ICliCommand.DefineParameters"/>. A <c>page</c> parameter is
	/// optional; when omitted or set to <c>*</c> every page in the section is processed.
	/// </para>
	/// </summary>
	public interface ICliPageCommand : ICliCommand
	{
	}
}
