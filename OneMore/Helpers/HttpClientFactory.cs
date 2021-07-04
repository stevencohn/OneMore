//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Net;
	using System.Net.Http;


	/// <summary>
	/// Singleton to return an HttpClient
	/// </summary>
	internal static class HttpClientFactory
	{
		private static HttpClient client;


		/// <summary>
		/// Get an HttpClient
		/// </summary>
		/// <returns>The common HttpClient</returns>
		/// <remarks>
		/// To change the timeout per caller, use a unique cancellation token such as
		///
		/// <code>
		/// using (var source = new CancellationTokenSource(timeout))
		/// {
		///     var response = await client.GetAsync(requestUri, source.Token)
		///     response.EnsureSuccessStatusCode();
		///     return await response.Content.ReadAsStringAsync();
		/// }
		/// </code>
		/// </remarks>
		public static HttpClient Create()
		{
			if (client == null)
			{
				ServicePointManager.SecurityProtocol =
					SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

				client = new HttpClient();
			}

			return client;
		}
	}
}
