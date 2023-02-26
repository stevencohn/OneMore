//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class NavigationProvider : Loggable
	{
		private static readonly SemaphoreSlim semaphore = new(1);
		private readonly string path;


		public NavigationProvider()
		{
			path = Path.Combine(PathHelper.GetAppDataPath(), "Navigator.xml");
		}


		public async Task<List<string>> ReadHistory()
		{
			try
			{
				await semaphore.WaitAsync();

				var root = await Read();

				var history = root.Element("history");
				if (history == null)
				{
					return new List<string>();
				}

				return history.Elements("page").Select(e => e.Value).ToList();
			}
			finally
			{
				semaphore.Release();
			}
		}


		private async Task<XElement> Read()
		{
			XElement root = null;

			if (File.Exists(path))
			{
				try
				{
					// ensure we have ReadWrite sharing enabled so we don't block access
					// between NavigationService and NavigationDialog
					using var stream = new FileStream(path,
						FileMode.Open,
						FileAccess.Read,
						FileShare.ReadWrite);

					using var reader = new StreamReader(stream);

					root = XElement.Parse(await reader.ReadToEndAsync());
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error reading {path}", exc);
				}
			}

			// file not found then initialize with defaults
			root ??= new XElement("navigation",
				new XElement("history"),
				new XElement("pinned")
			);

			return root;
		}


		public async Task<bool> RecordHistory(string pageID, int depth)
		{
			await semaphore.WaitAsync();

			try
			{
				var root = await Read();

				var history = root.Element("history");
				if (history == null)
				{
					history = new XElement("history");
					root.Add(history);
				}

				var updated = false;

				var node = history.Elements("page").FirstOrDefault(e => e.Value == pageID);
				if (node == null)
				{
					history.AddFirst(new XElement("page", pageID));
					updated = true;
				}
				else
				{
					if (node != history.Elements().First())
					{
						node.Remove();
						history.AddFirst(node);
						updated = true;
					}
				}

				if (updated)
				{
					if (history.Elements().Count() > depth)
					{
						history.Elements().Skip(depth).Remove();
					}

					var xml = root.ToString(SaveOptions.None);

					try
					{
						// ensure we have ReadWrite sharing enabled so we don't block access
						// between NavigationService and NavigationDialog
						using var stream = new FileStream(path,
							FileMode.OpenOrCreate,
							FileAccess.Write,
							FileShare.ReadWrite);

						using var writer = new StreamWriter(stream);

						await writer.WriteAsync(xml);
						
						logger.Verbose($"history {pageID}");
					}
					catch (Exception exc)
					{
						logger.WriteLine($"error reading {path}", exc);
					}
				}

				return updated;
			}
			finally
			{
				semaphore.Release();
			}
		}
	}
}
