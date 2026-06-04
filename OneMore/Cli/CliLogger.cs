//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Cli
{
	using System;
	using System.Text;
	using River.OneMoreAddIn;


	/// <summary>
	/// Special logger for CLI output, to be pumped through the named pipe back to the client.
	/// </summary>
	internal sealed class CliLogger : Logger
	{
		private readonly StringBuilder buffer;

		public CliLogger(StringBuilder buffer)
		{
			this.buffer = buffer;
		}

		public StringBuilder Buffer => buffer;


		public override void Write(string message)
		{
			buffer.Append(message);
		}


		public override void WriteLine(Exception exc)
		{
			buffer.AppendLine(exc.FormatDetails());
		}


		public override void WriteLine()
		{
			buffer.AppendLine();
		}


		public override void WriteLine(string message)
		{
			buffer.AppendLine(message);
		}
	}
}
