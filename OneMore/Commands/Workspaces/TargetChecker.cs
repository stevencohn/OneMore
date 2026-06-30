//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Workspaces
{
	using River.OneMoreAddIn.Commands.Favorites;
	using River.OneMoreAddIn.Commands.Layouts;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Validate the target URI for each page reference - a Favorite or a LayoutWindow.
	/// </summary>
	/// <remarks>
	/// This class needs to stay async so it works seemlessly with the UI
	/// </remarks>
	internal class TargetChecker : IAsyncDisposable
	{
		private sealed class Notebooks : Dictionary<string, XElement> { }

		private readonly ILogger logger;

		private OneNote one;
		private bool disposed;
		private Notebooks notebooks;
		private XElement rootbooks;


		public TargetChecker(ILogger logger)
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
		/// <param name="collection">The favorites to validate</param>
		/// <returns>
		/// Returns True if any invalid favorites are discovered.
		/// </returns>
		public async Task<bool> InvalidFavorites(FavoritesCollection collection)
		{
			var updated = false;

			foreach (var folder in collection.Folders)
			{
				if (folder.Items.Any())
				{
					updated |= await InvalidReferences(folder.Items);
				}
			}

			updated |= await InvalidReferences(collection.Items);

			return updated;
		}


		/// <summary>
		/// Confirm that each window of each layout exists and is reachable by ID. For those
		/// that are not reachable, confirm if the Location/Screentip/path is reachable and,
		/// if so, patch the window with an updated ID.
		/// </summary>
		/// <param name="collection">The layouts to validate</param>
		/// <returns>
		/// Returns True if any invalid layout windows are discovered.
		/// </returns>
		public async Task<bool> InvalidLayoutWindow(LayoutsCollection collection)
		{
			var updated = false;

			foreach (var layout in collection.Layouts)
			{
				if (layout.Windows.Any())
				{
					updated |= await InvalidReferences(layout.Windows);
				}
			}

			return updated;
		}


		private async Task<bool> InvalidReferences(IEnumerable<ITargetReference> references)
		{
			one ??= new OneNote();
			notebooks ??= new Notebooks();

			var updated = false;

			foreach (var reference in references)
			{
				if (!string.IsNullOrWhiteSpace(reference.NotebookID) &&
					!string.IsNullOrWhiteSpace(reference.SectionID))
				{
					updated = await ConfirmByID(notebooks, reference) || updated;
				}

				// ConfirmByID would only return either Known or Unknown
				if (reference.Status == TargetStatus.Unknown)
				{
					// infer from location and update ID
					updated = await ConfirmByLocation(notebooks, reference) || updated;
				}
			}

			return updated;
		}


		private async Task<bool> ConfirmByID(Notebooks notebooks, ITargetReference reference)
		{
			XElement notebook = null;
			if (notebooks.ContainsKey(reference.NotebookID))
			{
				notebook = notebooks[reference.NotebookID];
			}
			else
			{
				try
				{
					notebook = await one.GetNotebook(reference.NotebookID, OneNote.Scope.Pages);
					if (notebook is not null)
					{
						notebooks.Add(reference.NotebookID, notebook);
					}
				}
				catch (COMException exc) when ((uint)exc.ErrorCode == ErrorCodes.hrObjectMissing)
				{
					logger.WriteLine($"broken link to notebook {reference.Location}");
				}
				catch (Exception exc)
				{
					logger.WriteLine($"could not fetch notebook {reference.Location}", exc);
				}
			}

			var targetID = reference.PageID ?? reference.SectionID;

			if (notebook is not null &&
				notebook.Descendants()
					.FirstOrDefault(e => e.Attribute("ID")?.Value == targetID) is XElement node)
			{
				reference.Status = TargetStatus.Known;

				var name = node.Attribute("name").Value;
				if (reference.Name != name)
				{
					// auto-correct page/section name and its last Location segment
					logger.WriteLine($"patching name from '{reference.Name}' to '{name}'");
					reference.Name = name;
					var lastSlash = reference.Location.LastIndexOf('/');
					if (lastSlash >= 0)
					{
						reference.Location = reference.Location.Substring(0, lastSlash) + "/" + name;
					}
					return true;
				}
			}
			else
			{
				logger.WriteLine($"broken link to notebook {reference.Location}");
				reference.Status = TargetStatus.Unknown;
			}

			return false;
		}


		private async Task<bool> ConfirmByLocation(Notebooks notebooks, ITargetReference reference)
		{
			bool Broken(ITargetReference reference)
			{
				logger.WriteLine($"broken reference {reference.Location} [{reference.Name}]");
				reference.Status = TargetStatus.Suspect;
				return false;
			}

			var parts = reference.Location.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length < 2)
			{
				// must be at least: /notebook/section
				return Broken(reference);
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
					return Broken(reference);
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
				return Broken(reference);
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
				return Broken(reference);
			}

			reference.Status = TargetStatus.Known;

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

			if (reference.NotebookID != notebookID ||
				reference.SectionID != sectionID ||
				reference.PageID != pageID)
			{
				logger.WriteLine($"patching IDs {reference.Location}");

				reference.NotebookID = notebookID;
				reference.SectionID = sectionID;
				reference.PageID = pageID;

				return true;
			}

			return false;
		}
	}
}
