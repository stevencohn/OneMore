//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using WIA;


	internal static class WiaExtensions
	{
		/// <summary>
		/// Safe way to get a property value
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="properties"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static T Get<T>(this Properties properties, string key)
		{
			foreach (Property property in properties)
			{
				if (property.Name.Equals(key) || property.PropertyID.Equals(key))
				{
					return (T)property.get_Value();
				}
			}
			return default;
		}


		/// <summary>
		/// Safe way to set a property value
		/// </summary>
		/// <param name="properties"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public static void Set(this Properties properties, object key, object value)
		{
			foreach (Property property in properties)
			{
				if (property.Name.Equals(key) || property.PropertyID.Equals(key))
				{
					property.set_Value(value);
					return;
				}
			}
		}
	}
}
