//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Favorites
{
	using River.OneMoreAddIn.Commands.Workspaces;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Obsolete, Ribbon XML model
	/// </summary>
	internal sealed class FileFavorite
	{
		public int Index { get; set; }
		public string Name { get; set; }
		public string Location { get; set; }
		public string Uri { get; set; }
		public string NotebookID { get; set; }
		public string ObjectID { get; set; }
		public TargetStatus Status { get; set; }
		public XElement Root { get; set; }
	}


	/// <summary>
	/// Obsolete, Ribbon XML file reader
	/// </summary>
	internal sealed class FavoritesFileProvider : Loggable
	{

		private static readonly string GotoAction = "GotoFavoriteCmd";
		private readonly string path;


		public FavoritesFileProvider()
		{
			path = Path.Combine(PathHelper.GetAppDataPath(), Resx.FavoritesFilename);
		}


		/// <summary>
		/// Test-only constructor allowing the favorites file path to be overridden.
		/// </summary>
		internal FavoritesFileProvider(string path)
		{
			this.path = path;
		}


		public List<FileFavorite> LoadFavorites()
		{
			var list = new List<FileFavorite>();

			if (!File.Exists(path))
			{
				return list;
			}

			try
			{
				var root = XElement.Load(path, LoadOptions.None);
				var ns = root.Name.Namespace;

				var elements = root.Elements(ns + "button")
					.Where(e => e.Attribute("onAction")?.Value == GotoAction);

				int sortOrder = 0;
				foreach (var element in elements)
				{
					list.Add(new FileFavorite
					{
						Index = sortOrder++,
						Name = element.Attribute("label").Value,
						Location = element.Attribute("screentip").Value,
						Uri = element.Attribute("tag").Value,
						NotebookID = element.Attribute("notebookID")?.Value,
						ObjectID = element.Attribute("objectID")?.Value
					});
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error loading {Resx.FavoritesFilename}", exc);
				list.Clear();
			}

			return list;
		}
	}
}
