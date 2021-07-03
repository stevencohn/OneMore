//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Net;
	using System.Net.Http;


	/// <summary>
	/// Singleton to return an HttpClient
	/// </summary>
	internal static class HttpClientFactory
	{
		private static HttpClient client;


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
