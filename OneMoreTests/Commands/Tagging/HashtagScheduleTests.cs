//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Tagging
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using System;
	using System.Data.SQLite;


	[TestClass]
	public class HashtagScheduleTests
	{
		private SQLiteConnection connection;
		private HashtagScheduleProvider provider;


		[TestInitialize]
		public void Setup()
		{
			connection = new SQLiteConnection("Data Source=:memory:");
			connection.Open();
			provider = new HashtagScheduleProvider(connection);
		}


		[TestCleanup]
		public void Teardown()
		{
			provider?.Dispose();
			connection?.Dispose();
		}


		private static HashtagScheduleRecord Record(ScanningState state)
		{
			return new HashtagScheduleRecord
			{
				State = state,
				StartTime = new DateTime(2026, 11, 1, 2, 0, 0, DateTimeKind.Utc),
				Notebooks = new[] { "{a}", "{b}" }
			};
		}


		#region provider
		[TestMethod]
		public void ReadWithoutScheduleIsNull()
		{
			Assert.IsNull(provider.Read());
		}


		[TestMethod]
		public void SaveAndReadRoundTrips()
		{
			Assert.IsTrue(provider.Save(Record(ScanningState.PendingRebuild)));

			var read = provider.Read();
			Assert.IsNotNull(read);
			Assert.AreEqual(ScanningState.PendingRebuild, read.State);
			Assert.AreEqual(new DateTime(2026, 11, 1, 2, 0, 0, DateTimeKind.Utc), read.StartTime);
			CollectionAssert.AreEqual(new[] { "{a}", "{b}" }, read.Notebooks);
			Assert.IsNull(read.OwnerPid);
			Assert.AreEqual(0, read.Attempts);
		}


		[TestMethod]
		public void SaveReplacesSingleRow()
		{
			provider.Save(Record(ScanningState.PendingRebuild));
			provider.Save(Record(ScanningState.PendingScan));

			Assert.AreEqual(ScanningState.PendingScan, provider.Read().State);

			using var cmd = connection.CreateCommand();
			cmd.CommandText = "SELECT COUNT(1) FROM hashtag_schedule";
			Assert.AreEqual(1L, cmd.ExecuteScalar());
		}


		[TestMethod]
		public void ClearRemovesSchedule()
		{
			provider.Save(Record(ScanningState.PendingScan));
			Assert.IsTrue(provider.Clear());
			Assert.IsNull(provider.Read());
		}


		[TestMethod]
		public void ChangeStateOnlyFromExpectedState()
		{
			provider.Save(Record(ScanningState.PendingScan));

			Assert.IsFalse(provider.TryChangeState(ScanningState.PendingRebuild, ScanningState.Scanning));
			Assert.AreEqual(ScanningState.PendingScan, provider.Read().State);

			Assert.IsTrue(provider.TryChangeState(ScanningState.PendingScan, ScanningState.Scanning));
			Assert.AreEqual(ScanningState.Scanning, provider.Read().State);
		}


		[TestMethod]
		public void ClaimAndHeartbeatOnlyForOwner()
		{
			provider.Save(Record(ScanningState.Scanning));

			var start = new DateTime(2026, 11, 1, 1, 0, 0, DateTimeKind.Utc);
			Assert.IsTrue(provider.Claim(1234, start));

			var read = provider.Read();
			Assert.AreEqual(1234, read.OwnerPid);
			Assert.AreEqual(start, read.OwnerStart);
			Assert.IsNotNull(read.Heartbeat);

			Assert.IsTrue(provider.Heartbeat(1234));
			Assert.IsFalse(provider.Heartbeat(9999));
		}


		[TestMethod]
		public void ClaimWithoutScheduleDoesNothing()
		{
			Assert.IsFalse(provider.Claim(1234, DateTime.UtcNow));
			Assert.IsNull(provider.Read());
		}


		[TestMethod]
		public void RecordFailureCounts()
		{
			Assert.AreEqual(-1, provider.RecordFailure());

			provider.Save(Record(ScanningState.Scanning));
			Assert.AreEqual(1, provider.RecordFailure());
			Assert.AreEqual(2, provider.RecordFailure());
		}
		#endregion provider


		#region lease
		private static readonly DateTime Now = new(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc);

		private static bool Alive(int pid, DateTime start) => true;
		private static bool Dead(int pid, DateTime start) => false;


		[TestMethod]
		public void NoRecordIsNotAlive()
		{
			Assert.IsFalse(HashtagScheduler.IsLeaseAlive(null, Now, Alive));
		}


		[TestMethod]
		public void UnclaimedIsAliveOnlyDuringStartupGrace()
		{
			var record = Record(ScanningState.PendingScan);

			record.Updated = Now.AddSeconds(-10);
			Assert.IsTrue(HashtagScheduler.IsLeaseAlive(record, Now, Dead));

			record.Updated = Now.AddMinutes(-5);
			Assert.IsFalse(HashtagScheduler.IsLeaseAlive(record, Now, Alive));
		}


		[TestMethod]
		public void FreshHeartbeatWithMatchingProcessIsAlive()
		{
			var record = Record(ScanningState.Scanning);
			record.OwnerPid = 42;
			record.OwnerStart = Now.AddHours(-1);
			record.Heartbeat = Now.AddSeconds(-20);

			Assert.IsTrue(HashtagScheduler.IsLeaseAlive(record, Now, Alive));
		}


		[TestMethod]
		public void StaleHeartbeatIsNotAlive()
		{
			var record = Record(ScanningState.Scanning);
			record.OwnerPid = 42;
			record.OwnerStart = Now.AddHours(-1);
			record.Heartbeat = Now.AddMinutes(-10);

			Assert.IsFalse(HashtagScheduler.IsLeaseAlive(record, Now, Alive));
		}


		[TestMethod]
		public void DeadOrReusedProcessIsNotAlive()
		{
			var record = Record(ScanningState.Scanning);
			record.OwnerPid = 42;
			record.OwnerStart = Now.AddHours(-1);
			record.Heartbeat = Now.AddSeconds(-5);

			Assert.IsFalse(HashtagScheduler.IsLeaseAlive(record, Now, Dead));
		}
		#endregion lease


		#region ShouldActivate
		[TestMethod]
		public void RecoverableActivatesAfterFirstCheck()
		{
			Assert.IsTrue(HashtagStage.ShouldActivate(
				true, ScanningState.Scanning, trayActive: false, recoverable: true));
		}


		[TestMethod]
		public void RecoverableDoesNotActivateWhileTrayIsActive()
		{
			Assert.IsFalse(HashtagStage.ShouldActivate(
				true, ScanningState.Scanning, trayActive: true, recoverable: true));
		}


		[TestMethod]
		public void RecoverableDoesNotActivateWhenReady()
		{
			Assert.IsFalse(HashtagStage.ShouldActivate(
				true, ScanningState.Ready, trayActive: false, recoverable: true));
		}
		#endregion ShouldActivate
	}
}
