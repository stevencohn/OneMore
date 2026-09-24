//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
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
		private static readonly object locker = new();
		private static SettingsProvider current;

		private readonly string path;
		private XElement root;


		/// <summary>
		/// Initialize a new provider; use Current to share the single loaded instance.
		/// </summary>
		private SettingsProvider()
		{
			path = Path.Combine(
				PathHelper.GetAppDataPath(), "OneMoreCalendar.xml");

			Load();
		}


		/// <summary>
		/// Gets the shared provider, loading the settings file only the first time.
		/// </summary>
		public static SettingsProvider Current
		{
			get
			{
				lock (locker)
				{
					return current ??= new SettingsProvider();
				}
			}
		}


		/// <summary>
		/// Discards in-memory settings and re-reads the settings file.
		/// </summary>
		public void Reload()
		{
			lock (locker)
			{
				Load();
			}
		}


		private void Load()
		{
			root = null;

			if (File.Exists(path))
			{
				try
				{
					root = XElement.Load(path);
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine($"error reading {path}", exc);
					MessageBox.Show($"error reading {path}\n{exc.Message}");
				}
			}

			root ??= new XElement("settings");

			if (root.Element("filters") is null)
			{
				root.Add(new XElement("filters",
					new XElement("modified", true)
					));
			}

			if (root.Element("notebooks") is null)
			{
				root.Add(new XElement("notebooks"));
			}

			if (root.Element("theme") is null)
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
				return !empties.Any() || empties.Any(e => e.Value.Equals("true"));
			}
		}


		public bool Modified =>
			root.Elements("filters").Elements("modified").Any(e => e.Value.Equals("true"));


		public ThemeMode Theme
		{
			get
			{
				var element = root.Elements("theme").FirstOrDefault();
				if (element is not null && Enum.TryParse<ThemeMode>(element.Value, out var mode))
				{
					return mode;
				}

				return ThemeMode.System;
			}
		}


		public async Task<IEnumerable<string>> GetNotebookIDs()
		{
			var ids = root.Elements("notebooks").Elements("notebook")
				.Select(e => e.Value).ToList();

			if (ids.Count == 0)
			{
				var books = await new OneNoteProvider().GetNotebooks();
				ids = books.Select(b => b.ID).ToList();
			}

			return ids;
		}


		public async Task<IEnumerable<Notebook>> GetNotebooks()
		{
			var notebooks = new List<Notebook>();

			var ids = await GetNotebookIDs();

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
			try
			{
				PathHelper.EnsurePathExists(Path.GetDirectoryName(path));
				root.Save(path, SaveOptions.None);
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"error saving {path}", exc);
				throw;
			}
		}
	}
}
