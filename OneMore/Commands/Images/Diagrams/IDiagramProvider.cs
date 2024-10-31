//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading;
	using System.Threading.Tasks;


	/// <summary>
	/// The common interface implemented by each diagram provider.
	/// </summary>
	internal interface IDiagramProvider
	{
		/// <summary>
		/// Gets any error messages that may have been returned during rendering by the
		/// remote service.
		/// </summary>
		string ErrorMessages { get; }


		/// <summary>
		/// Reads the best title from the given diagram text. Each diagram type has its own
		/// way of specifying the global title. Some diagram types do not have titles.
		/// </summary>
		/// <param name="text">The diagram text</param>
		/// <returns>The title or null if no title specified</returns>
		string ReadTitle(string text);


		/// <summary>
		/// Renders a diagram specified by the given text.
		/// </summary>
		/// <param name="text">The diagram text</param>
		/// <param name="token">A cancellation token from ProgressDialog</param>
		/// <returns>
		/// A byte array of the internal image buffer or an empty array if an error has occurred
		/// </returns>
		Task<byte[]> RenderRemotely(string text, CancellationToken token);
	}
}
