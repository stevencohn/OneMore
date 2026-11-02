//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
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
		/// <summary>
		/// Ribbon control ID of the "My Snippets" dynamic menu
		/// </summary>
		public const string MenuID = "ribCustomSnippetsMenu";

		public const string SaveSnippetButtonId = "ribSaveSnippetButton";
		private const string ManageSnippetsButtonId = "ribManageSnippetsButton";
		private const string ExpandSnippetButtonId = "ribExpandSnippetButton";

		private const string DirectoryName = "Snippets";
		/// <summary>
		/// Legacy snippets are saved as clipboard HTML
		/// </summary>
		public const string HtmlExtension = ".snp";

		/// <summary>
		/// Snippets are saved as OneNote page XML to preserve full fidelity
		/// </summary>
		public const string XmlExtension = ".snpx";

		private readonly string store;


		public SnippetsProvider() : base()
		{
			store = Path.Combine(PathHelper.GetAppDataPath(), DirectoryName);
		}


		/// <summary>
		/// Delete the snippet with the specified path
		/// </summary>
		/// <param name="path"></param>
		public void Delete(string path)
		{
			if (File.Exists(path))
			{
				try
				{
					File.Delete(path);
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error deleting {path}", exc);
				}
			}
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

			foreach (var file in GetPaths())
			{
				yield return Path.GetFileNameWithoutExtension(file);
			}
		}


		/// <summary>
		/// Determines if the snippet at the given path is stored as page XML
		/// </summary>
		public static bool IsXml(string path)
		{
			return string.Equals(
				Path.GetExtension(path), XmlExtension, StringComparison.OrdinalIgnoreCase);
		}


		/// <summary>
		/// Gets the full path of the snippet with the given name, preferring the XML
		/// format over the legacy HTML format.
		/// </summary>
		/// <returns>The path or null if not found</returns>
		public string GetPathByName(string name)
		{
			var path = Path.Combine(store, $"{name}{XmlExtension}");
			if (File.Exists(path))
			{
				return path;
			}

			path = Path.Combine(store, $"{name}{HtmlExtension}");
			return File.Exists(path) ? path : null;
		}


		/// <summary>
		/// Gets a list of the full file paths of the custom snippets
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetPaths()
		{
			if (!Directory.Exists(store))
			{
				yield break;
			}

			// "*.snp" would also match "*.snpx" on some file systems (8.3 names) so filter explicitly
			var files = Directory.GetFiles(store)
				.Where(f => IsXml(f) ||
					string.Equals(Path.GetExtension(f), HtmlExtension, StringComparison.OrdinalIgnoreCase))
				.OrderBy(f => Path.GetFileNameWithoutExtension(f), StringComparer.OrdinalIgnoreCase)
				.ThenBy(f => IsXml(f) ? 0 : 1);

			// XML wins over HTML when both exist for the same name
			var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (var file in files)
			{
				if (seen.Add(Path.GetFileNameWithoutExtension(file)))
				{
					yield return file;
				}
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
				using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
				using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
				return await reader.ReadToEndAsync();
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error load snippet from {path}", exc);
				return null;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public async Task<string> LoadByName(string name)
		{
			var path = GetPathByName(name);
			return path is null ? null : await Load(path);
		}


		/// <summary>
		/// Rename the snippet with the specified path to the given name
		/// </summary>
		/// <param name="path">The existing full path of the snippet file</param>
		/// <param name="name">The new name of the snippet</param>
		public string Rename(string path, string name)
		{
			if (File.Exists(path))
			{
				try
				{
					var newpath = Path.Combine(
						Path.GetDirectoryName(path), $"{name}{Path.GetExtension(path)}");

					if (!string.Equals(path, newpath, StringComparison.OrdinalIgnoreCase))
					{
						// the user confirmed overwriting any snippet already using this name
						Delete(newpath);
						Delete(Path.ChangeExtension(newpath,
							IsXml(newpath) ? HtmlExtension : XmlExtension));
					}

					File.Move(path, newpath);

					return newpath;
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error deleting {path}", exc);
				}
			}

			return null;
		}


		/// <summary>
		/// Saves a new named snippet
		/// </summary>
		/// <param name="snippet"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public async Task Save(string snippet, string name, string extension = XmlExtension)
		{
			var path = Path.Combine(store, $"{name}{extension}");

			try
			{
				PathHelper.EnsurePathExists(store);

				using (var writer = new StreamWriter(path))
				{
					await writer.WriteAsync(snippet);
				}

				// when overwriting, remove a same-named snippet of the other format
				var other = Path.Combine(store, name +
					(extension == XmlExtension ? HtmlExtension : XmlExtension));

				Delete(other);
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
				MakeSaveSnippetButton(ns),
				new XElement(ns + "button",
					new XAttribute("id", ManageSnippetsButtonId),
					new XAttribute("getLabel", "GetRibbonLabel"),
					new XAttribute("getScreentip", "GetRibbonScreentip"),
					new XAttribute("imageMso", "BibliographyManageSources"),
					new XAttribute("onAction", "ManageSnippetsCmd")
					),
				new XElement(ns + "button",
					new XAttribute("id", ExpandSnippetButtonId),
					new XAttribute("getLabel", "GetRibbonLabel"),
					new XAttribute("getScreentip", "GetRibbonScreentip"),
					new XAttribute("imageMso", "ReplaceWithAutoText"),
					new XAttribute("onAction", "ExpandSnippetCmd")
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


		/// <summary>
		/// Generate the Save Snippets button
		/// </summary>
		/// <param name="ns"></param>
		/// <returns></returns>
		public static XElement MakeSaveSnippetButton(XNamespace ns)
		{
			return new XElement(ns + "button",
				new XAttribute("id", SaveSnippetButtonId),
				new XAttribute("getLabel", "GetRibbonLabel"),
				new XAttribute("getScreentip", "GetRibbonScreentip"),
				new XAttribute("imageMso", "SaveSelectionToQuickPartGallery"),
				new XAttribute("onAction", "SaveSnippetCmd")
				);
		}
	}
}
