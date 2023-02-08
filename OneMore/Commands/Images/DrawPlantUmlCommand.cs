//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using PlantUml.Net;
	using System;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Web;
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

			// build our own text block including Newlines...

			var builder = new StringBuilder();
			page.Root.Descendants(ns + "T")
				.Where(e => e.Attribute("selected")?.Value == "all")
				.ForEach(e =>
				{
					builder.AppendLine(e.TextValue(true));
				});

			var text = builder.ToString();

			if (string.IsNullOrWhiteSpace(text) || page.SelectionScope == SelectionScope.Empty)
			{
				UIHelper.ShowError(Resx.DrawPlantUml_EmptySelection);
				return;
			}

			text = HttpUtility.HtmlDecode(text);

			byte[] bytes;

			try
			{
				var factory = new RendererFactory();
				var renderer = factory.CreateRenderer(new PlantUmlSettings());
				bytes = await renderer.RenderAsync(text, OutputFormat.Png);
			}
			catch (Exception exc)
			{
				logger.WriteLine(text);
				logger.WriteLine("error rendering plantuml", exc);
				UIHelper.ShowError(Resx.DrawPlantUml_Error);
				return;
			}

			// find last OE of selected content so we can insert the image immediately after it...

			var container = page.Root.Descendants(ns + "T")
				.LastOrDefault(e => e.Attribute("selected")?.Value == "all")?
				.Parent;

			if (container == null || container.Name.LocalName != "OE")
			{
				UIHelper.ShowError(Resx.DrawPlantUml_NoParent);
				return;
			}

			using var stream = new MemoryStream(bytes);
			var image = (Bitmap)Image.FromStream(stream);

			container.AddAfterSelf(new XElement(ns + "OE",
				new XElement(ns + "Image",
					new XElement(ns + "Size",
						new XAttribute("width", image.Width.ToString(CultureInfo.InvariantCulture)),
						new XAttribute("height", image.Height.ToString(CultureInfo.InvariantCulture))),
					new XElement(ns + "Data", Convert.ToBase64String(bytes))
					)));

			await one.Update(page);
		}
	}
}
