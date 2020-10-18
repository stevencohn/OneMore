//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Xml.Linq;


	internal abstract class TableProperties
	{

		protected XNamespace ns;


		protected TableProperties()
		{
		}


		protected TableProperties(XNamespace ns)
		{
			this.ns = ns;
		}


		protected TableProperties(XElement root)
		{
			Root = root;
			ns = Root.GetNamespaceOfPrefix("one");
		}


		/// <summary>
		/// Gest the root XElement of this object
		/// </summary>
		public XElement Root { get; protected set; }


		public string ObjectId
		{
			get { return GetAttribute("objectID"); }
			set { SetAttribute("objectID", value); }
		}


		public Selection Selected
		{
			get { return GetEnumAttribute<Selection>("selected"); }
			set { SetAttribute("selected", value.ToString()); }
		}


		// CascadedTextAttributes - - - - - - - - - - - - -

		public string Style
		{
			get { return GetAttribute("style"); }
			set { SetAttribute("style", value); }
		}


		public int QuickStyleIndex
		{
			get { return int.Parse("0" + GetAttribute("quickStyleIndex")); }
			set { SetAttribute("quickStyleIndex", value.ToString()); }
		}


		public string Lang
		{
			get { return GetAttribute("lang"); }
			set { SetAttribute("lang", value); }
		}


		// EditedByAttributes - - - - - - - - - - - - - - -

		public string Author
		{
			get { return GetAttribute("author"); }
			set { SetAttribute("author", value); }
		}


		public string AuthorInitials
		{
			get { return GetAttribute("authorInitials"); }
			set { SetAttribute("authorInitials", value); }
		}


		public string AuthorResolutionID
		{
			get { return GetAttribute("authorResolutionID"); }
			set { SetAttribute("authorResolutionID", value); }
		}


		public string LastModifiedBy
		{
			get { return GetAttribute("lastModifiedBy"); }
			set { SetAttribute("lastModifiedBy", value); }
		}


		public string LastModifiedByInitials
		{
			get { return GetAttribute("lastModifiedByInitials"); }
			set { SetAttribute("lastModifiedByInitials", value); }
		}


		public string LastModifiedByResolutionID
		{
			get { return GetAttribute("lastModifiedByResolutionID"); }
			set { SetAttribute("lastModifiedByResolutionID", value); }
		}


		// Helpers - - - - - - - - - - - - - - - - - - - - -

		protected string GetAttribute(string name)
		{
			var attr = Root.Attribute(name);
			return attr == null ? string.Empty : attr.Value;
		}


		protected bool GetBooleanAttribute(string name)
		{
			var text = GetAttribute(name);

			if (Enum.TryParse(text, out bool value))
			{
				return value;
			}

			return default;
		}


		protected T GetEnumAttribute<T>(string name) where T : struct
		{
			var text = GetAttribute(name);

			if (Enum.TryParse(text, out T value))
			{
				return value;
			}

			return default;
		}


		protected void SetAttribute(string name, string value)
		{
			var attr = Root.Attribute(name);
			if (attr == null)
			{
				if (!string.IsNullOrEmpty(value))
				{
					Root.Add(new XAttribute(name, value));
				}
			}
			else
			{
				if (string.IsNullOrEmpty(value))
				{
					attr.Remove();
				}
				else
				{
					attr.Value = value;
				}
			}
		}
	}
}
