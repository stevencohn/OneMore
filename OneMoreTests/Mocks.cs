//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreTests
{
	public static class Mocks
	{
		public const string Notebook_N1 = "{00000001-0000-0000-0000-000000000000}{1}{B0}";
		public const string SectionGroup_N1G1 = "{00000001-0001-0000-0000-000000000000}{1}{B0}";
		public const string Section_N1G1S1 = "{00000001-0001-0001-0000-000000000000}{1}{B0}";
		public const string Page_N1G1S1P1 = "{00000001-0001-0001-0001-000000000000}{1}{E0000000000000000000000000000000000000000001}";

		public const string CurrentNotebookId = Notebook_N1;
		public const string CurrentSectionGroupId = SectionGroup_N1G1;
		public const string CurrentSectionId = Section_N1G1S1;
		public const string CurrentPageId = Page_N1G1S1P1;
	}
}
