//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using River.OneMoreAddIn;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;


	/// <summary>
	/// Loads, save, and manages user settings
	/// </summary>
	internal class SettingsProvider
	{
		private readonly string path;
		private readonly XElement root;


		/// <summary>
		/// Initialize a new provider.
		/// </summary>
		public SettingsProvider()
		{
			path = Path.Combine(
				PathFactory.GetAppDataPath(), "OneMoreCalendar.xml");

			if (File.Exists(path))
			{
				try
				{
					root = XElement.Load(path);
				}
				catch (Exception exc)
				{
					MessageBox.Show($"error reading {path}\n{exc.Message}");
				}
			}

			if (root == null)
			{
				root = new XElement("settings");
			}

			var filters = root.Element("filters");
			if (filters == null)
			{
				root.Add(new XElement("filter",
					new XElement("modified", true)
					));
			}

			var notebooks = root.Element("notebooks");
			if (notebooks == null || !notebooks.Elements().Any())
			{
				if (notebooks == null)
				{
					notebooks = new XElement("notebooks");
					root.Add(notebooks);
				}

				using (var one = new OneNote())
				{
					var books = one.GetNotebooks();
					var ns = books.GetNamespaceOfPrefix(OneNote.Prefix);
					foreach (var id in books.Elements(ns + "Notebook")
						.Select(e => e.Attribute("ID").Value))
					{
						notebooks.Add(new XElement("notebook", id));
					}
				}
			}
		}


		public bool ShowCreated =>
			root.Elements("filters").Elements("created").Any(e => e.Value.Equals("true"));

		public bool ShowModified => 
			root.Elements("filters").Elements("modified").Any(e => e.Value.Equals("true"));


		public IEnumerable<Notebook> GetNotebooks()
		{
			var notebooks = new List<Notebook>();
			var ids = root.Elements("notebooks").Elements("notebook").Select(e => e.Value);
			if (ids.Any())
			{
				using (var one = new OneNote())
				{
					var books = one.GetNotebooks();
					var ns = books.GetNamespaceOfPrefix(OneNote.Prefix);

					foreach (var id in ids)
					{
						var book = books.Elements(ns + "Notebook")
							.FirstOrDefault(e => e.Attribute("ID").Value == id);

						if (book != null)
						{
							notebooks.Add(new Notebook(book));
						}
					}
				}
			}

			if (!notebooks.Any())
			{
				using (var one = new OneNote())
				{
					var books = one.GetNotebooks();
					var ns = books.GetNamespaceOfPrefix(OneNote.Prefix);
					foreach (var book in books.Elements(ns + "Notebook"))
					{
						notebooks.Add(new Notebook(book));
					}
				}
			}

			return notebooks;
		}


		public void SetFilter(bool created, bool modified)
		{
			var filters = root.Element("filters");
			if (filters == null)
			{
				filters = new XElement("filters",
					new XElement("created", created.ToString().ToLower()),
					new XElement("modified", modified.ToString().ToLower())
					);

				root.Add(filters);
			}

			var element = filters.Element("created");
			if (element == null)
			{
				filters.Add(new XElement("created", created.ToString().ToLower()));
			}
			else
			{
				element.Value = created.ToString().ToLower();
			}

			element = filters.Element("modified");
			if (element == null)
			{
				filters.Add(new XElement("modified", created.ToString().ToLower()));
			}
			else
			{
				element.Value = created.ToString().ToLower();
			}
		}


		public void SetNotebookIDs(IEnumerable<string> ids)
		{
			var notebooks = root.Element("notebooks");
			if (notebooks == null)
			{
				notebooks = new XElement("notebooks");
				root.Add(notebooks);
			}
			else
			{
				notebooks.Elements().Remove();
			}

			foreach (var id in ids)
			{
				notebooks.Add(new XElement("notebook", id));
			}
		}


		public void Save()
		{
			PathFactory.EnsurePathExists(Path.GetDirectoryName(path));
			root.Save(path, SaveOptions.None);
		}
	}
}
