//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System.Xml.Linq;


	internal class Notebook
	{
		public Notebook(XElement element)
		{
			Color = element.Attribute("color").Value;
			ID = element.Attribute("ID").Value;
			Name = element.Attribute("name").Value;
		}


		public bool Checked { get; set; }

		public string Color { get; set; }


		public string ID { get; set; }


		public string Name { get; set; }


		public override string ToString()
		{
			return Name;
		}
	}
}
