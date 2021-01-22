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

				one.Export(pageId, filename, format);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error publishig page as {formatName}", exc);
				UIHelper.ShowError(string.Format(Resx.SaveAs_Error, formatName) + "\n\n" + exc.Message);
				return false;
			}

			return true;
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
						ArchiveAttachments(page, filename, path);
					}
				}
			}
			else
			{
				SaveAs(page.PageId, filename, OneNote.ExportFormat.HTML, "HTML");
			}
		}


		public void ArchiveAttachments(Page page, string filename, string path)
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
