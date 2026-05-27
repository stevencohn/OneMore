//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Cli
{


	/*
	internal sealed class MyCommand : Command, ICliCommand
	{
		public string CommandName => "MyCommand";
		public string Description  => "Does something useful.";

		public CliParameterDefinition DefineParameters() => new CliParameterDefinition()
			.AddString ("inputPath",  "Source file to process",  required: true)
			.AddEnum   ("mode",       "Processing mode",
						new[] { "Fast", "Thorough" },           defaultValue: "Fast")
			.AddInteger("retries",    "Max retry attempts",
						minimum: 0, maximum: 10,                required: false, defaultValue: 3);

		public override async Task Execute(params object[] args)
		{
			if (runningFromCli)
			{
				var parameters = args.Length > 0 ? args[0] as CliParameterSet : null;
				var path    = parameters?.Get<string>("inputPath");
				var mode    = parameters?.Get<string>("mode");
				var retries = parameters?.Get<int>("retries") ?? 0;
				// ...
				return;
			}

			// ribbon execution path
		}
	}
	*/

	/// <summary>
	/// Implemented by command classes that can be discovered and driven by a CLI host.
	/// The host calls <see cref="DefineParameters"/> to learn what inputs are needed,
	/// prompts the user for each, validates values against declared constraints, then
	/// invokes the command through <see cref="CommandFactory"/> which injects the standard
	/// protected fields and passes the populated <see cref="CliParameterSet"/> as
	/// <c>args[0]</c> to <c>Execute</c>.
	/// <para>
	/// CLI commands extend the ribbon <c>Command</c> base class and implement this interface.
	/// Inside <c>Execute</c> use the <c>runningFromCli</c> guard to branch between the CLI
	/// and ribbon code paths. Discovery is performed at runtime via reflection over all types
	/// in the assembly that implement this interface.
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
	}
}
