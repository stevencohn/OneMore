//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Newtonsoft.Json;
	using System;
	using System.Data;
	using System.Data.SQLite;
	using System.Globalization;
	using Resx = Properties.Resources;


	/// <summary>
	/// The persisted state of a scheduled hashtag scan or rebuild. All times are UTC.
	/// </summary>
	internal sealed class HashtagScheduleRecord
	{
		public ScanningState State { get; set; } = ScanningState.None;
		public DateTime StartTime { get; set; } = DateTime.UtcNow;
		public string[] Notebooks { get; set; } = Array.Empty<string>();
		public int? OwnerPid { get; set; }
		public DateTime? OwnerStart { get; set; }
		public DateTime? Heartbeat { get; set; }
		public int Attempts { get; set; }
		public DateTime Updated { get; set; } = DateTime.UtcNow;
	}


	/// <summary>
	/// Reads and writes the single-row schedule of the hashtag scanner. This is deliberately
	/// separate from HashtagProvider: the schedule must be writable before a hashtag catalog
	/// exists, and creating a HashtagProvider would create an empty catalog, which would then
	/// look built. It is also not dropped when the catalog is dropped for a rebuild.
	/// </summary>
	internal class HashtagScheduleProvider : DatabaseProvider
	{
		private const string Domain = "hashtag schedule";
		private const string Table = "hashtag_schedule";
		private const string Columns =
			"state, startTime, notebooks, ownerPid, ownerStart, heartbeat, attempts, updated";


		/// <summary>
		/// Initialize this provider, opening the standard database
		/// </summary>
		public HashtagScheduleProvider()
			: base()
		{
			OpenDatabase();
			RefreshDataSchema(Domain, Resx.HashtagScheduleDB);
		}


		/// <summary>
		/// Initialize this provider over an open connection, such as an in-memory database.
		/// </summary>
		internal HashtagScheduleProvider(SQLiteConnection connection)
		{
			con = connection;
			RefreshDataSchema(Domain, Resx.HashtagScheduleDB);
		}


		/// <summary>
		/// Reads the schedule, or null if there is none.
		/// </summary>
		public HashtagScheduleRecord Read()
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = $"SELECT {Columns} FROM {Table} WHERE scheduleID = 0";

			try
			{
				using var reader = cmd.ExecuteReader();
				if (!reader.Read())
				{
					return null;
				}

				var record = new HashtagScheduleRecord
				{
					State = Enum.TryParse<ScanningState>(reader.GetString(0), out var state)
						? state
						: ScanningState.None,

					StartTime = ParseTime(reader.GetString(1)) ?? DateTime.UtcNow,
					Notebooks = ParseNotebooks(reader.GetString(2)),
					OwnerPid = reader.IsDBNull(3) ? null : reader.GetInt32(3),
					OwnerStart = reader.IsDBNull(4) ? null : ParseTime(reader.GetString(4)),
					Heartbeat = reader.IsDBNull(5) ? null : ParseTime(reader.GetString(5)),
					Attempts = reader.GetInt32(6),
					Updated = ParseTime(reader.GetString(7)) ?? DateTime.UtcNow
				};

				return record;
			}
			catch (Exception exc)
			{
				ReportError("error reading hashtag schedule", cmd, exc);
				return null;
			}
		}


		/// <summary>
		/// Replaces the schedule in a single atomic statement.
		/// </summary>
		public bool Save(HashtagScheduleRecord record)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText =
				$"REPLACE INTO {Table} (scheduleID, {Columns}) " +
				"VALUES (0, @state, @start, @books, @pid, @pstart, @beat, @attempts, @updated)";

			cmd.Parameters.AddWithValue("@state", record.State.ToString());
			cmd.Parameters.AddWithValue("@start", FormatTime(record.StartTime));
			cmd.Parameters.AddWithValue("@books", JsonConvert.SerializeObject(record.Notebooks));
			cmd.Parameters.AddWithValue("@pid", (object)record.OwnerPid ?? DBNull.Value);
			cmd.Parameters.AddWithValue("@pstart", Format(record.OwnerStart));
			cmd.Parameters.AddWithValue("@beat", Format(record.Heartbeat));
			cmd.Parameters.AddWithValue("@attempts", record.Attempts);
			cmd.Parameters.AddWithValue("@updated", FormatTime(DateTime.UtcNow));

			return Execute(cmd, "error saving hashtag schedule") > 0;
		}


		/// <summary>
		/// Deletes the schedule.
		/// </summary>
		public bool Clear()
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = $"DELETE FROM {Table}";
			return Execute(cmd, "error clearing hashtag schedule") >= 0;
		}


		/// <summary>
		/// Changes the state only if it is still the expected state, so a stale writer cannot
		/// overwrite a change made by another process.
		/// </summary>
		/// <returns>True if the state was changed</returns>
		public bool TryChangeState(ScanningState expected, ScanningState state)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText =
				$"UPDATE {Table} SET state = @state, updated = @updated " +
				"WHERE scheduleID = 0 AND state = @expected";

			cmd.Parameters.AddWithValue("@state", state.ToString());
			cmd.Parameters.AddWithValue("@expected", expected.ToString());
			cmd.Parameters.AddWithValue("@updated", FormatTime(DateTime.UtcNow));

			return Execute(cmd, "error changing hashtag schedule state") > 0;
		}


		/// <summary>
		/// Records the tray process that owns the schedule and a fresh heartbeat. Does nothing
		/// if there is no schedule.
		/// </summary>
		public bool Claim(int pid, DateTime processStart)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText =
				$"UPDATE {Table} SET ownerPid = @pid, ownerStart = @pstart, heartbeat = @beat " +
				"WHERE scheduleID = 0";

			cmd.Parameters.AddWithValue("@pid", pid);
			cmd.Parameters.AddWithValue("@pstart", FormatTime(processStart));
			cmd.Parameters.AddWithValue("@beat", FormatTime(DateTime.UtcNow));

			return Execute(cmd, "error claiming hashtag schedule") > 0;
		}


		/// <summary>
		/// Refreshes the heartbeat, but only for the process that owns the schedule.
		/// </summary>
		public bool Heartbeat(int pid)
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText =
				$"UPDATE {Table} SET heartbeat = @beat WHERE scheduleID = 0 AND ownerPid = @pid";

			cmd.Parameters.AddWithValue("@pid", pid);
			cmd.Parameters.AddWithValue("@beat", FormatTime(DateTime.UtcNow));

			return Execute(cmd, "error updating hashtag schedule heartbeat") > 0;
		}


		/// <summary>
		/// Increments and returns the failure count, or -1 if there is no schedule.
		/// </summary>
		public int RecordFailure()
		{
			using var cmd = con.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText =
				$"UPDATE {Table} SET attempts = attempts + 1, updated = @updated " +
				"WHERE scheduleID = 0";

			cmd.Parameters.AddWithValue("@updated", FormatTime(DateTime.UtcNow));

			if (Execute(cmd, "error recording hashtag schedule failure") <= 0)
			{
				return -1;
			}

			return Read()?.Attempts ?? -1;
		}


		private int Execute(SQLiteCommand cmd, string error)
		{
			try
			{
				return cmd.ExecuteNonQuery();
			}
			catch (Exception exc)
			{
				ReportError(error, cmd, exc);
				return -1;
			}
		}


		private static string FormatTime(DateTime time)
		{
			return time.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
		}


		private static object Format(DateTime? time)
		{
			return time.HasValue ? FormatTime(time.Value) : DBNull.Value;
		}


		private static DateTime? ParseTime(string text)
		{
			return DateTime.TryParse(text, CultureInfo.InvariantCulture,
				DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var time)
				? time
				: null;
		}


		private static string[] ParseNotebooks(string json)
		{
			try
			{
				return JsonConvert.DeserializeObject<string[]>(json) ?? Array.Empty<string>();
			}
			catch (JsonException)
			{
				return Array.Empty<string>();
			}
		}
	}
}
