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
	using System.Threading.Tasks;
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
				root.Add(new XElement("filters",
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
			}
		}


		public bool ShowCreated =>
			root.Elements("filters").Elements("created").Any(e => e.Value.Equals("true"));

		public bool ShowModified => 
			root.Elements("filters").Elements("modified").Any(e => e.Value.Equals("true"));


		public async Task<IEnumerable<string>> GetNotebookIDs()
		{
			var ids = root.Elements("notebooks").Elements("notebook").Select(e => e.Value);
			if (!ids.Any())
			{
				var books = await new OneNoteProvider().GetNotebooks();
				ids = books.Select(b => b.ID).ToList();
			}

			return ids;
		}


		public async Task<IEnumerable<Notebook>> GetNotebooks()
		{
			var notebooks = new List<Notebook>();

			var provider = new SettingsProvider();
			var ids = await provider.GetNotebookIDs();

			var books = await new OneNoteProvider().GetNotebooks();
			foreach (var book in books)
			{
				book.Checked = ids.Contains(book.ID);
				notebooks.Add(book);
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
				filters.Add(new XElement("modified", modified.ToString().ToLower()));
			}
			else
			{
				element.Value = modified.ToString().ToLower();
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
