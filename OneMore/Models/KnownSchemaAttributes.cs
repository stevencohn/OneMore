//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Collections.Generic;


	internal static class KnownSchemaAttributes
	{
		public static List<string> GetEditedByAttributes()
		{
			return new List<string>
			{
				"author",
				"authorInitials",
				"authorResolutionID",
				"lastModifiedBy",
				"lastModifiedByInitials",
				"lastModifiedByResolutionID"
			};
		}
	}
}
