//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class SnippetsProvider : Loggable
	{
		private const string SaveSnippetButtonId = "omSaveSnippetButton";
		private const string ManageSnippetsButtonId = "omManageSnippetsButton";

		private const string DirectoryName = "Snippets";
		private const string Extension = ".snp";

		private readonly string store;


		public SnippetsProvider() : base()
		{
			store = Path.Combine(PathFactory.GetAppDataPath(), DirectoryName);
		}


		/// <summary>
		/// Gets a list of snippet names for comparison when naming and saving a new snippet
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetNames()
		{
			if (!Directory.Exists(store))
			{
				yield break;
			}

			foreach (var file in Directory.GetFiles(store, $"*{Extension}"))
			{
				yield return Path.GetFileNameWithoutExtension(file);
			}
		}


		/// <summary>
		/// Loads the contents of a snippet
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public async Task<string> Load(string path)
		{
			if (!File.Exists(path))
			{
				return null;
			}

			try
			{
				using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
				using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
				{
					return await reader.ReadToEndAsync();
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error load snippet from {path}", exc);
				return null;
			}
		}


		/// <summary>
		/// Saves a new named snippet
		/// </summary>
		/// <param name="snippet"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public async Task Save(string snippet, string name)
		{
			var path = Path.Combine(store, $"{name}{Extension}");

			try
			{
				PathFactory.EnsurePathExists(store);

				using (var writer = new StreamWriter(path))
				{
					await writer.WriteAsync(snippet);
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error saving snippet to {path}", exc);
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		public XElement MakeSnippetsMenu(XNamespace ns)
		{
			var menu = new XElement(ns + "menu",
				new XAttribute("id", "ribMySnippets"),
				new XAttribute("label", "My Custom Snippets"), // translate
				new XAttribute("imageMso", "GroupInsertShapes"),
				new XElement(ns + "button",
					new XAttribute("id", SaveSnippetButtonId),
					new XAttribute("label", "Save Custom Snippet"), // translate Resx.Favorites_addButton_Label),
					new XAttribute("imageMso", "SaveSelectionToQuickPartGallery"),
					new XAttribute("onAction", "SaveSnippetCmd")
					),
				new XElement(ns + "button",
					new XAttribute("id", ManageSnippetsButtonId),
					new XAttribute("label", "Manage Custom Snippets"), // translate
					new XAttribute("imageMso", "BibliographyManageSources"),
					new XAttribute("onAction", "ManageSnippetsCmd")
					),
				new XElement(ns + "menuSeparator",
					new XAttribute("id", "ribSnippetsMenuSep")
					)
				);

			var snippets = GetPaths();
			if (snippets.Any())
			{
				var b = 0;
				foreach (var snippet in snippets)
				{
					menu.Add(new XElement(ns + "button",
						new XAttribute("id", $"ribMySnippet{b++}"),
						new XAttribute("imageMso", "PasteAlternative"),
						new XAttribute("label", Path.GetFileNameWithoutExtension(snippet)),
						new XAttribute("tag", snippet),
						new XAttribute("onAction", "InsertSnippetCmd")
						));
				}
			}

			return menu;
		}


		private IEnumerable<string> GetPaths()
		{
			if (!Directory.Exists(store))
			{
				yield break;
			}

			foreach (var file in Directory.GetFiles(store, $"*{Extension}"))
			{
				yield return file;
			}
		}
	}
}
