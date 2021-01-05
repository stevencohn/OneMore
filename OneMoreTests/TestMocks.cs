//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreTests
{
	using Microsoft.Office.Interop.OneNote;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using System;
	using System.Linq;
	using System.Xml.Linq;


	[TestClass]
	public class TestMocks
	{

		[TestMethod]
		public void GetHierarchyTest_Basic()
		{
			string xml;
			XElement root;

			var app = new MockApplication();

			app.GetHierarchy(string.Empty, HierarchyScope.hsNotebooks, out xml);
			Assert.IsNotNull(xml);
			root = XElement.Parse(xml);
			Assert.AreEqual("Notebooks", root.Name.LocalName);
			Assert.AreEqual("Notebook", root.Elements().First().Name.LocalName);
			Assert.IsFalse(root.Elements().First().HasElements);
			Console.WriteLine("\nNotebooks:");
			Console.WriteLine(root.ToString());

			app.GetHierarchy(string.Empty, HierarchyScope.hsSections, out xml);
			Assert.IsNotNull(xml);
			root = XElement.Parse(xml);
			Assert.AreEqual("Notebooks", root.Name.LocalName);
			Assert.AreEqual("Notebook", root.Elements().First().Name.LocalName);
			var section = root.Descendants().FirstOrDefault(e => e.Attribute("name").Value == "N1.G1.S1");
			Assert.IsNotNull(section);
			Assert.AreEqual("Section", section.Name.LocalName);
			Assert.IsFalse(section.HasElements);
			Console.WriteLine("\nSections:");
			Console.WriteLine(root.ToString());

			app.GetHierarchy(string.Empty, HierarchyScope.hsPages, out xml);
			Assert.IsNotNull(xml);
			root = XElement.Parse(xml);
			Assert.AreEqual("Notebooks", root.Name.LocalName);
			var page = root.Descendants().FirstOrDefault(e => e.Attribute("name").Value == "N1.G1.S1.P1");
			Assert.IsNotNull(page);
			Console.WriteLine("\nPages:");
			Console.WriteLine(root.ToString());

			app.GetHierarchy(string.Empty, HierarchyScope.hsChildren, out xml);
			Assert.IsNotNull(xml);
			root = XElement.Parse(xml);
			Assert.AreEqual("Notebooks", root.Name.LocalName);
			Assert.AreEqual("Notebook", root.Elements().First().Name.LocalName);
			Assert.IsFalse(root.Elements().First().HasElements);
			Console.WriteLine("\nChildren:");
			Console.WriteLine(root.ToString());

			app.GetHierarchy(string.Empty, HierarchyScope.hsSelf, out xml);
			Assert.IsNotNull(xml);
			root = XElement.Parse(xml);
			Assert.AreEqual("Notebooks", root.Name.LocalName);
			Assert.IsFalse(root.HasElements);
			Console.WriteLine("\nSelf:");
			Console.WriteLine(root.ToString());
		}


		[TestMethod]
		public void GetHierarchyTest_IDs()
		{
			string xml;
			XElement root;

			var app = new MockApplication();

			app.GetHierarchy(Mocks.Section_N1G1S1, HierarchyScope.hsNotebooks, out xml);
			Assert.IsNull(xml);

			app.GetHierarchy(Mocks.Section_N1G1S1, HierarchyScope.hsPages, out xml);
			Assert.IsNotNull(xml);
			root = XElement.Parse(xml);
			Assert.AreEqual("Section", root.Name.LocalName);
			var page = root.Elements().First();
			Assert.AreEqual("Page", page.Name.LocalName);
			Assert.AreEqual("N1.G1.S1.P1", page.Attribute("name").Value);
			Console.WriteLine($"\nSection {Mocks.Section_N1G1S1}:");
			Console.WriteLine(root.ToString());
		}


		[TestMethod]
		public void GetPageContent()
		{
			var app = new MockApplication();

			app.GetPageContent(Mocks.CurrentPageId, out var xml);
			Assert.IsNotNull(xml);
		}
	}
}
