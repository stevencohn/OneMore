//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
	using System;
	using System.IO;
	using System.Windows.Forms;


	/// <summary>
	/// Copies the user's OneMore.db, including any WAL sidecar files, to a uniquely named
	/// backup file before the new version is installed. Never overwrites existing files.
	/// </summary>
	/// <remarks>
	/// The source and destination are resolved by the installer UI in the interactive user's
	/// context because this action runs as SYSTEM, where %LOCALAPPDATA% is the wrong profile.
	/// Failures are warnings only; a failed backup must not abort the installation.
	/// </remarks>
	internal class BackupDatabaseAction : CustomAction
	{
		private const string BackupName = "OneMore-backup";
		private static readonly string[] Sidecars = new[] { "-wal", "-shm" };

		private readonly string source;
		private readonly string folder;


		/// <summary>
		/// Initializes a new action.
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="stepper"></param>
		/// <param name="data">
		/// The destination folder and source file path separated by a '|' character
		/// </param>
		public BackupDatabaseAction(Logger logger, Stepper stepper, string data)
			: base(logger, stepper)
		{
			var parts = (data ?? string.Empty).Split(new[] { '|' }, 2);
			if (parts.Length == 2)
			{
				folder = parts[0].Trim().Trim('"');
				source = parts[1].Trim().Trim('"');
			}
		}


		public override int Install()
		{
			logger.WriteLine();
			logger.WriteLine("BackupDatabaseAction.Install ---");

			if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(folder))
			{
				logger.WriteLine("backup skipped; source or destination not specified");
				return SUCCESS;
			}

			try
			{
				if (!File.Exists(source))
				{
					logger.WriteLine($"backup skipped; database not found: {source}");
					return SUCCESS;
				}

				// release locks and flush the WAL before copying
				new ShutdownOneNoteAction(logger, stepper).Install();

				Directory.CreateDirectory(folder);

				var target = MakeUniquePath();
				logger.WriteLine($"copying {source} to {target}");
				File.Copy(source, target, false);

				foreach (var suffix in Sidecars)
				{
					if (File.Exists(source + suffix))
					{
						File.Copy(source + suffix, target + suffix, false);
					}
				}

				logger.WriteLine("database backup completed");
			}
			catch (Exception exc)
			{
				logger.WriteLine("database backup failed");
				logger.WriteLine(exc);

				MessageBox.Show(
					"OneMore could not back up your database. The installation will continue. " +
					"You can copy OneMore.db manually. For more information, check the logs at\n" +
					logger.LogPath,
					"Database Backup Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}

			return SUCCESS;
		}


		/// <summary>
		/// Builds a destination path that does not collide with any existing file or sidecar.
		/// </summary>
		private string MakeUniquePath()
		{
			var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
			var path = Path.Combine(folder, $"{BackupName}-{stamp}.db");

			var n = 1;
			while (File.Exists(path) || File.Exists(path + Sidecars[0]) || File.Exists(path + Sidecars[1]))
			{
				path = Path.Combine(folder, $"{BackupName}-{stamp}-{n}.db");
				n++;
			}

			return path;
		}


		public override int Uninstall()
		{
			return SUCCESS;
		}
	}
}
