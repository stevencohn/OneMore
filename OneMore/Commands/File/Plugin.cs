//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Web.Script.Serialization;


	/// <summary>
	/// Specifies the properties needed to describe a plugin, its arguments, and options.
	/// </summary>
	internal class Plugin
	{
		public const int SchemaVersion = 2;
		public const int DefaultTimeout = 15;
		public const int MaxTimeout = 3600;


		/// <summary>
		/// Gets or sets the schema version of the plugin record
		/// </summary>
		public int Version { get; set; } = SchemaVersion;


		/// <summary>
		/// Preserve name while editing; used to check for renaming
		/// </summary>
		[ScriptIgnore]
		public string OriginalName { get; set; }

		/// <summary>
		/// Gets or sets the path to the plugin stored file
		/// </summary>
		[ScriptIgnore]
		public string Path { get; set; }


		/// <summary>
		/// Gets the full command and arguments as a string
		/// </summary>
		[ScriptIgnore]
		public string FullCommand => $"{Command} {Arguments}";


		/// <summary>
		/// Gets or sets the name of this plugin.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the plugin command. This could be a standalone script or executable.
		/// Or it could be a driver executable (python.exe) that accepts a script as 
		/// specified by the Arguments property.
		/// </summary>
		public string Command { get; set; }


		/// <summary>
		/// Gets or sets the command-line arguments to pass to the Command.
		/// </summary>
		public string Arguments { get; set; }


		/// <summary>
		/// Gets or sets whether this plugin target a page or a hierarchy.
		/// </summary>
		public bool TargetPage { get; set; } = true;


		/// <summary>
		/// Gets or sets a value indicating whether to create a new page based on
		/// the output of Command processing the current page.
		/// </summary>
		public bool CreateNewPage { get; set; }


		/// <summary>
		/// Gets or sets a value indicating whether to create the new page as a child of 
		/// the current page; this only applies if CreateNewPage is true.
		/// </summary>
		public bool AsChildPage { get; set; }


		/// <summary>
		/// Gets or sets the name of the new page to create, if CreateNewPage is true.
		/// </summary>
		public string PageName { get; set; }


		/// <summary>
		/// Gets or sets a value indicating whether to skip or fail locked sections.
		/// </summary>
		public bool SkipLocked { get; set; }


		/// <summary>
		/// Gets or sets the timeout of the plugin.
		/// Can be set to 0 for no timeout or from 1 to 300 seconds (5 mins)
		/// </summary>
		public int Timeout { get; set; } = DefaultTimeout;
	}
}
