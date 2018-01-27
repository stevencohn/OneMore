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
	public class StylesProviderTests
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
			var provider = new StylesProvider();
			int count = provider.GetCount();
			Assert.AreEqual(15, count);
		}


		[TestMethod]
		public void GetName ()
		{
			var provider = new StylesProvider();
			var name = provider.GetName(1);
			Assert.AreEqual("Heading 2", name);
		}


		[TestMethod]
		public void GetStyle ()
		{
			var provider = new StylesProvider();
			using (var style = provider.GetStyle(1))
			{
				Assert.IsNotNull(style);
				Assert.AreEqual("Heading 2", style.Name);
				Assert.IsTrue(style.IsHeading);
			}
		}


		[TestMethod]
		public void GetStyles ()
		{
			var provider = new StylesProvider();
			var styles = provider.GetStyles();
			Assert.IsNotNull(styles);
			Assert.AreEqual(15, styles.Count);

			foreach (var style in styles)
			{
				style.Dispose();
			}
		}


		[TestMethod]
		public void Filter ()
		{
			var provider = new StylesProvider();

			var styles = provider.Filter(
				f => f.Attributes("isHeading").Any(a => a.Value.ToLower()
				.Equals("true", StringComparison.InvariantCultureIgnoreCase)));

			Assert.IsNotNull(styles);
			Assert.AreEqual(6, styles.Count());

			foreach (var style in styles)
			{
				var a = style.Attribute("isHeading");
				Assert.IsNotNull(a);
				Assert.AreEqual("true", a.Value);
			}
		}
	}
}
