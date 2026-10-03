//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Tagging
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;


	[TestClass]
	public class HashtagStageTests
	{
		[TestMethod]
		public void ShouldActivate_FirstCheck_CatalogNeedsBuildingAndNoTrayIsRunning_IsTrue()
		{
			Assert.IsTrue(HashtagStage.ShouldActivate(false, ScanningState.PendingRebuild, trayActive: false));
			Assert.IsTrue(HashtagStage.ShouldActivate(false, ScanningState.PendingScan, trayActive: false));
		}


		[TestMethod]
		public void ShouldActivate_FirstCheck_CatalogIsReady_IsFalse()
		{
			Assert.IsFalse(HashtagStage.ShouldActivate(false, ScanningState.Ready, trayActive: false));
			Assert.IsFalse(HashtagStage.ShouldActivate(false, ScanningState.None, trayActive: false));
		}


		[TestMethod]
		public void ShouldActivate_FirstCheck_TrayIsAlreadyWorking_IsFalse()
		{
			Assert.IsFalse(HashtagStage.ShouldActivate(false, ScanningState.Scanning, trayActive: true));
			Assert.IsFalse(HashtagStage.ShouldActivate(false, ScanningState.Rebuilding, trayActive: true));
		}


		[TestMethod]
		public void ShouldActivate_LaterChecks_NeverActivate()
		{
			// the case that went wrong: the tray finished its scan and exited, so the state still
			// reads Scanning but no tray is running; that must not start another scan
			Assert.IsFalse(HashtagStage.ShouldActivate(true, ScanningState.Scanning, trayActive: false));
			Assert.IsFalse(HashtagStage.ShouldActivate(true, ScanningState.PendingRebuild, trayActive: false));
			Assert.IsFalse(HashtagStage.ShouldActivate(true, ScanningState.PendingScan, trayActive: false));
		}
	}
}
