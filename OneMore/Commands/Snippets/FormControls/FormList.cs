//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;


	/// <summary>
	/// A user-defined list of items offered by a List form control.
	/// </summary>
	internal class FormList
	{
		public FormList(string name)
		{
			Name = name;
		}


		public string Name { get; set; }


		public List<string> Items { get; } = new();
	}
}
