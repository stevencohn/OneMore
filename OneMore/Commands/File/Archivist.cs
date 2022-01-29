//************************************************************************************************
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
	using System.Threading.Tasks;
	using System.Web;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class Archivist : Loggable
	{
		private readonly OneNote one;
		private readonly string home;
		private Dictionary<string, OneNote.HyperlinkInfo> map;


		public Archivist(OneNote one) : this(one, null)
		{
		}


		public Archivist(OneNote one, string home)
		{
			this.one = one;
			this.home = Path.GetDirectoryName(home);
		}


		public async Task BuildHyperlinkMap(
			OneNote.Scope scope, UI.ProgressDialog progress, CancellationToken token)
		{
			logger.WriteLine("building hyperlink map");

			map = await one.BuildHyperlinkMap(
				scope,
				token,
				async (count) =>
				{
					progress.SetMaximum(count);
					progress.SetMessage($"Scanning {count} page references");
					await Task.Yield();
				},
				async () =>
				{
					progress.Increment();
					await Task.Yield();
				});
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		/// <summary>
		/// Export a page in the given format to the specified file
		/// </summary>
		/// <param name="pageId">The ID of the single page to export</param>
		/// <param name="filename">The output file to create/overwrite</param>
		/// <param name="format">The OneNote ExportFormat</param>
		/// <param name="withAttachments">True if copy and relink attachments</param>
		/// <returns>True if the export was successful</returns>
		public bool Export(string pageId, string filename,
			OneNote.ExportFormat format, bool withAttachments = false)
		{
			logger.WriteLine($"publishing page to {filename}");

			try
			{
				if (File.Exists(filename))
				{
					File.Delete(filename);
				}

				PathFactory.EnsurePathExists(Path.GetDirectoryName(filename));

				if (one.Export(pageId, filename, format))
				{
					if (withAttachments && format == OneNote.ExportFormat.Word)
					{
						using (var word = new Helpers.Office.Word())
						{
							var page = one.GetPage(pageId);
							word.LinkupAttachments(filename, page.Root);
						}
					}

					return true;
				}

				return false;
			}
			catch (Exception exc)
			{
				var fmt = format.ToString();
				logger.WriteLine($"error publishig page as {fmt}", exc);
				UIHelper.ShowError(string.Format(Resx.SaveAs_Error, fmt) + "\n\n" + exc.Message);
				return false;
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		/// <summary>
		/// 
		/// </summary>
		/// <param name="page"></param>
		/// <param name="filename"></param>
		/// <param name="hpath"></param>
		/// <param name="bookScope"></param>
		public void ExportHTML(
			Page page, ref string filename, string hpath = null, bool bookScope = false)
		{
			// expand C:\folder\name.htm --> C:\folder\name\name.htm
			var name = Path.GetFileNameWithoutExtension(filename);				// "name"
			var fame = PathFactory.CleanFileName(Path.GetFileName(filename));	// "name.htm"
			var path = Path.Combine(Path.GetDirectoryName(filename), name);		// "c:\folder\name"
			filename = Path.Combine(path, fame);								// "c:\folder\name\name.htm"

			if (PathFactory.EnsurePathExists(path))
			{
				if (Export(page.PageId, filename, OneNote.ExportFormat.HTML))
				{
					if (map != null)
					{
						RewirePageLinks(page, filename, hpath, bookScope);
					}

					ArchiveAttachments(page, filename, path);
				}
			}
		}


		#region ExportHtml
		private void RewirePageLinks(Page page, string filename, string hpath, bool bookScope)
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
				@"<a\s+href=""(?<u>onenote:[^;]*?[#;]section-id=(?<s>{[^}]*?})(?:&amp;page-id=(?<p>{[^}]*?}))?[^""]*?)"">(?<n>.*?)</a>",
				RegexOptions.Singleline);

			var updated = false;
			var pageUri = new Uri(Path.Combine(Path.Combine(home, hpath), "x.x"));
			//logger.WriteLine($"homeUri {pageUri}");

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
						var item = map[id];

						//logger.WriteLine();
						var fpath = bookScope ? item.FullPath : item.FullPath.Substring(item.FullPath.IndexOf('/') + 1);
						//logger.WriteLine($"name {name} fpath:{fpath} FullPath:{item.FullPath}");

						var absolute = new Uri(Path.Combine(home, Path.Combine(fpath, $"{name}.htm")));
						//logger.WriteLine($"absolute {absolute}");

						var relative = HttpUtility.UrlDecode(pageUri.MakeRelativeUri(absolute).ToString());
						//logger.WriteLine($"relative {relative}");

						builder.Append(text.Substring(index, uri.Index - index));
						builder.Append(relative);

						index = uri.Index + uri.Length;
					}
					else
					{
						//logger.WriteLine($"replacing [{groups[0].Value}] with [{groups["n"].Value}]");

						builder.Append(text.Substring(index, groups[0].Index - index));
						builder.Append(groups["n"].Value);

						index = groups[0].Index + groups[0].Length;
					}

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
				// get and validate source

				var source = attachment.Attribute("pathSource")?.Value;
				if (string.IsNullOrEmpty(source) || !File.Exists(source))
				{
					source = attachment.Attribute("pathCache")?.Value;
					if (string.IsNullOrEmpty(source) || !File.Exists(source))
					{
						logger.WriteLine("broken attachment, missing pathSource/pathCache");
						logger.WriteLine(attachment);
						continue;
					}
				}

				// get preferredName; this will become the output file name

				var name = attachment.Attribute("preferredName")?.Value;
				if (string.IsNullOrEmpty(name))
				{
					logger.WriteLine($"broken attachment, missing preferredName for source:{source}");
					logger.WriteLine(attachment);
					continue;
				}

				// match <<escaped-name>>

				var escape = name.Replace(@"\", @"\\").Replace(".", @"\.");
				var matches = Regex.Matches(text, $@">(&lt;&lt;{escape}&gt;&gt;)</");
				if (matches.Count == 0)
				{
					logger.WriteLine($"attachment mis-match, cannot find name in HTML [{name}]");
					continue;
				}

				var target = Path.Combine(path, name);

				try
				{
					File.Copy(source, target, true);
					logger.WriteLine($"archived attachment {target}");

					// this is a relative path that allows us to move the folder around
					var link = $@"<a href=""./{name}"">{name}</a>";

					foreach (Match match in matches)
					{
						text = text.Substring(0, match.Groups[1].Index) +
							link +
							text.Substring(match.Groups[1].Index + match.Groups[1].Length);

						updated = true;
					}
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error copying attachment {source}", exc);
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

		#endregion ExportHtml


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		/// <summary>
		/// 
		/// </summary>
		/// <param name="root"></param>
		/// <param name="filename"></param>
		public void ExportMarkdown(Page page, string filename, bool withAttachments)
		{
			try
			{
				if (File.Exists(filename))
				{
					File.Delete(filename);
				}

				PathFactory.EnsurePathExists(Path.GetDirectoryName(filename));

				var writer = new MarkdownWriter(page, withAttachments);
				writer.Save(filename);
			}
			catch (Exception exc)
			{
				logger.WriteLine("error publishig page as Markdown", exc);
				UIHelper.ShowError(string.Format(Resx.SaveAs_Error, "Markdown") + "\n\n" + exc.Message);
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		/// <summary>
		/// Export the given page content as raw XML to the specified file
		/// </summary>
		/// <param name="root">The root content of the page</param>
		/// <param name="filename">The full path of the file to create/overwrite</param>
		public void ExportXML(XElement root, string filename, bool withAttachments)
		{
			try
			{
				var path = Path.GetDirectoryName(filename);
					
				if (withAttachments)
				{
					CopyXmlAttachments(root, path);
				}

				if (File.Exists(filename))
				{
					File.Delete(filename);
				}

				PathFactory.EnsurePathExists(path);

				root.Save(filename);
			}
			catch (Exception exc)
			{
				logger.WriteLine("error publishig page as XML", exc);
				UIHelper.ShowError(string.Format(Resx.SaveAs_Error, "XML") + "\n\n" + exc.Message);
			}
		}


		// copies attachments from the OneNote cache folder into the export folder and
		// updates the XML to reference the copies
		private void CopyXmlAttachments(XElement root, string path)
		{
			var ns = root.GetNamespaceOfPrefix(OneNote.Prefix);
			var insertedFiles = root.Descendants(ns + "InsertedFile");

			// <one:InsertedFile
			//   pathCache="C:\Users\steve\AppData\Local\Microsoft\OneNote\16.0\cache\000007LE.bin"
			//   pathSource="C:\Users\steve\OneDrive\Desktop\Steven Cohn 091621 3942 Leaf Blaster .pdf"
			//   preferredName="Steven Cohn 091621 3942 Leaf Blaster .pdf" />

			insertedFiles.ForEach(element =>
			{
				// get and validate source
				var source = element.Attribute("pathSource")?.Value;
				var name = element.Attribute("preferredName")?.Value;

				if (string.IsNullOrEmpty(source) || !File.Exists(source))
				{
					source = element.Attribute("pathCache")?.Value;
					if (!string.IsNullOrEmpty(source) && !File.Exists(source))
					{
						// broken link
						name = string.IsNullOrEmpty(name)
							? $"{Path.GetFileName(source)} (missing attachment)"
							: $"{name} (missing attachment)";

						element.SetAttributeValue("preferredName", name);
						source = null;
					}
				}

				// preferredName is used as the output file name
				if (!string.IsNullOrEmpty(name))
				{
					var target = Path.Combine(path, name);

					try
					{
						// copy cached/source file to md output directory
						File.Copy(source, target, true);
					}
					catch
					{
						// error copying, drop marker
						return;
					}

					element.SetAttributeValue("pathSource", target);
				}
			});
		}
	}
}
