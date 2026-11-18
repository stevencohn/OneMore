//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace OneMoreCli
{
	using System;


	/// <summary>
	/// An expected, user-facing error, such as one reported by the add-in. Only the message is
	/// shown to the user, without type information or a stack trace.
	/// </summary>
	internal sealed class CliException : Exception
	{
		public CliException(string message)
			: base(message)
		{
		}
	}
}
