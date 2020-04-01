//************************************************************************************************
// Copyright © 2015 Waters Corporation. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
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

		public static string FormatDetails (this Exception exc, bool full = true)
		{
			var builder = new StringBuilder();
			FormatDetails(exc, builder, 0, full);

			return builder.ToString();
		}


		private static void FormatDetails (
			Exception exc, StringBuilder builder, int level, bool full)
		{
			string h = new string(' ', level * 2);

			if (exc is XmlSchemaException xse)
			{
				builder.AppendLine(h + (level == 0 ? String.Empty : "Inner ") +
					"Exception @line:" + xse.LineNumber + ",col:" + xse.LinePosition);
			}

			builder.AppendLine(h + "  " + exc.Message);

			if (full)
			{
				if (!String.IsNullOrEmpty(exc.Source))
				{
					builder.AppendLine(h + "  Source: " + exc.Source);
				}

				if (!String.IsNullOrEmpty(exc.StackTrace))
				{
					builder.AppendLine(h + "  StackTrace: " + exc.StackTrace);
				}

				if (!String.IsNullOrEmpty(exc.HelpLink))
				{
					builder.AppendLine(h + "  HelpLink: " + exc.HelpLink);
				}

				if (exc.TargetSite != null)
				{
					builder.AppendLine(h + "  TargetSite: " + exc.TargetSite.Name);

					builder.AppendLine(h + "    assembly: " +
						exc.TargetSite.DeclaringType.Assembly.GetName().Name);

					builder.AppendLine(h + "    declaringType: " +
						exc.TargetSite.DeclaringType.ToString());
				}

				if ((exc.Data != null) && (exc.Data.Count > 0))
				{
					var e = exc.Data.GetEnumerator();
					while (e.MoveNext())
					{
						builder.AppendLine(h + "  Data: " +
							e.Key.ToString() + " = " + e.Current.ToString());
					}
				}
			}

			if (exc.InnerException != null)
			{
				FormatDetails(exc, builder, level + 1, full);
			}
		}
	}
}
