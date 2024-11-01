//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Net;
	using System.Net.Http;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using Resx = Properties.Resources;



	internal class KrokiDiagramProvider : Loggable, IDiagramProvider
	{
		private readonly string diagramType;


		public KrokiDiagramProvider(string diagramType)
		{
			this.diagramType = diagramType;
		}


		public string ErrorMessages { get; private set; }


		public string ReadTitle(string text)
		{
			if (diagramType == DiagramProviderFactory.MermaidType)
			{
				// mermaid title is specified as YAML frontmatter, like:
				//
				// ---
				// title: my super cool title
				// ---
				// rest of text here...
				//

				var match = Regex.Match(text,
					@"[\n\r]+---[\n\r]+title:\s+([^\n\r]+)[\n\r]+---[\n\r]+");

				if (match.Success)
				{
					var title = match.Groups[1].Value.Trim();
					if (title.Length > 0)
					{
						return title;
					}
				}

				return "Mermaid";
			}

			return null;
		}


		public async Task<byte[]> RenderRemotely(string text, CancellationToken token)
		{
			var settings = new SettingsProvider().GetCollection(nameof(ImagesSheet));

			var uri = settings == null
				? Resx.DiagramCommand_Uri
				: settings.Get("krokiUri", Resx.DiagramCommand_Uri);

			if (!uri.EndsWith("/"))
			{
				uri = $"{uri}/";
			}

			var url = $"{uri}{diagramType}";

			var client = HttpClientFactory.Create();
			client.DefaultRequestHeaders.Add("user-agent", "OneMore");
			client.DefaultRequestHeaders.Add("accept", "image/png");

			var content = new StringContent(text, Encoding.UTF8, "text/plain");

			try
			{
				using var response = await client
					.PostAsync(url, content, token)
					.ConfigureAwait(false);

				if (!response.IsSuccessStatusCode)
				{
					if (response.StatusCode == HttpStatusCode.BadRequest)
					{
						// kroki.io does not return errors in response header :-(

						//var messages = string.Join(Environment.NewLine,
						//	response.Headers.GetValues(DiagramErrorHeader));

						ErrorMessages = $"{response.ReasonPhrase}\nPossible diagram syntax error";
					}

					return new byte[0];
				}

				var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
				return bytes;
			}
			catch (Exception exc)
			{
				logger.WriteLine("error rendering diagram from kroki.io", exc);
				return new byte[0];
			}
		}
	}
}
