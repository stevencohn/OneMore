//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S112 // General exceptions should never be thrown
#pragma warning disable S2372 // Exceptions should not be thrown from property getters

namespace River.OneMoreAddIn.Models
{
	using System;
	using System.Xml.Linq;


	/// <summary>
	/// OneMore page models are typically used in conjunction with the PageNamespace class.
	/// Set the PageNamespace.Value property prior to using a model class. Otherwise, use
	/// the model constructors that accept an XNamespace argument.
	/// </summary>
	internal static class PageNamespace
	{
		private static XNamespace ns;


		public static XNamespace Value
		{
			get
			{
				if (ns == null)
				{
					throw new NullReferenceException("PageNamespace must be set");
				}

				return ns;
			}

			private set
			{
				ns = value;
			}
		}


		public static void Set(string value)
		{
			ns = value;
		}


		public static void Set(XNamespace value)
		{
			ns = value;
		}
	}
}
