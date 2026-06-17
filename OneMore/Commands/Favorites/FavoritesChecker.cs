//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Favorites
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Validate the target URI for each favorite.
	/// </summary>
	/// <remarks>
	/// This class needs to stay async so it works seemlessly with the UI
	/// </remarks>
	internal class FavoritesChecker : IAsyncDisposable
	{
		private sealed class Notebooks : Dictionary<string, XElement> { }

		private readonly ILogger logger;

		private OneNote one;
		private bool disposed;
		private Notebooks notebooks;
		private XElement rootbooks;


		public FavoritesChecker(ILogger logger)
		{
			this.logger = logger;
		}


		public async ValueTask DisposeAsync()
		{
			await DisposeAsyncCore().ConfigureAwait(false);
			// DO NOT call SuppressFinalize, otherwise OneNote will not shutdown properly
		}


		async ValueTask DisposeAsyncCore()
		{
			if (!disposed && one is not null)
			{
				await one.DisposeAsync();
				disposed = true;
			}
		}


		/// <summary>
		/// Confirm that each favorite exists and is reachable with the by ID. For those that
		/// are not reachable, confirm if the Location/Screentip/path is reachable and, if so,
		/// patch the favorite with an updated ID.
		/// </summary>
		/// <param name="favorites">List of favorites to validate</param>
		/// <returns>
		/// Returns True if any invalid favorites are discovered.
		/// </returns>
		public async Task<bool> InvalidFavorites(List<Favorite> favorites)
		{
			one ??= new OneNote();
			notebooks ??= new Notebooks();

			var updated = false;

			foreach (var favorite in favorites)
			{
				if (!string.IsNullOrWhiteSpace(favorite.NotebookID) &&
					!string.IsNullOrWhiteSpace(favorite.SectionID))
				{
					updated = await ConfirmByID(notebooks, favorite) || updated;
				}

				// ConfirmByID would only return either Known or Unknown
				if (favorite.Status == FavoriteStatus.Unknown)
				{
					// infer from location and update ID
					updated = await ConfirmByLocation(notebooks, favorite) || updated;
				}
			}

			return updated;
		}


		private async Task<bool> ConfirmByID(Notebooks notebooks, Favorite favorite)
		{
			XElement notebook = null;
			if (notebooks.ContainsKey(favorite.NotebookID))
			{
				notebook = notebooks[favorite.NotebookID];
			}
			else
			{
				try
				{
					notebook = await one.GetNotebook(favorite.NotebookID, OneNote.Scope.Pages);
					if (notebook is not null)
					{
						notebooks.Add(favorite.NotebookID, notebook);
					}
				}
				catch (COMException exc) when ((uint)exc.ErrorCode == ErrorCodes.hrObjectMissing)
				{
					logger.WriteLine($"broken link to favorite notebook {favorite.Location}");
				}
				catch (Exception exc)
				{
					logger.WriteLine($"could not fetch favorite notebook {favorite.Location}", exc);
				}
			}

			var targetID = favorite.PageID ?? favorite.SectionID;

			if (notebook is not null &&
				notebook.Descendants()
					.First(e => e.Attribute("ID")?.Value == targetID) is XElement node)
			{
				favorite.Status = FavoriteStatus.Known;

				var name = node.Attribute("name").Value;
				if (favorite.Name != name)
				{
					// auto-correct page/section name
					favorite.Name = name;
					return true;
				}
			}
			else
			{
				logger.WriteLine($"broken link to favorite notebook {favorite.Location}");
				favorite.Status = FavoriteStatus.Unknown;
			}

			return false;
		}


		private async Task<bool> ConfirmByLocation(Notebooks notebooks, Favorite favorite)
		{
			bool Broken(Favorite favorite)
			{
				logger.WriteLine($"broken favorite {favorite.Location} [{favorite.Name}]");
				favorite.Status = FavoriteStatus.Suspect;
				return false;
			}

			var parts = favorite.Location.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length < 2)
			{
				// must be at least: /notebook/section
				return Broken(favorite);
			}

			// match and cache notebook...

			var notebook = notebooks.Values.FirstOrDefault(n => n.Attribute("name")?.Value == parts[0]);
			if (notebook is null)
			{
				rootbooks ??= await one.GetNotebooks();

				// first part should be name of notebook
				var nx = rootbooks.GetNamespaceOfPrefix(OneNote.Prefix);
				var book = rootbooks.Elements(nx + "Notebook")
					.FirstOrDefault(n => n.Attribute("name")?.Value == parts[0]);

				if (book is null)
				{
					return Broken(favorite);
				}

				var id = book.Attribute("ID").Value;

				if (notebooks.ContainsKey(id))
				{
					notebook = notebooks[id];
				}
				else
				{
					notebook = await one.GetNotebook(id, OneNote.Scope.Pages);
					notebooks.Add(id, notebook);
				}
			}

			// match section/group/page parts...

			parts = parts.Skip(1).ToArray();

			if (parts.Length == 0)
			{
				return Broken(favorite);
			}

			var node = notebook;
			var previous = notebook;
			foreach (var part in parts)
			{
				node = node.Elements()
					.FirstOrDefault(e =>
						string.Equals(e.Attribute("name")?.Value, part,
							StringComparison.InvariantCultureIgnoreCase));

				if (node is null) break;
				previous = node;
			}

			if (node is null)
			{
				return Broken(favorite);
			}

			favorite.Status = FavoriteStatus.Known;

			// resolve and path...

			var notebookID = notebook.Attribute("ID").Value;
			string sectionID;
			string pageID;

			if (node.Name.LocalName == "Page")
			{
				sectionID = previous.Attribute("ID").Value;
				pageID = node.Attribute("ID").Value;
			}
			else
			{
				// Section or SectionGroup
				sectionID = node.Attribute("ID").Value;
				pageID = null;
			}

			if (favorite.NotebookID != notebookID ||
				favorite.SectionID != sectionID ||
				favorite.PageID != pageID)
			{
				logger.WriteLine($"patching favorite IDs {favorite.Location}");

				favorite.NotebookID = notebookID;
				favorite.SectionID = sectionID;
				favorite.PageID = pageID;

				return true;
			}

			return false;
		}
	}
}
