//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable CS8632 // ignore nullable reference types

//#define DIAG

namespace River.OneMoreAddIn
{
	using System;
	using System.Net;
	using System.Net.Http;
	using System.Threading.Tasks;
	using Newtonsoft.Json;


	internal static class TelemetryClient
	{
		// these are not secrets
		private static readonly string ApiRoot = "uetc84spi9.execute-api.us-east-1.amazonaws.com";
		private static readonly string ApiUrl = $"https://{ApiRoot}/prod/telemetry";

		private static readonly string ApiKey = "F3J9FKPYsX7gypXLaBQmITRu5DCoIe77x8jgV4m0";

		// cached for each new session (OneNote process lifetime)
		public static readonly TelemetryEvent Template =
			Helpers.SessionLogger.MakeTelemetryTemplate();


		#region Schema classes
		public sealed class TelemetryEvent
		{
			public string? EventId { get; set; }        // GUID, unique for each event
			public DateTime Timestamp { get; set; }     // UTC time of the event
			public string? EventName { get; set; }      // action
			public string? EventType { get; set; }      // "event" | "error"
			public string? Version { get; set; }        // e.g. "6.8.2" of OneMoreAddIn.dll
			public string? SessionId { get; set; }      // correlation id
			public ClientInfo? Client { get; set; }
			public Payload? Data { get; set; }
		}

		public sealed class ClientInfo
		{
			public string? OneVersion { get; set; }     // e.g. "6.8.2"
			public string? OneArc { get; set; }         // e.g. "x64" of ONENOTE.exe
			public string? MoreArc { get; set; }        // e.g. "x64" of OneMoreAddIn.dll
			public int? OsMajor { get; set; }           // e.g. 10
			public int? OsMinor { get; set; }           // e.g. 0
			public int? OsBuild { get; set; }           // e.g. 26100
			public string? OsEdition { get; set; }      // e.g. "Professional"
			public string? OsArc { get; set; }          // e.g. "x64"
			public string? Culture { get; set; }        // e.g. "en-US"
		}

		public sealed class Payload
		{
			public string? Message { get; set; }
			public string? Info { get; set; }
		}
		#endregion Schema classes


		public static async Task Warmup()
		{
			var logger = Logger.Current;
			logger.StartClock();

			_ = JsonConvert.SerializeObject(new TelemetryEvent());
			await Dns.GetHostEntryAsync(ApiRoot);

			var client = HttpClientFactory.Create();
			await client.GetAsync($"https://{ApiRoot}/prod/ping");

			logger.StopClock();
			logger.WriteTime("telemetry warmup completed", after: true);
		}


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
			if (Template is null || !HttpClientFactory.IsNetworkAvailable())
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
#if DIAG
				var response = await client.SendAsync(request);
				Logger.Current.WriteLine($"Status: {response.StatusCode}");
				Logger.Current.WriteLine(await response.Content.ReadAsStringAsync());
#else
				// fire-and-forget on ThreadPool - DO NOT await
				_ = Task.Run(() =>
				{
					try
					{
						var task = client.SendAsync(request);
						task.ContinueWith(t =>
						{
							if (t.IsFaulted)
							{
								Logger.Current.WriteLine("error sending telemetry", t.Exception);
							}
						}, TaskScheduler.Default);
					}
					catch (Exception exc)
					{
						Logger.Current.WriteLine("error sending telemetry", exc);
					}
				});
#endif
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

				Version = Template.Version,

				// correlation id
				SessionId = Template.SessionId,

				// refeence template.Client - it never changes
				Client = Template.Client,

				// new payload for this event
				Data = new Payload
				{
					Message = message,
					Info = info ?? string.Empty
				}
			};
		}
	}
}