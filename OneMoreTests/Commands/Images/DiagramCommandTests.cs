//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Images
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Tests.Builders;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	/*
	 * Test Protocol - DiagramCommand
	 * Render image from selected PlantUML and Mermaid text. These both consume external
	 * Internet REST API services.
	 *
	 * Use a simple PlantUML description such as:
	 *
	 *     @startjson
	 *     {
	 *     "Numeric": [1, 2, 3],
	 *     "String ": ["v1a", "v2b", "v3c"],
	 *     "Boolean": [true, false, true]
	 *     }
	 *     @endjson
	 *
	 * Use a simple Mermaid description such as:
	 *
	 *     erDiagram
	 *         CUSTOMER ||--o{ ORDER : places
	 *         ORDER ||--|{ LINE_ITEM : contains
	 *         CUSTOMER }|..|{ DELIVERY-ADDRESS : uses
	 *
	 * Test Steps:
	 *     1. Select the PlantUML text, from "@startuml" to "@enduml". Invoke Images/Draw PlantUML.
	 *     2. Confirm that an image is inserted immediately following the PlantUML text and that
	 *        the PlantUML text is collapsed in a collapsible paragraph.
	 *     3. Select the Mermaid text, from "erDiagram" to "uses". Invoke Images/Draw Mermaid Diagram.
	 *     4. Confirm that an image is inserted immediately following the Mermaid text and that
	 *        the Mermaid text is collapsed in a collapsible paragraph.
	 */

	[TestClass]
	public class DiagramCommandTests : TestBase
	{
		private const string PageId = "page-1";
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";

		private static readonly byte[] FixturePng = MakeFixturePng();

		private static byte[] MakeFixturePng()
		{
			using var bmp = new Bitmap(10, 10);
			using var ms = new MemoryStream();
			bmp.Save(ms, ImageFormat.Png);
			return ms.ToArray();
		}


		// Subclass of PlantUmlCommand that skips HTTP and ProgressDialog
		private sealed class TestablePlantUmlCommand : PlantUmlCommand
		{
			private readonly byte[] fixture;

			public TestablePlantUmlCommand(byte[] fixture)
			{
				this.fixture = fixture;
			}

			protected override byte[] RenderDiagram(string text)
			{
				provider = new PlantUmlDiagramProvider();
				return fixture;
			}
		}


		// Subclass of MermaidCommand that skips HTTP and ProgressDialog
		private sealed class TestableMermaidCommand : MermaidCommand
		{
			private readonly byte[] fixture;

			public TestableMermaidCommand(byte[] fixture)
			{
				this.fixture = fixture;
			}

			protected override byte[] RenderDiagram(string text)
			{
				provider = new KrokiDiagramProvider(DiagramKeys.MermaidKey);
				return fixture;
			}
		}


		[TestMethod]
		public async Task PlantUml_SelectedParagraphs_InsertsImageWithData()
		{
			// Arrange: three selected paragraphs representing PlantUML source
			var xml = new PageBuilder(PageId, "PlantUML Test")
				.WithParagraph("@startjson", selected: true)
				.WithParagraph("{}", selected: true)
				.WithParagraph("@endjson", selected: true)
				.Build();

			SetupPage(PageId, xml);

			// Act
			await new TestablePlantUmlCommand(FixturePng).Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var image = updated.Descendants(Ns + "Image").FirstOrDefault();
			Assert.IsNotNull(image, "Expected a one:Image element to be inserted");

			var data = image.Element(Ns + "Data");
			Assert.IsNotNull(data, "Expected one:Image to contain one:Data");
			Assert.IsFalse(string.IsNullOrWhiteSpace(data.Value),
				"Expected one:Data to contain base64-encoded image bytes");
		}


		[TestMethod]
		public async Task Mermaid_SelectedParagraphs_InsertsImageWithData()
		{
			// Arrange: two selected paragraphs representing Mermaid source
			var xml = new PageBuilder(PageId, "Mermaid Test")
				.WithParagraph("erDiagram", selected: true)
				.WithParagraph("    CUSTOMER ||--o{ ORDER : places", selected: true)
				.Build();

			SetupPage(PageId, xml);

			// Act
			await new TestableMermaidCommand(FixturePng).Execute();

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var image = updated.Descendants(Ns + "Image").FirstOrDefault();
			Assert.IsNotNull(image, "Expected a one:Image element to be inserted");

			var data = image.Element(Ns + "Data");
			Assert.IsNotNull(data, "Expected one:Image to contain one:Data");
			Assert.IsFalse(string.IsNullOrWhiteSpace(data.Value),
				"Expected one:Data to contain base64-encoded image bytes");
		}


		[TestMethod]
		public async Task PlantUml_InsertedImage_CarriesDiagramIdMeta()
		{
			// Arrange
			var xml = new PageBuilder(PageId, "Meta Test")
				.WithParagraph("@startuml", selected: true)
				.WithParagraph("@enduml", selected: true)
				.Build();

			SetupPage(PageId, xml);

			// Act
			await new TestablePlantUmlCommand(FixturePng).Execute();

			// Assert: the OE containing the image carries the omPlantImage Meta element
			// that links the image to its source text for refresh/extract operations
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var imageMeta = updated
				.Descendants(Ns + "Meta")
				.FirstOrDefault(m =>
					(string)m.Attribute("name") == "omPlantImage");

			Assert.IsNotNull(imageMeta,
				"Expected an omPlantImage Meta element on the image OE");

			var diagramId = (string)imageMeta.Attribute("content");
			Assert.IsFalse(string.IsNullOrWhiteSpace(diagramId),
				"Expected omPlantImage Meta content to carry a non-empty diagram ID");
		}


		[TestMethod]
		public async Task PlantUml_ImageInsertedAfterLastSelectedParagraph()
		{
			// Arrange: two selected paragraphs followed by an unselected one
			var xml = new PageBuilder(PageId, "Position Test")
				.WithParagraph("@startuml", selected: true)
				.WithParagraph("@enduml", selected: true)
				.WithParagraph("trailing text", selected: false)
				.Build();

			SetupPage(PageId, xml);

			// Act
			await new TestablePlantUmlCommand(FixturePng).Execute();

			// Assert: the image element appears before the trailing-text paragraph in DOM order
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var allElements = updated.Descendants().ToList();

			var imageIndex = allElements
				.Select((e, i) => (e, i))
				.FirstOrDefault(x => x.e.Name == Ns + "Image").i;

			var trailingIndex = allElements
				.Select((e, i) => (e, i))
				.FirstOrDefault(x => x.e.Name == Ns + "T" &&
					x.e.Value.Contains("trailing text")).i;

			Assert.IsTrue(imageIndex > 0, "Expected to find the one:Image element");
			Assert.IsTrue(trailingIndex > 0, "Expected to find the trailing-text T element");
			Assert.IsTrue(imageIndex < trailingIndex,
				"Expected the image to appear before the trailing paragraph in DOM order");
		}
	}
}
