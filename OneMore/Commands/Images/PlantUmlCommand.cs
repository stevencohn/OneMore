//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Render image from selected PlantUML or Graphviz text
	/// </summary>
	internal class PlantUmlCommand : Command
	{
		private const string PlantMeta = "omPlant";
		private const string ImageMeta = "omPlantImage";

		private string errorMessage;


		public PlantUmlCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (!HttpClientFactory.IsNetworkAvailable())
			{
				UIHelper.ShowInfo(Resx.NetwordConnectionUnavailable);
				return;
			}

			if ((args.Length > 0) && (args[0] is string pid))
			{
				// not sure why logger is null here so we have to set it
				logger = Logger.Current;
				await RefreshDiagram(pid);
				return;
			}

			using var one = new OneNote(out var page, out var ns);

			// get selected content...

			var runs = page.GetSelectedElements();
			if (!runs.Any() || page.SelectionScope != SelectionScope.Region)
			{
				UIHelper.ShowError(Resx.PlantUml_EmptySelection);
				return;
			}

			// build our own text block including Newlines...

			var builder = new StringBuilder();
			runs.ForEach(e =>
			{
				builder.AppendLine(e.TextValue(true));
			});

			var text = builder.ToString();
			if (string.IsNullOrWhiteSpace(text))
			{
				UIHelper.ShowError(Resx.PlantUml_EmptySelection);
				return;
			}

			// convert...

			var bytes = ConvertToDiagram(text);
			if (!string.IsNullOrWhiteSpace(errorMessage))
			{
				UIHelper.ShowError(errorMessage);
				return;
			}

			// get settings...

			var after = true;
			var collapse = false;
			var settings = new SettingsProvider().GetCollection("ImagesSheet");
			if (settings != null)
			{
				after = settings.Get("plantAfter", true);
				collapse = settings.Get("plantCollapsed", false);
			}

			// insert image immediately before or after text...

			var anchor = after
				? runs.Last().Parent
				: runs.First().Parent;

			using var stream = new MemoryStream(bytes);
			using var image = (Bitmap)Image.FromStream(stream);

			var plantID = Guid.NewGuid().ToString("N");
			PageNamespace.Set(ns);

			var element = new XElement(ns + "OE",
				new XAttribute("selected", "partial"),
				new Meta(ImageMeta, plantID),
				new XElement(ns + "Image",
					new XAttribute("selected", "all"),
					new XElement(ns + "Size",
						new XAttribute("width", FormattableString.Invariant($"{image.Width:0.0}")),
						new XAttribute("height", FormattableString.Invariant($"{image.Height:0.0}"))),
					new XElement(ns + "Data", Convert.ToBase64String(bytes))
					));

			if (after)
			{
				anchor.AddAfterSelf(element);
			}
			else
			{
				anchor.AddBeforeSelf(element);
			}

			// collapse text into sub-paragraph...

			if (collapse)
			{
				var url = $"onemore://PlantUmlCommand/{plantID}";

				var container = new XElement(ns + "OE",
					new XAttribute("collapsed", "1"),
					new Meta(PlantMeta, plantID),
					new XElement(ns + "T",
						new XCData(
							"<span style='font-style:italic'>PlantUML " +
							$"(<a href=\"{url}\">{Resx.word_Refresh}</a>)</span>"))
					);

				runs.First().Parent.AddBeforeSelf(container);

				var parents = runs.Select(e => e.Parent).Distinct().ToList();

				parents.DescendantsAndSelf()
					.Attributes("selected")?.Remove();

				parents.Remove();

				container.Add(new XElement(ns + "OEChildren", parents));
			}

			await one.Update(page);
		}


		private byte[] ConvertToDiagram(string text)
		{
			using var progress = new ProgressDialog(10);
			progress.Tag = text;
			progress.SetMessage("Converting using the service http://www.plantuml.com...");

			// text will have gone through wrapping and unwrapping so needs decoding
			text = HttpUtility.HtmlDecode(text);
			byte[] bytes = null;

			var result = progress.ShowTimedDialog(
				async (ProgressDialog dialog, CancellationToken token) =>
				{
					try
					{
						var renderer = new PlantUmlRenderer();
						bytes = await renderer.RenderRemotely(text, token);

						if (bytes.Length > 0)
						{
							//logger.WriteLine($"received {bytes.Length} bytes");
							return true;
						}

						if (!string.IsNullOrWhiteSpace(renderer.ErrorMessages))
						{
							errorMessage = renderer.ErrorMessages;
							logger.WriteLine("rendering messages:");
							logger.WriteLine(renderer.ErrorMessages);
						}

						return false;
					}
					catch (Exception exc)
					{
						errorMessage = exc.Message;
						logger.WriteLine($"error rendering PlantUml\n{text}", exc);
						return false;
					}
				});

			return result == DialogResult.OK ? bytes : new byte[0];
		}


		private async Task<bool> RefreshDiagram(string plantID)
		{
			using var one = new OneNote(out var page, out var ns, OneNote.PageDetail.All);

			var image = page.Root.Descendants(ns + "OE").Elements(ns + "Meta")
				.Where(e =>
					e.Attribute("name")?.Value == ImageMeta &&
					e.Attribute("content")?.Value == plantID)
				.Select(e => e.Parent.Elements(ns + "Image").FirstOrDefault())
				.FirstOrDefault();

			if (image == null)
			{
				UIHelper.ShowError(Resx.PlantUml_broken);
				return false;
			}

			var plant = page.Root.Descendants(ns + "OE").Elements(ns + "Meta")
				.Where(e =>
					e.Attribute("name")?.Value == PlantMeta &&
					e.Attribute("content")?.Value == plantID)
				.Select(e => e.Parent.Elements(ns + "OEChildren").FirstOrDefault())
				.FirstOrDefault();

			if (plant == null)
			{
				UIHelper.ShowError(Resx.PlantUml_broken);
				return false;
			}

			var runs = plant.Descendants(ns + "T");
			if (!runs.Any())
			{
				UIHelper.ShowError(Resx.PlantUml_broken);
				return false;
			}

			// build our own text block including Newlines...

			var builder = new StringBuilder();
			runs.ForEach(e =>
			{
				builder.AppendLine(e.TextValue(true));
			});

			var text = builder.ToString();
			if (string.IsNullOrWhiteSpace(text))
			{
				UIHelper.ShowError(Resx.PlantUml_broken);
				return false;
			}

			// convert...

			var bytes = ConvertToDiagram(text);
			if (bytes.Length == 0)
			{
				UIHelper.ShowError(Resx.PlantUmlCommand_tooBig);
			}

			// update image...

			var data = image.Elements(ns + "Data").FirstOrDefault();
			if (data == null)
			{
				UIHelper.ShowError(Resx.PlantUml_broken);
				return false;
			}

			data.Value = Convert.ToBase64String(bytes);

			// check image size to maintain aspect ratio...

			var size = image.Element(ns + "Size");
			if (size != null)
			{
				var width = size.GetAttributeDouble("width");
				var height = size.GetAttributeDouble("height");

				using var bitmap = (Bitmap)new ImageConverter().ConvertFrom(bytes);
				if (width < height && bitmap.Width < bitmap.Height)
				{
					width *= (bitmap.Width / bitmap.Height);
					size.SetAttributeValue("width", FormattableString.Invariant($"{width:0.0}"));
				}
				else if (width > height && bitmap.Width > bitmap.Height)
				{
					height *= (bitmap.Height / bitmap.Width);
					size.SetAttributeValue("height", FormattableString.Invariant($"{height:0.0}"));
				}
				else
				{
					size.SetAttributeValue("width", FormattableString.Invariant($"{bitmap.Width:0.0}"));
					size.SetAttributeValue("height", FormattableString.Invariant($"{bitmap.Height:0.0}"));
				}
			}

			await one.Update(page);

			return true;
		}
	}
}
