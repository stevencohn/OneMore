//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Net;
	using System.Net.Http;
	using System.Net.NetworkInformation;


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
		/// using var source = new CancellationTokenSource(timeout);
		/// var response = await client.GetAsync(requestUri, source.Token)
		/// response.EnsureSuccessStatusCode();
		/// return await response.Content.ReadAsStringAsync();
		/// </code>
		/// </remarks>
		public static HttpClient Create()
		{
			if (client == null)
			{
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

				var handler = new HttpClientHandler()
				{
					AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
				};

				client = new HttpClient(handler);

				// required headers otherwise some sites may not respond
				client.DefaultRequestHeaders.Add("user-agent", "OneMore");
				client.DefaultRequestHeaders.Add("accept", "text/html,application/xhtml+xml,application/xml,application/json");
				client.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate");
				client.DefaultRequestHeaders.Add("accept-language", "en-US;q=0.9");
			}

			return client;
		}


		/// <summary>
		/// Determines if there is at least one network interface capable of reaching
		/// the interwebs
		/// </summary>
		/// <returns>True if there is a viable connection</returns>
		public static bool IsNetworkAvailable()
		{
			// only recognizes changes related to Internet adapters
			if (NetworkInterface.GetIsNetworkAvailable())
			{
				// however, this will include all adapters
				var interfaces = NetworkInterface.GetAllNetworkInterfaces();
				foreach (var face in interfaces)
				{
					// filter so we see only Internet adapters
					if (face.OperationalStatus == OperationalStatus.Up)
					{
						if ((face.NetworkInterfaceType != NetworkInterfaceType.Tunnel) &&
							(face.NetworkInterfaceType != NetworkInterfaceType.Loopback))
						{
							var statistics = face.GetIPv4Statistics();

							// all testing seems to prove that once an interface comes online
							// it has already accrued statistics for both received and sent...

							if ((statistics.BytesReceived > 0) &&
								(statistics.BytesSent > 0))
							{
								return true;
							}
						}
					}
				}
			}

			return false;
		}
	}
}
