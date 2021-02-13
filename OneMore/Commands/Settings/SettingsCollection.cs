//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Globalization;
	using System.Xml.Linq;


	internal class SettingsCollection
	{

		private readonly Dictionary<string, object> properties;


		public SettingsCollection(string name)
		{
			Name = name;
			properties = new Dictionary<string, object>();
		}


		public int Count => properties.Count;

		public IEnumerable<string> Keys => properties.Keys;


		public string Name { get; }


		public string this[string name] => properties.ContainsKey(name) ? (string)properties[name] : null;


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


		public void Add(string name, XElement element)
		{
			if (!properties.ContainsKey(name))
			{
				properties.Add(name, element);
			}
			else
			{
				properties[name] = element;
			}
		}


		public T Get<T>(string name, T defaultValue = default)
		{
			if (defaultValue == null)
			{
				defaultValue = default;
			}

			try
			{
				if (typeof(T) == typeof(XElement))
				{
					return properties.ContainsKey(name)
						? (T)properties[name]
						: defaultValue;
				}
				else
				{
					var converter = TypeDescriptor.GetConverter(typeof(T));
					return properties.ContainsKey(name)
						? (T)converter.ConvertFromString(
							null, CultureInfo.InvariantCulture, (string)properties[name])
						: defaultValue;
				}
			}
			catch
			{
				return default;
			}
		}


		public bool IsElement(string name)
		{
			if (properties.ContainsKey(name))
			{
				return properties[name] is XElement;
			}

			return false;
		}


		public void Remove(string name)
		{
			if (properties.ContainsKey(name))
			{
				properties.Remove(name);
			}
		}
	}
}
