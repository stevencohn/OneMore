//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Web.Script.Serialization;


	internal enum PluginTarget
	{
		Page,           // Current page
		Section,        // Current section and its pages
		Notebook,       // Current notebook and its sections
		NotebookPages,  // Current notebook and its pages
		Notebooks       // All notebooks and their pages
	}


	/// <summary>
	/// Specifies the properties needed to describe a plugin, its arguments, and options.
	/// </summary>
	internal class Plugin : INotifyPropertyChanged
	{
		public const int SchemaVersion = 3;
		public const int DefaultTimeout = 15;
		public const int MaxTimeout = 3600;

		private string name;
		private string command;
		private string arguments;
		private string userArguments;
		private PluginTarget target;
		private bool createNewPage;
		private bool asChildPage;
		private string pageName;
		private bool skipLocked;
		private int timeout;


		public Plugin()
		{
			Version = SchemaVersion;
			timeout = DefaultTimeout;
		}


		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="source">The original source to copy</param>
		public Plugin(Plugin source)
		{
			Version = source.Version;
			OriginalName = source.OriginalName;
			Path = source.Path;
			Name = source.Name;
			Command = source.Command;
			Arguments = source.Arguments;
			UserArguments = source.UserArguments;
			Target = source.Target;
			CreateNewPage = source.CreateNewPage;
			AsChildPage = source.AsChildPage;
			PageName = source.PageName;
			SkipLocked = source.SkipLocked;
			Timeout = source.Timeout;
		}


		/// <summary>
		/// Gets or sets the schema version of the plugin record
		/// </summary>
		public int Version { get; set; }


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
		public string Name
		{
			get => name;
			set => SetField(ref name, value, nameof(Name));
		}

		/// <summary>
		/// Gets or sets the plugin command. This could be a standalone script or executable.
		/// Or it could be a driver executable (python.exe) that accepts a script as 
		/// specified by the Arguments property.
		/// </summary>
		public string Command
		{
			get => command;
			set => SetField(ref command, value, nameof(Command));
		}



		/// <summary>
		/// Gets or sets the command-line arguments to pass to the Command.
		/// </summary>
		public string Arguments
		{
			get => arguments;
			set => SetField(ref arguments, value, nameof(Arguments));
		}


		/// <summary>
		/// Gest or sets additional user arguments to pass to the Command.
		/// These are appended after the Arguments value.
		/// </summary>
		public string UserArguments
		{
			get => userArguments;
			set => SetField(ref userArguments, value, nameof(UserArguments));
		}


		/// <summary>
		/// Gets or set the scope of the hierarchy information to fetch.
		/// </summary>
		public PluginTarget Target
		{
			get => target;
			set => SetField(ref target, value, nameof(Target));
		}



		/// <summary>
		/// Gets or sets whether this plugin target a page or a hierarchy.
		/// </summary>
		[Obsolete("Use Plugin.Target instead. Here just for backwards-compatibility")]
		public bool TargetPage { get; set; } = true;


		/// <summary>
		/// Gets or sets a value indicating whether to create a new page based on
		/// the output of Command processing the current page.
		/// </summary>
		public bool CreateNewPage
		{
			get => createNewPage;
			set => SetField(ref createNewPage, value, nameof(CreateNewPage));
		}



		/// <summary>
		/// Gets or sets a value indicating whether to create the new page as a child of 
		/// the current page; this only applies if CreateNewPage is true.
		/// </summary>
		public bool AsChildPage
		{
			get => asChildPage;
			set => SetField(ref asChildPage, value, nameof(AsChildPage));
		}



		/// <summary>
		/// Gets or sets the name of the new page to create, if CreateNewPage is true.
		/// </summary>
		public string PageName
		{
			get => pageName;
			set => SetField(ref pageName, value, nameof(PageName));
		}



		/// <summary>
		/// Gets or sets a value indicating whether to skip or fail locked sections.
		/// </summary>
		public bool SkipLocked
		{
			get => skipLocked;
			set => SetField(ref skipLocked, value, nameof(SkipLocked));
		}



		/// <summary>
		/// Gets or sets the timeout of the plugin.
		/// Can be set to 0 for no timeout or from 1 to 300 seconds (5 mins)
		/// </summary>
		public int Timeout
		{
			get => timeout;
			set => SetField(ref timeout, value, nameof(Timeout));
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		public event PropertyChangedEventHandler PropertyChanged;


		protected void OnPropertyChanged(string propertyName) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


		protected bool SetField<T>(ref T field, T value, string propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(field, value))
			{
				return false;
			}

			field = value;
			OnPropertyChanged(propertyName ?? string.Empty);
			return true;
		}


		public override bool Equals(object obj)
		{
			if (obj is Plugin source)
			{
				if (Version != source.Version) return false;
				if (OriginalName != source.OriginalName) return false;
				if (Path != source.Path) return false;
				if (Name != source.Name) return false;
				if (Command != source.Command) return false;
				if (Arguments != source.Arguments) return false;
				if (UserArguments != source.UserArguments) return false;
				if (Target != source.Target) return false;
				if (CreateNewPage != source.CreateNewPage) return false;
				if (AsChildPage != source.AsChildPage) return false;
				if (PageName != source.PageName) return false;
				if (SkipLocked != source.SkipLocked) return false;
				if (Timeout != source.Timeout) return false;

				return true;
			}

			return false;
		}

		public override int GetHashCode()
		{
			// arbitrary value; we don't call GetHashCode
			return Version.GetHashCode();
		}
	}
}
