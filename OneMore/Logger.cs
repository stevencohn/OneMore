//************************************************************************************************
// Copyright © 2016 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Text;
	using System.Threading;


	/// <summary>
	/// 
	/// </summary>

	public interface ILogger : IDisposable
	{
		string LogPath { get; }
		void Write(string message);
		void WriteLine();
		void WriteLine(string message);
		void WriteLine(Exception exc);
		void WriteLine(string message, Exception exc);
	}


	/// <summary>
	/// Provide clean access to a simple output text file.
	/// </summary>

	internal class Logger : ILogger
	{
		private static ILogger instance;
		private static bool designMode;

		private readonly bool stdio;
		private readonly int processId;
		private bool isNewline;
		private bool isDisposed;
		private TextWriter writer;


		private Logger()
		{
			using (var process = Process.GetCurrentProcess())
			{
				processId = process.Id;
				stdio = process.ProcessName.StartsWith("LINQPad");
			}

			if (!stdio)
			{
				LogPath = Path.Combine(
					Path.GetTempPath(),
					designMode ? "OneMore-design.log" : "OneMore.log");
			}

			writer = null;
			isNewline = true;
			isDisposed = false;
		}


		/// <summary>
		/// Close this file and release internal resources.
		/// </summary>

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


		public static ILogger Current
		{
			get
			{
				if (instance == null)
				{
					instance = new Logger();
				}

				return instance;
			}
		}


		public string LogPath { get; }


		/// <summary>
		/// Open the file for writing.
		/// </summary>
		/// <returns></returns>
		private bool EnsureWriter()
		{
			if (stdio)
				return true;

			if (writer == null)
			{
				try
				{
					writer = new StreamWriter(
						LogPath, true, GetEncodingWithFallback(new UTF8Encoding(false)));
				}
				catch
				{
					writer = null;
				}
			}

			return (writer != null);
		}


		private static Encoding GetEncodingWithFallback(Encoding encoding)
		{
			var copy = (Encoding)encoding.Clone();
			copy.EncoderFallback = EncoderFallback.ReplacementFallback;
			copy.DecoderFallback = DecoderFallback.ReplacementFallback;
			return copy;
		}


		public void Clear()
		{
			if (writer != null)
			{
				writer.Flush();
				writer.Dispose();
				writer = null;
			}

			File.Delete(LogPath);

			if (EnsureWriter())
			{
				WriteLine("Log restarted");
			}
		}


		// For VS Forms designer
		public static void SetDesignMode(bool mode)
		{
			designMode = mode;
		}


		/// <summary>
		/// Write text to the file without a newline character.
		/// </summary>
		/// <param name="message"></param>

		public void Write(string message)
		{
			if (EnsureWriter())
			{
				if (isNewline && !stdio)
				{
					writer.Write(MakeHeader());
				}

				if (stdio)
					Console.Write(message);
				else
					writer.Write(message);

				isNewline = false;
			}
		}


		public void WriteLine()
		{
			if (EnsureWriter())
			{
				if (stdio)
				{
					Console.WriteLine();
				}
				else
				{
					writer.WriteLine();
				}
			}
		}


		public void WriteLine(string message)
		{
			if (EnsureWriter())
			{
				if (isNewline && !stdio)
				{
					writer.Write(MakeHeader());
				}

				if (stdio)
				{
					Console.WriteLine(message);
				}
				else
				{
					writer.WriteLine(message);
					writer.Flush();
				}

				isNewline = true;
			}
		}


		public void WriteLine(Exception exc)
		{
			if (EnsureWriter())
			{
				if (isNewline && !stdio)
				{
					writer.Write(MakeHeader());
				}

				if (stdio)
				{
					Console.WriteLine(Serialize(exc));
				}
				else
				{
					writer.WriteLine(Serialize(exc));
					writer.Flush();
				}

				isNewline = true;
			}
		}


		public void WriteLine(string message, Exception exc)
		{
			WriteLine(message);
			WriteLine(exc);
		}


		private string MakeHeader()
		{
			//return DateTime.Now.ToString("hh:mm:ss.fff") +
			//	" [" + Thread.CurrentThread.ManagedThreadId + "] ";
			return $"{processId}:{Thread.CurrentThread.ManagedThreadId}] ";
		}


		private string Serialize(Exception exc)
		{
			var builder = new StringBuilder("EXCEPTION - ");
			builder.AppendLine(exc.GetType().FullName);

			Serialize(exc, builder);
			return builder.ToString();
		}


		private void Serialize(Exception exc, StringBuilder builder, int depth = 0)
		{
			if (depth > 0)
			{
				builder.AppendLine($"-- inner exception at depth {depth} ---------------");
			}

			builder.AppendLine("Message...: " + exc.Message);
			builder.AppendLine("StackTrace: " + exc.StackTrace);

			if (exc.TargetSite != null)
			{
				builder.AppendLine("TargetSite: [" +
					exc.TargetSite.DeclaringType.Assembly.GetName().Name + "] " +
					exc.TargetSite.DeclaringType + "::" +
					exc.TargetSite.Name + "()");
			}

			if (exc.InnerException != null)
			{
				Serialize(exc.InnerException, builder, depth + 1);
			}
		}
	}
}
