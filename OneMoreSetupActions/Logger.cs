﻿//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
	using System;
	using System.IO;
	using System.Text;


	internal class Logger : IDisposable
	{
		private bool isDisposed;
		private TextWriter writer;


		public Logger(string name)
		{
			LogPath = Path.Combine(Path.GetTempPath(), $"{name}.log");
			writer = new StreamWriter(LogPath, true);
		}


		#region Lifecycle
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}


		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!isDisposed)
				{
					if (writer != null)
					{
						writer.Flush();
						writer.Dispose();
						writer = null;
					}

					isDisposed = true;
				}
			}
		}
		#endregion Lifecycle


		public string LogPath { get; private set; }


		public bool Indented { get; set; }


		public void WriteLine()
		{
			WriteLine(string.Empty);
		}


		public void WriteLine(string message)
		{
			var m = Indented ? $"... {message}" : message;
			Console.WriteLine(m);
			writer.WriteLine(m);
			writer.Flush();
		}


		public void WriteLine(Exception exc)
		{
			var msg = FormatDetails(exc);
			Console.WriteLine(msg);
			writer.WriteLine(msg);
			writer.Flush();
		}


		private static string FormatDetails(Exception exc)
		{
			var builder = new StringBuilder();
			builder.AppendLine(exc.GetType().FullName);

			FormatDetails(exc, builder, 0);

			return builder.ToString();
		}


		private static void FormatDetails(Exception exc, StringBuilder builder, int depth)
		{
			string h = new string(' ', depth * 2);
			if (depth > 0)
			{
				builder.AppendLine($"{h}-- inner exception at depth {depth} ---------------");
			}

			builder.AppendLine($"{h}Message: {exc.Message}");

			if (!string.IsNullOrEmpty(exc.Source))
			{
				builder.AppendLine($"{h}Source: {exc.Source}");
			}

			if (!string.IsNullOrEmpty(exc.StackTrace))
			{
				builder.AppendLine($"{h}StackTrace: {exc.StackTrace}");
			}

			if (!string.IsNullOrEmpty(exc.HelpLink))
			{
				builder.AppendLine($"{h}HelpLink: {exc.HelpLink}");
			}

			if (exc.TargetSite != null)
			{
				var asm = exc.TargetSite.DeclaringType.Assembly.GetName().Name;
				var typ = exc.TargetSite.DeclaringType;
				var nam = exc.TargetSite.Name;
				builder.AppendLine($"{h}TargetSite: [{asm}] {typ}::{nam}()");
			}

			if (exc.Data?.Count > 0)
			{
				var e = exc.Data.GetEnumerator();
				while (e.MoveNext())
				{
					builder.AppendLine(
						$"{h} Data: {e.Key.ToString() + " = " + e.Current.ToString()}");
				}
			}

			if (exc.InnerException != null)
			{
				FormatDetails(exc, builder, depth + 1);
			}
		}
	}
}
