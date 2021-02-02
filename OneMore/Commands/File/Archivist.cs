//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Web;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class Archivist
	{
		private readonly ILogger logger;
		private readonly OneNote one;


		public Archivist(OneNote one)
		{
			logger = Logger.Current;
			this.one = one;
		}


		public bool SaveAs(
			string pageId, string filename,
			OneNote.ExportFormat format, string formatName)
		{
			logger.WriteLine($"publishing page to {filename}");

			try
			{
				if (File.Exists(filename))
				{
					File.Delete(filename);
				}

				return one.Export(pageId, filename, format);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error publishig page as {formatName}", exc);
				UIHelper.ShowError(string.Format(Resx.SaveAs_Error, formatName) + "\n\n" + exc.Message);
				return false;
			}
		}


		public void SaveAsHTML(Page page, ref string filename, bool archive)
		{
			if (archive)
			{
				// expand C:\folder\name.htm --> C:\folder\name\name.htm
				var name = Path.GetFileNameWithoutExtension(filename);
				var fame = Path.GetFileName(filename);
				var path = Path.Combine(Path.GetDirectoryName(filename), name);
				filename = Path.Combine(path, fame);

				if (PathFactory.EnsurePathExists(path))
				{
					if (SaveAs(page.PageId, filename, OneNote.ExportFormat.HTML, "HTML"))
					{
						RewirePageLinks(page, filename);
						ArchiveAttachments(page, filename, path);
					}
				}
			}
			else
			{
				SaveAs(page.PageId, filename, OneNote.ExportFormat.HTML, "HTML");
			}
		}


		private void RewirePageLinks(Page page, string filename)
		{
			/*
			<one:OE alignment="left" quickStyleIndex="2">
			  <one:T><![CDATA[<a href="onenote:#Alpha&amp;section-id={6B04F76E-CC8E-4666-A0E3-A8F2234C2590}&amp;
				 page-id={28D2402F-394F-4052-A3F0-AC5D31038C95}&amp;end&amp;
				 base-path=https://d.docs.live.net/6925d0374517d4b4/Documents/Flux/Duke.one">Alpha</a>]]></one:T>
			</one:OE>
			*/
			var links = page.Root.DescendantNodes().OfType<XCData>()
				.Where(c => c.Value.Contains("href=\"onenote:"));
			if (!links.Any())
			{
				return;
			}

			var text = File.ReadAllText(filename);

			// group[1] = full URI, group[2] = section-path, group[3] = text
			var matches = Regex.Matches(text, @"<a href=""(onenote:([^#]*?)(?:\.one)?#[^""]*)"">(.*?)</a>");

			foreach (Match match in matches)
			{
				if (match.Success && match.Groups[2].Length > 0)
				{
					var uri = HttpUtility.UrlDecode(match.Groups[2].Value);

					text = text.Substring(0, match.Groups[1].Index) +
						$"./{uri}/{match.Groups[3].Value}" +
						text.Substring(match.Groups[1].Index + match.Groups[1].Length);
				}
			}

			if (matches.Count > 0)
			{
				try
				{
					File.WriteAllText(filename, text);
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error writing {filename}", exc);
				}
			}
		}


		private void ArchiveAttachments(Page page, string filename, string path)
		{
			var attachments = page.Root.Descendants(page.Namespace + "InsertedFile");
			if (!attachments.Any())
			{
				return;
			}

			//<one:InsertedFile pathCache=".."
			//  pathSource="..\Attached.docx" preferredName="Attached.docx" />

			var text = File.ReadAllText(filename);
			var updated = false;

			foreach (var attachment in attachments)
			{
				var nameAttr = attachment.Attribute("preferredName");
				if (nameAttr == null)
					break;

				var name = nameAttr.Value;

				var escape = name.Replace(@"\", @"\\").Replace(".", @"\.");
				var matches = Regex.Matches(text, $@">(&lt;&lt;{escape}&gt;&gt;)</");
				if (matches.Count > 0)
				{
					var sourceAttr = attachment.Attribute("pathSource");
					if (sourceAttr == null)
						break;

					var source = sourceAttr.Value;

					try
					{
						if (File.Exists(source))
						{
							var target = Path.Combine(path, Path.GetFileName(source));
							File.Copy(source, target, true);
							logger.WriteLine($"archived attachment {target}");

							// this is a relative path that allows us to move the folder around
							var link = $@"<a href=""./{Path.GetFileName(source)}"">{name}</a>";

							foreach (Match match in matches)
							{
								text = text.Substring(0, match.Groups[1].Index) +
									link +
									text.Substring(match.Groups[1].Index + match.Groups[1].Length);

								updated = true;
							}
						}
					}
					catch (Exception exc)
					{
						logger.WriteLine($"error copying attachment {path}", exc);
					}
				}
			}

			if (updated)
			{
				try
				{
					File.WriteAllText(filename, text);
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error writing {filename}", exc);
				}
			}
		}


		public void SaveAsXML(XElement root, string filename)
		{
			try
			{
				if (File.Exists(filename))
				{
					File.Delete(filename);
				}

				root.Save(filename);
			}
			catch (Exception exc)
			{
				logger.WriteLine("error publishig page as XML", exc);
				UIHelper.ShowError(string.Format(Resx.SaveAs_Error, "XML") + "\n\n" + exc.Message);
			}
		}
	}
}
