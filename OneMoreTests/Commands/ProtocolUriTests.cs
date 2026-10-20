//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;


	[TestClass]
	public class ProtocolUriTests
	{
		[TestMethod]
		public void ParseActionAndArguments()
		{
			Assert.IsTrue(ProtocolUri.TryParse(
				"onemore://InsertTocCommand/refresh/level6/style0", out var action, out var args));

			Assert.AreEqual("InsertTocCommand", action);
			CollectionAssert.AreEqual(new[] { "refresh", "level6", "style0" }, args);
		}


		[TestMethod]
		public void ParseActionOnly()
		{
			Assert.IsTrue(ProtocolUri.TryParse(
				"onemore://RefreshFootnotesCommand/", out var action, out var args));

			Assert.AreEqual("RefreshFootnotesCommand", action);
			Assert.AreEqual(0, args.Length);
		}


		[TestMethod]
		public void ParseDecodesArguments()
		{
			Assert.IsTrue(ProtocolUri.TryParse(
				"onemore://ReportRemindersCommand/refresh/a%20b/c%2Fd%3Be", out _, out var args));

			CollectionAssert.AreEqual(new[] { "refresh", "a b", "c/d;e" }, args);
		}


		[TestMethod]
		public void ParseSchemeIsCaseInsensitive()
		{
			Assert.IsTrue(ProtocolUri.TryParse(
				"ONEMORE://RemindCommand/page;obj", out var action, out var args));

			Assert.AreEqual("RemindCommand", action);
			CollectionAssert.AreEqual(new[] { "page;obj" }, args);
		}


		[TestMethod]
		public void ParseRejectsBadInput()
		{
			Assert.IsFalse(ProtocolUri.TryParse(null, out _, out _));
			Assert.IsFalse(ProtocolUri.TryParse(string.Empty, out _, out _));
			Assert.IsFalse(ProtocolUri.TryParse("onemore://", out _, out _));
			Assert.IsFalse(ProtocolUri.TryParse("onemore:///", out _, out _));
			Assert.IsFalse(ProtocolUri.TryParse("onenote:///page", out _, out _));
			Assert.IsFalse(ProtocolUri.TryParse("onemorecli://FooCommand", out _, out _));
		}


		[TestMethod]
		public void ParseRejectsUnsafeActionNames()
		{
			// assembly-qualified and nested type names must not reach Type.GetType
			Assert.IsFalse(ProtocolUri.TryParse("onemore://Foo,Bar/x", out _, out _));
			Assert.IsFalse(ProtocolUri.TryParse("onemore://Outer+Inner/x", out _, out _));
			Assert.IsFalse(ProtocolUri.TryParse("onemore://Foo.Bar/x", out _, out _));
			Assert.IsFalse(ProtocolUri.TryParse("onemore://1Command/x", out _, out _));
		}


		[TestMethod]
		public void ValidAction()
		{
			Assert.IsTrue(ProtocolUri.IsValidAction("InsertTocCommand"));
			Assert.IsTrue(ProtocolUri.IsValidAction("_x1"));
			Assert.IsFalse(ProtocolUri.IsValidAction(null));
			Assert.IsFalse(ProtocolUri.IsValidAction(string.Empty));
			Assert.IsFalse(ProtocolUri.IsValidAction("a b"));
		}
	}
}
