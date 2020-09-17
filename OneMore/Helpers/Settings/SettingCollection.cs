//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Settings
{
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;


	internal class SettingCollection
	{
		private readonly Dictionary<string, string> properties;


		public SettingCollection(string name)
		{
			Name = name;
			properties = new Dictionary<string, string>();
		}


		public IEnumerable<string> Keys => properties.Keys.ToList();


		public string Name { get; }


		public string this[string name] => properties.ContainsKey(name) ? properties[name] : null;


		public void Add(string name, string value)
		{
			if (!properties.ContainsKey(name))
			{
				properties.Add(name, value);
			}
			else
			{
				properties[name] = value;
			}
		}


		public void Add(string name, bool value)
		{
			Add(name, value.ToString().ToLower());
		}


		public void Add(string name, int value)
		{
			Add(name, value.ToString());
		}


		public T Get<T>(string name)
		{
			var converter = TypeDescriptor.GetConverter(typeof(T));

			try
			{
				return properties.ContainsKey(name)
					? (T)converter.ConvertFromString(properties[name])
					: default;
			}
			catch
			{
				return default;
			}
		}
	}
}
