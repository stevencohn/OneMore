//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Settings
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Drawing;
	using System.IO;
	using System.Web.Script.Serialization;


	/// <summary>
	/// Loads, save, and manages user settings
	/// </summary>
	internal class SearchEngineProvider
	{
		private readonly string path;


		/// <summary>
		/// Initialize a new provider.
		/// </summary>
		public SearchEngineProvider()
		{
			path = Path.Combine(
				PathFactory.GetAppDataPath(), "SearchEngines.json");
		}


		public List<SearchEngine> LoadEngines()
		{
			var engines = new List<SearchEngine>();

			if (File.Exists(path))
			{
				try
				{
					var json = File.ReadAllText(path);
					var serializer = new JavaScriptSerializer();
					engines.AddRange(serializer.Deserialize<SearchEngine[]>(json));
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine($"Error reading {path}", exc);
				}
			}

			return engines;
		}


		public void Save(List<SearchEngine> engines)
		{
			try
			{
				var serializer = new JavaScriptSerializer();
				serializer.RegisterConverters(new JavaScriptConverter[]
				{
					new EnginesConverter()
				});
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"Error saving {path}", exc);
			}
		}


		private class EnginesConverter : JavaScriptConverter
		{
			public override IEnumerable<Type> SupportedTypes =>
				 new ReadOnlyCollection<Type>(new List<Type>(new Type[] { typeof(List<SearchEngine>) }));


			public override object Deserialize(
				IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
			{
				if (dictionary == null)
					throw new ArgumentNullException(nameof(dictionary));

				if (type == typeof(List<SearchEngine>))
				{
					var engines = new List<SearchEngine>();

					var list = (ArrayList)dictionary["Engines"];
					for (int i = 0; i < list.Count; i++)
					{
						engines.Add(serializer.ConvertToType<SearchEngine>(list[i]));
					}

					return engines;
				}

				return null;
			}


			public override IDictionary<string, object> Serialize(
				object obj, JavaScriptSerializer serializer)
			{
				var engines = obj as List<SearchEngine>;
				if (engines != null)
				{
					var result = new Dictionary<string, object>();
					var list = new ArrayList();
					foreach (var engine in engines)
					{
						var data = Convert.ToBase64String(
							(byte[])new ImageConverter().ConvertTo(engine.Image, typeof(byte[])));

						var dict = new Dictionary<string, object>
						{
							{ "Image", data },
							{ "Name", engine.Name },
							{ "Uri", engine.Uri }
						};
						list.Add(dict);
					}
					result["Engines"] = list;
					return result;
				}

				return new Dictionary<string, object>();
			}
		}
	}
}
