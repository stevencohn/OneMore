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
				PathHelper.GetAppDataPath(), "OneMoreCalendar.xml");

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

			root ??= new XElement("settings");

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

			var theme = root.Elements("theme");
			if (theme == null)
			{
				root.Add(new XElement("theme", ThemeMode.System.ToString()));
			}
		}


		public bool Created =>
			root.Elements("filters").Elements("created").Any(e => e.Value.Equals("true"));


		public bool Deleted =>
			root.Elements("filters").Elements("deleted").Any(e => e.Value.Equals("true"));


		public bool Empty
		{
			get
			{
				// default is true if empty filter is missing
				var empties = root.Elements("filters").Elements("empty");
				return empties == null || empties.Any(e => e.Value.Equals("true"));
			}
		}


		public bool Modified => 
			root.Elements("filters").Elements("modified").Any(e => e.Value.Equals("true"));


		public ThemeMode Theme
		{
			get
			{
				var element = root.Elements("theme").FirstOrDefault();
				if (element != null)
				{
					return (ThemeMode)Enum.Parse(typeof(ThemeMode), element.Value);
				}

				return ThemeMode.System;
			}
		}


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


		public void SetFilter(bool created, bool modified, bool deleted, bool empty)
		{
			var filters = root.Element("filters");
			if (filters == null)
			{
				filters = new XElement("filters");
				root.Add(filters);
			}

			SetFilter(filters, "created", created);
			SetFilter(filters, "modified", modified);
			SetFilter(filters, "deleted", deleted);
			SetFilter(filters, "empty", empty);
		}


		private void SetFilter(XElement filters, string name, bool value)
		{
			var element = filters.Element(name);
			if (element == null)
			{
				filters.Add(new XElement(name, value.ToString().ToLower()));
			}
			else
			{
				element.Value = value.ToString().ToLower();
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


		public void SetTheme(ThemeMode mode)
		{
			var theme = root.Element("theme");
			if (theme == null)
			{
				root.Add(new XElement("theme", mode.ToString()));
			}
			else
			{
				theme.Value = mode.ToString();
			}
		}


		public void Save()
		{
			PathHelper.EnsurePathExists(Path.GetDirectoryName(path));
			root.Save(path, SaveOptions.None);
		}
	}
}
