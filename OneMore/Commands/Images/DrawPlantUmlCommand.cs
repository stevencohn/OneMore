//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using PlantUml.Net;
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using System.Linq;
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
	internal class DrawPlantUmlCommand : Command
	{
		public DrawPlantUmlCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var one = new OneNote(out var page, out var ns);

			// get selecte content...

			var runs = page.GetSelectedElements();

			if (!runs.Any() || page.SelectionScope != SelectionScope.Region)
			{
				UIHelper.ShowError(Resx.DrawPlantUml_EmptySelection);
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
				UIHelper.ShowError(Resx.DrawPlantUml_EmptySelection);
				return;
			}

			// convert...

			var bytes = ConvertToDiagram(text);

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
			var image = (Bitmap)Image.FromStream(stream);

			var element = new XElement(ns + "OE",
				new XAttribute("selected", "partial"),
				new XElement(ns + "Image",
					new XAttribute("selected", "all"),
					new XElement(ns + "Size",
						new XAttribute("width", image.Width.ToString(CultureInfo.InvariantCulture)),
						new XAttribute("height", image.Height.ToString(CultureInfo.InvariantCulture))),
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
				var container = new XElement(ns + "OE",
					new XAttribute("collapsed", "1"),
					new XElement(ns + "T",
						new XCData("<span style='font-style:italic'>PlantUML</span>"))
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

			text = HttpUtility.HtmlDecode(text);
			byte[] bytes = null;

			var result = progress.ShowTimedDialog(
				async (ProgressDialog dialog, CancellationToken token) =>
				{
					try
					{
						var factory = new RendererFactory();
						var renderer = factory.CreateRenderer(new PlantUmlSettings());
						bytes = await renderer.RenderAsync((string)dialog.Tag, OutputFormat.Png);
					}
					catch (Exception exc)
					{
						logger.WriteLine(text);
						logger.WriteLine("error rendering plantuml", exc);
						UIHelper.ShowError(Resx.DrawPlantUml_Error);
						return false;
					}

					return true;
				});

			return result == DialogResult.OK ? bytes : new byte[0];
		}
	}
}
