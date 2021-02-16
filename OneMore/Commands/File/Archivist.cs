﻿//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Web;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class Archivist : Loggable
	{
		private readonly OneNote one;
		private readonly string home;
		private Dictionary<string, River.OneMoreAddIn.OneNote.OneHyperlink> map;


		public Archivist(OneNote one) : this(one, null)
		{
		}


		public Archivist(OneNote one, string home)
		{
			this.one = one;
			this.home = Path.GetDirectoryName(home);
		}


		public void BuildHyperlinkMap(OneNote.Scope scope, CancellationToken token)
		{

			System.Diagnostics.Debugger.Launch();

			logger.WriteLine("building hyperlink map");
			map = one.BuildHyperlinkMap(scope, token);
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
						if (map != null)
						{
							RewirePageLinks(page, filename);
						}

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
            <one:T><![CDATA[. . <a href="onenote:#N1.G1.S1.P1&amp;
			  section-id={A640CEA0-536E-4ED0-ACC1-428AAB96501F}&amp;
			  page-id={660B56BC-B6BE-4791-B556-E4BC9BA2E60C}&amp;
			  end&amp;base-path=https://../Documents/Flux/Testing.one">Groovy Page</a>]]>
			</one:T>
			*/

			var links = page.Root.DescendantNodes().OfType<XCData>()
				.Where(c => c.Value.Contains("href=\"onenote:"));

			if (!links.Any())
			{
				return;
			}

			var text = File.ReadAllText(filename);
			var index = 0;
			var builder = new StringBuilder();

			var matches = Regex.Matches(text,
				@"<a\s+href=""(?<u>onenote:[^;]*?;section-id=(?<s>{[^}]*?})(?:&amp;page-id=(?<p>{[^}]*?}))?[^""]*?)"">(?<n>.*?)</a>",
				RegexOptions.Singleline);

			var updated = false;
			var homeUri = new Uri(home);
			logger.WriteLine($"homeUri {homeUri}");

			foreach (Match match in matches)
			{
				if (match.Success)
				{
					var groups = match.Groups;

					var uri = groups["u"];
					var id = groups["p"].Success ? groups["p"].Value : null;

					if (id != null && map.ContainsKey(id))
					{
						var name = groups["n"].Value;
						if (name.Contains('<'))
						{
							// strip html from the name to get raw text
							name = name.ToXmlWrapper().Value;
						}

						name = HttpUtility.UrlDecode(PathFactory.CleanFileName(name));
						logger.WriteLine($"name {name}");

						var item = map[id];
						var link = $"{item.Path}/{name}.htm".Replace('\\', '/');
						logger.WriteLine($"link {link}");

						var luri = new Uri(Path.Combine(home, link.Substring(link.IndexOf('/') + 1)));
						logger.WriteLine($"luri {luri} ({link})");

						var linkUri = homeUri.MakeRelativeUri(luri);
						logger.WriteLine($"linkUri {linkUri}");

						builder.Append(text.Substring(index, uri.Index - index));
						builder.Append(linkUri);
					}
					else
					{
						builder.Append(text.Substring(index, uri.Index - index));
					}

					index = uri.Index + uri.Length;
					updated = true;
				}
			}

			if (updated)
			{
				try
				{
					if (index < text.Length - 1)
					{
						builder.Append(text.Substring(index));
					}

					File.WriteAllText(filename, builder.ToString());
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
