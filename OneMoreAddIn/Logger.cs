//************************************************************************************************
// Copyright © 2016 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Text;
	using System.Threading;
	using IO = System.IO;


	/// <summary>
	/// 
	/// </summary>

	internal interface ILogger : IDisposable
	{
		void Write (string message);
		void WriteLine ();
		void WriteLine (string message);
		void WriteLine (Exception exc);
	}


	/// <summary>
	/// Provide clean access to a simple output text file.
	/// </summary>

	internal class Logger : ILogger
	{
		private static ILogger instance;

		private string path;
		private bool isNewline;
		private bool isDisposed;
		private IO.TextWriter writer;


		private Logger ()
		{
			path = IO.Path.Combine(IO.Path.GetTempPath(), "OneMore.log");
			writer = null;
			isNewline = true;
			isDisposed = false;
		}


		/// <summary>
		/// Close this file and release internal resources.
		/// </summary>

		public void Dispose ()
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


		public string Path
		{
			get { return path; }
		}


		/// <summary>
		/// Open the file for writing.
		/// </summary>
		/// <returns></returns>

		private bool EnsureWriter ()
		{
			if (writer == null)
			{
				Encoding encoding = GetEncodingWithFallback(new UTF8Encoding(false));

				try
				{
					writer = new IO.StreamWriter(
						path: path,
						append: true,
						encoding: encoding);
				}
				catch
				{
					writer = null;
				}
			}

			return (writer != null);
		}


		private static Encoding GetEncodingWithFallback (Encoding encoding)
		{
			Encoding copy = (Encoding)encoding.Clone();
			copy.EncoderFallback = EncoderFallback.ReplacementFallback;
			copy.DecoderFallback = DecoderFallback.ReplacementFallback;
			return copy;
		}


		/// <summary>
		/// Write text to the file without a newline character.
		/// </summary>
		/// <param name="message"></param>

		public void Write (string message)
		{
			if (EnsureWriter())
			{
				if (isNewline)
				{
					writer.Write(MakeHeader());
				}

				writer.Write(message);
				isNewline = false;
			}
		}


		public void WriteLine ()
		{
			if (EnsureWriter())
			{
				writer.WriteLine();
			}
		}


		public void WriteLine (string message)
		{
			if (EnsureWriter())
			{
				if (isNewline)
				{
					writer.Write(MakeHeader());
				}

				writer.WriteLine(message);
				writer.Flush();
				isNewline = true;
			}
		}


		public void WriteLine (Exception exc)
		{
			if (EnsureWriter())
			{
				if (isNewline)
				{
					writer.Write(MakeHeader());
				}

				writer.WriteLine(Serialize(exc));
				writer.Flush();
				isNewline = true;
			}
		}


		private string MakeHeader ()
		{
			//return DateTime.Now.ToString("yyyy-mm-dd hh:mm:ss.fff") +
			return DateTime.Now.ToString("hh:mm:ss.fff") +
				" [" + Thread.CurrentThread.ManagedThreadId + "] ";
		}


		private string Serialize (Exception exc)
		{
			var builder = new StringBuilder("EXCEPTION - " + exc.GetType().FullName + Environment.NewLine);
			Serialize(exc, builder, 0);
			return builder.ToString();
		}


		private void Serialize (Exception exc, StringBuilder builder, int depth)
		{
			string indent = new string(' ', depth * 2);

			builder.Append("Message...: " + exc.Message + Environment.NewLine);
			builder.Append("StackTrace: " + exc.StackTrace + Environment.NewLine);

			if (exc.TargetSite != null)
			{
				builder.Append("TargetSite: [" +
					exc.TargetSite.DeclaringType.Assembly.GetName().Name + "] " +
					exc.TargetSite.DeclaringType + "::" +
					exc.TargetSite.Name + "()");
			}

			if (exc.InnerException != null)
			{
				builder.Append("InnerExcetpion..." + Environment.NewLine);
				Serialize(exc.InnerException, builder, depth + 1);
			}
		}
	}
}
