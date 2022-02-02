//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;


	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="https://evernote.com/blog/es/how-evernotes-xml-export-format-works/"/>
	/// <seealso cref="https://dev.evernote.com/doc/articles/enml.php"/>

	internal class EvernoteReader
	{
		private readonly string sectionID;


		public EvernoteReader(string sectionID)
		{
			this.sectionID = sectionID;
		}


		public Task<bool> Import(string path)
		{
			return Task.FromResult(true);
		}
	}
}
