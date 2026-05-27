//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Cli
{
	using System.Threading.Tasks;

	/// <summary>
	/// Implemented by command classes that can be discovered and driven by a CLI host.
	/// The host calls <see cref="DefineParameters"/> to learn what inputs are needed,
	/// prompts the user for each, validates values against declared constraints, then
	/// calls <see cref="CLIExecute"/> with a fully populated <see cref="CliParameterSet"/>.
	/// <para>
	/// CLI commands are standalone classes; they do not extend the ribbon <c>Command</c>
	/// base class and are not wired to the AddIn ribbon. Discovery is performed at runtime
	/// via reflection over all types in the assembly that implement this interface.
	/// </para>
	/// </summary>
	public interface ICliCommand
	{
		/// <summary>
		/// Gets a short, machine-friendly name that uniquely identifies this command,
		/// e.g. <c>"ExportSection"</c>. Used as the command key in menus and automation.
		/// </summary>
		string CommandName { get; }

		/// <summary>
		/// Gets a one-line human-readable description displayed in the CLI command menu.
		/// </summary>
		string Description { get; }

		/// <summary>
		/// Returns the ordered set of parameter descriptors for this command.
		/// Called once by the CLI host before it begins prompting the user.
		/// </summary>
		CliParameterDefinition DefineParameters();

		/// <summary>
		/// Executes the command using fully validated, typed parameter values
		/// collected by the CLI host.
		/// </summary>
		/// <param name="parameters">
		/// A value bag populated by the host. Each parameter declared in
		/// <see cref="DefineParameters"/> will be present and valid.
		/// </param>
		Task CLIExecute(CliParameterSet parameters);
	}
}
