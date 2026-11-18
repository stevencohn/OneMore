//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.UI
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Newtonsoft.Json.Linq;
	using River.OneMoreAddIn.UI;
	using System.Collections.Generic;
	using System.Linq;


	[TestClass]
	public class ThemeCatalogTests
	{
		// optional keys that only some themes provide; ThemeManager checks for their presence
		private static readonly string[] OptionalKeys = { "TitleBar", "TitleBarText" };


		[TestMethod]
		public void BuiltIns_AreRegisteredInOrder()
		{
			var ids = ThemeCatalog.GetThemes().Take(7).Select(t => t.Id).ToArray();

			CollectionAssert.AreEqual(
				new[] { "System", "Light", "Dark", "DarkBlue", "LightWarm", "PurpleHaze", "DarkTeal" }, ids);
		}


		[TestMethod]
		public void System_IsNotAThemeFile()
		{
			Assert.IsTrue(ThemeCatalog.Find(ThemeCatalog.SystemId).IsSystem);
			Assert.IsFalse(ThemeCatalog.Find(ThemeCatalog.LightId).IsSystem);
			Assert.IsFalse(ThemeCatalog.Find(ThemeCatalog.DarkId).IsSystem);
		}


		[TestMethod]
		public void BaseThemes_HaveExpectedDarkMode()
		{
			Assert.IsFalse(Load(ThemeCatalog.LightId).Value<bool>("DarkMode"));
			Assert.IsTrue(Load(ThemeCatalog.DarkId).Value<bool>("DarkMode"));
		}


		// a missing key renders as magenta in the UI by design; catch it here instead
		[TestMethod]
		public void EveryBuiltInTheme_DefinesAllLightThemeColors()
		{
			var required = ColorKeys(Load(ThemeCatalog.LightId))
				.Except(OptionalKeys)
				.ToList();

			Assert.IsTrue(required.Count > 0);

			foreach (var info in ThemeCatalog.GetThemes().Where(t => t.IsBuiltIn && !t.IsSystem))
			{
				var keys = ColorKeys(Load(info.Id));
				var missing = required.Except(keys).ToList();

				Assert.AreEqual(0, missing.Count,
					$"theme {info.Id} is missing colors: {string.Join(", ", missing)}");
			}
		}


		[TestMethod]
		public void EveryBuiltInTheme_HasNameAndColors()
		{
			foreach (var info in ThemeCatalog.GetThemes().Where(t => t.IsBuiltIn && !t.IsSystem))
			{
				var root = Load(info.Id);
				Assert.IsFalse(string.IsNullOrWhiteSpace(root.Value<string>("Name")), info.Id);
				Assert.IsTrue(root["Colors"] is JObject, info.Id);
			}
		}


		[TestMethod]
		public void Find_UnknownId_ReturnsNull()
		{
			Assert.IsNull(ThemeCatalog.Find("no-such-theme-xyz"));
			Assert.IsNull(ThemeCatalog.Find(null));
			Assert.IsNull(ThemeCatalog.Find(" "));
		}


		[TestMethod]
		[DataRow(0, "System")]
		[DataRow(1, "Light")]
		[DataRow(2, "Dark")]
		[DataRow(3, "DarkBlue")]
		[DataRow(4, "System")]
		[DataRow(-1, "System")]
		public void FromLegacyIndex_MapsToId(int index, string expected)
		{
			Assert.AreEqual(expected, ThemeCatalog.FromLegacyIndex(index));
		}


		private static JObject Load(string id)
		{
			return JObject.Parse(ThemeCatalog.Find(id).ReadJson());
		}


		private static IEnumerable<string> ColorKeys(JObject root)
		{
			return ((JObject)root["Colors"]).Properties().Select(p => p.Name);
		}
	}
}
