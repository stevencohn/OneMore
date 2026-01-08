//************************************************************************************************
// Service and settings for OneSearch integration
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.OneSearch
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Threading.Tasks;


	internal sealed class OneSearchSettings
	{
		private const string FolderName = "OneSearch";
		private const string SettingsFileName = "settings.ini";

		public string CacheRoot { get; set; }
		public bool UseRegex { get; set; }
		public bool CaseSensitive { get; set; }
		public SearchScope DefaultScope { get; set; }


		public static OneSearchSettings Load()
		{
			var settings = new OneSearchSettings
			{
				CacheRoot = GetDefaultCacheRoot(),
				UseRegex = false,
				CaseSensitive = false,
				DefaultScope = SearchScope.AllNotebooks
			};

			var path = GetSettingsPath();
			if (!File.Exists(path))
			{
				return settings;
			}

			foreach (var line in File.ReadAllLines(path, Encoding.UTF8))
			{
				var trimmed = line.Trim();
				if (trimmed.Length == 0 || trimmed.StartsWith("#", StringComparison.Ordinal))
				{
					continue;
				}

				var parts = trimmed.Split(new[] { '=' }, 2);
				if (parts.Length != 2)
				{
					continue;
				}

				var key = parts[0].Trim();
				var value = parts[1].Trim();

				switch (key)
				{
					case "cacheRoot":
						if (!string.IsNullOrWhiteSpace(value))
						{
							settings.CacheRoot = value;
						}
						break;

					case "useRegex":
						settings.UseRegex = value.Equals("true", StringComparison.OrdinalIgnoreCase);
						break;

					case "caseSensitive":
						settings.CaseSensitive = value.Equals("true", StringComparison.OrdinalIgnoreCase);
						break;

					case "defaultScope":
						if (Enum.TryParse(value, true, out SearchScope scope))
						{
							settings.DefaultScope = scope;
						}
						break;
				}
			}

			return settings;
		}


		public void Save()
		{
			var path = GetSettingsPath();
			var directory = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(directory))
			{
				Directory.CreateDirectory(directory);
			}

			var lines = new[]
			{
				"cacheRoot=" + (CacheRoot ?? string.Empty),
				"useRegex=" + (UseRegex ? "true" : "false"),
				"caseSensitive=" + (CaseSensitive ? "true" : "false"),
				"defaultScope=" + DefaultScope
			};

			File.WriteAllLines(path, lines, new UTF8Encoding(false));
		}


		public static string GetSettingsPath()
		{
			var baseFolder = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				FolderName);

			return Path.Combine(baseFolder, SettingsFileName);
		}


		public static string GetDefaultCacheRoot()
		{
			return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				FolderName,
				"cache");
		}
	}


	internal sealed class OneSearchContext
	{
		public string CurrentPageId { get; set; }
		public string CurrentSectionId { get; set; }
		public string CurrentNotebookId { get; set; }
	}


	internal sealed class OneSearchService
	{
		private readonly ILogger logger;

		public OneSearchService(ILogger logger)
		{
			this.logger = logger;
		}


		public OneSearchSettings LoadSettings() => OneSearchSettings.Load();


		public void SaveSettings(OneSearchSettings settings) => settings?.Save();


		public OneSearchContext GetContext()
		{
			using var one = new OneNote();

			var pageId = one.CurrentPageId;
			var sectionId = one.GetParent(pageId);
			var notebookId = one.GetParent(sectionId);

			return new OneSearchContext
			{
				CurrentPageId = pageId,
				CurrentSectionId = sectionId,
				CurrentNotebookId = notebookId
			};
		}


		public ExportSummary Sync(string cacheRoot)
			=> new OneNoteExporter(logger).ExportAll(cacheRoot);


		public Task<ExportSummary> SyncAsync(
			string cacheRoot,
			IProgress<SyncProgress> progress,
			System.Threading.CancellationToken cancellationToken,
			System.Threading.ManualResetEventSlim pauseEvent)
		{
			return StaTask.Run(() =>
				new OneNoteExporter(logger).ExportAll(cacheRoot, progress, cancellationToken, pauseEvent));
		}


		public List<SearchResult> Search(SearchQuery query)
		{
			return new SearchEngine().Search(query);
		}


		public void ClearCache(string cacheRoot) => CacheManager.ClearCache(cacheRoot);


		public bool NavigateTo(string pageId)
		{
			if (string.IsNullOrWhiteSpace(pageId))
			{
				return false;
			}

			using var one = new OneNote();
			return one.NavigateTo(pageId, string.Empty).GetAwaiter().GetResult();
		}
	}


	internal static class StaTask
	{
		public static Task<T> Run<T>(Func<T> action)
		{
			var tcs = new TaskCompletionSource<T>();

			var thread = new System.Threading.Thread(() =>
			{
				try
				{
					var result = action();
					tcs.SetResult(result);
				}
				catch (Exception exc)
				{
					tcs.SetException(exc);
				}
			});

			thread.IsBackground = true;
			thread.SetApartmentState(System.Threading.ApartmentState.STA);
			thread.Start();

			return tcs.Task;
		}
	}
}
