//************************************************************************************************
// Copyright © 2016 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Xml.Linq;


	/// <summary>
	/// Provide clean access to a simple output text file.
	/// </summary>

	internal class Logger : ILogger
	{
		private static ILogger instance;
		private static bool designMode;
		private static string appname = "OneMore";

		private readonly bool stdio;
		private readonly bool longHeader;
		private readonly bool verbose;
		private string preamble;
		private bool isNewline;
		private bool isDisposed;
		private bool writeHeader;
		private TextWriter writer;
		private Stopwatch clock;


		private Logger()
		{
			using var process = Process.GetCurrentProcess();
			stdio = process.ProcessName.StartsWith("LINQPad");

			LogPath = Path.Combine(
				Path.GetTempPath(),
				designMode ? $"{appname}-design.log" : $"{appname}.log");

			preamble = string.Empty;
			writer = null;
			isNewline = true;
			isDisposed = false;
			writeHeader = true;

			// read settings, without dependency on SettingsProvider...

			var path = Path.Combine(
				PathHelper.GetAppDataPath(), Properties.Resources.SettingsFilename);

			if (File.Exists(path))
			{
				try
				{
					var root = XElement.Load(path);
					var settings = root
						.Elements(nameof(Settings.GeneralSheet))
						.FirstOrDefault();

					if (settings != null)
					{
						longHeader = "true".EqualsICIC(settings.Element("longHeader")?.Value);
						verbose = "true".EqualsICIC(settings.Element("verbose")?.Value);
					}
				}
				catch (Exception exc)
				{
					WriteLine("error reading settings for Logger", exc);
				}
			}
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
					clock?.Stop();
					clock = null;

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


		public static ILogger Current
		{
			get
			{
				instance ??= new Logger();
				return instance;
			}
		}


		public string LogPath { get; private set; }


		private bool EnsureWriter()
		{
			if (stdio)
				return true;

			if (writer == null)
			{
				try
				{
					// allow the UTF8 output stream to handle Unicode characters
					// by falling back to default replacement characters like '?'
					var encodingWithFallback = (Encoding)(new UTF8Encoding(false)).Clone();
					encodingWithFallback.EncoderFallback = EncoderFallback.ReplacementFallback;
					encodingWithFallback.DecoderFallback = DecoderFallback.ReplacementFallback;

					writer = new StreamWriter(LogPath, true, encodingWithFallback);
				}
				catch
				{
					writer = null;
				}
			}

			return (writer != null);
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

			preamble = string.Empty;
			isNewline = true;

			if (EnsureWriter())
			{
				WriteLine("Log restarted");
			}
		}


		public void End()
		{
			preamble = string.Empty;
			writeHeader = true;
		}


		public static void SetApplication(string name)
		{
			appname = name;
		}


		// For VS Forms designer
		public static void SetDesignMode(bool mode)
		{
			designMode = mode;
		}



		public void Start(string message = null)
		{
			if (message != null)
			{
				WriteLine(message);
			}

			preamble = "..";
		}


		public void StartClock()
		{
			if (clock == null)
			{
				clock = new Stopwatch();
			}
			else
			{
				clock.Reset();
			}

			clock.Start();
		}


		public void StartDiagnostic()
		{
			writeHeader = false;
		}


		public void StopClock()
		{
			clock?.Stop();
		}


		public void Write(string message)
		{
			if (EnsureWriter())
			{
				if (isNewline && writeHeader)
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
				if (isNewline && writeHeader)
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
				if (isNewline && writeHeader)
				{
					writer.Write(MakeHeader());
				}

				if (stdio)
				{
					Console.WriteLine(exc.FormatDetails());
				}
				else
				{
					writer.WriteLine(exc.FormatDetails());
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


		public void WriteLine(XElement element)
		{
			WriteLine(element.ToString());
		}


		public void WriteVerbose(string message)
		{
			if (verbose)
			{
				Write(message);
			}
		}


		public void Verbose()
		{
			if (verbose)
			{
				WriteLine();
			}
		}


		public void Verbose(string message)
		{
			if (verbose)
			{
				WriteLine(message);
			}
		}


		public void Verbose(XElement element)
		{
			if (verbose)
			{
				WriteLine(element);
			}
		}



		public void WriteTime(string message, bool keepRunning = false)
		{
			if (clock == null)
			{
				WriteLine($"{message} @ <no time to report>");
				return;
			}

			if (!keepRunning && clock.IsRunning)
			{
				clock.Stop();
			}

			WriteLine($"{message} @ {clock.Elapsed:mm\\:ss\\.ff}");
		}


		private string MakeHeader()
		{
			if (!stdio)
			{
				if (longHeader)
				{
					return
						$"{Thread.CurrentThread.ManagedThreadId:00}|{DateTime.Now:hh:mm:ss.fff}| {preamble}";
				}

				return $"{Thread.CurrentThread.ManagedThreadId:00}| {preamble}";
			}

			return string.Empty;
		}
	}
}
