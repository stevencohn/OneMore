//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Snippets
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;

	/*
	 * Test Protocol - BoxTypesProvider
	 * Verifies GetDefault(), the shipped-JSON parsing path used both to seed the Box Types
	 * settings UI and to reset a built-in type to its as-installed values. Deliberately does
	 * not exercise LoadAll()/GetTheme()/SaveAll(), since those read and write through the real
	 * SettingsProvider, which persists to the current user's actual AppData settings file -
	 * not something a unit test should touch.
	 */

	[TestClass]
	public class BoxTypesProviderTests
	{
		[TestMethod]
		public void GetDefault_Info_ReturnsShippedValues()
		{
			var box = new BoxTypesProvider().GetDefault("info");

			Assert.IsNotNull(box);
			Assert.AreEqual("info", box.Id);
			Assert.AreEqual("#DEEBF6", box.Shading);
			Assert.AreEqual("1F6C8", box.Symbol);
			Assert.AreEqual("#2E75B5", box.SymbolColor);
			Assert.AreEqual(22, box.SymbolSize);
			Assert.AreEqual("#333333", box.TitleColor);
			Assert.AreEqual("#333333", box.TextColor);
			Assert.IsTrue(box.IsBuiltin);
			Assert.IsFalse(string.IsNullOrEmpty(box.Title));
		}


		[TestMethod]
		public void GetDefault_Warn_ReturnsShippedValues()
		{
			var box = new BoxTypesProvider().GetDefault("warn");

			Assert.IsNotNull(box);
			Assert.AreEqual("#FADBD2", box.Shading);
			Assert.AreEqual("26A0", box.Symbol);
			Assert.AreEqual("#E84C22", box.SymbolColor);
		}


		[TestMethod]
		public void GetDefault_Note_ReturnsShippedValues()
		{
			var box = new BoxTypesProvider().GetDefault("note");

			Assert.IsNotNull(box);
			Assert.AreEqual("#E5E0EC", box.Shading);
			Assert.AreEqual("1F4D3", box.Symbol);
			Assert.AreEqual("#5F497A", box.SymbolColor);
		}


		[TestMethod]
		public void GetDefault_UnknownId_ReturnsNull()
		{
			var box = new BoxTypesProvider().GetDefault("does-not-exist");

			Assert.IsNull(box);
		}
	}
}
