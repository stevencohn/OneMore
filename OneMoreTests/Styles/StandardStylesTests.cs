//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Styles
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Styles;
	using System.Globalization;
	using System.Threading;

	[TestClass]
	public class StandardStylesTests
	{
		// issue 2751: GetDefaults formatted the default font size with the thread culture
		// but parsed it with the invariant culture, so a non-'.' decimal separator threw
		// a FormatException

		[TestMethod]
		public void GetDefaults_CustomDecimalSeparator_DoesNotThrow()
		{
			var thread = Thread.CurrentThread;
			var original = thread.CurrentCulture;
			try
			{
				var culture = (CultureInfo)CultureInfo.GetCultureInfo("en-US").Clone();
				culture.NumberFormat.NumberDecimalSeparator = "'";
				thread.CurrentCulture = culture;

				Assert.AreEqual("16.0", StandardStyles.Heading1.GetDefaults().FontSize);
				Assert.AreEqual(
					StyleBase.DefaultFontSize.ToString("0.0", CultureInfo.InvariantCulture),
					StandardStyles.Normal.GetDefaults().FontSize);
			}
			finally
			{
				thread.CurrentCulture = original;
			}
		}


		[TestMethod]
		public void GetDefaults_CommaDecimalSeparator_KeepsDefaultSize()
		{
			var thread = Thread.CurrentThread;
			var original = thread.CurrentCulture;
			try
			{
				thread.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");

				// "11,0" would have silently parsed as 110 before the fix
				Assert.AreEqual(
					StyleBase.DefaultFontSize.ToString("0.0", CultureInfo.InvariantCulture),
					StandardStyles.Normal.GetDefaults().FontSize);
			}
			finally
			{
				thread.CurrentCulture = original;
			}
		}
	}
}
