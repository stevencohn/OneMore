//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Diagnostics;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using Resx = Properties.Resources;


	/// <summary>
	/// Available from the image context menu, this opens the selected image in an external
	/// image viewer/editor. By default, this is MS Paint. This can be customized in the
	/// Settings General page.
	/// </summary>
	internal class OpenImageWithCommand : Command
	{

		public OpenImageWithCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out var ns, OneNote.PageDetail.All);

			var elements = page.Root.Descendants(ns + "Image")?
				.Where(e => e.Attribute("selected")?.Value == "all");

			if (elements.IsNullOrEmpty() || (elements.Count() > 1))
			{
				ShowMessage(Resx.OpenImageWithCommand_selectOne);
				return;
			}

			try
			{
				var element = elements.Last();
				var data = Convert.FromBase64String(element.Element(ns + "Data").Value);
				using var stream = new MemoryStream(data, 0, data.Length);
				using var image = Image.FromStream(stream);
				var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

				// <xsd:pattern value="auto|png|emf|jpg"/>

				element.GetAttributeValue("format", out var formatAtt, "jpg");

				var format = formatAtt switch
				{
					"jpg" => ImageFormat.Jpeg,
					"png" => ImageFormat.Png,
					"emf" => ImageFormat.Emf,
					_ => DetectFormat(image),
				};

				if (format == ImageFormat.Jpeg) path = $"{path}.jpg";
				else if (format == ImageFormat.Png) path = $"{path}.png";
				else if (format == ImageFormat.Emf) path = $"{path}.emf";
				else path = $"{path}.bmp";

				image.Save(path, format);

				var editor = GetEditor();
				if (editor is not null)
				{
					var info = new ProcessStartInfo
					{
						FileName = editor,
						Arguments = $"\"{path}\""
					};

					if (editor == string.Empty)
					{
						info.FileName = "cmd.exe";
						info.Arguments = $"/c start {path}";
						info.UseShellExecute = true; // required for cmd.exe
						info.CreateNoWindow = true;
						info.WindowStyle = ProcessWindowStyle.Hidden;
					}

					logger.WriteLine($"opening image viewer {info.FileName} {info.Arguments}");
					Process.Start(info);
				}
				else
				{
					logger.WriteLine("invalid image viewer");
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
			}

			await Task.Yield();
		}


		private static ImageFormat DetectFormat(Image image)
		{
			var stream = new MemoryStream();
			image.Save(stream, image.RawFormat);
			stream.Position = 0;

			var detector = new ImageDetector();
			var signature = detector.GetSignature(stream);

			return signature switch
			{
				ImageSignature.JPG => ImageFormat.Jpeg,
				ImageSignature.PNG => ImageFormat.Png,
				_ => ImageFormat.Bmp,
			};
		}


		private string GetEditor()
		{
			// reader-makes-right...
			var settings = new SettingsProvider();

			var editor =
				settings.GetCollection(nameof(ImagesSheet)).Get<string>("imageViewer")
				?? settings.GetCollection("images").Get("viewer", string.Empty);

			if (editor is not null)
			{
				editor = Environment.ExpandEnvironmentVariables(editor);
			}

			return editor;
		}
	}
}
