
namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	using System;


	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Critical Code Smell", "S3871:Exception types should be public")]

	internal sealed class CalculatorException : Exception
	{
		private const string UnknownPosition = "<position:unknown>";


		public CalculatorException(string message)
			: base(message)
		{
			Position = UnknownPosition;
		}


		public CalculatorException(string message, string position)
			: base(message)
		{
			Position = position;
		}


		public CalculatorException(string message, Exception innerException)
			: base(message, innerException)
		{
			Position = UnknownPosition;
		}


		public string Position { private set; get; }


		public override string Message =>
			string.Format("{0}, cell:{1}", base.Message, Position);


		/// <summary>
		/// Plain, non-localized message, used for logging.
		/// </summary>
		public string Text => base.Message;
	}
}
