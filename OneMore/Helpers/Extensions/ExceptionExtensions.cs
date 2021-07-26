//************************************************************************************************
// Copyright © 2015 Waters Corporation. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Xml.Schema;


	/// <summary>
	/// Some extension methods for .NET types...
	/// </summary>

	internal static class ExceptionExtensions
	{

		/// <summary>
		/// Generate a detailed report of the given exception and all inner exceptions
		/// </summary>
		/// <param name="exc">The exception to report</param>
		/// <param name="full">
		/// True if all details are to be included such as Data, HelpLink, Source,
		/// StrackTrace, and TragetSite.
		/// </param>
		/// <returns>A string containing the formatted report</returns>

		public static string FormatDetails(this Exception exc)
		{
			var builder = new StringBuilder();
			builder.AppendLine(exc.GetType().FullName);

			FormatDetails(exc, builder, 0);

			return builder.ToString();
		}


		private static void FormatDetails(
			Exception exc, StringBuilder builder, int depth)
		{
			string h = new string(' ', depth * 2);
			if (depth > 0)
			{
				builder.AppendLine($"{h}-- inner exception at depth {depth} ---------------");
			}

			if (exc is XmlSchemaException xse)
			{
				builder.AppendLine(h + (depth == 0 ? String.Empty : "Inner ") +
					$"Exception @line:{xse.LineNumber},col:{xse.LinePosition}");
			}

			builder.AppendLine($"{h}Message: {exc.Message}");

			if (exc is COMException cex)
			{
				builder.AppendLine($"{h}Description: {ErrorCodes.GetDescription(cex.ErrorCode)}");
				builder.AppendLine($"{h}ErrorCode: 0x{cex.ErrorCode:X} ({cex.ErrorCode})");
				builder.AppendLine($"{h}HResult: 0x{cex.HResult:X} ({cex.HResult})");
			}

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


		/// <summary>
		/// Get a concatenated string of the exception messages and its inner exceptions.
		/// </summary>
		/// <param name="exc">The exception</param>
		/// <returns>A string with one or more lines</returns>
		public static string Messages(this Exception exc)
		{
			var builder = new StringBuilder();
			builder.AppendLine(exc.Message);

			while (exc.InnerException != null)
			{
				exc = exc.InnerException;
				builder.AppendLine(exc.Message);
			}

			return builder.ToString();
		}
	}
}
