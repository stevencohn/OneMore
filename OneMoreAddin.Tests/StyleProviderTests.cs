//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreAddin.Tests
{
	using System;
	using System.IO;
	using System.Linq;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	[TestClass]
	public class StyleProviderTests
	{
		[ClassInitialize]
		public static void Initialize (TestContext context)
		{
			var path = Path.Combine(PathFactory.GetAppDataPath(), Resx.CustomStylesFilename);
			if (File.Exists(path))
			{
				// might fail; don't catch
				File.Delete(path);
			}
		}


		[TestMethod]
		public void GetCount ()
		{
			var provider = new StyleProvider();
			int count = provider.Count;
			Assert.AreEqual(15, count);
		}


		[TestMethod]
		public void GetName ()
		{
			var provider = new StyleProvider();
			var name = provider.GetName(1);
			Assert.AreEqual("Heading 2", name);
		}


		[TestMethod]
		public void GetStyle ()
		{
			var provider = new StyleProvider();
			var style = provider.GetStyle(1);
			Assert.IsNotNull(style);
			Assert.AreEqual("Heading 2", style.Name);
			Assert.IsTrue(style.StyleType == StyleType.Heading);
		}


		[TestMethod]
		public void GetStyles ()
		{
			var provider = new StyleProvider();
			var styles = provider.GetStyles();
			Assert.IsNotNull(styles);
			Assert.AreEqual(15, styles.Count);
		}
	}
}
