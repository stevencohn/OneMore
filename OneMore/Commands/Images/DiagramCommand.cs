//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
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
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	#region Wrappers
	[CommandService]
	internal class MermaidCommand : DiagramCommand
	{
		public MermaidCommand() : base(DiagramKeys.MermaidKey) { }
		protected override string DiagramName => nameof(MermaidCommand);
		public override async Task Execute(params object[] args)
		{
			await base.Execute(args);
		}
	}

	[CommandService]
	internal class PlantUmlCommand : DiagramCommand
	{
		public PlantUmlCommand() : base(DiagramKeys.PlantUmlKey) { }
		protected override string DiagramName => nameof(PlantUmlCommand);
		protected override string DiagramTextMetaKey => "omPlant";
		protected override string DiagramImageMetaKey => "omPlantImage";
		public override async Task Execute(params object[] args)
		{
			await base.Execute(args);
		}
	}
	#endregion


	/// <summary>
	/// Render image from selected PlantUML or Mermaid \text
	/// </summary>
	[CommandService]
	internal class DiagramCommand : Command
	{
		private string keyword;
		private string errorMessage;
		private IDiagramProvider provider;


		public DiagramCommand()
		{
		}


		public DiagramCommand(string keyword)
		{
			this.keyword = keyword;
		}


		protected virtual string DiagramName => "DiagramCommand";


		protected virtual string DiagramImageMetaKey => "omDiagramImage";


		protected virtual string DiagramTextMetaKey => "omDiagramText";


		public override async Task Execute(params object[] args)
		{

			System.Diagnostics.Debugger.Launch();

			if (!HttpClientFactory.IsNetworkAvailable())
			{
				ShowInfo(Resx.NetwordConnectionUnavailable);
				return;
			}

			if (args.Length > 0)
			{
				// not sure why logger is null here so we have to set it
				logger = Logger.Current;

				var arguments = args.Cast<string>().ToArray();
				var gramID = arguments[0];

				// keyword may not be set if called from onemore://
				keyword = arguments.Any(a => a == DiagramKeys.MermaidKey)
					? DiagramKeys.MermaidKey
					: DiagramKeys.PlantUmlKey;

				if (arguments.Any(a => a == "extract"))
				{
					await ExtractDiagram(gramID);
				}
				else
				{
					await RefreshDiagram(gramID);
				}

				return;
			}

			await using var one = new OneNote(out var page, out var ns);

			// get selected content...

			var range = new Models.SelectionRange(page);

			var runs = range.GetSelections().ToList();
			if (range.Scope != SelectionScope.Range &&
				range.Scope != SelectionScope.Run) // incase diagram lines end with soft-breaks
			{
				ShowError(Resx.DiagramCommand_EmptySelection);
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
				ShowError(Resx.DiagramCommand_EmptySelection);
				return;
			}

			// convert...

			var bytes = Render(text);
			if (!string.IsNullOrWhiteSpace(errorMessage))
			{
				ShowError(errorMessage);
				return;
			}

			// get settings...

			var after = true;
			var collapse = false;
			var remove = false;
			var settings = new SettingsProvider().GetCollection("ImagesSheet");
			if (settings != null)
			{
				after = settings.Get("plantAfter", true);
				collapse = settings.Get("plantCollapsed", false);
				remove = settings.Get("plantRemoveText", false);
			}

			// insert image immediately before or after text...

			using var stream = new MemoryStream(bytes);
			using var image = (Bitmap)Image.FromStream(stream);

			var diagramID = Guid.NewGuid().ToString("N");
			var url = $"onemore://{DiagramName}/{diagramID}/{keyword}";
			PageNamespace.Set(ns);

			var content = new XElement(ns + "OE",
				new Meta(DiagramImageMetaKey, diagramID),
				new XElement(ns + "Image",
					new XElement(ns + "Size",
						new XAttribute("width", FormattableString.Invariant($"{image.Width:0.0}")),
						new XAttribute("height", FormattableString.Invariant($"{image.Height:0.0}"))),
					new XElement(ns + "Data", Convert.ToBase64String(bytes))
					));

			var title = provider.ReadTitle(text);
			var caption = $"{title} <span style='font-style:italic'>(" +
				$"<a href=\"{url}/extract\">{Resx.word_Extract}</a>)</span>";

			var table = AddCaptionCommand.MakeCaptionTable(ns, content, caption, null, out var cdata);

			XElement body;
			if (after)
			{
				body = new Paragraph(table.Root);
				runs[runs.Count - 1].Parent.AddAfterSelf(body);
			}
			else
			{
				body = new Paragraph(table.Root);
				runs[0].Parent.AddBeforeSelf(body);
			}

			// collapse text into sub-paragraph...

			if (collapse)
			{
				if (keyword == DiagramKeys.PlantUmlKey)
				{
					new ColorizeCommand(page, true).Colorize("plantuml", runs);
				}

				if (title != "PlantUML")
				{
					title = $"{title} PlantUML";
				}

				var container = new XElement(ns + "OE",
					new XAttribute("collapsed", "1"),
					new Meta(DiagramTextMetaKey, diagramID),
					new XElement(ns + "T",
						new XCData(
							$"<span style='font-style:italic'>{title} " +
							$"(<a href=\"{url}\">{Resx.word_Refresh}</a>)</span>"))
					);

				var parents = runs.Select(e => e.Parent).Distinct().ToList();
				parents.DescendantsAndSelf().Attributes("selected")?.Remove();

				parents.Remove();
				container.Add(new XElement(ns + "OEChildren", parents));

				if (after)
				{
					body.AddBeforeSelf(container, new Paragraph(string.Empty));
				}
				else
				{
					body.AddAfterSelf(new Paragraph(string.Empty), container);
				}
			}
			else if (remove)
			{
				runs.Select(e => e.Parent).Distinct().Remove();
			}
			else
			{
				if (after)
				{
					body.AddBeforeSelf(new Paragraph(string.Empty));
				}
				else
				{
					body.AddAfterSelf(new Paragraph(string.Empty));
				}
			}

			await one.Update(page);
		}


		private byte[] Render(string text)
		{
			provider = DiagramProviderFactory.MakeProvider(keyword);

			using var progress = new ProgressDialog(10);
			progress.Tag = text;
			progress.SetMessage("Rendering diagram from remote service...");

			// text will have gone through wrapping and unwrapping so needs decoding
			text = HttpUtility.HtmlDecode(text);
			byte[] bytes = null;

			var result = progress.ShowTimedDialog(
				async (ProgressDialog dialog, CancellationToken token) =>
				{
					try
					{
						bytes = await provider.RenderRemotely(text, token);

						if (bytes.Length > 0)
						{
							//logger.WriteLine($"received {bytes.Length} bytes");
							return true;
						}

						if (!string.IsNullOrWhiteSpace(provider.ErrorMessages))
						{
							errorMessage = provider.ErrorMessages;
							logger.WriteLine("rendering messages:");
							logger.WriteLine(provider.ErrorMessages);
							logger.WriteLine("text ---");
							logger.WriteLine(text);
						}

						return false;
					}
					catch (Exception exc)
					{
						errorMessage = exc.Message;
						logger.WriteLine($"error rendering diagram\n{text}", exc);
						return false;
					}
				});

			return result == DialogResult.OK ? bytes : new byte[0];
		}


		private async Task ExtractDiagram(string plantID)
		{
			await using var one = new OneNote(
				out var page, out var ns, OneNote.PageDetail.BinaryDataSelection);

			var element = page.Root.Descendants(ns + "OE").Elements(ns + "Meta")
				.Where(e =>
					e.Attribute("name")?.Value == DiagramImageMetaKey &&
					e.Attribute("content")?.Value == plantID)
				.Select(e => e.Parent.Elements(ns + "Image").FirstOrDefault())
				.FirstOrDefault();

			if (element == null)
			{
				ShowError(Resx.DiagramCommand_broken);
				return;
			}

			var editor = new ImageMetaTextEditor(element.Element(ns + "Data").Value);
			var uml = editor.ReadImageMetaTextEntry();
			if (!string.IsNullOrWhiteSpace(uml))
			{
				var container = element.FirstAncestor(ns + "Table").Parent;
				PageNamespace.Set(ns);

				var lines = Regex.Split(uml, "\r\n|\r|\n");

				container.AddAfterSelf(
					new Paragraph(string.Empty),
					lines.Select(line => new Paragraph(line)));

				await one.Update(page);
			}
			// else show error message?
		}


		private async Task<bool> RefreshDiagram(string plantID)
		{
			await using var one = new OneNote(
				out var page, out var ns, OneNote.PageDetail.BinaryDataSelection);

			var element = page.Root.Descendants(ns + "OE").Elements(ns + "Meta")
				.Where(e =>
					e.Attribute("name")?.Value == DiagramImageMetaKey &&
					e.Attribute("content")?.Value == plantID)
				.Select(e => e.Parent.Elements(ns + "Image").FirstOrDefault())
				.FirstOrDefault();

			if (element == null)
			{
				ShowError(Resx.DiagramCommand_broken);
				return false;
			}

			var plant = page.Root.Descendants(ns + "OE").Elements(ns + "Meta")
				.Where(e =>
					e.Attribute("name")?.Value == DiagramTextMetaKey &&
					e.Attribute("content")?.Value == plantID)
				.Select(e => e.Parent.Elements(ns + "OEChildren").FirstOrDefault())
				.FirstOrDefault();

			if (plant == null)
			{
				ShowError(Resx.DiagramCommand_broken);
				return false;
			}

			var runs = plant.Descendants(ns + "T");
			if (!runs.Any())
			{
				ShowError(Resx.DiagramCommand_broken);
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
				ShowError(Resx.DiagramCommand_broken);
				return false;
			}

			// convert...

			var bytes = Render(text);
			if (!string.IsNullOrWhiteSpace(errorMessage))
			{
				ShowError(errorMessage);
				return false;
			}

			// update image...

			var data = element.Elements(ns + "Data").FirstOrDefault();
			if (data == null)
			{
				ShowError(Resx.DiagramCommand_broken);
				return false;
			}

			data.Value = Convert.ToBase64String(bytes);

			// check image size to maintain aspect ratio...

			var size = element.Element(ns + "Size");
			if (size != null)
			{
				using var bitmap = (Bitmap)new ImageConverter().ConvertFrom(bytes);
				size.SetAttributeValue("width", FormattableString.Invariant($"{bitmap.Width:0.0}"));
				size.SetAttributeValue("height", FormattableString.Invariant($"{bitmap.Height:0.0}"));
			}

			if (keyword == DiagramKeys.PlantUmlKey)
			{
				new ColorizeCommand(page, true).Colorize("plantuml", runs);
			}

			await one.Update(page);

			return true;
		}
	}
}
