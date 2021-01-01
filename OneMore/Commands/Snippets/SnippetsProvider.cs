//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;


	internal class SnippetsProvider
	{
		private const string DirectoryName = "Snippets";
		private const string Extension = ".snp";

		private readonly string store;


		public SnippetsProvider()
		{
			store = Path.Combine(PathFactory.GetAppDataPath(), DirectoryName);
		}


		public List<string> GetNames()
		{
			var names = new List<string>();

			if (Directory.Exists(store))
			{
				foreach (var file in Directory.GetFiles(
					store, $"*{Extension}", SearchOption.AllDirectories))
				{
					names.Add(Path.GetFileNameWithoutExtension(file));
				}
			}

			return names;
		}


		public async Task Save(string snippet, string name)
		{
			var path = Path.Combine(store, $"{name}{Extension}");

			try
			{
				PathFactory.EnsurePathExists(store);

				using (var writer = new StreamWriter(path))
				{
					await writer.WriteAsync(snippet);
				}
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"error saving snippet to {path}", exc);
			}
		}
	}
}
