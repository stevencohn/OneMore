//************************************************************************************************
// Copyright © 2016 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Newtonsoft.Json;
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
		private const string DebugFlag = "!";
		private const string VerboseFlag = "+";

		private static ILogger instance;
		private static bool designMode;
		private static string appname = "OneMore";

		private readonly bool stdio;
		private bool debug;
		private bool verbose;
		private string preamble;
		private string timeBar;
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
			timeBar = "|";
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
					// keep Logger independent of SettingsProvider
					var root = XElement.Load(path);
					var settings = root
						.Elements(nameof(Settings.GeneralSheet))
						.FirstOrDefault();

					if (settings is not null)
					{
						if (settings.Element("logging") is XElement loption)
						{
							if (loption.Value.EqualsICIC("debug"))
							{
								verbose = debug = true;
							}
							else if (loption.Value.EqualsICIC("verbose"))
							{
								verbose = true;
							}
						}
						else if (settings.Element("verbose") is XElement voption)
						{
							verbose = "true".EqualsICIC(voption.Value);
						}
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

					if (writer is not null)
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


		public bool IsDebug => debug;


		public bool IsVerbose => verbose;


		public string LogPath { get; private set; }


		public void Clear()
		{
			if (writer is not null)
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


		public void Debug()
		{
			if (debug)
			{
				timeBar = DebugFlag;
				WriteLine();
				timeBar = "|";
			}
		}


		public void Debug(string message)
		{
			if (debug)
			{
				timeBar = DebugFlag;
				WriteLine(message);
				timeBar = "|";
			}
		}


		public void Debug(XElement element)
		{
			if (debug)
			{
				timeBar = DebugFlag;
				WriteLine(element);
				timeBar = "|";
			}
		}


		public void DebugTime(string message, bool keepRunning = false)
		{
			if (debug)
			{
				timeBar = DebugFlag;
				WriteTime(message, keepRunning);
				timeBar = "|";
			}
		}


		public void Dump(object obj)
		{
			var frame = new StackTrace(true).GetFrame(1);
			WriteLine($"DUMP {obj.GetType().FullName} " +
				$"from ({Path.GetFileName(frame.GetFileName())} " +
				$"@line {frame.GetFileLineNumber()})");

			WriteLine(JsonConvert.SerializeObject(obj, Formatting.Indented));
		}


		public void End()
		{
			preamble = string.Empty;
			writeHeader = true;
		}


		/// <summary>
		/// Call this immediately after starting the application and before any logging.
		/// The application name implies the name of the log file on disk.
		/// </summary>
		/// <param name="name">The simple name of the application</param>
		public static void SetApplication(string name)
		{
			appname = name;
		}


		/// <summary>
		/// For VS Forms designer
		/// </summary>
		/// <param name="mode">Pass in the Forms.DesignMode property!</param>
		public static void SetDesignMode(bool mode)
		{
			designMode = mode;
		}


		/// <summary>
		/// Directly set verbose/debug logging flags, for use by GeneralSettings.
		/// Consumer needs explict cast to Logger class
		/// </summary>
		/// <param name="verbose"></param>
		/// <param name="debug"></param>
		public void SetLoggingLevel(bool verbose, bool debug)
		{
			this.verbose = verbose || debug;
			this.debug = debug;
		}


		public void Start(string message = null)
		{
			if (message is not null)
			{
				WriteLine(message);
			}

			preamble = "..";
		}


		public void StartClock()
		{
			if (clock is null)
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


		public void Verbose()
		{
			if (verbose)
			{
				timeBar = VerboseFlag;
				WriteLine();
				timeBar = "|";
			}
		}


		public void Verbose(string message)
		{
			if (verbose)
			{
				timeBar = VerboseFlag;
				WriteLine(message);
				timeBar = "|";
			}
		}


		public void Verbose(XElement element)
		{
			if (verbose)
			{
				timeBar = VerboseFlag;
				WriteLine(element);
				timeBar = "|";
			}
		}


		public void VerboseTime(string message, bool keepRunning = false)
		{
			if (verbose)
			{
				timeBar = "+";
				WriteTime(message, keepRunning);
				timeBar = "|";
			}
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

			var wh = writeHeader;
			writeHeader = false;

			WriteLine(exc);

			writeHeader = wh;
		}


		public void WriteLine(string message, XElement element)
		{
			WriteLine(message);

			var wh = writeHeader;
			writeHeader = false;

			WriteLine(element);

			writeHeader = wh;
		}


		public void WriteLine(XElement element)
		{
			var wh = writeHeader;
			writeHeader = false;

			WriteLine(element.ToString());

			writeHeader = wh;
		}


		public void WriteTime(string message, bool keepRunning = false)
		{
			if (clock is null)
			{
				WriteLine($"--:--.-- {message} @ <no time to report>");
				return;
			}

			if (!keepRunning && clock.IsRunning)
			{
				clock.Stop();
			}

			WriteLine($"{clock.Elapsed:mm\\:ss\\.ff} {message}");
		}


		// helpers...

		private bool EnsureWriter()
		{
			if (stdio)
				return true;

			if (writer is null)
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

			return (writer is not null);
		}


		private string MakeHeader()
		{
			if (!stdio)
			{
				return
					$"{Thread.CurrentThread.ManagedThreadId:00}|" +
					$"{DateTime.Now:hh:mm:ss.fff}{timeBar} {preamble}";
			}

			return string.Empty;
		}
	}
}
