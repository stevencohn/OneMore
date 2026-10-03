//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Identity
{
	using River.OneMoreAddIn.Properties;
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.SQLite;
	using System.Linq;


	/// <summary>
	/// Stores page identities in OneMore.db, giving each page a stable database-side key
	/// that survives OneNote regenerating its IDs. Nothing here ever writes to a page.
	/// </summary>
	internal class PageIdentityProvider : DatabaseProvider
	{
		private const string PrimaryTable = "identity_page";
		private const string VersionTable = "identity_schema";
		private const int CurrentVersion = 1;

		private const string SelectColumns =
			"pageKey, pageID, notebookKey, sectionKey, title, created, modified, level, " +
			"missingSince, pageGuid";


		/// <summary>
		/// Initialize this provider, opening the standard database
		/// </summary>
		public PageIdentityProvider()
			: base()
		{
			if (CatalogExists())
			{
				OpenDatabase();
				UpgradeCatalog();
			}
			else
			{
				RefreshDataSchema("page identity", Resources.PageIdentityDB);
			}
		}


		/// <summary>
		/// Initialize this provider over an open connection, such as an in-memory database.
		/// </summary>
		internal PageIdentityProvider(SQLiteConnection connection)
		{
			con = connection;

			if (TableExists(PrimaryTable))
			{
				UpgradeCatalog();
			}
			else
			{
				RefreshDataSchema("page identity", Resources.PageIdentityDB);
			}
		}


		public static bool CatalogExists()
		{
			return CatalogExists(PrimaryTable);
		}


		public bool DropCatalog()
		{
			return DropCatalog("page identity", Resources.PageIdentityDB);
		}


		// A catalog newer than this build knows is left alone. There are no upgrade steps yet; the
		// first release is version 1. Later releases chain their steps here, after that check, as
		// if (version == N) version = UpgradeNtoN+1().
		private void UpgradeCatalog()
		{
			var version = ReadSchemaVersion(VersionTable, "schemaID", missing: CurrentVersion);
			if (IsNewerThanKnown("page identity", version, CurrentVersion))
			{
				return;
			}
		}


		#region Write transactions
		// The identity catalog can be written by the add-in and by the tray at the same time.
		// A deferred transaction would read first and only then try to write, so two of them
		// could each find that a new page has no key and each give it one. An immediate
		// transaction takes the write lock before reading, so the second waits for the first
		// and then finds its row. The connection's busy timeout covers the wait.

		private void BeginImmediate()
		{
			using var cmd = con.CreateCommand();
			cmd.CommandText = "BEGIN IMMEDIATE";
			cmd.ExecuteNonQuery();
		}


		private void Commit()
		{
			using var cmd = con.CreateCommand();
			cmd.CommandText = "COMMIT";
			cmd.ExecuteNonQuery();
		}


		private void Rollback()
		{
			try
			{
				using var cmd = con.CreateCommand();
				cmd.CommandText = "ROLLBACK";
				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				// never mask the error that caused the rollback
				logger.WriteLine("error rolling back page identity changes", exc);
			}
		}
		#endregion Write transactions


		/// <summary>
		/// Resolves every current page to a stored identity, creating identities for new
		/// pages and keeping existing ones through ID changes, moves and edited dates.
		/// </summary>
		/// <param name="notebookKeys">The notebooks fully represented in <paramref name="pages"/>.
		/// Stored pages of these notebooks that no current page claims are marked missing.</param>
		/// <param name="pages">All current pages of those notebooks, from the hierarchy.
		/// Pass every notebook of a scan together so a page moved between notebooks is found.</param>
		/// <param name="skipSections">Optional. Sections, as built by
		/// <see cref="PageIdentityKeys.SectionScope"/>, that could not be listed this time, such
		/// as locked sections. Their stored pages are left alone rather than marked missing.</param>
		/// <returns>One resolution per page, in order, each with its page key.</returns>
		public IReadOnlyList<PageResolution> Reconcile(
			IReadOnlyCollection<string> notebookKeys,
			IReadOnlyList<PageRef> pages,
			IReadOnlyCollection<string> skipSections = null)
		{
			BeginImmediate();

			try
			{
				var rows = ReadCandidates(notebookKeys, skipSections);
				var result = PageIdentityMatcher.Match(rows, pages);
				var now = DateTime.Now.ToZuluString();

				using var cmd = con.CreateCommand();
				cmd.CommandType = CommandType.Text;

				foreach (var resolution in result.Pages)
				{
					if (resolution.Row is null)
					{
						resolution.PageKey = Insert(cmd, resolution.Page, now);
					}
					else if (Changed(resolution))
					{
						Update(cmd, resolution, now);
					}
				}

				foreach (var orphan in result.Orphans.Where(o => !o.IsMissing))
				{
					cmd.Parameters.Clear();
					cmd.CommandText =
						"UPDATE identity_page SET missingSince = @now WHERE pageKey = @pageKey";

					cmd.Parameters.AddWithValue("@now", now);
					cmd.Parameters.AddWithValue("@pageKey", orphan.PageKey);
					cmd.ExecuteNonQuery();
				}

				Commit();
				return result.Pages;
			}
			catch (Exception exc)
			{
				Rollback();
				logger.WriteLine("error reconciling page identities", exc);
				throw;
			}
		}


		// the stored identities that can be claimed: pages of the notebooks being reconciled
		// and any page still marked missing from an earlier pass
		private List<IdentityRow> ReadCandidates(
			IReadOnlyCollection<string> notebookKeys, IReadOnlyCollection<string> skipSections)
		{
			var rows = ReadCandidates(notebookKeys);
			if (skipSections is not null && skipSections.Count > 0)
			{
				var skip = new HashSet<string>(skipSections, StringComparer.Ordinal);
				rows.RemoveAll(r => skip.Contains(
					PageIdentityKeys.SectionScope(r.NotebookKey, r.SectionKey)));
			}

			return rows;
		}


		private List<IdentityRow> ReadCandidates(IReadOnlyCollection<string> notebookKeys)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;

			var names = new List<string>();
			var index = 0;
			foreach (var key in notebookKeys ?? Array.Empty<string>())
			{
				var name = $"@nb{index++}";
				names.Add(name);
				cmd.Parameters.AddWithValue(name, key);
			}

			cmd.CommandText = $"SELECT {SelectColumns} FROM {PrimaryTable} WHERE missingSince IS NOT NULL"
				+ (names.Count > 0 ? $" OR notebookKey IN ({string.Join(",", names)})" : string.Empty)
				+ " ORDER BY pageKey";

			return ReadRows(cmd);
		}


		private static List<IdentityRow> ReadRows(SQLiteCommand cmd)
		{
			var rows = new List<IdentityRow>();

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				rows.Add(new IdentityRow
				{
					PageKey = reader.GetInt64(0),
					PageID = reader.GetString(1),
					NotebookKey = reader.GetString(2),
					SectionKey = reader.GetString(3),
					Title = reader.GetString(4),
					Created = reader.GetString(5),
					Modified = reader.GetString(6),
					Level = reader.GetInt32(7),
					MissingSince = reader.IsDBNull(8) ? null : reader.GetString(8),
					PageGuid = reader.IsDBNull(9) ? null : reader.GetString(9)
				});
			}

			return rows;
		}


		private static bool Changed(PageResolution resolution)
		{
			var page = resolution.Page;
			var row = resolution.Row;

			return row.IsMissing
				|| row.PageID != page.PageID
				|| row.NotebookKey != page.NotebookKey
				|| row.SectionKey != page.SectionKey
				|| row.Title != page.Title
				|| row.Created != page.Created
				|| row.Modified != page.Modified
				|| row.Level != page.Level
				|| (page.PageGuid is not null && !string.Equals(row.PageGuid, page.PageGuid, StringComparison.OrdinalIgnoreCase));
		}


		private static long Insert(SQLiteCommand cmd, PageRef page, string now)
		{
			cmd.Parameters.Clear();
			cmd.CommandText =
				"INSERT INTO identity_page (pageID, notebookKey, sectionKey, title, created, " +
				"modified, level, lastSeen, pageGuid) VALUES (@pageID, @notebookKey, @sectionKey, " +
				"@title, @created, @modified, @level, @now, @pageGuid); " +
				"SELECT last_insert_rowid()";

			AddPageParameters(cmd, page);
			cmd.Parameters.AddWithValue("@now", now);

			return Convert.ToInt64(cmd.ExecuteScalar());
		}


		private static void Update(SQLiteCommand cmd, PageResolution resolution, string now)
		{
			cmd.Parameters.Clear();
			cmd.CommandText =
				"UPDATE identity_page SET pageID = @pageID, notebookKey = @notebookKey, " +
				"sectionKey = @sectionKey, title = @title, created = @created, " +
				"modified = @modified, level = @level, missingSince = NULL, lastSeen = @now, " +
				"pageGuid = COALESCE(@pageGuid, pageGuid) " +
				"WHERE pageKey = @pageKey";

			AddPageParameters(cmd, resolution.Page);
			cmd.Parameters.AddWithValue("@now", now);
			cmd.Parameters.AddWithValue("@pageKey", resolution.PageKey);
			cmd.ExecuteNonQuery();
		}


		private static void AddPageParameters(SQLiteCommand cmd, PageRef page)
		{
			cmd.Parameters.AddWithValue("@pageID", page.PageID);
			cmd.Parameters.AddWithValue("@notebookKey", page.NotebookKey);
			cmd.Parameters.AddWithValue("@sectionKey", page.SectionKey);
			cmd.Parameters.AddWithValue("@title", page.Title);
			cmd.Parameters.AddWithValue("@created", page.Created);
			cmd.Parameters.AddWithValue("@modified", page.Modified);
			cmd.Parameters.AddWithValue("@level", page.Level);
			cmd.Parameters.AddWithValue("@pageGuid", (object)page.PageGuid ?? DBNull.Value);
		}


		/// <summary>
		/// Finds the pages whose hyperlink GUID is worth reading before they are reconciled: the
		/// pages that no stored identity matches, or only weakly, but only when some stored
		/// identity has a GUID that could rescue one. Reading a GUID costs a call to OneNote per
		/// page, so this keeps that cost to the few pages where it can change the answer. It does
		/// not write anything, and it is safe to call outside a transaction because the result is
		/// only a hint about which pages to look up.
		/// </summary>
		public IReadOnlyList<PageRef> FindPagesNeedingGuid(
			IReadOnlyCollection<string> notebookKeys,
			IReadOnlyList<PageRef> pages,
			IReadOnlyCollection<string> skipSections = null)
		{
			var rows = ReadCandidates(notebookKeys, skipSections);
			var result = PageIdentityMatcher.Match(rows, pages);

			var couldRescue =
				result.Orphans.Any(o => o.PageGuid is not null) ||
				result.Pages.Any(r => r.Kind == ResolutionKind.SameTitle && r.Row?.PageGuid is not null);

			if (!couldRescue)
			{
				return Array.Empty<PageRef>();
			}

			return result.Pages
				.Where(r => r.Kind == ResolutionKind.New || r.Kind == ResolutionKind.SameTitle)
				.Select(r => r.Page)
				.ToList();
		}


		/// <summary>
		/// Records the hyperlink GUIDs of pages, after they were read from OneNote.
		/// </summary>
		/// <returns>The number of identities updated</returns>
		public int WritePageGuids(IReadOnlyCollection<(long PageKey, string PageGuid)> guids)
		{
			if (guids.Count == 0)
			{
				return 0;
			}

			BeginImmediate();

			try
			{
				using var cmd = con.CreateCommand();
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "UPDATE identity_page SET pageGuid = @guid WHERE pageKey = @pageKey";
				cmd.Parameters.Add("@guid", DbType.String);
				cmd.Parameters.Add("@pageKey", DbType.Int64);

				var count = 0;
				foreach (var (pageKey, pageGuid) in guids)
				{
					cmd.Parameters["@guid"].Value = pageGuid;
					cmd.Parameters["@pageKey"].Value = pageKey;
					count += cmd.ExecuteNonQuery();
				}

				Commit();
				return count;
			}
			catch (Exception exc)
			{
				Rollback();
				logger.WriteLine("error writing page GUIDs", exc);
				throw;
			}
		}


		/// <summary>
		/// Gets the stored identities that have the given hyperlink GUID, present ones first.
		/// The GUID is not unique, so a caller must treat more than one result as ambiguous.
		/// </summary>
		public IReadOnlyList<IdentityRow> ReadByGuid(string pageGuid)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText =
				$"SELECT {SelectColumns} FROM {PrimaryTable} WHERE pageGuid = @guid COLLATE NOCASE " +
				"ORDER BY missingSince IS NOT NULL, pageKey";

			cmd.Parameters.AddWithValue("@guid", pageGuid);

			return ReadRows(cmd);
		}

		/// <summary>
		/// Gets the stored identity of a page, or null if there is none.
		/// </summary>
		public IdentityRow Read(long pageKey)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = $"SELECT {SelectColumns} FROM {PrimaryTable} WHERE pageKey = @pageKey";
			cmd.Parameters.AddWithValue("@pageKey", pageKey);

			return ReadRows(cmd).FirstOrDefault();
		}


		/// <summary>
		/// Gets the stored identity currently holding the given OneNote page ID, or null.
		/// The ID is valid only until its notebook is next reopened, so use this for the
		/// page the user is looking at now, not for anything remembered. A present identity
		/// is preferred over one that is marked missing.
		/// </summary>
		public IdentityRow ReadByPageID(string pageID)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText =
				$"SELECT {SelectColumns} FROM {PrimaryTable} WHERE pageID = @pageID " +
				"ORDER BY missingSince IS NOT NULL, pageKey";

			cmd.Parameters.AddWithValue("@pageID", pageID);

			return ReadRows(cmd).FirstOrDefault();
		}


		/// <summary>
		/// Gets every stored identity, ordered by page key.
		/// </summary>
		public IReadOnlyList<IdentityRow> ReadAll()
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = $"SELECT {SelectColumns} FROM {PrimaryTable} ORDER BY pageKey";

			return ReadRows(cmd);
		}


		/// <summary>
		/// Deletes identities that have been missing from the hierarchy for longer than the
		/// grace period, and returns their keys so the caller can remove everything stored
		/// against them. A notebook that was just reopened fills in gradually, so a page
		/// should not be forgotten the first time it is not seen.
		/// </summary>
		public IReadOnlyList<long> PurgeMissing(TimeSpan grace)
		{
			var cutoff = (DateTime.Now - grace).ToZuluString();
			var keys = new List<long>();

			BeginImmediate();

			try
			{
				using var cmd = con.CreateCommand();
				cmd.CommandType = CommandType.Text;
				cmd.CommandText =
					"SELECT pageKey FROM identity_page " +
					"WHERE missingSince IS NOT NULL AND missingSince < @cutoff";

				cmd.Parameters.AddWithValue("@cutoff", cutoff);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						keys.Add(reader.GetInt64(0));
					}
				}

				cmd.CommandText = "DELETE FROM identity_page WHERE pageKey = @pageKey";
				cmd.Parameters.Clear();
				var parameter = cmd.Parameters.Add("@pageKey", DbType.Int64);
				foreach (var key in keys)
				{
					parameter.Value = key;
					cmd.ExecuteNonQuery();
				}

				Commit();
				return keys;
			}
			catch (Exception exc)
			{
				Rollback();
				logger.WriteLine("error purging missing page identities", exc);
				return Array.Empty<long>();
			}
		}
	}
}
