//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.Net.Http;
	using System.Threading.Tasks;
	using Newtonsoft.Json;


	internal static class TelemetryClient
	{
		// these are not secrets
		private const string ApiUrl = "https:" + "//uetc84spi9.execute-api.us-east-1.amazonaws.com/prod/telemetry";
		private const string ApiKey = "F3J9FKPYsX7gypXLaBQmITRu5DCoIe77x8jgV4m0";

		// cached for each new session (OneNote process lifetime)
		private static readonly string sessionID = Guid.NewGuid().ToString("N");
		private static readonly Dictionary<string, string> props = Helpers.SessionLogger.CollectProperties();


		#region Schema classes
		private sealed class TelemetryEvent
		{
			public string? EventId { get; set; }
			public DateTime Timestamp { get; set; }
			public string? EventName { get; set; }
			public string? EventType { get; set; }
			public string? Version { get; set; }
			public string? SessionId { get; set; }
			public ClientInfo? Client { get; set; }
			public Payload? Data { get; set; }
		}

		private sealed class ClientInfo
		{
			public string? OneNoteVersion { get; set; }
			public string? Windows { get; set; }
			public string? WindowsBitness { get; set; }
			public string? OneNoteBitness { get; set; }
			public string? InstallBitness { get; set; }
			public string? Culture { get; set; }
		}

		private sealed class Payload
		{
			public string? Message { get; set; }
			public string? Info { get; set; }
		}
		#endregion Schema classes


		public static async Task LogEvent(string eventName, string message, string info = "")
		{
			await Log("event", eventName, message, info);
		}


		public static async Task LogException(string eventName, string message, Exception exc)
		{
			await Log("error", eventName, message, exc.FormatDetails());
		}


		private static async Task Log(
			string eventType, string eventName, string message, string info)
		{
			if (!HttpClientFactory.IsNetworkAvailable())
			{
				return;
			}

			if (eventName.EndsWith("Command"))
			{
				eventName = eventName.Substring(0, eventName.Length - 7);
			}

			var payload = MakeEvent(eventType, eventName, message, info);
			var json = JsonConvert.SerializeObject(payload);
			Logger.Current.Verbose("telemetry:");
			Logger.Current.Verbose(json);

			// add per-call header to a HttpRequestMessage instead of the HttpClient
			var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
			request.Headers.Add("x-api-key", ApiKey);
			request.Content = new StringContent(json);

			var client = HttpClientFactory.Create();

			try
			{
				// fire-and-forget...

				_ = await client.SendAsync(request);

				// comment out for production!
				//Logger.Current.WriteLine($"Status: {response.StatusCode}");
				//Logger.Current.WriteLine(await response.Content.ReadAsStringAsync());
			}
			catch (Exception ex)
			{
				Logger.Current.WriteLine("error sending telemetry", ex);
			}
		}


		static TelemetryEvent MakeEvent(
			string eventType, string eventName, string message, string info)
		{
			return new TelemetryEvent
			{
				EventId = Guid.NewGuid().ToString("N"), // unique record id
				Timestamp = DateTime.UtcNow,            // event timestamp
				EventName = eventName,                  // event/fn/op name
				EventType = eventType,                  // event | error | ...
				Version = props["Version"],             // OneMore addin version
				SessionId = sessionID,                  // OneNote sessionId/correlationId
				Client = new ClientInfo
				{
					OneNoteVersion = props["OneNoteVersion"],
					Windows = props["Windows"],
					WindowsBitness = props["WindowsBitness"],
					OneNoteBitness = props["OneNoteBitness"],
					InstallBitness = props["InstallBitness"],
					Culture = props["Culture"]
				},
				Data = new Payload
				{
					Message = message,
					Info = info ?? string.Empty
				}
			};
		}
	}
}