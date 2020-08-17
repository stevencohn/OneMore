//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Xml.Linq;


	internal abstract class CommonAttributes
	{

		protected XElement root;


		protected CommonAttributes()
		{
		}


		protected CommonAttributes(XElement root)
		{
			this.root = root;
		}


		// CascadedTextAttributes

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


		// EditedByAttributes

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


		// Helpers

		protected string GetAttribute(string name)
		{
			var attr = root.Attribute("name");
			if (attr == null)
			{
				return string.Empty;
			}

			return attr.Value;
		}


		protected void SetAttribute(string name, string value)
		{
			var attr = root.Attribute("name");
			if (attr == null)
			{
				root.Add(new XAttribute(name, value));
			}
			else
			{
				attr.Value = value;
			}
		}
	}
}
