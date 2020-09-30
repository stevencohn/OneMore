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

		#region EnginesConverter
		/// <summary>
		/// Custom converter to handle serializing Web site favicon icons to/from base64 strings
		/// </summary>
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
						dynamic item = list[i];
						SearchEngine engine = null;

						string data = item["Image"];
						if (!string.IsNullOrEmpty(data))
						{
							var bytes = Convert.FromBase64String(data);
							using (var stream = new MemoryStream(bytes, 0, bytes.Length))
							{
								engine = new SearchEngine
								{
									Image = Image.FromStream(stream),
									Name = item["Name"],
									Uri = item["Uri"]
								};
							}
						}
						else
						{
							engine = new SearchEngine
							{
								Name = item["Name"],
								Uri = item["Uri"]
							};
						}

						engines.Add(engine);
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
						var dict = new Dictionary<string, object>
						{
							{ "Image", engine.Image.ToBase64String() },
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
		#endregion EnginesConverter


		private readonly string path;


		/// <summary>
		/// Initialize a new provider.
		/// </summary>
		public SearchEngineProvider()
		{
			path = Path.Combine(
				PathFactory.GetAppDataPath(), "SearchEngines.json");
		}


		public List<SearchEngine> Load()
		{
			var engines = new List<SearchEngine>();

			if (File.Exists(path))
			{
				try
				{
					var json = File.ReadAllText(path);

					var serializer = new JavaScriptSerializer();
					serializer.RegisterConverters(new JavaScriptConverter[] { new EnginesConverter() });

					engines.AddRange(serializer.Deserialize<List<SearchEngine>>(json));
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
				serializer.RegisterConverters(new JavaScriptConverter[] { new EnginesConverter() });

				File.WriteAllText(path, serializer.Serialize(engines));
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"Error saving {path}", exc);
			}
		}
	}
}
