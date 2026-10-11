//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.TextIndex
{
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Properties;
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.SQLite;
	using System.IO;
	using System.Linq;
	using System.Runtime.InteropServices;


	/// <summary>
	/// A paragraph found by a query of the text index.
	/// </summary>
	internal sealed class IndexHit
	{
		/// <summary>Gets the page key of the page, from the identity catalog.</summary>
		public long PageKey { get; set; }

		/// <summary>Gets the OneNote object ID of the paragraph. It is valid only until the
		/// notebook is reopened, after which the page is indexed again.</summary>
		public string ObjectID { get; set; }

		public int Ordinal { get; set; }

		public string Text { get; set; }

		/// <summary>Gets whether this is typed text or text recognized in an image.</summary>
		public ParagraphKind Kind { get; set; }
	}


	/// <summary>
	/// What was true of a page when it was indexed.
	/// </summary>
	internal sealed class IndexStamp
	{
		/// <summary>Gets the OneNote page ID the page had. OneNote gives a page a new ID, and new
		/// object IDs for its paragraphs, when its notebook is reopened.</summary>
		public string PageID { get; set; }

		/// <summary>Gets the modified time the page had.</summary>
		public string Modified { get; set; }
	}


	/// <summary>
	/// A notebook's choice of whether to be indexed, keyed as the identity catalog keys it.
	/// </summary>
	internal sealed class IndexNotebook
	{
		public string NotebookKey { get; set; }
		public bool Included { get; set; }
		public string LastIndexed { get; set; }
	}


	/// <summary>
	/// Counts of what the index holds.
	/// </summary>
	internal sealed class IndexStatistics
	{
		public int Pages { get; set; }
		public int Paragraphs { get; set; }

		/// <summary>Gets the size the index takes in the database file, or zero if it could
		/// not be measured.</summary>
		public long Bytes { get; set; }
	}


	/// <summary>
	/// Stores the text of pages in OneMore.db with an FTS5 trigram index. Nothing here ever
	/// writes to a page; pages are known by the key given to them by the identity catalog.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The bundled System.Data.SQLite build does not register FTS5, but its interop library
	/// contains it and exports <c>sqlite3_fts5_init</c>, so it is loaded into each connection that
	/// opens this provider; see <see cref="LoadFts5"/>. A connection that has not loaded it can
	/// still use every other table of the database, including the plain tables of this index, but
	/// any statement that reaches the FTS5 table or the triggers that feed it fails. So everything
	/// that writes <c>index_text</c> must go through this provider.
	/// </para>
	/// <para>
	/// The trigram tokenizer finds a term anywhere inside a paragraph, ignoring case, which is how
	/// <see cref="TextMatchBuilder"/> treats a term. Paragraph text is kept in a plain table with
	/// the FTS5 table as an external-content index over it, so the text is stored once and callers
	/// can apply exact matching to the paragraphs returned. Check <see cref="IsAvailable"/>
	/// before constructing a provider.
	/// </para>
	/// </remarks>
	internal class IndexProvider : DatabaseProvider
	{
		private const string PrimaryTable = "index_page";
		private const string VersionTable = "index_schema";
		private const int CurrentVersion = 1;

		private const string InteropName = "SQLite.Interop.dll";
		private const string EntryPoint = "sqlite3_fts5_init";

		/// <summary>
		/// The shortest term the trigram tokenizer can look up; a shorter term cannot be answered
		/// by the index and must be matched some other way.
		/// </summary>
		public const int MinTermLength = 3;

		private static readonly Lazy<bool> available = new Lazy<bool>(ProbeFts5);


		/// <summary>
		/// Gets whether FTS5 with the trigram tokenizer can be used in this process. When false,
		/// callers must not use the text index and should fall back to scanning pages.
		/// </summary>
		public static bool IsAvailable => available.Value;


		/// <summary>
		/// Initialize this provider, opening the standard database
		/// </summary>
		public IndexProvider()
			: base()
		{
			RequireAvailable();

			if (CatalogExists())
			{
				OpenDatabase();
				UpgradeCatalog();
			}
			else
			{
				RefreshDataSchema("text index", Resources.TextIndexDB);
			}
		}


		/// <summary>
		/// Initialize this provider over an open connection, such as an in-memory database.
		/// </summary>
		internal IndexProvider(SQLiteConnection connection)
		{
			RequireAvailable();

			con = connection;
			OnDatabaseOpened(con);

			if (TableExists(PrimaryTable))
			{
				UpgradeCatalog();
			}
			else
			{
				RefreshDataSchema("text index", Resources.TextIndexDB);
			}
		}


		public static bool CatalogExists()
		{
			return CatalogExists(PrimaryTable);
		}


		public bool DropCatalog()
		{
			return DropCatalog("text index", Resources.TextIndexDB);
		}


		/// <summary>
		/// Removes everything that has been indexed and gives the space back to the database file,
		/// but keeps which notebooks the user chose, so that turning indexing on again does not
		/// ask for them again. Nothing is re-indexed until indexing is next run.
		/// </summary>
		/// <returns>The number of pages that had been indexed</returns>
		public int Clear()
		{
			int pages;

			// the triggers keep the FTS5 index in step with the deletes
			BeginImmediate();

			try
			{
				using var cmd = con.CreateCommand();
				cmd.CommandType = CommandType.Text;

				cmd.CommandText = "DELETE FROM index_text";
				cmd.ExecuteNonQuery();

				cmd.CommandText = "DELETE FROM index_page";
				pages = cmd.ExecuteNonQuery();

				// the times are no longer true; whether a notebook is chosen is kept
				cmd.CommandText = "UPDATE index_notebook SET lastIndexed = NULL";
				cmd.ExecuteNonQuery();

				Commit();
			}
			catch (Exception exc)
			{
				Rollback();
				logger.WriteLine("error clearing the text index", exc);
				throw;
			}

			// the deleted rows only free pages in the file; VACUUM, outside of a transaction,
			// gives the space back. If it cannot run, the space is reused by the next index.
			try
			{
				using var cmd = con.CreateCommand();
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "VACUUM";
				cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				logger.WriteLine("could not reclaim the space of the cleared text index", exc);
			}

			return pages;
		}


		// A catalog newer than this build knows is left alone. There are no upgrade steps yet; the
		// first release is version 1. Later releases chain their steps here, after that check, as
		// if (version == N) version = UpgradeNtoN+1().
		private void UpgradeCatalog()
		{
			var version = ReadSchemaVersion(VersionTable, "schemaID", missing: CurrentVersion);
			if (IsNewerThanKnown("text index", version, CurrentVersion))
			{
				return;
			}
		}


		#region FTS5
		private static void RequireAvailable()
		{
			if (!IsAvailable)
			{
				throw new NotSupportedException(
					"The text index requires FTS5, which is not available in this SQLite build");
			}
		}


		protected override void OnDatabaseOpened(SQLiteConnection connection)
		{
			if (!LoadFts5(connection))
			{
				throw new NotSupportedException("Could not load FTS5 for the text index");
			}
		}


		// Verifies, once, that FTS5 loads and supports the trigram tokenizer.
		private static bool ProbeFts5()
		{
			try
			{
				using var probe = new SQLiteConnection("Data Source=:memory:");
				probe.Open();

				if (!LoadFts5(probe))
				{
					Logger.Current.WriteLine("FTS5 could not be loaded; text index is disabled");
					return false;
				}

				using var cmd = probe.CreateCommand();
				cmd.CommandText =
					"CREATE VIRTUAL TABLE fts5_probe USING fts5(x, tokenize='trigram')";
				cmd.ExecuteNonQuery();
				return true;
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("FTS5 trigram is not available; text index is disabled", exc);
				return false;
			}
		}


		/// <summary>
		/// Loads FTS5 into the connection from the System.Data.SQLite interop library, which
		/// exports sqlite3_fts5_init even though it does not register the module itself.
		/// Extension loading is enabled only for the duration of the call so SQL on the
		/// connection can never load arbitrary libraries.
		/// </summary>
		/// <remarks>
		/// The load applies to a single connection, so every connection that touches the
		/// index tables must call this. The interop is tried by name first, which finds it
		/// when already loaded by System.Data.SQLite, then by its architecture folder.
		/// </remarks>
		private static bool LoadFts5(SQLiteConnection connection)
		{
			var candidates = new List<string> { InteropName };

			var folder = Path.GetDirectoryName(typeof(SQLiteConnection).Assembly.Location);
			if (!string.IsNullOrEmpty(folder))
			{
				var arch = RuntimeInformation.ProcessArchitecture switch
				{
					Architecture.X86 => "x86",
					Architecture.Arm64 => "arm64",
					_ => "x64"
				};

				candidates.Add(Path.Combine(folder, arch, InteropName));
				candidates.Add(Path.Combine(folder, InteropName));
			}

			foreach (var candidate in candidates)
			{
				try
				{
					connection.EnableExtensions(true);
					connection.LoadExtension(candidate, EntryPoint);
					return true;
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine($"could not load FTS5 from {candidate}", exc);
				}
				finally
				{
					connection.EnableExtensions(false);
				}
			}

			return false;
		}
		#endregion FTS5


		#region Write
		/// <summary>
		/// Replaces everything stored for a page with its current paragraphs, in one transaction,
		/// and records the modified time of the page that was indexed.
		/// </summary>
		/// <param name="pageKey">The page key from the identity catalog</param>
		/// <param name="pageID">The OneNote page ID the paragraph object IDs belong to</param>
		/// <param name="modified">The modified time of the page, from the hierarchy, as it was when
		/// the page was read</param>
		/// <param name="paragraphs">The paragraphs of the page that have text</param>
		public void ReplacePage(
			long pageKey, string pageID, string modified, IReadOnlyCollection<ParagraphText> paragraphs)
		{
			BeginImmediate();

			try
			{
				using var cmd = con.CreateCommand();
				cmd.CommandType = CommandType.Text;

				cmd.CommandText = "DELETE FROM index_text WHERE pageKey = @pageKey";
				cmd.Parameters.AddWithValue("@pageKey", pageKey);
				cmd.ExecuteNonQuery();

				cmd.Parameters.Clear();
				cmd.CommandText =
					"INSERT INTO index_text (pageKey, objectID, ordinal, text, kind) " +
					"VALUES (@pageKey, @objectID, @ordinal, @text, @kind)";

				cmd.Parameters.Add("@pageKey", DbType.Int64).Value = pageKey;
				var objectID = cmd.Parameters.Add("@objectID", DbType.String);
				var ordinal = cmd.Parameters.Add("@ordinal", DbType.Int32);
				var text = cmd.Parameters.Add("@text", DbType.String);
				var kind = cmd.Parameters.Add("@kind", DbType.Int32);

				foreach (var paragraph in paragraphs)
				{
					objectID.Value = paragraph.ObjectID;
					ordinal.Value = paragraph.Ordinal;
					text.Value = paragraph.Text;
					kind.Value = (int)paragraph.Kind;
					cmd.ExecuteNonQuery();
				}

				cmd.Parameters.Clear();
				cmd.CommandText =
					"INSERT INTO index_page (pageKey, pageID, indexedModified, indexedAt, paragraphs) " +
					"VALUES (@pageKey, @pageID, @modified, @now, @count) " +
					"ON CONFLICT(pageKey) DO UPDATE SET pageID = @pageID, indexedModified = @modified, " +
					"indexedAt = @now, paragraphs = @count";

				cmd.Parameters.AddWithValue("@pageKey", pageKey);
				cmd.Parameters.AddWithValue("@pageID", pageID ?? string.Empty);
				cmd.Parameters.AddWithValue("@modified", modified ?? string.Empty);
				cmd.Parameters.AddWithValue("@now", DateTime.Now.ToZuluString());
				cmd.Parameters.AddWithValue("@count", paragraphs.Count);
				cmd.ExecuteNonQuery();

				Commit();
			}
			catch (Exception exc)
			{
				Rollback();
				logger.WriteLine($"error indexing page {pageKey}", exc);
				throw;
			}
		}


		/// <summary>
		/// Removes everything stored for pages, such as pages the identity catalog has forgotten.
		/// </summary>
		/// <returns>The number of pages that had been indexed</returns>
		public int DeletePages(IReadOnlyCollection<long> pageKeys)
		{
			if (pageKeys is null || pageKeys.Count == 0)
			{
				return 0;
			}

			BeginImmediate();

			try
			{
				using var cmd = con.CreateCommand();
				cmd.CommandType = CommandType.Text;
				var key = cmd.Parameters.Add("@pageKey", DbType.Int64);

				var count = 0;
				foreach (var pageKey in pageKeys)
				{
					key.Value = pageKey;

					cmd.CommandText = "DELETE FROM index_text WHERE pageKey = @pageKey";
					cmd.ExecuteNonQuery();

					cmd.CommandText = "DELETE FROM index_page WHERE pageKey = @pageKey";
					count += cmd.ExecuteNonQuery();
				}

				Commit();
				return count;
			}
			catch (Exception exc)
			{
				Rollback();
				logger.WriteLine("error removing pages from the text index", exc);
				throw;
			}
		}


		/// <summary>
		/// Removes everything stored for the pages of notebooks that are no longer chosen, so that
		/// the index does not keep out of date text, and the space it takes, for notebooks that are
		/// not being kept current. A page is known to belong to a notebook by the identity
		/// catalog, which lives in the same database, so this also reaches notebooks that are closed.
		/// </summary>
		/// <param name="isChosen">Says whether the notebook, by its notebook key, is chosen</param>
		/// <returns>The number of pages removed</returns>
		public int PruneNotebooks(Func<string, bool> isChosen)
		{
			// without the catalog there is no way to tell which notebook a page belongs to
			if (!TableExists("identity_page"))
			{
				return 0;
			}

			var unchosen = new List<string>();

			using (var cmd = con.CreateCommand())
			{
				cmd.CommandType = CommandType.Text;
				cmd.CommandText =
					"SELECT DISTINCT i.notebookKey FROM identity_page i " +
					"JOIN index_page p ON p.pageKey = i.pageKey";

				using var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					var key = reader.GetString(0);
					if (!isChosen(key))
					{
						unchosen.Add(key);
					}
				}
			}

			if (unchosen.Count == 0)
			{
				return 0;
			}

			BeginImmediate();

			try
			{
				using var cmd = con.CreateCommand();
				cmd.CommandType = CommandType.Text;
				var key = cmd.Parameters.Add("@key", DbType.String);

				var pages = 0;
				foreach (var notebookKey in unchosen)
				{
					key.Value = notebookKey;

					cmd.CommandText =
						"DELETE FROM index_text WHERE pageKey IN " +
						"(SELECT pageKey FROM identity_page WHERE notebookKey = @key)";
					cmd.ExecuteNonQuery();

					cmd.CommandText =
						"DELETE FROM index_page WHERE pageKey IN " +
						"(SELECT pageKey FROM identity_page WHERE notebookKey = @key)";
					pages += cmd.ExecuteNonQuery();
				}

				Commit();
				return pages;
			}
			catch (Exception exc)
			{
				Rollback();
				logger.WriteLine("error removing unchosen notebooks from the text index", exc);
				throw;
			}
		}


		/// <summary>
		/// Records whether a notebook is indexed.
		/// </summary>
		/// <param name="notebookKey">The notebook key, as <see cref="Identity.PageIdentityKeys"/>
		/// builds it</param>
		/// <param name="included">True if the notebook should be indexed</param>
		/// <param name="lastIndexed">Optional. When the notebook was last indexed; null leaves
		/// the recorded time as it is</param>
		public void WriteNotebook(string notebookKey, bool included, string lastIndexed = null)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText =
				"INSERT INTO index_notebook (notebookKey, included, lastIndexed) " +
				"VALUES (@key, @included, @last) " +
				"ON CONFLICT(notebookKey) DO UPDATE SET included = @included, " +
				"lastIndexed = COALESCE(@last, lastIndexed)";

			cmd.Parameters.AddWithValue("@key", notebookKey);
			cmd.Parameters.AddWithValue("@included", included ? 1 : 0);
			cmd.Parameters.AddWithValue("@last", (object)lastIndexed ?? DBNull.Value);
			cmd.ExecuteNonQuery();
		}


		/// <summary>
		/// Rebuilds the FTS5 index from the text table, as after a bulk load that bypassed the
		/// triggers, and then merges its segments.
		/// </summary>
		public void Rebuild()
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;

			cmd.CommandText = "INSERT INTO index_fts(index_fts) VALUES ('rebuild')";
			cmd.ExecuteNonQuery();

			cmd.CommandText = "INSERT INTO index_fts(index_fts) VALUES ('optimize')";
			cmd.ExecuteNonQuery();
		}
		#endregion Write


		#region Read
		/// <summary>
		/// Gets what was true of each indexed page when it was indexed, by page key. A page that is
		/// absent has never been indexed.
		/// </summary>
		public Dictionary<long, IndexStamp> ReadStamps()
		{
			var stamps = new Dictionary<long, IndexStamp>();

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "SELECT pageKey, pageID, indexedModified FROM index_page";

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				stamps[reader.GetInt64(0)] = new IndexStamp
				{
					PageID = reader.GetString(1),
					Modified = reader.GetString(2)
				};
			}

			return stamps;
		}


		/// <summary>
		/// Gets every notebook that has been given a choice about being indexed.
		/// </summary>
		public List<IndexNotebook> ReadNotebooks()
		{
			var notebooks = new List<IndexNotebook>();

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText =
				"SELECT notebookKey, included, lastIndexed FROM index_notebook ORDER BY notebookKey";

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				notebooks.Add(new IndexNotebook
				{
					NotebookKey = reader.GetString(0),
					Included = reader.GetInt32(1) != 0,
					LastIndexed = reader.IsDBNull(2) ? null : reader.GetString(2)
				});
			}

			return notebooks;
		}


		/// <summary>
		/// Finds paragraphs with an FTS5 MATCH expression. The expression is passed to FTS5 as it
		/// is, so the caller is responsible for quoting terms; see the trigram note on
		/// <see cref="MinTermLength"/>.
		/// </summary>
		/// <param name="match">An FTS5 query, such as <c>"error" AND "fail"</c></param>
		/// <param name="includeOcr">True to include the text recognized in images</param>
		/// <returns>The paragraphs, in page and document order</returns>
		public List<IndexHit> Find(string match, bool includeOcr = true)
		{
			var hits = new List<IndexHit>();

			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText =
				"SELECT t.pageKey, t.objectID, t.ordinal, t.text, t.kind " +
				"FROM index_fts f JOIN index_text t ON t.textID = f.rowid " +
				"WHERE index_fts MATCH @match " +
				(includeOcr ? string.Empty : "AND t.kind = 0 ") +
				"ORDER BY t.pageKey, t.ordinal";

			cmd.Parameters.AddWithValue("@match", match);

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				hits.Add(new IndexHit
				{
					PageKey = reader.GetInt64(0),
					ObjectID = reader.GetString(1),
					Ordinal = reader.GetInt32(2),
					Text = reader.GetString(3),
					Kind = (ParagraphKind)reader.GetInt32(4)
				});
			}

			return hits;
		}


		/// <summary>
		/// Streams the paragraphs that may match a query, so that a large index is not held in
		/// memory at once.
		/// </summary>
		/// <param name="match">An FTS5 query as for <see cref="Find"/>, or null for every paragraph
		/// of the index</param>
		/// <param name="includeOcr">True to include the text recognized in images</param>
		/// <returns>The paragraphs, in page and document order. The index must not be written
		/// through this provider until the enumeration is finished or disposed.</returns>
		public IEnumerable<IndexHit> Query(string match, bool includeOcr = true)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;

			if (match is null)
			{
				cmd.CommandText =
					"SELECT pageKey, objectID, ordinal, text, kind FROM index_text " +
					(includeOcr ? string.Empty : "WHERE kind = 0 ") +
					"ORDER BY pageKey, ordinal";
			}
			else
			{
				cmd.CommandText =
					"SELECT t.pageKey, t.objectID, t.ordinal, t.text, t.kind " +
					"FROM index_fts f JOIN index_text t ON t.textID = f.rowid " +
					"WHERE index_fts MATCH @match " +
					(includeOcr ? string.Empty : "AND t.kind = 0 ") +
					"ORDER BY t.pageKey, t.ordinal";

				cmd.Parameters.AddWithValue("@match", match);
			}

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				yield return new IndexHit
				{
					PageKey = reader.GetInt64(0),
					ObjectID = reader.GetString(1),
					Ordinal = reader.GetInt32(2),
					Text = reader.GetString(3),
					Kind = (ParagraphKind)reader.GetInt32(4)
				};
			}
		}


		/// <summary>
		/// Gets how much the index holds.
		/// </summary>
		/// <param name="isChosen">Optional. Says whether a notebook, by its notebook key, is chosen.
		/// When given, only the pages of chosen notebooks are counted, which is what the index is
		/// for even before the pages of other notebooks have been removed. The size is always that
		/// of the whole index. A page whose notebook is not known to the identity catalog is not
		/// counted.</param>
		public IndexStatistics ReadStatistics(Func<string, bool> isChosen = null)
		{
			var stats = new IndexStatistics();

			using (var cmd = con.CreateCommand())
			{
				cmd.CommandType = CommandType.Text;

				if (isChosen is null || !TableExists("identity_page"))
				{
					cmd.CommandText =
						"SELECT (SELECT COUNT(1) FROM index_page), (SELECT COUNT(1) FROM index_text)";

					using var reader = cmd.ExecuteReader();
					reader.Read();

					stats.Pages = reader.GetInt32(0);
					stats.Paragraphs = reader.GetInt32(1);
				}
				else
				{
					cmd.CommandText =
						"SELECT i.notebookKey, COUNT(1), COALESCE(SUM(p.paragraphs), 0) " +
						"FROM index_page p JOIN identity_page i ON i.pageKey = p.pageKey " +
						"GROUP BY i.notebookKey";

					using var reader = cmd.ExecuteReader();
					while (reader.Read())
					{
						if (isChosen(reader.GetString(0)))
						{
							stats.Pages += reader.GetInt32(1);
							stats.Paragraphs += reader.GetInt32(2);
						}
					}
				}
			}

			// what the index takes in the file: its tables and the shadow tables of the FTS5 index
			try
			{
				using var cmd = con.CreateCommand();
				cmd.CommandType = CommandType.Text;
				cmd.CommandText =
					"SELECT COALESCE(SUM(pgsize), 0) FROM dbstat " +
					"WHERE name LIKE 'index\\_%' ESCAPE '\\' OR name LIKE 'IDX\\_index\\_%' ESCAPE '\\'";

				stats.Bytes = Convert.ToInt64(cmd.ExecuteScalar());
			}
			catch (Exception exc)
			{
				// dbstat is a diagnostic aid and may not be present in every build
				logger.Verbose($"could not measure the text index size: {exc.Message}");
			}

			return stats;
		}
		#endregion Read
	}
}
