//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;
	using Win = System.Windows;


	/// <summary>
	/// Copies the hyperlink to the current page or paragraph to the clipboard
	/// </summary>
	internal class CopyLinkCommand : Command
	{
		private const string RightArrow = "\u2192";


		public CopyLinkCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var specific = (bool)args[0];
			string hyperlink = null;
			string text = null;

			using (var one = new OneNote(out var page, out var ns))
			{
				if (specific)
				{
					var selected = page.Root.Elements(ns + "Outline")
						.Where(e => !e.Elements(ns + "Meta")
							.Any(m => m.Attribute("name").Value.Equals(MetaNames.TaggingBank)))
						.Descendants(ns + "OE")
						.LastOrDefault(e => e.Attributes().Any(a => a.Name == "selected"));

					if (selected != null)
					{
						if (selected.GetAttributeValue("objectID", out var objectID))
						{
							hyperlink = one.GetHyperlink(page.PageId, objectID);
							if (!string.IsNullOrEmpty(hyperlink))
							{
								text = selected.TextValue();
								if (text.Length > 20)
								{
									text = $"{text.Substring(0, 20)}...";
								}
							}
						}
					}
					else
					{
						UIHelper.ShowError(Resx.Error_BodyContext);
					}
				}
				else
				{
					hyperlink = one.GetHyperlink(page.PageId, null);
				}

				if (!string.IsNullOrEmpty(hyperlink))
				{
					var info = one.GetPageInfo();
					var path = info.Path.Substring(1).Replace("/", $" {RightArrow} ");
					if (specific && !string.IsNullOrEmpty(text))
					{
						path = $"{path} {RightArrow} <i>{text}</i>";
					}

					var builder = new StringBuilder();
					builder.AppendLine("<html>");
					builder.AppendLine("<body>");
					builder.AppendLine("<!--StartFragment-->");
					builder.AppendLine($"<a href='{hyperlink}'>{path}</a>");
					builder.AppendLine("<!--EndFragment-->");
					builder.AppendLine("</body>");
					builder.AppendLine("</html>");
					var html = PasteRtfCommand.AddHtmlPreamble(builder.ToString());

					Logger.Current.WriteLine(html);


					// copy hyperlink to clipboard
					await SingleThreaded.Invoke(() =>
					{
						Win.Clipboard.SetText(html, Win.TextDataFormat.Html);
					});

					UIHelper.ShowInfo(specific
						? Resx.CopyLinkCommand_LinkToParagraph
						: Resx.CopyLinkCommand_LinkToPage);
				}
			}

			await Task.Yield();
		}
	}
}
